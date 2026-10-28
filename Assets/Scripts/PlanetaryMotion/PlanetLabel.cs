using Mirror;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1100)]
public class PlanetLabel : MonoBehaviour
{
    [SerializeField] TextMeshPro label;
    [SerializeField] Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("Apparent height on screen in pixels, constant at any distance.")]
    [SerializeField] float targetPixelHeight = 20f;
    [SerializeField] string overrideText;

    Camera _camera;
    Transform _cameraTransform;

    void Awake()
    {
        if (label == null)
            label = GetComponent<TextMeshPro>();

        // Keep the local pose authored under EarthWrapper in the scene.
        localOffset = transform.localPosition;

        ApplyPlanetName();
        ApplyLocalPose();
    }

    void LateUpdate()
    {
        if (!TryResolveCamera())
            return;

        ApplyLocalPose();

        // Face the player: match camera orientation (TMP Example FloatingText pattern).
        transform.rotation = _cameraTransform.rotation;

        float distance = Vector3.Distance(_cameraTransform.position, transform.position);
        float parentScale = transform.parent != null
            ? Mathf.Max(0.0001f, Mathf.Abs(transform.parent.lossyScale.x))
            : 1f;

        float fontSize = label != null ? Mathf.Max(1f, label.fontSize) : Mathf.Max(1f, targetPixelHeight);
        float halfFovRad = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float screenHeight = Mathf.Max(1f, Screen.height);
        float worldHeight = (targetPixelHeight / screenHeight)
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

    void ApplyLocalPose()
    {
        transform.localPosition = localOffset;
    }

    void ApplyPlanetName()
    {
        if (label == null)
            return;

        if (!string.IsNullOrWhiteSpace(overrideText))
        {
            label.text = overrideText;
            label.fontSize = targetPixelHeight;
            return;
        }

        PlanetOrbitMotion[] motions = GetComponentsInParent<PlanetOrbitMotion>(true);
        for (int i = 0; i < motions.Length; i++)
        {
            if (motions[i] != null && motions[i].IsOrbitEnabled)
            {
                label.text = motions[i].PlanetProfile.ToString();
                label.fontSize = targetPixelHeight;
                return;
            }
        }

        if (motions.Length > 0 && motions[0] != null)
        {
            label.text = motions[0].PlanetProfile.ToString();
            label.fontSize = targetPixelHeight;
        }
    }

    bool TryResolveCamera()
    {
        if (_camera != null && _camera.enabled && _camera.gameObject.activeInHierarchy)
            return true;

        _camera = null;
        _cameraTransform = null;

        if (NetworkClient.active && NetworkClient.localPlayer != null)
        {
            Camera cam = NetworkClient.localPlayer.GetComponent<Camera>();
            if (cam != null && cam.enabled)
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

        return false;
    }
}
