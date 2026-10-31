using UnityEngine;

public readonly struct SolarMotionSnapshot
{
    public const float IncreasedTimeScaleMultiplier = 1000f;
    public const float NearDistanceScale = 3f;
    public const float MinDistanceScale = 0.01f;

    public readonly float DefaultTimeScale;
    public readonly float TimeScale;
    public readonly float DistanceScale;
    public readonly bool InteractionOrbsEnabled;
    public readonly bool ShowPlanetLabels;
    public readonly float MaxDistanceScale;

    public SolarMotionSnapshot(
        float defaultTimeScale,
        float timeScale,
        float distanceScale,
        bool interactionOrbsEnabled,
        bool showPlanetLabels,
        float maxDistanceScale)
    {
        DefaultTimeScale = defaultTimeScale;
        TimeScale = timeScale;
        DistanceScale = distanceScale;
        InteractionOrbsEnabled = interactionOrbsEnabled;
        ShowPlanetLabels = showPlanetLabels;
        MaxDistanceScale = maxDistanceScale;
    }

    public bool IsNearDistance => Mathf.Approximately(DistanceScale, NearDistanceScale);

    public bool IsIncreasedTimeScale =>
        Mathf.Approximately(TimeScale, DefaultTimeScale * IncreasedTimeScaleMultiplier);

    public SolarMotionSnapshot ToggleDistance()
    {
        float nextDistance = IsNearDistance ? MaxDistanceScale : NearDistanceScale;
        return WithDistance(nextDistance);
    }

    public SolarMotionSnapshot WithDistance(float scale)
    {
        bool wasNear = IsNearDistance;
        bool nowNear = Mathf.Approximately(scale, NearDistanceScale);
        float nextTime = TimeScale;
        if (wasNear != nowNear)
            nextTime = nowNear ? 0f : DefaultTimeScale;

        return new SolarMotionSnapshot(
            DefaultTimeScale,
            nextTime,
            scale,
            InteractionOrbsEnabled,
            ShowPlanetLabels,
            MaxDistanceScale);
    }

    public SolarMotionSnapshot ToggleTime()
    {
        float next = IsIncreasedTimeScale
            ? DefaultTimeScale
            : DefaultTimeScale * IncreasedTimeScaleMultiplier;
        return WithTime(next);
    }

    public SolarMotionSnapshot WithTime(float scale)
    {
        return new SolarMotionSnapshot(
            DefaultTimeScale,
            scale,
            DistanceScale,
            InteractionOrbsEnabled,
            ShowPlanetLabels,
            MaxDistanceScale);
    }

    public SolarMotionSnapshot ToggleOrbs()
    {
        return WithOrbs(!InteractionOrbsEnabled);
    }

    public SolarMotionSnapshot WithOrbs(bool show)
    {
        return new SolarMotionSnapshot(
            DefaultTimeScale,
            TimeScale,
            DistanceScale,
            show,
            ShowPlanetLabels,
            MaxDistanceScale);
    }

    public SolarMotionSnapshot StoppedIfNear()
    {
        if (!IsNearDistance)
            return this;

        return WithTime(0f);
    }

    public SolarMotionSnapshot Clamped()
    {
        float clamped = Mathf.Clamp(DistanceScale, MinDistanceScale, MaxDistanceScale);
        if (Mathf.Approximately(clamped, DistanceScale))
            return this;

        return new SolarMotionSnapshot(
            DefaultTimeScale,
            TimeScale,
            clamped,
            InteractionOrbsEnabled,
            ShowPlanetLabels,
            MaxDistanceScale);
    }
}
