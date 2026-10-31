using UnityEngine;

[DisallowMultipleComponent]
public class ClassController : MonoBehaviour
{
    public static ClassController Instance { get; private set; }

    [Header("Areas")]
    [SerializeField] Transform hostArea;
    [SerializeField] Transform guestArea;
    [Tooltip("Height above the area surface used when placing players.")]
    [SerializeField] float eyeHeight = 1.6f;

    [Header("Controls")]
    [Tooltip("When true, guest microphones stop transmitting.")]
    [SerializeField] bool muteGuests;

    [Tooltip("When true, HostArea and GuestArea mesh renderers are hidden.")]
    [SerializeField] bool hideFloorAreas;

    bool _appliedMuteGuests;
    bool _appliedHideFloorAreas = true; // force first apply

    public bool MuteGuests => muteGuests;
    public bool HideFloorAreas => hideFloorAreas;

    public void ToggleMuteGuests()
    {
        SetMuteGuests(!muteGuests);
    }

    public void SetMuteGuests(bool mute)
    {
        if (muteGuests == mute)
            return;

        muteGuests = mute;
        ApplyControls(force: true);
    }

    public void ToggleHideFloorAreas()
    {
        SetHideFloorAreas(!hideFloorAreas);
    }

    public void SetHideFloorAreas(bool hide)
    {
        if (hideFloorAreas == hide)
            return;

        hideFloorAreas = hide;
        ApplyControls(force: true);
    }

    void Awake()
    {
        Instance = this;
        ResolveAreas();
        ApplyControls(force: true);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnValidate()
    {
        ResolveAreas();
        if (Application.isPlaying)
            ApplyControls(force: true);
        else
            ApplyFloorVisibility();
    }

    void LateUpdate()
    {
        ApplyControls(force: false);
    }

    /// <summary>
    /// Places a player above HostArea (host) or GuestArea (clients).
    /// </summary>
    public void PlacePlayer(Transform player, bool isHost)
    {
        if (player == null)
            return;

        ResolveAreas();
        Transform area = isHost ? hostArea : guestArea;
        if (area == null)
            return;

        player.SetPositionAndRotation(GetStandPosition(area), area.rotation);
    }

    public bool ShouldMuteGuestMicrophone(bool isHostPlayer)
    {
        return muteGuests && !isHostPlayer;
    }

    void ResolveAreas()
    {
        if (hostArea == null)
        {
            Transform found = transform.Find("HostArea");
            if (found != null)
                hostArea = found;
        }

        if (guestArea == null)
        {
            Transform found = transform.Find("GuestArea");
            if (found != null)
                guestArea = found;
        }
    }

    Vector3 GetStandPosition(Transform area)
    {
        Renderer renderer = area.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            return new Vector3(bounds.center.x, bounds.max.y + eyeHeight, bounds.center.z);
        }

        return area.position + Vector3.up * eyeHeight;
    }

    void ApplyControls(bool force)
    {
        bool muteChanged = force || _appliedMuteGuests != muteGuests;
        bool floorChanged = force || _appliedHideFloorAreas != hideFloorAreas;
        if (!muteChanged && !floorChanged)
            return;

        if (muteChanged)
        {
            ApplyGuestMute();
            _appliedMuteGuests = muteGuests;
        }

        if (floorChanged)
        {
            ApplyFloorVisibility();
            _appliedHideFloorAreas = hideFloorAreas;
        }
    }

    void ApplyGuestMute()
    {
        VoiceChatController[] voices = FindObjectsByType<VoiceChatController>(FindObjectsSortMode.None);
        for (int i = 0; i < voices.Length; i++)
        {
            if (voices[i] != null)
                voices[i].RefreshMicrophoneMuteState();
        }
    }

    void ApplyFloorVisibility()
    {
        SetAreaVisible(hostArea, !hideFloorAreas);
        SetAreaVisible(guestArea, !hideFloorAreas);
    }

    static void SetAreaVisible(Transform area, bool visible)
    {
        if (area == null)
            return;

        Renderer[] renderers = area.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = visible;
        }
    }
}
