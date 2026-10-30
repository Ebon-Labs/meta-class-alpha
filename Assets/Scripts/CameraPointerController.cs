using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraPointerController : NetworkBehaviour
{
    private const float _maxDistance = float.PositiveInfinity;
    private GameObject _gazedAtObject = null;
    private GameObject _pointerDisabledTarget = null;

    private bool hasPointerEntered = false;
    private float onPointerEnterCounter = 0f;
    public float onPointerClickTime = 3f;

    private UILogger loggerScript;
    private CameraPointerController cameraPointerController;
    private Camera camera;
    private Slider SliderObject;
    private string[] interactiveTags = { "Interactive", "Teleportable", "Player" };
    public GameObject Loader;
    public Image pointerIcon;
    public Sprite idleIcon;
    public Sprite gazedIcon;

    public override void OnStartServer()
    {
        PlaceAboveGuestArea();
    }

    public override void OnStartClient()
    {
        SliderObject = Loader.GetComponentInChildren<Slider>();
        if (isLocalPlayer)
            PlaceAboveGuestArea();

        cameraPointerController = this.GetComponentInChildren<CameraPointerController>();
        loggerScript = this.GetComponentInChildren<UILogger>();

        if (isLocalPlayer)
        {
            Camera camera = cameraPointerController.GetComponent<Camera>();
            Canvas canvas = this.GetComponentInChildren<Canvas>();
            camera.enabled = true;
            canvas.enabled = true;
            return;
        }

    }

    void PlaceAboveGuestArea()
    {
        GameObject area = GameObject.Find("GuestArea");
        if (area == null)
            return;

        const float eyeHeight = 1.6f;
        Vector3 position = area.transform.position + Vector3.up * eyeHeight;
        Renderer renderer = area.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            position = new Vector3(bounds.center.x, bounds.max.y + eyeHeight, bounds.center.z);
        }

        transform.position = position;
    }

    public void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Casts ray towards camera's forward direction, to detect if a GameObject is being gazed
        // at.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hit.transform.gameObject;
            bool isTeleportable = IsTeleportable(hitObject);
            bool atTargetEdge = isTeleportable && IsObserverAtTargetEdge(hitObject);
            bool pointerDisabled = IsPointerDisabledFor(hitObject);

            if (_gazedAtObject != hitObject)
            {
                loggerScript.SetMessage("");
                sendMessage(_gazedAtObject, "OnPointerExit");

                if (_pointerDisabledTarget != null && hitObject != _pointerDisabledTarget)
                    _pointerDisabledTarget = null;

                _gazedAtObject = hitObject;

                if (_gazedAtObject.tag != "Environment" && !pointerDisabled && !(isTeleportable && atTargetEdge))
                {
                    sendMessage(_gazedAtObject, "OnPointerEnter");
                    loggerScript.SetMessage(GetPointerLogName(_gazedAtObject));
                    hasPointerEntered = true;
                }
                else
                {
                    hasPointerEntered = false;
                }
            }
            else if (pointerDisabled || (isTeleportable && atTargetEdge))
            {
                hasPointerEntered = false;
                loggerScript.SetMessage("");
            }
        }
        else
        {
            // No GameObject detected in front of the camera.
            loggerScript.SetMessage("");
            sendMessage(_gazedAtObject, "OnPointerExit");
            _gazedAtObject = null;
            _pointerDisabledTarget = null;

            hasPointerEntered = false;
        }

        if (hasPointerEntered)
        {
            onPointerEnterCounter += Time.deltaTime;

            Loader.SetActive(true);
            SliderObject.value = onPointerEnterCounter / onPointerClickTime;

            if (onPointerEnterCounter >= onPointerClickTime)
            {
                GameObject target = _gazedAtObject;
                cameraPointerController.handlePointerClick(target);

                if (!IsTeleportable(target))
                    loggerScript.SetMessage("Clique em: " + target.name);
                else
                    loggerScript.SetMessage("");

                Loader.SetActive(false);
                SliderObject.value = 0f;
            }
        }
        else
        {
            onPointerEnterCounter = 0;

            Loader.SetActive(false);
            SliderObject.value = 0f;
        }

        UpdatePointerIcon(hasPointerEntered);
    }

    void UpdatePointerIcon(bool gazed)
    {
        if (pointerIcon == null)
            return;

        Sprite next = gazed ? gazedIcon : idleIcon;
        if (next != null && pointerIcon.sprite != next)
            pointerIcon.sprite = next;
    }

    public float getOnPointerEnterCounter()
    {
        return onPointerEnterCounter;
    }

    public void handlePointerClick(GameObject target)
    {
        if (IsTeleportable(target))
        {
            Debug.Log("handlePointerClick: " + target.tag);
            transform.position = GetTargetEdgePosition(target);
            DisablePointerForTarget(target);
            return;
        }

        target.SendMessage("OnPointerClick", SendMessageOptions.DontRequireReceiver);
        DisablePointerForTarget(target);
    }

    void DisablePointerForTarget(GameObject target)
    {
        sendMessage(target, "OnPointerExit");
        loggerScript.SetMessage("");
        hasPointerEntered = false;
        onPointerEnterCounter = 0f;
        _pointerDisabledTarget = target;
    }

    bool IsPointerDisabledFor(GameObject target)
    {
        return target != null && target == _pointerDisabledTarget;
    }

    static string GetPointerLogName(GameObject target)
    {
        if (target == null)
            return "";

        Transform parent = target.transform.parent;
        if (parent != null && parent.CompareTag("Planet"))
            return "Planeta: " + parent.name;

        return target.name;
    }

    static bool IsTeleportable(GameObject target)
    {
        return target != null && target.CompareTag("Teleportable");
    }

    bool IsObserverAtTargetEdge(GameObject target)
    {
        if (!IsTeleportable(target) || !TryGetTargetBounds(target, out Bounds bounds))
            return false;

        Vector3 edgePosition = GetTargetEdgePosition(target);
        float tolerance = Mathf.Max(0.1f, bounds.extents.magnitude * 0.02f);
        return (transform.position - edgePosition).sqrMagnitude <= tolerance * tolerance;
    }

    Vector3 GetTargetEdgePosition(GameObject target)
    {
        if (!IsTeleportable(target) || !TryGetTargetBounds(target, out Bounds bounds))
            return target.transform.position;

        Vector3 center = bounds.center;
        Vector3 direction = transform.position - center;
        if (direction.sqrMagnitude < 0.0001f)
            direction = -transform.forward;
        else
            direction.Normalize();

        const float margin = 0.05f;
        return GetBoundsSurfacePoint(bounds, direction) + direction * margin;
    }

    static bool TryGetTargetBounds(GameObject target, out Bounds bounds)
    {
        bounds = default;
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
            return false;

        bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
            bounds.Encapsulate(colliders[i].bounds);
        return true;
    }

    static Vector3 GetBoundsSurfacePoint(Bounds bounds, Vector3 direction)
    {
        Vector3 extents = bounds.extents;
        float distance = float.PositiveInfinity;

        for (int i = 0; i < 3; i++)
        {
            float component = Mathf.Abs(direction[i]);
            if (component > 0.0001f)
                distance = Mathf.Min(distance, extents[i] / component);
        }

        return bounds.center + direction * distance;
    }

    public void sendMessage(GameObject target, string message)
    {
        if (target && System.Array.IndexOf(interactiveTags, target.tag) >= 0)
        {
            target.SendMessage(message);
        }
    }
}
