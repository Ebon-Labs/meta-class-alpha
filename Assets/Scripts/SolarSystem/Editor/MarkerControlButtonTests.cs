using NUnit.Framework;

public class MarkerControlButtonTests
{
    [Test]
    public void MissingMotion_ShowsTheVisibleIcon()
    {
        Assert.IsTrue(MarkerControlButton.ShowsVisibleIcon(motionMissing: true, orbsEnabled: false));
    }

    [Test]
    public void ExistingMotion_FollowsOrbVisibility()
    {
        Assert.IsTrue(MarkerControlButton.ShowsVisibleIcon(motionMissing: false, orbsEnabled: true));
        Assert.IsFalse(MarkerControlButton.ShowsVisibleIcon(motionMissing: false, orbsEnabled: false));
        Assert.AreEqual(
            "hidden",
            ControlButtonAppearance.Select(!MarkerControlButton.ShowsVisibleIcon(false, false), "shown", "hidden"));
    }
}
