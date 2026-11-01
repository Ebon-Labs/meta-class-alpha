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
        return Move(start, edge, elapsed, duration);
    }

    public static Vector3 Move(Vector3 start, Vector3 end, float elapsed, float duration)
    {
        return Vector3.Lerp(start, end, Ease(Progress(elapsed, duration)));
    }

    public static Vector3 RestOn(Vector3 position, float bottomOffset, float groundTop, float clearance)
    {
        return new Vector3(position.x, groundTop + clearance + bottomOffset, position.z);
    }

    public static Vector3 AboveGround(Vector3 position, float bottomOffset, float groundTop, float clearance)
    {
        float floor = groundTop + clearance;
        float bottom = position.y - bottomOffset;
        if (bottom >= floor)
            return position;

        return new Vector3(position.x, floor + bottomOffset, position.z);
    }
}
