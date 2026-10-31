using UnityEngine;

public static class TeleportPlacement
{
    public const float EdgeMargin = 0.05f;

    public static bool IsAtEdge(Vector3 observerPosition, Vector3 edgePosition, Bounds bounds)
    {
        float tolerance = Mathf.Max(0.1f, bounds.extents.magnitude * 0.02f);
        return (observerPosition - edgePosition).sqrMagnitude <= tolerance * tolerance;
    }

    public static Vector3 EdgePosition(Bounds bounds, Vector3 observerPosition, Vector3 fallbackForward)
    {
        Vector3 direction = observerPosition - bounds.center;
        if (direction.sqrMagnitude < 0.0001f)
            direction = -fallbackForward;
        else
            direction.Normalize();

        return SurfacePoint(bounds, direction) + direction * EdgeMargin;
    }

    public static Vector3 SurfacePoint(Bounds bounds, Vector3 direction)
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
}
