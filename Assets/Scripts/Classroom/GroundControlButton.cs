using UnityEngine;

[DisallowMultipleComponent]
public class GroundControlButton : MonoBehaviour
{
    [SerializeField] Material showGroundMaterial;
    [SerializeField] Material hideGroundMaterial;
    [SerializeField] ObjectBasicController appearance;

    public static bool ShowsRevealIcon(bool floorsHidden)
    {
        return floorsHidden;
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

        classController.ToggleHideFloorAreas();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        ClassController classController = ResolveClassController();
        bool floorsHidden = classController != null && classController.HideFloorAreas;
        Material icon = ControlButtonAppearance.Select(ShowsRevealIcon(floorsHidden), hideGroundMaterial, showGroundMaterial);
        ControlButtonAppearance.Apply(appearance, GetComponent<Renderer>(), icon, createInstances);
    }

    static ClassController ResolveClassController()
    {
        return ClassController.ResolveExisting(FindFirstObjectByType<ClassController>());
    }
}
