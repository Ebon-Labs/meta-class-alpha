using System;
using UnityEngine;

[Serializable]
public struct PlanetScientificData
{
    [Tooltip("Semi-major axis in astronomical units (Earth = 1).")]
    public float semiMajorAxisAu;

    [Tooltip("Sidereal orbital period in mean solar days.")]
    public float orbitalPeriodDays;

    [Tooltip("Sidereal rotation period in hours. Negative = retrograde.")]
    public float siderealDayHours;

    [Tooltip("Axial obliquity in degrees.")]
    public float axialTiltDegrees;

    public float SceneOrbitRadius(float earthOrbitSceneUnits)
    {
        return semiMajorAxisAu * earthOrbitSceneUnits;
    }
}

public static class PlanetScientificCatalog
{
    public const float EarthOrbitSceneUnits = 300f;
    public const float MeanSolarDaySeconds = 86400f;

    public static PlanetScientificData Get(SolarPlanetId id)
    {
        switch (id)
        {
            case SolarPlanetId.Mercury:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 0.387f,
                    orbitalPeriodDays = 87.969f,
                    siderealDayHours = 1407.6f,
                    axialTiltDegrees = 0.034f
                };
            case SolarPlanetId.Venus:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 0.723f,
                    orbitalPeriodDays = 224.701f,
                    siderealDayHours = -5832.6f,
                    axialTiltDegrees = 177.36f
                };
            case SolarPlanetId.Earth:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 1f,
                    orbitalPeriodDays = 365.256f,
                    siderealDayHours = 23.934f,
                    axialTiltDegrees = 23.44f
                };
            case SolarPlanetId.Mars:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 1.524f,
                    orbitalPeriodDays = 686.98f,
                    siderealDayHours = 24.623f,
                    axialTiltDegrees = 25.19f
                };
            case SolarPlanetId.Jupiter:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 5.203f,
                    orbitalPeriodDays = 4332.59f,
                    siderealDayHours = 9.925f,
                    axialTiltDegrees = 3.13f
                };
            case SolarPlanetId.Saturn:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 9.537f,
                    orbitalPeriodDays = 10759.22f,
                    siderealDayHours = 10.656f,
                    axialTiltDegrees = 26.73f
                };
            case SolarPlanetId.Uranus:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 19.191f,
                    orbitalPeriodDays = 30688.5f,
                    siderealDayHours = -17.24f,
                    axialTiltDegrees = 97.77f
                };
            case SolarPlanetId.Neptune:
                return new PlanetScientificData
                {
                    semiMajorAxisAu = 30.069f,
                    orbitalPeriodDays = 60182f,
                    siderealDayHours = 16.11f,
                    axialTiltDegrees = 28.32f
                };
            default:
                return Get(SolarPlanetId.Earth);
        }
    }
}
