using UnityEngine;

[DisallowMultipleComponent]
public class DistanceControlButton : MonoBehaviour
{
    [SerializeField] Material nearMaterial;
    [SerializeField] Material commonMaterial;
    [SerializeField] ObjectBasicController appearance;

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
        Material icon = near ? nearMaterial : commonMaterial;
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

    static SolarSystemMotionController ResolveMotion()
    {
        return FindFirstObjectByType<SolarSystemMotionController>();
    }
}
