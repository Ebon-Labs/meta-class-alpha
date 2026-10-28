using UnityEngine;

public static class PlanetSpinCalculator
{
    public static float DaySeconds(float siderealDayHours)
    {
        return Mathf.Max(0.001f, Mathf.Abs(siderealDayHours)) * 3600f;
    }

    public static float DegreesPerSecond(float siderealDayHours, float timeScale)
    {
        float daySeconds = DaySeconds(siderealDayHours);
        float sign = siderealDayHours < 0f ? -1f : 1f;
        return sign * 360f / (daySeconds / Mathf.Max(0.0001f, timeScale));
    }

    public static float AdvanceAngle(float currentAngleDegrees, float degreesPerSecond, float deltaTime)
    {
        return Mathf.Repeat(currentAngleDegrees + degreesPerSecond * deltaTime, 360f);
    }

    public static Quaternion ApplyAxialTilt(Quaternion baseLocalRotation, float axialTiltDegrees)
    {
        return Quaternion.Euler(axialTiltDegrees, 0f, 0f) * baseLocalRotation;
    }

    public static Quaternion EvaluateLocalRotation(Quaternion baseLocalRotation, float spinAngleDegrees)
    {
        return baseLocalRotation * Quaternion.AngleAxis(spinAngleDegrees, Vector3.up);
    }
}
