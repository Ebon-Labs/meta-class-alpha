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

    public static float AdvanceAngle(float currentAngleDegrees, float degreesPerSecond, float deltaTime)
    {
        return Mathf.Repeat(currentAngleDegrees + degreesPerSecond * deltaTime, 360f);
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
