using NUnit.Framework;

public class SessionExitButtonTests
{
    [Test]
    public void Shows_OnEverySceneExceptTheMenu()
    {
        Assert.IsFalse(SessionExitButton.Shows(MenuReturn.MenuScene));
        Assert.IsFalse(SessionExitButton.Shows(""));
        Assert.IsFalse(SessionExitButton.Shows(null));
        Assert.IsTrue(SessionExitButton.Shows("Tutorial"));
        Assert.IsTrue(SessionExitButton.Shows("SolarSystem"));
    }

    [Test]
    public void VisibleTo_IncludesTheGuest()
    {
        Assert.IsTrue(SessionExitButton.VisibleTo("SolarSystem", isHost: false));
        Assert.IsTrue(SessionExitButton.VisibleTo("Tutorial", isHost: false));
        Assert.IsTrue(SessionExitButton.VisibleTo("SolarSystem", isHost: true));
        Assert.IsFalse(SessionExitButton.VisibleTo(MenuReturn.MenuScene, isHost: false));
    }

    [Test]
    public void Label_SaysSair()
    {
        Assert.AreEqual("SAIR", SessionExitButton.Label);
    }
}
