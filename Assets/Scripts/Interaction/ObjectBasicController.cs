using UnityEngine;

public class ObjectBasicController : MonoBehaviour
{
    public Material InactiveMaterial;
    public Material GazedAtMaterial;

    private Renderer _myRenderer;
    private Material _inactiveInstance;
    private Material _gazedInstance;
    private bool _isGazedAt;

    void Start()
    {
        _myRenderer = GetComponent<Renderer>();
        SyncInactiveMaterialFromRenderer();

        if (_inactiveInstance == null || _gazedInstance == null)
            CreateMaterialInstances();
        SetMaterial(_isGazedAt);
    }

    void OnDestroy()
    {
        if (_inactiveInstance != null)
            Destroy(_inactiveInstance);
        if (_gazedInstance != null)
            Destroy(_gazedInstance);
    }

    /// <summary>
    /// Planet/control buttons often override only the MeshRenderer material in the scene
    /// while leaving InactiveMaterial as the Button prefab default. Prefer the renderer
    /// material in that case so Start() does not wipe the visible icon.
    /// </summary>
    void SyncInactiveMaterialFromRenderer()
    {
        if (_myRenderer == null)
            return;

        Material current = _myRenderer.sharedMaterial;
        if (!ShouldAdoptSharedMaterial(current != null, InactiveMaterial == null || InactiveMaterial != current))
            return;

        InactiveMaterial = current;
    }

    void CreateMaterialInstances()
    {
        if (InactiveMaterial != null)
            _inactiveInstance = new Material(InactiveMaterial);
        if (GazedAtMaterial != null)
            _gazedInstance = new Material(GazedAtMaterial);
    }

    /// <summary>
    /// Recreates material instances after InactiveMaterial / GazedAtMaterial change.
    /// Preserves current gaze highlight when possible.
    /// </summary>
    public void RefreshMaterials()
    {
        if (_myRenderer == null)
            _myRenderer = GetComponent<Renderer>();

        if (_inactiveInstance != null)
        {
            Destroy(_inactiveInstance);
            _inactiveInstance = null;
        }

        if (_gazedInstance != null)
        {
            Destroy(_gazedInstance);
            _gazedInstance = null;
        }

        CreateMaterialInstances();
        SetMaterial(_isGazedAt);
    }

    /// <summary>
    /// Tints the live inactive and gazed copies. Source materials stay unchanged.
    /// </summary>
    public void SetColor(Color color)
    {
        if (_inactiveInstance != null)
            _inactiveInstance.color = color;
        if (_gazedInstance != null)
            _gazedInstance.color = color;
    }

    /// Sets this instance's material according to gazedAt status.
    /// Value `true` if this object is being gazed at, `false` otherwise.
    private void SetMaterial(bool gazedAt)
    {
        _isGazedAt = gazedAt;

        if (_myRenderer == null)
            return;

        if (UseGazedMaterial(gazedAt))
        {
            if (_gazedInstance != null)
                _myRenderer.sharedMaterial = _gazedInstance;
            return;
        }

        if (_inactiveInstance != null)
            _myRenderer.sharedMaterial = _inactiveInstance;
    }

    /// This method is called by the Main Camera when it starts gazing at this GameObject.
    public void OnPointerEnter()
    {
        SetMaterial(true);
    }

    /// This method is called by the Main Camera when it stops gazing at this GameObject.
    public void OnPointerExit()
    {
        SetMaterial(false);
    }

    public static bool ShouldAdoptSharedMaterial(bool hasSharedMaterial, bool inactiveMissingOrDifferent)
    {
        return hasSharedMaterial && inactiveMissingOrDifferent;
    }

    public static bool UseGazedMaterial(bool gazedAt)
    {
        return gazedAt;
    }
}
