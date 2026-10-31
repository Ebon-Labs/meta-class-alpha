using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VoiceChatController : NetworkBehaviour
{
    const int StreamClipSeconds = 10;

    [Header("Capture")]
    [SerializeField] int sampleRate = 16000;
    [SerializeField] int transmitChunkMs = 100;

    [Header("Playback")]
    [SerializeField] float voiceVolume = 1f;
    [SerializeField] float spatialBlend = 1f;
    [SerializeField] float minDistance = 1f;
    [SerializeField] float maxDistance = 25f;

    AudioSource audioSource;
    AudioClip micClip;
    string micDevice;
    int lastMicPos;
    float nextTransmitTime;

    AudioClip streamClip;
    readonly Queue<float> playbackQueue = new Queue<float>();
    readonly object playbackLock = new object();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = spatialBlend;
        audioSource.volume = voiceVolume;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    public override void OnStartLocalPlayer()
    {
        AudioListener listener = GetComponentInChildren<AudioListener>(true);
        if (listener != null)
            listener.enabled = true;

        RequestMicrophonePermission();
        StartMicrophone();
        RefreshMicrophoneMuteState();
    }

    public override void OnStopLocalPlayer()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        if (isLocalPlayer)
            StopMicrophone();
    }

    void Update()
    {
        if (isLocalPlayer)
            CaptureAndSend();
    }

    /// <summary>
    /// Called by ClassController when muteGuests changes.
    /// Host mic stays active; guests stop/start capture.
    /// </summary>
    public void RefreshMicrophoneMuteState()
    {
        if (!isLocalPlayer)
            return;

        bool isHostPlayer = NetworkServer.active;
        bool shouldMute = ClassController.Instance != null
            && ClassController.Instance.ShouldMuteGuestMicrophone(isHostPlayer);

        if (shouldMute)
            StopMicrophone();
        else if (micClip == null || string.IsNullOrEmpty(micDevice) || !Microphone.IsRecording(micDevice))
            StartMicrophone();
    }

    static void RequestMicrophonePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
#endif
    }

    void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("VoiceChat: no microphone found.");
            return;
        }
        else
        {
            Debug.Log("VoiceChat: microphone found.");
        }

        micDevice = Microphone.devices[0];
        micClip = Microphone.Start(micDevice, true, 1, sampleRate);
        lastMicPos = 0;
    }

    void StopMicrophone()
    {
        if (string.IsNullOrEmpty(micDevice))
            return;

        if (Microphone.IsRecording(micDevice))
            Microphone.End(micDevice);

        micDevice = null;
        micClip = null;
    }

    void CaptureAndSend()
    {
        bool isHostPlayer = NetworkServer.active;
        if (ClassController.Instance != null
            && ClassController.Instance.ShouldMuteGuestMicrophone(isHostPlayer))
            return;

        if (micClip == null || string.IsNullOrEmpty(micDevice) || !Microphone.IsRecording(micDevice))
            return;

        if (Time.unscaledTime < nextTransmitTime)
            return;

        nextTransmitTime = Time.unscaledTime + transmitChunkMs / 1000f;

        int micPos = Microphone.GetPosition(micDevice);
        if (micPos < 0 || micPos == lastMicPos)
            return;

        int sampleCount = micPos - lastMicPos;
        if (sampleCount < 0)
            sampleCount += micClip.samples;

        if (sampleCount <= 0)
            return;

        float[] samples = new float[sampleCount];
        micClip.GetData(samples, lastMicPos);
        lastMicPos = micPos;

        CmdSendVoice(FloatToPCM16(samples));
    }

    [Command(channel = Channels.Unreliable)]
    void CmdSendVoice(byte[] pcmData)
    {
        if (pcmData == null || pcmData.Length == 0)
            return;

        RpcReceiveVoice(netId, pcmData);
    }

    [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
    void RpcReceiveVoice(uint speakerNetId, byte[] pcmData)
    {
        if (!NetworkClient.spawned.TryGetValue(speakerNetId, out NetworkIdentity identity))
            return;

        VoiceChatController voice = identity.GetComponent<VoiceChatController>();
        if (voice != null)
            voice.EnqueuePlayback(pcmData);
    }

    void EnqueuePlayback(byte[] pcmData)
    {
        float[] samples = PCM16ToFloat(pcmData);
        lock (playbackLock)
        {
            for (int i = 0; i < samples.Length; i++)
                playbackQueue.Enqueue(samples[i]);

            EnsureStreamPlaying();
        }
    }

    void EnsureStreamPlaying()
    {
        if (streamClip == null)
        {
            streamClip = AudioClip.Create(
                "VoiceStream",
                sampleRate * StreamClipSeconds,
                1,
                sampleRate,
                true,
                OnStreamRead);
            audioSource.clip = streamClip;
        }

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    void OnStreamRead(float[] data)
    {
        lock (playbackLock)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = playbackQueue.Count > 0 ? playbackQueue.Dequeue() : 0f;
        }
    }

    static byte[] FloatToPCM16(float[] samples)
    {
        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
            bytes[i * 2] = (byte)(value & 0xff);
            bytes[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        return bytes;
    }

    static float[] PCM16ToFloat(byte[] bytes)
    {
        int sampleCount = bytes.Length / 2;
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short value = (short)(bytes[i * 2] | (bytes[i * 2 + 1] << 8));
            samples[i] = value / (float)short.MaxValue;
        }

        return samples;
    }
}
