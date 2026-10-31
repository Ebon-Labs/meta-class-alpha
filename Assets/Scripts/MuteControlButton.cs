using UnityEngine;

[DisallowMultipleComponent]
public class MuteControlButton : MonoBehaviour
{
    [SerializeField] Material voiceMaterial;
    [SerializeField] Material muteMaterial;
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
        ClassController classController = ResolveClassController();
        if (classController == null)
            return;

        classController.ToggleMuteGuests();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        ClassController classController = ResolveClassController();

        // Icon matches the current guest-mute state.
        bool guestsMuted = classController != null && classController.MuteGuests;
        Material icon = guestsMuted ? muteMaterial : voiceMaterial;
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

    static ClassController ResolveClassController()
    {
        if (ClassController.Instance != null)
            return ClassController.Instance;

        return FindFirstObjectByType<ClassController>();
    }
}
