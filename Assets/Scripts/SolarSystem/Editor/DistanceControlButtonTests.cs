using NUnit.Framework;

public class DistanceControlButtonTests
{
    [Test]
    public void NearScale_SelectsTheNearIcon()
    {
        Assert.IsTrue(DistanceControlButton.ShowsNearIcon(true));
        Assert.IsFalse(DistanceControlButton.ShowsNearIcon(false));
        Assert.AreEqual("near", ControlButtonAppearance.Select(DistanceControlButton.ShowsNearIcon(true), "common", "near"));
    }
}
