using NUnit.Framework;

public class GroundControlButtonTests
{
    [Test]
    public void HiddenFloor_OffersTheRevealIcon()
    {
        Assert.IsTrue(GroundControlButton.ShowsRevealIcon(true));
        Assert.IsFalse(GroundControlButton.ShowsRevealIcon(false));
        Assert.AreEqual("show", ControlButtonAppearance.Select(GroundControlButton.ShowsRevealIcon(true), "hide", "show"));
    }
}
