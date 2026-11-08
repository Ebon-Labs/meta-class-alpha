using UnityEngine;

[DisallowMultipleComponent]
public class OrbitControlButton : MonoBehaviour
{
    public static readonly Color DisabledTint = new Color(0.55f, 0.55f, 0.55f, 1f);

    [SerializeField] Material commonMaterial;
    [SerializeField] Material increasedMaterial;
    [SerializeField] ObjectBasicController appearance;

    bool appliedSpeedLocked;
    bool hasAppliedSpeedLock;

    public static bool ShowsIncreasedIcon(bool isIncreasedTimeScale)
    {
        return isIncreasedTimeScale;
    }

    public static bool IsSpeedLocked(bool hasMotion, bool isNearDistance)
    {
        return hasMotion && isNearDistance;
    }

    public static Color TintForLock(bool locked)
    {
        return locked ? DisabledTint : Color.white;
    }

    void Awake()
    {
        if (appearance == null)
            appearance = GetComponent<ObjectBasicController>();

        // Set before ObjectBasicController.Start builds material instances.
        ApplyIconToAppearance(createInstances: false);
    }

    void Start()
    {
        ApplyIconToAppearance(createInstances: true);
    }

    void LateUpdate()
    {
        ApplySpeedLock(force: !hasAppliedSpeedLock);
        hasAppliedSpeedLock = true;
    }

    public void OnPointerClick()
    {
        SolarSystemMotionController motion = ResolveMotion();
        if (motion == null || motion.IsNearDistance)
            return;

        if (RoomSync.RequestToggleTime())
            return;

        motion.ToggleTimeScale();
        ApplyIconToAppearance(createInstances: true);
    }

    public void RefreshAppearance()
    {
        ApplySpeedLock(force: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        SolarSystemMotionController motion = ResolveMotion();

        // Icon is the opposite of the current pace: fast shows the common icon.
        bool increased = motion != null && motion.IsIncreasedTimeScale;
        Material icon = ControlButtonAppearance.Select(ShowsIncreasedIcon(increased), increasedMaterial, commonMaterial);
        ControlButtonAppearance.Apply(appearance, GetComponent<Renderer>(), icon, createInstances);
    }

    void ApplySpeedLock(bool force)
    {
        SolarSystemMotionController motion = ResolveMotion();
        bool locked = IsSpeedLocked(motion != null, motion != null && motion.IsNearDistance);
        if (!force && locked == appliedSpeedLocked)
            return;

        appliedSpeedLocked = locked;

        Collider buttonCollider = GetComponent<Collider>();
        if (buttonCollider != null)
            buttonCollider.enabled = !locked;

        ApplyIconToAppearance(createInstances: true);

        if (appearance != null)
            appearance.SetColor(TintForLock(locked));
    }

    static SolarSystemMotionController ResolveMotion()
    {
        return FindFirstObjectByType<SolarSystemMotionController>();
    }
}
