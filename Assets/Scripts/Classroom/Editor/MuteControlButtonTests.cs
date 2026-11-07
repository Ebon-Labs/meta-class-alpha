using NUnit.Framework;

public class MuteControlButtonTests
{
    [Test]
    public void Icon_IsTheOppositeOfTheMute()
    {
        Assert.IsTrue(MuteControlButton.ShowsMutedIcon(true));
        Assert.IsFalse(MuteControlButton.ShowsMutedIcon(false));
        Assert.AreEqual("voice", ControlButtonAppearance.Select(MuteControlButton.ShowsMutedIcon(true), "mute", "voice"));
        Assert.AreEqual("mute", ControlButtonAppearance.Select(MuteControlButton.ShowsMutedIcon(false), "mute", "voice"));
    }
}
