using UnityEngine;

[DisallowMultipleComponent]
public class MarkerControlButton : MonoBehaviour
{
    [SerializeField] Material shownMaterial;
    [SerializeField] Material hiddenMaterial;
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

        motion.ToggleShowInteractionOrbs();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        SolarSystemMotionController motion = ResolveMotion();

        // Icon matches whether interaction orbs are visible.
        bool shown = motion == null || motion.ShowInteractionOrbs;
        Material icon = shown ? shownMaterial : hiddenMaterial;
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
