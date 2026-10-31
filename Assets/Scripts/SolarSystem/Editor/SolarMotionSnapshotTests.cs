using NUnit.Framework;
using UnityEngine;

public class SolarMotionSnapshotTests
{
    SolarMotionSnapshot Common()
    {
        return new SolarMotionSnapshot(
            defaultTimeScale: 4320f,
            timeScale: 4320f,
            distanceScale: 300f,
            interactionOrbsEnabled: false,
            showPlanetLabels: true,
            maxDistanceScale: 300f);
    }

    [Test]
    public void NearDistance_StopsTime_AndLeavingItRestoresTheDefaultPace()
    {
        SolarMotionSnapshot near = Common().ToggleDistance();
        Assert.AreEqual(SolarMotionSnapshot.NearDistanceScale, near.DistanceScale, 0.0001f);
        Assert.AreEqual(0f, near.TimeScale, 0.0001f);
        Assert.IsTrue(near.IsNearDistance);

        SolarMotionSnapshot far = near.ToggleDistance();
        Assert.AreEqual(300f, far.DistanceScale, 0.0001f);
        Assert.AreEqual(4320f, far.TimeScale, 0.0001f);
    }

    [Test]
    public void DistanceChangeThatStaysFar_KeepsTheCurrentPace()
    {
        SolarMotionSnapshot fast = Common().ToggleTime();
        SolarMotionSnapshot moved = fast.WithDistance(100f);

        Assert.AreEqual(100f, moved.DistanceScale, 0.0001f);
        Assert.AreEqual(4320f * SolarMotionSnapshot.IncreasedTimeScaleMultiplier, moved.TimeScale, 0.01f);
    }

    [Test]
    public void TimeToggle_MultipliesThenReturnsToTheDefault()
    {
        SolarMotionSnapshot increased = Common().ToggleTime();
        Assert.IsTrue(increased.IsIncreasedTimeScale);
        Assert.AreEqual(4320f * 1000f, increased.TimeScale, 0.01f);

        SolarMotionSnapshot restored = increased.ToggleTime();
        Assert.IsFalse(restored.IsIncreasedTimeScale);
        Assert.AreEqual(4320f, restored.TimeScale, 0.0001f);
    }

    [Test]
    public void OrbToggle_FlipsVisibilityOnly()
    {
        SolarMotionSnapshot shown = Common().ToggleOrbs();
        Assert.IsTrue(shown.InteractionOrbsEnabled);
        Assert.AreEqual(4320f, shown.TimeScale, 0.0001f);
        Assert.IsTrue(shown.ShowPlanetLabels);
    }

    [Test]
    public void Clamp_KeepsDistanceInsideTheScientificRange()
    {
        SolarMotionSnapshot tooSmall = Common().WithDistance(0f).Clamped();
        SolarMotionSnapshot tooLarge = Common().WithDistance(9999f).Clamped();

        Assert.AreEqual(SolarMotionSnapshot.MinDistanceScale, tooSmall.DistanceScale, 0.0001f);
        Assert.AreEqual(300f, tooLarge.DistanceScale, 0.0001f);
    }

    [Test]
    public void StartingNear_StopsTheOrbits()
    {
        SolarMotionSnapshot near = new SolarMotionSnapshot(4320f, 4320f, 3f, false, true, 300f);
        SolarMotionSnapshot stopped = near.StoppedIfNear();
        Assert.AreEqual(0f, stopped.TimeScale, 0.0001f);

        SolarMotionSnapshot far = Common().StoppedIfNear();
        Assert.AreEqual(4320f, far.TimeScale, 0.0001f);
    }
}
