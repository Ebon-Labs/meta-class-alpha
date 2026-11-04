using UnityEngine;

[DisallowMultipleComponent]
public class LogoBounce : MonoBehaviour
{
    public const float Amplitude = 0.35f;
    public const float Period = 1.6f;

    RectTransform rect;
    Vector2 rest;
    bool captured;

    public static float Offset(float time, float amplitude, float period)
    {
        if (period <= 0.0001f)
            return 0f;

        float cycle = time * Mathf.PI * 2f / period;
        return amplitude * (0.5f - 0.5f * Mathf.Cos(cycle));
    }

    public static Vector2 Place(Vector2 rest, float offset)
    {
        return new Vector2(rest.x, rest.y + offset);
    }

    void OnEnable()
    {
        rect = transform as RectTransform;
        if (rect == null)
            return;

        rest = rect.anchoredPosition;
        captured = true;
    }

    void OnDisable()
    {
        if (!captured || rect == null)
            return;

        rect.anchoredPosition = rest;
    }

    void LateUpdate()
    {
        if (!captured || rect == null)
            return;

        rect.anchoredPosition = Place(rest, Offset(Time.time, Amplitude, Period));
    }
}
