using NUnit.Framework;
using UnityEngine;

public class PlanetOrbitCalculatorTests
{
    [Test]
    public void OneDayAtRealTime_TurnsAFullCircle()
    {
        float degrees = PlanetOrbitCalculator.DegreesPerSecond(1f, PlanetScientificCatalog.MeanSolarDaySeconds);
        Assert.AreEqual(360f, degrees, 0.01f);
    }

    [Test]
    public void AdvanceAngle_WrapsAtAFullTurn()
    {
        Assert.AreEqual(10f, PlanetOrbitCalculator.AdvanceAngle(350f, 20f, 1f), 0.0001f);
    }

    [Test]
    public void ResolveRadius_KeepsClearanceAndFallsBackToCurrentDistance()
    {
        Assert.AreEqual(10f, PlanetOrbitCalculator.ResolveRadius(5f, 1f, 10f, true), 0.0001f);
        Assert.AreEqual(4f, PlanetOrbitCalculator.ResolveRadius(0f, 4f, 1f, false), 0.0001f);
    }

    [Test]
    public void BuildOffset_UsesRightWhenThePlanetSitsOnTheCenter()
    {
        Vector3 offset = PlanetOrbitCalculator.BuildOffsetFromCenter(Vector3.zero, 2f);
        Assert.AreEqual(2f, offset.x, 0.0001f);
        Assert.AreEqual(0f, offset.y, 0.0001f);
        Assert.AreEqual(0f, offset.z, 0.0001f);
    }

    [Test]
    public void BuildOffset_RescalesTheCurrentDirection()
    {
        Vector3 offset = PlanetOrbitCalculator.BuildOffsetFromCenter(new Vector3(3f, 0f, 0f), 2f);
        Assert.AreEqual(2f, offset.x, 0.0001f);
    }

    [Test]
    public void EvaluatePosition_AtZeroAngle_KeepsTheOffset()
    {
        Vector3 position = PlanetOrbitCalculator.EvaluatePosition(
            new Vector3(1f, 2f, 3f),
            new Vector3(4f, 0f, 0f),
            0f,
            Vector3.up);

        Assert.AreEqual(5f, position.x, 0.0001f);
        Assert.AreEqual(2f, position.y, 0.0001f);
        Assert.AreEqual(3f, position.z, 0.0001f);
    }
}
