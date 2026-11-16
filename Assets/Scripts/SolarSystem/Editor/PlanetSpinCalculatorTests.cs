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
    public void AngleAtClock_MatchesAScaledStep()
    {
        const float hours = 23.934f;
        const float scale = 4320f;
        const float dt = 0.016f;
        float stepped = PlanetSpinCalculator.AdvanceAngle(0f, PlanetSpinCalculator.DegreesPerSecond(hours, scale), dt);
        Assert.AreEqual(stepped, PlanetSpinCalculator.AngleAtClock(hours, scale * dt), 0.0001f);
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

    [Test]
    public void SpinAroundMeshForward_KeepsThatPoleFixed()
    {
        Quaternion baseRotation = Quaternion.Euler(10f, 20f, 30f);
        Vector3 restPole = baseRotation * Vector3.forward;
        Quaternion spun = PlanetSpinCalculator.EvaluateLocalRotation(baseRotation, 80f, Vector3.forward);
        Vector3 pole = spun * Vector3.forward;
        Assert.AreEqual(restPole.x, pole.x, 0.0001f);
        Assert.AreEqual(restPole.y, pole.y, 0.0001f);
        Assert.AreEqual(restPole.z, pole.z, 0.0001f);
    }

    [Test]
    public void SpinAroundAnOffsetCenter_LeavesThatPointFixed()
    {
        Vector3 center = new Vector3(0f, 1.19f, 0.027f);
        Quaternion baseRotation = Quaternion.Euler(0f, 35f, 23.44f);
        Quaternion spun = PlanetSpinCalculator.EvaluateLocalRotation(baseRotation, 90f, Vector3.forward);
        Vector3 position = PlanetSpinCalculator.EvaluateLocalPosition(Vector3.zero, baseRotation, spun, center);
        Vector3 spunCenter = position + spun * center;
        Vector3 restCenter = baseRotation * center;
        Assert.AreEqual(restCenter.x, spunCenter.x, 0.0001f);
        Assert.AreEqual(restCenter.y, spunCenter.y, 0.0001f);
        Assert.AreEqual(restCenter.z, spunCenter.z, 0.0001f);
    }
}
