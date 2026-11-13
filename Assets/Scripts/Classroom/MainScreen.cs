using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class MainScreen : MonoBehaviour, IScreenPlayback
{
    public enum TintPropertyKind
    {
        Tint,
        TintColor,
        Color
    }

    public const string TutorialScene = "Tutorial";
    public const string PortalOneName = "PortalOne";

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int TintId = Shader.PropertyToID("_Tint");
    static readonly int TintColorId = Shader.PropertyToID("_TintColor");

    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] Renderer screenRenderer;
    [SerializeField] GameObject showWhenFinished;
    [SerializeField] float fadeDuration = 3f;

    Material material;
    int tintProperty;
    Color tint;
    Coroutine tintFade;
    bool sawPlayback;
    bool revealed;

    void Awake()
    {
        if (screenRenderer == null)
            screenRenderer = FindScreenRenderer();
        if (videoPlayer == null)
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        material = screenRenderer.material;
        tintProperty = ResolveTintProperty(material);
        EnableFade(material);

        tint = material.GetColor(tintProperty);
        tint.a = 0f;
        material.SetColor(tintProperty, tint);
    }

    void OnEnable()
    {
        BindPlayer(true);
    }

    void OnDisable()
    {
        BindPlayer(false);
    }

    void Update()
    {
        if (revealed || videoPlayer == null)
            return;

        if (videoPlayer.isPlaying)
        {
            sawPlayback = true;
            return;
        }

        if (PlaybackEnded(sawPlayback, false, videoPlayer.isPaused, videoPlayer.time, videoPlayer.length))
            FinishVideo();
    }

    void BindPlayer(bool listen)
    {
        if (videoPlayer == null)
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        if (videoPlayer == null)
            return;

        videoPlayer.started -= OnVideoStarted;
        videoPlayer.loopPointReached -= OnVideoFinished;
        if (!listen)
            return;

        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnDestroy()
    {
        if (material != null)
            Destroy(material);
    }

    public void Play()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.Play();
    }

    public void Pause()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.Pause();
    }

    public void Stop()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.Stop();
    }

    void OnVideoStarted(VideoPlayer source)
    {
        if (tintFade != null)
            StopCoroutine(tintFade);

        tintFade = StartCoroutine(FadeTint());
    }

    void OnVideoFinished(VideoPlayer source)
    {
        sawPlayback = true;
        FinishVideo();
    }

    void FinishVideo()
    {
        if (revealed)
            return;

        revealed = true;
        Reveal(ChoosePortal(showWhenFinished, gameObject.scene.name, FindNamed(PortalOneName)));
    }

    public static void Reveal(GameObject target)
    {
        if (target != null)
            target.SetActive(true);
    }

    public static GameObject ChoosePortal(GameObject assigned, string sceneName, GameObject namedPortal)
    {
        if (assigned != null)
            return assigned;
        if (sceneName != TutorialScene)
            return null;
        return namedPortal;
    }

    public static bool PlaybackEnded(bool sawPlayback, bool isPlaying, bool isPaused, double time, double length)
    {
        if (!sawPlayback || isPlaying)
            return false;
        if (length > 0.01)
            return time >= length - 0.25;
        return !isPaused && time > 0.5;
    }

    static GameObject FindNamed(string objectName)
    {
        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == objectName)
                return all[i].gameObject;
        }

        return null;
    }

    System.Collections.IEnumerator FadeTint()
    {
        tint.a = 0f;
        material.SetColor(tintProperty, tint);

        float duration = ClampFadeDuration(fadeDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tint.a = FadeAlpha(elapsed, fadeDuration);
            material.SetColor(tintProperty, tint);
            yield return null;
        }

        tint.a = 1f;
        material.SetColor(tintProperty, tint);
        tintFade = null;
    }

    Renderer FindScreenRenderer()
    {
        Transform screen = transform.Find("MainScreen");
        if (screen != null)
        {
            Renderer renderer = screen.GetComponent<Renderer>();
            if (renderer != null)
                return renderer;
        }

        return GetComponentInChildren<Renderer>(true);
    }

    public static float ClampFadeDuration(float fadeDuration)
    {
        return Mathf.Max(0.01f, fadeDuration);
    }

    public static float FadeAlpha(float elapsed, float fadeDuration)
    {
        return Mathf.Clamp01(elapsed / ClampFadeDuration(fadeDuration));
    }

    public static TintPropertyKind ResolveTintKind(bool hasTint, bool hasTintColor)
    {
        if (hasTint)
            return TintPropertyKind.Tint;
        if (hasTintColor)
            return TintPropertyKind.TintColor;
        return TintPropertyKind.Color;
    }

    static int ResolveTintProperty(Material source)
    {
        switch (ResolveTintKind(source.HasProperty(TintId), source.HasProperty(TintColorId)))
        {
            case TintPropertyKind.Tint:
                return TintId;
            case TintPropertyKind.TintColor:
                return TintColorId;
            default:
                return ColorId;
        }
    }

    static void EnableFade(Material source)
    {
        if (!source.HasProperty("_Mode"))
            return;

        source.SetFloat("_Mode", 2f);
        source.SetOverrideTag("RenderType", "Transparent");
        source.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        source.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        source.SetInt("_ZWrite", 0);
        source.DisableKeyword("_ALPHATEST_ON");
        source.EnableKeyword("_ALPHABLEND_ON");
        source.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        source.renderQueue = (int)RenderQueue.Transparent;
    }
}
