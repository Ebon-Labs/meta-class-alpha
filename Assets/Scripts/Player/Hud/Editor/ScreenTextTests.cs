using NUnit.Framework;

public class ScreenTextTests
{
    [Test]
    public void DefaultLanguage_IsPortugueseBrazil()
    {
        Assert.AreEqual(ScreenText.PortugueseBrazil, "pt-BR");
        Assert.AreEqual(ScreenText.PortugueseBrazil, ScreenText.Language);
    }

    [Test]
    public void GazePhrases_UseThePortugueseCatalog()
    {
        Assert.AreEqual("Planeta: Terra", ScreenText.Format("gaze.planet", ScreenText.Name("Earth")));
        Assert.AreEqual("Clique em: Portal", ScreenText.Format("gaze.click", ScreenText.Name("Portal")));
    }

    [Test]
    public void KnownBodies_UsePortugueseNames()
    {
        Assert.AreEqual("Mercúrio", ScreenText.Name("Mercury"));
        Assert.AreEqual("Vênus", ScreenText.Name("Venus"));
        Assert.AreEqual("Terra", ScreenText.Name("Earth"));
        Assert.AreEqual("Marte", ScreenText.Name("Mars"));
        Assert.AreEqual("Júpiter", ScreenText.Name("Jupiter"));
        Assert.AreEqual("Saturno", ScreenText.Name("Saturn"));
        Assert.AreEqual("Urano", ScreenText.Name("Uranus"));
        Assert.AreEqual("Netuno", ScreenText.Name("Neptune"));
        Assert.AreEqual("Lua", ScreenText.Name("Moon"));
        Assert.AreEqual("Sol", ScreenText.Name("Sun"));
    }

    [Test]
    public void UnknownWords_StayAsWritten()
    {
        Assert.AreEqual("Portal", ScreenText.Name(" Portal "));
        Assert.AreEqual("", ScreenText.Name(null));
        Assert.AreEqual("", ScreenText.Name("  "));
    }

    [Test]
    public void MissingLanguage_FallsBackToPortuguese()
    {
        Assert.AreEqual("Planeta: {0}", ScreenText.Phrase("gaze.planet", "en"));
        Assert.AreEqual("gaze.missing", ScreenText.Phrase("gaze.missing", ScreenText.PortugueseBrazil));
    }
}
