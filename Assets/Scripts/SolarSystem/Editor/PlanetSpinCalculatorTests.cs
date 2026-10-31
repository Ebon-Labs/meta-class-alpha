using NUnit.Framework;
using UnityEngine;

public class PlanetSpinCalculatorTests
{
    [Test]
    public void OneSiderealDayAtRealTime_TurnsAFullCircle()
    {
        float degrees = PlanetSpinCalculator.DegreesPerSecond(24f, 24f * 3600f);
        Assert.AreEqual(360f, degrees, 0.01f);
    }

    [Test]
    public void NegativeDay_SpinsRetrograde()
    {
        float degrees = PlanetSpinCalculator.DegreesPerSecond(-24f, 24f * 3600f);
        Assert.AreEqual(-360f, degrees, 0.01f);
    }

    [Test]
    public void AdvanceAngle_WrapsNegativeMotionIntoAPositiveTurn()
    {
        float angle = PlanetSpinCalculator.AdvanceAngle(10f, -30f, 1f);
        Assert.AreEqual(340f, angle, 0.0001f);
    }

    [Test]
    public void AxialTilt_PitchesTheBaseRotation()
    {
        Quaternion tilted = PlanetSpinCalculator.ApplyAxialTilt(Quaternion.identity, 23.44f);
        Vector3 euler = tilted.eulerAngles;
        Assert.AreEqual(23.44f, euler.x, 0.02f);
    }

    [Test]
    public void LocalSpin_TurnsAroundUpFromTheBaseRotation()
    {
        Quaternion spun = PlanetSpinCalculator.EvaluateLocalRotation(Quaternion.identity, 90f);
        Vector3 euler = spun.eulerAngles;
        Assert.AreEqual(90f, euler.y, 0.02f);
    }
}
