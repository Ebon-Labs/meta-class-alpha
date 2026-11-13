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
    public void Label_SaysSair()
    {
        Assert.AreEqual("SAIR", SessionExitButton.Label);
    }
}
