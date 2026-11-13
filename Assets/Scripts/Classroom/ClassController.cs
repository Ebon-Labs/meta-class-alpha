using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-40)]
public class ClassController : MonoBehaviour
{
    public const float ClockwiseStepDegrees = 45f;
    public const float TurnDuration = 3f;
    public const string TutorialScene = "Tutorial";
    public const string GroundName = "Ground";

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

    Vector3 pivot;
    Vector3 restPosition;
    Vector3 restPivot;
    Quaternion restRotation = Quaternion.identity;
    Vector3 positionFrom;
    Quaternion rotationFrom = Quaternion.identity;
    float turnElapsed;
    int appliedTurns;
    bool turning;

    public bool IsTurning => turning;

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

    public static Transform AreaForRoleOrGround(bool isHost, Transform hostArea, Transform guestArea, string sceneName, Transform ground)
    {
        Transform area = AreaForRole(isHost, hostArea, guestArea);
        if (area != null || sceneName != TutorialScene)
            return area;

        return ground;
    }

    public static Quaternion TurnAroundOwnY(Quaternion localRotation, float degrees)
    {
        return localRotation * Quaternion.AngleAxis(-Mathf.Abs(degrees), Vector3.up);
    }

    public static Quaternion ClockwiseYaw(float degrees)
    {
        return Quaternion.AngleAxis(-Mathf.Abs(degrees), Vector3.up);
    }

    public static Quaternion TurnStep(Quaternion from, Quaternion to, float elapsed, float duration)
    {
        return Quaternion.Slerp(from, to, EdgeApproach.Ease(EdgeApproach.Progress(elapsed, duration)));
    }

    public static Vector3 Orbit(Vector3 position, Vector3 pivot, Quaternion yaw)
    {
        return pivot + yaw * (position - pivot);
    }

    public void TurnClockwise()
    {
        ResolveAreas();
        pivot = hostArea != null ? hostArea.position : transform.position;
        positionFrom = transform.position;
        rotationFrom = transform.rotation;
        turnElapsed = 0f;
        turning = true;
    }

    public static bool AnimateSingleStep(int applied, int target)
    {
        return target == applied + 1;
    }

    public void MatchTurnCount(int count)
    {
        if (count < 0)
            count = 0;
        if (count == appliedTurns)
            return;

        if (AnimateSingleStep(appliedTurns, count))
        {
            TurnClockwise();
            appliedTurns = count;
            return;
        }

        ResolveAreas();
        if (turning)
            turning = false;

        Quaternion yaw = ClockwiseYaw(ClockwiseStepDegrees * count);
        transform.SetPositionAndRotation(Orbit(restPosition, restPivot, yaw), yaw * restRotation);
        appliedTurns = count;
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
        if (GetComponent<FloorGuard>() == null)
            gameObject.AddComponent<FloorGuard>();
        if (GetComponent<FloorFollow>() == null)
            gameObject.AddComponent<FloorFollow>();
        ResolveAreas();
        restPosition = transform.position;
        restRotation = transform.rotation;
        restPivot = hostArea != null ? hostArea.position : restPosition;
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

    void Update()
    {
        if (!turning)
            return;

        turnElapsed += Time.deltaTime;
        Quaternion yaw = TurnStep(Quaternion.identity, ClockwiseYaw(ClockwiseStepDegrees), turnElapsed, TurnDuration);
        transform.SetPositionAndRotation(Orbit(positionFrom, pivot, yaw), yaw * rotationFrom);
        if (turnElapsed < TurnDuration)
            return;

        turning = false;
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
        Transform area = AreaForRoleOrGround(isHost, hostArea, guestArea, gameObject.scene.name, FindGround());
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

    public int PlaceGuests(IList<Transform> players, Func<Transform, bool> isHost)
    {
        if (players == null)
            return 0;

        int moved = 0;
        for (int i = 0; i < players.Count; i++)
        {
            Transform player = players[i];
            if (player == null || (isHost != null && isHost(player)))
                continue;

            PlacePlayer(player, false);
            moved++;
        }

        return moved;
    }

    public bool ShouldMuteGuestMicrophone(bool isHostPlayer)
    {
        return GuestMutePolicy.ShouldMute(muteGuests, isHostPlayer);
    }

    static Transform FindGround()
    {
        GameObject ground = GameObject.Find(GroundName);
        return ground != null ? ground.transform : null;
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
