using NUnit.Framework;

public class SolarSystemMotionControllerTests
{
    [Test]
    public void PublishedScales_MatchTheMotionRules()
    {
        Assert.AreEqual(SolarMotionSnapshot.NearDistanceScale, SolarSystemMotionController.NearDistanceScale);
        Assert.AreEqual(SolarMotionSnapshot.IncreasedTimeScaleMultiplier, SolarSystemMotionController.IncreasedTimeScaleMultiplier);
        Assert.AreEqual(3f, SolarSystemMotionController.NearDistanceScale, 0.0001f);
        Assert.AreEqual(1000f, SolarSystemMotionController.IncreasedTimeScaleMultiplier, 0.0001f);
    }

    [Test]
    public void DistanceToggle_UsesTheSameNearAndFarPairTheControllerStores()
    {
        SolarMotionSnapshot state = new SolarMotionSnapshot(
            4320f,
            4320f,
            SolarSystemMotionController.NearDistanceScale,
            false,
            true,
            PlanetScientificCatalog.EarthOrbitSceneUnits);

        SolarMotionSnapshot far = state.ToggleDistance();
        Assert.AreEqual(PlanetScientificCatalog.EarthOrbitSceneUnits, far.DistanceScale, 0.0001f);
        Assert.AreEqual(4320f, far.TimeScale, 0.0001f);
    }
}
