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

        motion.ToggleDistanceScale();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        SolarSystemMotionController motion = ResolveMotion();

        // Icon matches the current orbit scale: near (3) or common (300).
        bool near = motion != null && motion.IsNearDistance;
        Material icon = ControlButtonAppearance.Select(ShowsNearIcon(near), commonMaterial, nearMaterial);
        ControlButtonAppearance.Apply(appearance, GetComponent<Renderer>(), icon, createInstances);
    }

    static SolarSystemMotionController ResolveMotion()
    {
        return FindFirstObjectByType<SolarSystemMotionController>();
    }
}
