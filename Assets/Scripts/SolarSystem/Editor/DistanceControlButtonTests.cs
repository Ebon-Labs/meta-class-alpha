using NUnit.Framework;

public class DistanceControlButtonTests
{
    [Test]
    public void NearScale_ShowsTheCommonIcon()
    {
        Assert.IsTrue(DistanceControlButton.ShowsNearIcon(true));
        Assert.IsFalse(DistanceControlButton.ShowsNearIcon(false));
        Assert.AreEqual("common", ControlButtonAppearance.Select(DistanceControlButton.ShowsNearIcon(true), "near", "common"));
        Assert.AreEqual("near", ControlButtonAppearance.Select(DistanceControlButton.ShowsNearIcon(false), "near", "common"));
    }
}
