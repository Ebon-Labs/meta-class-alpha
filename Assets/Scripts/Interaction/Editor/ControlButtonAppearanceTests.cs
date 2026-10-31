using NUnit.Framework;

public class ControlButtonAppearanceTests
{
    [Test]
    public void Select_PicksTheSecondOptionOnlyWhenAsked()
    {
        Assert.AreEqual("voice", ControlButtonAppearance.Select(false, "voice", "mute"));
        Assert.AreEqual("mute", ControlButtonAppearance.Select(true, "voice", "mute"));
    }

    [Test]
    public void Select_PreservesANullChoice()
    {
        Assert.IsNull(ControlButtonAppearance.Select(true, "voice", null));
    }
}
