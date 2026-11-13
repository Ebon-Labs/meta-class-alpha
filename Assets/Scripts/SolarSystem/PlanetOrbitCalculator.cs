using UnityEngine;

public static class PlanetOrbitCalculator
{
    public static float DegreesPerSecond(float orbitalPeriodDays, float timeScale)
    {
        float yearSeconds = Mathf.Max(0.001f, orbitalPeriodDays) * PlanetScientificCatalog.MeanSolarDaySeconds;
        return 360f / (yearSeconds / Mathf.Max(0.0001f, timeScale));
    }

    public static float ResolveRadius(
        float configuredRadius,
        float currentDistance,
        float minClearanceRadius,
        bool ensureClearsSun)
    {
        float radius = configuredRadius > 0f ? configuredRadius : currentDistance;
        if (ensureClearsSun && radius < minClearanceRadius)
            radius = minClearanceRadius;
        return Mathf.Max(0.001f, radius);
    }

    public static Vector3 BuildOffsetFromCenter(Vector3 toPlanet, float radius)
    {
        if (toPlanet.sqrMagnitude < 0.0001f)
            return Vector3.right * radius;
        return toPlanet.normalized * radius;
    }

    public static float MinimumClearRadius(float centerRadius, float bodyRadius, float clearanceMultiplier)
    {
        float multiplier = clearanceMultiplier < 1f ? 1f : clearanceMultiplier;
        return (Mathf.Max(0f, centerRadius) + Mathf.Max(0f, bodyRadius)) * multiplier;
    }

    public static float RadiusFromBounds(Vector3 origin, Vector3 boundsCenter, Vector3 boundsExtents)
    {
        float reach = Mathf.Max(boundsExtents.x, Mathf.Max(boundsExtents.y, boundsExtents.z));
        return Vector3.Distance(origin, boundsCenter) + Mathf.Max(0f, reach);
    }

    public static Vector3 PivotForVisualCenter(Vector3 orbitPoint, Vector3 visualOffset)
    {
        return orbitPoint - visualOffset;
    }

    public static float AdvanceAngle(float currentAngleDegrees, float degreesPerSecond, float deltaTime)
    {
        return Mathf.Repeat(currentAngleDegrees + degreesPerSecond * deltaTime, 360f);
    }

    public static float AngleAtClock(float orbitalPeriodDays, float clock)
    {
        return Mathf.Repeat(DegreesPerSecond(orbitalPeriodDays, 1f) * clock, 360f);
    }

    public static Vector3 EvaluatePosition(
        Vector3 orbitCenter,
        Vector3 offsetFromCenter,
        float angleDegrees,
        Vector3 orbitAxis)
    {
        Vector3 axis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;
        return orbitCenter + Quaternion.AngleAxis(angleDegrees, axis) * offsetFromCenter;
    }
}
