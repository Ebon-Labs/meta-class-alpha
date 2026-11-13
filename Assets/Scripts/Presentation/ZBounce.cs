using UnityEngine;

[DisallowMultipleComponent]
public class ZBounce : MonoBehaviour
{
    public const float Amplitude = 1f;
    public const float Period = 16f;

    Vector3 rest;
    float elapsed;
    bool wasShown;
    bool captured;
    Renderer body;

    public static float Offset(float time, float amplitude, float period)
    {
        if (period <= 0.0001f)
            return 0f;

        float cycle = time * Mathf.PI * 2f / period;
        return amplitude * (0.5f - 0.5f * Mathf.Cos(cycle));
    }

    public static Vector3 Place(Vector3 rest, float offset)
    {
        return new Vector3(rest.x, rest.y, rest.z + offset);
    }

    public static float ShownElapsed(float elapsed, float deltaTime, bool shown, bool wasShown)
    {
        if (!shown || !wasShown || deltaTime <= 0f)
            return 0f;

        return elapsed + deltaTime;
    }

    public static bool IsShown(bool active, bool rendererEnabled, bool rendererVisible)
    {
        return active && rendererEnabled && rendererVisible;
    }

    void OnEnable()
    {
        body = GetComponent<Renderer>();
        rest = transform.localPosition;
        elapsed = 0f;
        wasShown = false;
        captured = true;
    }

    void OnDisable()
    {
        if (!captured)
            return;

        transform.localPosition = rest;
        elapsed = 0f;
        wasShown = false;
    }

    void LateUpdate()
    {
        if (!captured)
            return;

        bool shown = body != null && IsShown(isActiveAndEnabled, body.enabled, body.isVisible);
        elapsed = ShownElapsed(elapsed, Time.deltaTime, shown, wasShown);
        wasShown = shown;
        transform.localPosition = shown ? Place(rest, Offset(elapsed, Amplitude, Period)) : rest;
    }
}
