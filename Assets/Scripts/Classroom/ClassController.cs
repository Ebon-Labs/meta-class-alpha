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

    bool appliedMuteGuests;
    bool appliedHideFloorAreas = true;

    public bool MuteGuests => muteGuests;
    public bool HideFloorAreas => hideFloorAreas;

    public static ClassController ResolveExisting(ClassController located)
    {
        if (Instance != null)
            return Instance;

        return located;
    }

    public static bool SettingChanges(bool force, bool applied, bool current)
    {
        return force || applied != current;
    }

    public static Vector3 StandAboveBounds(Bounds bounds, float eyeHeight)
    {
        return new Vector3(bounds.center.x, bounds.max.y + eyeHeight, bounds.center.z);
    }

    public static Vector3 StandAboveAnchor(Vector3 anchor, float eyeHeight)
    {
        return anchor + Vector3.up * eyeHeight;
    }

    public static Transform AreaForRole(bool isHost, Transform hostArea, Transform guestArea)
    {
        return isHost ? hostArea : guestArea;
    }

    public void ToggleMuteGuests()
    {
        SetMuteGuests(!muteGuests);
    }

    public void SetMuteGuests(bool mute)
    {
        if (!SettingChanges(force: false, muteGuests, mute))
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
        if (!SettingChanges(force: false, hideFloorAreas, hide))
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

    public void PlacePlayer(Transform player, bool isHost)
    {
        if (player == null)
            return;

        ResolveAreas();
        Transform area = AreaForRole(isHost, hostArea, guestArea);
        if (area == null)
            return;

        Vector3 stand = GetStandPosition(area);
        Quaternion rotation = area.rotation;
        player.SetPositionAndRotation(stand, rotation);

        Rigidbody body = player.GetComponent<Rigidbody>();
        if (body == null)
            return;

        body.position = stand;
        body.rotation = rotation;
        if (body.isKinematic)
            return;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }

    public bool ShouldMuteGuestMicrophone(bool isHostPlayer)
    {
        return GuestMutePolicy.ShouldMute(muteGuests, isHostPlayer);
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
            return StandAboveBounds(renderer.bounds, eyeHeight);

        return StandAboveAnchor(area.position, eyeHeight);
    }

    void ApplyControls(bool force)
    {
        bool muteChanged = SettingChanges(force, appliedMuteGuests, muteGuests);
        bool floorChanged = SettingChanges(force, appliedHideFloorAreas, hideFloorAreas);
        if (!muteChanged && !floorChanged)
            return;

        if (muteChanged)
        {
            ApplyGuestMute();
            appliedMuteGuests = muteGuests;
        }

        if (floorChanged)
        {
            ApplyFloorVisibility();
            appliedHideFloorAreas = hideFloorAreas;
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
