using System;
using NUnit.Framework;
using UnityEngine;

public class PlanetScientificDataTests
{
    [Test]
    public void EarthOrbit_UsesTheSceneScaleDirectly()
    {
        PlanetScientificData earth = PlanetScientificCatalog.Get(SolarPlanetId.Earth);
        Assert.AreEqual(1f, earth.semiMajorAxisAu, 0.0001f);
        Assert.AreEqual(
            PlanetScientificCatalog.EarthOrbitSceneUnits,
            earth.SceneOrbitRadius(PlanetScientificCatalog.EarthOrbitSceneUnits),
            0.0001f);
    }

    [Test]
    public void Mercury_KeepsTheAstronomicalUnitProportion()
    {
        PlanetScientificData mercury = PlanetScientificCatalog.Get(SolarPlanetId.Mercury);
        Assert.AreEqual(0.387f * 300f, mercury.SceneOrbitRadius(300f), 0.01f);
    }

    [Test]
    public void Venus_HasARetrogradeDay()
    {
        Assert.Less(PlanetScientificCatalog.Get(SolarPlanetId.Venus).siderealDayHours, 0f);
    }

    [Test]
    public void EveryCatalogBody_HasAPositiveOrbit()
    {
        foreach (SolarPlanetId id in Enum.GetValues(typeof(SolarPlanetId)))
        {
            PlanetScientificData data = PlanetScientificCatalog.Get(id);
            Assert.Greater(data.semiMajorAxisAu, 0f);
            Assert.Greater(Mathf.Abs(data.orbitalPeriodDays), 0f);
            Assert.AreNotEqual(0f, data.siderealDayHours);
        }
    }

    [Test]
    public void UnknownId_FallsBackToEarth()
    {
        PlanetScientificData unknown = PlanetScientificCatalog.Get((SolarPlanetId)999);
        PlanetScientificData earth = PlanetScientificCatalog.Get(SolarPlanetId.Earth);
        Assert.AreEqual(earth.orbitalPeriodDays, unknown.orbitalPeriodDays, 0.0001f);
        Assert.AreEqual(earth.semiMajorAxisAu, unknown.semiMajorAxisAu, 0.0001f);
    }
}
