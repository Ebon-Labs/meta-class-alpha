using NUnit.Framework;

public class MuteControlButtonTests
{
    [Test]
    public void Icon_MatchesWhetherGuestsAreMuted()
    {
        Assert.IsTrue(MuteControlButton.ShowsMutedIcon(true));
        Assert.IsFalse(MuteControlButton.ShowsMutedIcon(false));
        Assert.AreEqual("mute", ControlButtonAppearance.Select(MuteControlButton.ShowsMutedIcon(true), "voice", "mute"));
    }
}
