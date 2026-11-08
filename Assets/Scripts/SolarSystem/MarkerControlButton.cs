using UnityEngine;

[DisallowMultipleComponent]
public class MarkerControlButton : MonoBehaviour
{
    [SerializeField] Material shownMaterial;
    [SerializeField] Material hiddenMaterial;
    [SerializeField] ObjectBasicController appearance;

    public static bool ShowsVisibleIcon(bool motionMissing, bool orbsEnabled)
    {
        return motionMissing || orbsEnabled;
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

        if (RoomSync.RequestToggleOrbs())
            return;

        motion.ToggleShowInteractionOrbs();
        ApplyIconToAppearance(createInstances: true);
    }

    public void RefreshAppearance()
    {
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        SolarSystemMotionController motion = ResolveMotion();

        // Icon is the opposite of the orbs: visible markers show the hidden icon.
        bool shown = ShowsVisibleIcon(motion == null, motion != null && motion.ShowInteractionOrbs);
        Material icon = ControlButtonAppearance.Select(shown, shownMaterial, hiddenMaterial);
        ControlButtonAppearance.Apply(appearance, GetComponent<Renderer>(), icon, createInstances);
    }

    static SolarSystemMotionController ResolveMotion()
    {
        return FindFirstObjectByType<SolarSystemMotionController>();
    }
}
