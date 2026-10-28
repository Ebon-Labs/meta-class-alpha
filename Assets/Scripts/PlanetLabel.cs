using Mirror;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1100)]
public class PlanetLabel : MonoBehaviour
{
    [SerializeField] TextMeshPro label;
    [SerializeField] Material billboardMaterial;
    [SerializeField] Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] Quaternion localRotation = Quaternion.identity;
    [SerializeField] string overrideText = "Planet";

    Camera _camera;
    Transform _cameraTransform;
    bool _capturedPose;

    void Awake()
    {
        if (label == null)
            label = GetComponent<TextMeshPro>();

        CaptureLocalPoseFromScene();
        ApplyBillboardMaterial();
        ApplyPlanetName();
        ApplyLocalPose();
    }

    void LateUpdate()
    {
        if (!TryResolveCamera())
            return;

        // Keep authored transform rotation — facing is done in the billboard shader.
        ApplyLocalPose();

        float fontSize = label != null ? Mathf.Max(1f, label.fontSize) : 200f;
        float distance = Mathf.Max(0.01f, Vector3.Distance(_cameraTransform.position, transform.position));
        float parentScale = transform.parent != null
            ? Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.x))
            : 1f;

        // Apparent height in pixels ≈ TextMeshPro.fontSize (authored on the TMP component).
        float halfFovRad = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float screenHeight = Mathf.Max(1f, Screen.height);
        float worldHeight = (fontSize / screenHeight)
            * 2f
            * distance
            * Mathf.Tan(halfFovRad);

        float localUniform = worldHeight / (fontSize * parentScale);
        transform.localScale = Vector3.one * Mathf.Max(0.000001f, localUniform);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    void CaptureLocalPoseFromScene()
    {
        if (_capturedPose)
            return;

        localOffset = transform.localPosition;
        localRotation = transform.localRotation;
        _capturedPose = true;
    }

    void ApplyLocalPose()
    {
        transform.localPosition = localOffset;
        transform.localRotation = localRotation;
    }

    void ApplyBillboardMaterial()
    {
        if (label == null)
            return;

        if (billboardMaterial != null)
            label.fontSharedMaterial = billboardMaterial;
    }

    void ApplyPlanetName()
    {
        if (label == null)
            return;

        label.enableAutoSizing = false;
        label.alignment = TextAlignmentOptions.Center;

        if (!string.IsNullOrWhiteSpace(overrideText))
        {
            label.text = overrideText;
            return;
        }

        PlanetOrbitMotion[] motions = GetComponentsInParent<PlanetOrbitMotion>(true);
        for (int i = 0; i < motions.Length; i++)
        {
            if (motions[i] != null && motions[i].IsOrbitEnabled)
            {
                label.text = motions[i].PlanetProfile.ToString();
                return;
            }
        }

        if (motions.Length > 0 && motions[0] != null)
            label.text = motions[0].PlanetProfile.ToString();
    }

    bool TryResolveCamera()
    {
        if (_camera != null && _cameraTransform != null && _cameraTransform.gameObject.activeInHierarchy)
            return true;

        _camera = null;
        _cameraTransform = null;

        if (NetworkClient.active && NetworkClient.localPlayer != null)
        {
            Camera cam = NetworkClient.localPlayer.GetComponent<Camera>();
            if (cam == null)
                cam = NetworkClient.localPlayer.GetComponentInChildren<Camera>(true);

            if (cam != null)
            {
                _camera = cam;
                _cameraTransform = cam.transform;
                return true;
            }
        }

        Camera[] cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].enabled)
            {
                _camera = cameras[i];
                _cameraTransform = cameras[i].transform;
                return true;
            }
        }

        Camera fallback = Object.FindFirstObjectByType<Camera>();
        if (fallback != null)
        {
            _camera = fallback;
            _cameraTransform = fallback.transform;
            return true;
        }

        return false;
    }
}
