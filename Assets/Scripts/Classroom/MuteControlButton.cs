using UnityEngine;

[DisallowMultipleComponent]
public class MuteControlButton : MonoBehaviour
{
    [SerializeField] Material voiceMaterial;
    [SerializeField] Material muteMaterial;
    [SerializeField] ObjectBasicController appearance;

    public static bool ShowsMutedIcon(bool guestsMuted)
    {
        return guestsMuted;
    }

    void Awake()
    {
        if (appearance == null)
            appearance = GetComponent<ObjectBasicController>();

        ApplyIconToAppearance(createInstances: false);
    }

    void Start()
    {
        ApplyIconToAppearance(createInstances: true);
    }

    public void OnPointerClick()
    {
        ClassController classController = ResolveClassController();
        if (classController == null)
            return;

        classController.ToggleMuteGuests();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        ClassController classController = ResolveClassController();
        // Icon is the opposite of the mute: silenced guests show the voice icon.
        bool guestsMuted = classController != null && classController.MuteGuests;
        Material icon = ControlButtonAppearance.Select(ShowsMutedIcon(guestsMuted), muteMaterial, voiceMaterial);
        ControlButtonAppearance.Apply(appearance, GetComponent<Renderer>(), icon, createInstances);
    }

    static ClassController ResolveClassController()
    {
        return ClassController.ResolveExisting(FindFirstObjectByType<ClassController>());
    }
}
