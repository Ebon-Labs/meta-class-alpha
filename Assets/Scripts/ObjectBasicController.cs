using UnityEngine;

public class ObjectBasicController : MonoBehaviour
{
    public Material InactiveMaterial;
    public Material GazedAtMaterial;

    private Renderer _myRenderer;
    private Material _inactiveInstance;
    private Material _gazedInstance;

    void Start()
    {
        _myRenderer = GetComponent<Renderer>();
        if (_inactiveInstance == null || _gazedInstance == null)
            CreateMaterialInstances();
        SetMaterial(false);
    }

    void OnDestroy()
    {
        if (_inactiveInstance != null)
            Destroy(_inactiveInstance);
        if (_gazedInstance != null)
            Destroy(_gazedInstance);
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
        SetMaterial(false);
    }

    /// Sets this instance's material according to gazedAt status.
    /// Value `true` if this object is being gazed at, `false` otherwise.
    private void SetMaterial(bool gazedAt)
    {
        if (_myRenderer == null || _inactiveInstance == null || _gazedInstance == null)
            return;

        // Use the same material instances every time so Shader Graph Time/Emission
        // pulse keeps running across repeated gaze enter/exit.
        _myRenderer.sharedMaterial = gazedAt ? _gazedInstance : _inactiveInstance;
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
}
