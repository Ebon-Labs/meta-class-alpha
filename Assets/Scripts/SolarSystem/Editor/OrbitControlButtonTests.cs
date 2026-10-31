using NUnit.Framework;
using UnityEngine;

public class OrbitControlButtonTests
{
    [Test]
    public void IncreasedPace_SelectsTheFastIcon()
    {
        Assert.IsTrue(OrbitControlButton.ShowsIncreasedIcon(true));
        Assert.AreEqual("fast", ControlButtonAppearance.Select(OrbitControlButton.ShowsIncreasedIcon(true), "common", "fast"));
    }

    [Test]
    public void NearDistance_LocksTheButtonAndDimsIt()
    {
        Assert.IsTrue(OrbitControlButton.IsSpeedLocked(hasMotion: true, isNearDistance: true));
        Assert.IsFalse(OrbitControlButton.IsSpeedLocked(hasMotion: false, isNearDistance: true));
        Assert.IsFalse(OrbitControlButton.IsSpeedLocked(hasMotion: true, isNearDistance: false));
        Assert.AreEqual(OrbitControlButton.DisabledTint, OrbitControlButton.TintForLock(true));
        Assert.AreEqual(Color.white, OrbitControlButton.TintForLock(false));
    }
}
