using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CameraPointerController : NetworkBehaviour
{
    const float MaxDistance = float.PositiveInfinity;

    readonly string[] interactiveTags = { "Interactive", "Teleportable", "Player" };

    GameObject gazedAtObject;
    GameObject pointerDisabledTarget;
    bool hasPointerEntered;
    float onPointerEnterCounter;

    UILogger loggerScript;
    Camera camera;
    Slider sliderObject;

    public float onPointerClickTime = 3f;
    public GameObject Loader;
    public Image pointerIcon;
    public Sprite idleIcon;
    public Sprite gazedIcon;

    public override void OnStartServer()
    {
        PlaceAtClassArea();
    }

    public override void OnStartClient()
    {
        sliderObject = Loader.GetComponentInChildren<Slider>();
        if (isLocalPlayer)
            PlaceAtClassArea();

        loggerScript = GetComponentInChildren<UILogger>();
        camera = GetComponentInChildren<Camera>(true);

        if (!isLocalPlayer)
            return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (camera != null)
            camera.enabled = true;
        if (canvas != null)
            canvas.enabled = true;
    }

    void PlaceAtClassArea()
    {
        ClassController classController = ClassController.ResolveExisting(FindFirstObjectByType<ClassController>());
        if (classController == null)
            return;

        classController.PlacePlayer(transform, IsHostPlayer());
    }

    public void ReturnToClassArea()
    {
        PlaceAtClassArea();
    }

    bool IsHostPlayer()
    {
        if (!NetworkServer.active)
            return false;

        return connectionToClient != null
            && NetworkServer.localConnection != null
            && connectionToClient == NetworkServer.localConnection;
    }

    public void Update()
    {
        if (!isLocalPlayer)
            return;

        if (camera == null)
            camera = GetComponentInChildren<Camera>(true);

        Transform look = camera != null ? camera.transform : transform;
        if (Physics.Raycast(look.position, look.forward, out RaycastHit hit, MaxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            GazeAt(hit.transform.gameObject);
        else
            ClearGaze();

        bool showLoading = GazeDwellTimer.ShowLoader(hasPointerEntered, HasObjectBasicController(gazedAtObject));
        if (hasPointerEntered)
            AdvanceDwell(showLoading);
        else
            ResetDwell();

        UpdatePointerIcon(showLoading);
    }

    void GazeAt(GameObject hitObject)
    {
        bool isTeleportable = IsTeleportable(hitObject);
        bool atTargetEdge = isTeleportable && IsObserverAtTargetEdge(hitObject);
        bool pointerDisabled = IsPointerDisabledFor(hitObject);

        if (gazedAtObject != hitObject)
        {
            loggerScript.SetMessage("");
            SendPointerMessage(gazedAtObject, "OnPointerExit");

            if (pointerDisabledTarget != null && hitObject != pointerDisabledTarget)
                pointerDisabledTarget = null;

            gazedAtObject = hitObject;

            if (!GazeFocusRules.BlocksNewFocus(gazedAtObject.tag == GazeFocusRules.EnvironmentTag, pointerDisabled, isTeleportable && atTargetEdge))
            {
                SendPointerMessage(gazedAtObject, "OnPointerEnter");
                loggerScript.SetMessage(HasObjectBasicController(gazedAtObject) ? GetPointerLogName(gazedAtObject) : "");
                hasPointerEntered = true;
            }
            else
            {
                hasPointerEntered = false;
            }

            return;
        }

        if (GazeFocusRules.ShouldClearFocus(pointerDisabled, isTeleportable && atTargetEdge))
        {
            hasPointerEntered = false;
            loggerScript.SetMessage("");
        }
    }

    void ClearGaze()
    {
        loggerScript.SetMessage("");
        SendPointerMessage(gazedAtObject, "OnPointerExit");
        gazedAtObject = null;
        pointerDisabledTarget = null;
        hasPointerEntered = false;
    }

    void AdvanceDwell(bool showLoading)
    {
        onPointerEnterCounter = GazeDwellTimer.Advance(onPointerEnterCounter, Time.deltaTime);

        if (showLoading)
        {
            Loader.SetActive(true);
            sliderObject.value = GazeDwellTimer.SliderFraction(onPointerEnterCounter, onPointerClickTime);
        }
        else
        {
            Loader.SetActive(false);
            sliderObject.value = 0f;
        }

        if (!GazeDwellTimer.IsComplete(onPointerEnterCounter, onPointerClickTime))
            return;

        GameObject target = gazedAtObject;
        HandlePointerClick(target);

        if (!IsTeleportable(target) && HasObjectBasicController(target))
            loggerScript.SetMessage(PointerLog.Click(target.name));
        else
            loggerScript.SetMessage("");

        Loader.SetActive(false);
        sliderObject.value = 0f;
    }

    void ResetDwell()
    {
        onPointerEnterCounter = 0f;
        Loader.SetActive(false);
        sliderObject.value = 0f;
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
        HandlePointerClick(target);
    }

    void HandlePointerClick(GameObject target)
    {
        if (GoHomeOrb.IsTarget(target != null ? target.transform : null))
        {
            ReturnToClassArea();
            DisablePointerForTarget(target);
            return;
        }

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
        SendPointerMessage(target, "OnPointerExit");
        loggerScript.SetMessage("");
        hasPointerEntered = false;
        onPointerEnterCounter = 0f;
        pointerDisabledTarget = target;
    }

    bool IsPointerDisabledFor(GameObject target)
    {
        return target != null && target == pointerDisabledTarget;
    }

    static string GetPointerLogName(GameObject target)
    {
        if (target == null)
            return "";

        Transform parent = target.transform.parent;
        bool parentIsPlanet = parent != null && parent.CompareTag(GazeFocusRules.PlanetTag);
        return PointerLog.FocusName(target.name, parent != null ? parent.name : null, parentIsPlanet);
    }

    static bool IsTeleportable(GameObject target)
    {
        return target != null && target.CompareTag(GazeFocusRules.TeleportableTag);
    }

    static bool HasObjectBasicController(GameObject target)
    {
        if (target == null)
            return false;

        return target.GetComponentInParent<ObjectBasicController>() != null;
    }

    bool IsObserverAtTargetEdge(GameObject target)
    {
        if (!IsTeleportable(target) || !TryGetTargetBounds(target, out Bounds bounds))
            return false;

        Vector3 edgePosition = GetTargetEdgePosition(target);
        return TeleportPlacement.IsAtEdge(transform.position, edgePosition, bounds);
    }

    Vector3 GetTargetEdgePosition(GameObject target)
    {
        if (!IsTeleportable(target) || !TryGetTargetBounds(target, out Bounds bounds))
            return target.transform.position;

        return TeleportPlacement.EdgePosition(bounds, transform.position, transform.forward);
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

    public void sendMessage(GameObject target, string message)
    {
        SendPointerMessage(target, message);
    }

    void SendPointerMessage(GameObject target, string message)
    {
        if (target && GazeFocusRules.AcceptsPointerMessage(target.tag, interactiveTags))
            target.SendMessage(message);
    }
}
