using UnityEngine;

[DisallowMultipleComponent]
public class DistanceControlButton : MonoBehaviour
{
    [SerializeField] Material nearMaterial;
    [SerializeField] Material commonMaterial;
    [SerializeField] ObjectBasicController appearance;

    public static bool ShowsNearIcon(bool isNearDistance)
    {
        return isNearDistance;
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

    public void OnPointerClick()
    {
        SolarSystemMotionController motion = ResolveMotion();
        if (motion == null)
            return;

        if (RoomSync.RequestToggleDistance())
            return;

        motion.ToggleDistanceScale();
        ApplyIconToAppearance(createInstances: true);
    }

    public void RefreshAppearance()
    {
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        SolarSystemMotionController motion = ResolveMotion();

        // Icon is the opposite of the current scale: near shows the common icon.
        bool near = motion != null && motion.IsNearDistance;
        Material icon = ControlButtonAppearance.Select(ShowsNearIcon(near), nearMaterial, commonMaterial);
        ControlButtonAppearance.Apply(appearance, GetComponent<Renderer>(), icon, createInstances);
    }

    static SolarSystemMotionController ResolveMotion()
    {
        return FindFirstObjectByType<SolarSystemMotionController>();
    }
}
