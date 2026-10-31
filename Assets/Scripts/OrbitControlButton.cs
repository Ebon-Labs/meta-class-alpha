using UnityEngine;

[DisallowMultipleComponent]
public class OrbitControlButton : MonoBehaviour
{
    static readonly Color DisabledColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [SerializeField] Material commonMaterial;
    [SerializeField] Material increasedMaterial;
    [SerializeField] ObjectBasicController appearance;

    bool _appliedSpeedLocked;
    bool _hasAppliedSpeedLock;

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
        ApplySpeedLock(force: !_hasAppliedSpeedLock);
        _hasAppliedSpeedLock = true;
    }

    public void OnPointerClick()
    {
        SolarSystemMotionController motion = ResolveMotion();
        if (motion == null || motion.IsNearDistance)
            return;

        motion.ToggleTimeScale();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        SolarSystemMotionController motion = ResolveMotion();

        // Icon matches the current pace: common default, or default times 10000.
        bool increased = motion != null && motion.IsIncreasedTimeScale;
        Material icon = increased ? increasedMaterial : commonMaterial;
        if (icon == null)
            return;

        if (appearance != null)
        {
            appearance.InactiveMaterial = icon;
            if (createInstances)
                appearance.RefreshMaterials();
            return;
        }

        if (createInstances)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = icon;
        }
    }

    void ApplySpeedLock(bool force)
    {
        SolarSystemMotionController motion = ResolveMotion();
        bool locked = motion != null && motion.IsNearDistance;
        if (!force && locked == _appliedSpeedLocked)
            return;

        _appliedSpeedLocked = locked;

        Collider buttonCollider = GetComponent<Collider>();
        if (buttonCollider != null)
            buttonCollider.enabled = !locked;

        ApplyIconToAppearance(createInstances: true);

        if (appearance != null)
            appearance.SetColor(locked ? DisabledColor : Color.white);
    }

    static SolarSystemMotionController ResolveMotion()
    {
        return FindFirstObjectByType<SolarSystemMotionController>();
    }
}
