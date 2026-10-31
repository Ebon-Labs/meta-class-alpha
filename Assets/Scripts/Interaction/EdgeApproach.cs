using UnityEngine;

public static class EdgeApproach
{
    public static float Progress(float elapsed, float duration)
    {
        if (duration <= 0f)
            return 1f;

        return Mathf.Clamp01(elapsed / duration);
    }

    public static float Ease(float progress)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
    }

    public static Vector3 Position(Vector3 start, Bounds bounds, Vector3 fallbackForward, float elapsed, float duration)
    {
        Vector3 edge = TeleportPlacement.EdgePosition(bounds, start, fallbackForward);
        return Vector3.Lerp(start, edge, Ease(Progress(elapsed, duration)));
    }
}
