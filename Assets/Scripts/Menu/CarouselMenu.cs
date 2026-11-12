using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CarouselMenu : MonoBehaviour
{
    public const float StepDegrees = 90f;
    public const float TurnDuration = 1.5f;
    public const string CreateName = "CreateButton";
    public const string CreateBackName = "CreateBackButton";
    public const string EnterName = "EnterButton";
    public const string EnterBackName = "EnterBackButton";
    public const string TutorialName = "TutorialButton";

    Quaternion rotationFrom = Quaternion.identity;
    Quaternion rotationTo = Quaternion.identity;
    float elapsed;
    bool turning;

    public static Quaternion Clockwise(Quaternion localRotation)
    {
        return localRotation * Quaternion.AngleAxis(-Mathf.Abs(StepDegrees), Vector3.up);
    }

    public static Quaternion CounterClockwise(Quaternion localRotation)
    {
        return localRotation * Quaternion.AngleAxis(Mathf.Abs(StepDegrees), Vector3.up);
    }

    public static Quaternion Turn(Quaternion from, Quaternion to, float elapsed, float duration)
    {
        return Quaternion.Slerp(from, to, EdgeApproach.Ease(EdgeApproach.Progress(elapsed, duration)));
    }

    public static float SignedStep(string objectName)
    {
        if (objectName == CreateName || objectName == EnterBackName || objectName == SolarSystemThumbHost.ObjectName)
            return StepDegrees;

        if (objectName == CreateBackName || objectName == EnterName)
            return -StepDegrees;

        return 0f;
    }

    void Update()
    {
        if (turning)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Turn(rotationFrom, rotationTo, elapsed, TurnDuration);
            if (elapsed >= TurnDuration)
            {
                transform.localRotation = rotationTo;
                turning = false;
            }
        }

        CreateRoomButton.Refresh();

        if (!TryPress(out Vector2 screen))
            return;

        Camera view = Camera.main;
        if (RoomList.JoinPressed(screen, view))
            return;

        if (CreateRoomButton.Press(screen, view))
            return;

        if (SolarSystemThumbHost.Press(screen, view))
        {
            Begin(SignedStep(SolarSystemThumbHost.ObjectName));
            return;
        }

        Press(screen);
    }

    public void Press(Vector2 screen)
    {
        Camera view = Camera.main;
        if (view == null)
            return;

        float step = SignedStep(screen, view, PressLabels());
        if (step != 0f)
            Begin(step);
    }

    public static GameObject[] PressLabels()
    {
        TextMeshPro[] labels = Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int count = 0;
        for (int i = 0; i < labels.Length; i++)
            if (AcceptsPress(labels[i]))
                count++;

        GameObject[] matches = new GameObject[count];
        int written = 0;
        for (int i = 0; i < labels.Length; i++)
        {
            if (!AcceptsPress(labels[i]))
                continue;

            matches[written++] = labels[i].gameObject;
        }

        return matches;
    }

    static bool AcceptsPress(TextMeshPro label)
    {
        return label != null && label.name != SolarSystemThumbHost.ObjectName && (SignedStep(label.name) != 0f || label.name == TutorialName);
    }

    public static float SignedStep(Vector2 screen, Camera view, params GameObject[] targets)
    {
        float chosen = 0f;
        float nearest = float.PositiveInfinity;
        if (targets == null)
            return 0f;

        for (int i = 0; i < targets.Length; i++)
            Consider(screen, view, targets[i], ref chosen, ref nearest);

        return chosen;
    }

    static void Consider(Vector2 screen, Camera view, GameObject target, ref float chosen, ref float nearest)
    {
        if (target == null || view == null || !SolarSystemThumbHost.FacesCamera(target.transform.forward, view.transform.forward))
            return;

        if (!TryScreenBounds(target, view, out Vector2 min, out Vector2 max, out Vector2 center))
            return;

        Vector2 padding = new Vector2(48f, 48f);
        min -= padding;
        max += padding;
        if (screen.x < min.x || screen.x > max.x || screen.y < min.y || screen.y > max.y)
            return;

        float step = SignedStep(target.name);
        if (step == 0f && target.name != TutorialName)
            return;

        float distance = (screen - center).sqrMagnitude;
        if (distance >= nearest)
            return;

        nearest = distance;
        chosen = step;
    }

    public static bool TryScreenBounds(GameObject target, Camera view, out Vector2 min, out Vector2 max, out Vector2 center)
    {
        min = max = center = default;
        if (target == null || view == null)
            return false;

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null && renderer.bounds.size.sqrMagnitude > 0.000001f)
            return ScreenBounds(view, renderer.bounds, out min, out max, out center);

        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect == null)
            return false;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return ScreenBounds(view, corners, out min, out max, out center);
    }

    public static bool ScreenBounds(Camera view, Bounds bounds, out Vector2 min, out Vector2 max, out Vector2 center)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    corners[i++] = c + Vector3.Scale(e, new Vector3(x, y, z));

        return ScreenBounds(view, corners, out min, out max, out center);
    }

    public static bool ScreenBounds(Camera view, Vector3[] worldCorners, out Vector2 min, out Vector2 max, out Vector2 center)
    {
        min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        center = default;
        if (view == null || worldCorners == null || worldCorners.Length == 0)
            return false;

        bool any = false;
        Vector2 accumulated = Vector2.zero;
        int count = 0;
        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector3 screen = view.WorldToScreenPoint(worldCorners[i]);
            if (screen.z < 0f)
                continue;

            any = true;
            min = Vector2.Min(min, screen);
            max = Vector2.Max(max, screen);
            accumulated += (Vector2)screen;
            count++;
        }

        if (!any)
            return false;

        center = accumulated / count;
        return true;
    }

    public static bool ContainsScreenPoint(Vector2 screen, Vector3 a, Vector3 b, Vector3 c, Vector3 d, out float depth)
    {
        depth = (a.z + b.z + c.z + d.z) * 0.25f;
        if (a.z < 0f || b.z < 0f || c.z < 0f || d.z < 0f)
            return false;

        float minX = Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c.x, d.x));
        float maxX = Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c.x, d.x));
        float minY = Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c.y, d.y));
        float maxY = Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c.y, d.y));
        return screen.x >= minX && screen.x <= maxX && screen.y >= minY && screen.y <= maxY;
    }

    void Begin(float signedDegrees)
    {
        rotationFrom = transform.localRotation;
        rotationTo = signedDegrees > 0f ? CounterClockwise(rotationFrom) : Clockwise(rotationFrom);
        elapsed = 0f;
        turning = true;
    }

    public static bool TryPress(out Vector2 screen)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase != TouchPhase.Ended)
                continue;

            screen = touch.position;
            return true;
        }

        if (Input.touchCount == 0 && Input.GetMouseButtonUp(0))
        {
            screen = Input.mousePosition;
            return true;
        }

        screen = default;
        return false;
    }
}
