using UnityEngine;

[DisallowMultipleComponent]
public class GroundControlButton : MonoBehaviour
{
    [SerializeField] Material showGroundMaterial;
    [SerializeField] Material hideGroundMaterial;
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

        classController.ToggleHideFloorAreas();
        ApplyIconToAppearance(createInstances: true);
    }

    void ApplyIconToAppearance(bool createInstances)
    {
        ClassController classController = ResolveClassController();

        // When floor is visible, button offers "hide"; when hidden, offers "show".
        bool floorsHidden = classController != null && classController.HideFloorAreas;
        Material icon = floorsHidden ? showGroundMaterial : hideGroundMaterial;
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
