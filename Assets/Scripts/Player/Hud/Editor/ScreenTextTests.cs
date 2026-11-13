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
    public void InteractiveControls_UsePortugueseNames()
    {
        Assert.AreEqual("Vênus", ScreenText.Name("VenusButton"));
        Assert.AreEqual("Júpiter", ScreenText.Name("JupterButton"));
        Assert.AreEqual("Voltar", ScreenText.Name("BackButton"));
        Assert.AreEqual("Silenciar", ScreenText.Name("MuteButton"));
        Assert.AreEqual("Órbita", ScreenText.Name("OrbitButton"));
        Assert.AreEqual("Distância", ScreenText.Name("DistanceButton"));
        Assert.AreEqual("Marcadores", ScreenText.Name("MarkerButton"));
        Assert.AreEqual("Chão", ScreenText.Name("GroundButton"));
        Assert.AreEqual("Reproduzir", ScreenText.Name("PlayButton"));
        Assert.AreEqual("Pausar", ScreenText.Name("PauseButton"));
        Assert.AreEqual("Parar", ScreenText.Name("StopButton"));
        Assert.AreEqual("Voltar à sala", ScreenText.Name("GoHomeOrb"));
        Assert.AreEqual("Marcador", ScreenText.Name("InteractionMarkerOrb"));
        Assert.AreEqual("1", ScreenText.Name("PortalOne"));
        Assert.AreEqual("2", ScreenText.Name("PortalTwo"));
        Assert.AreEqual("3", ScreenText.Name("PortalThree"));
        Assert.AreEqual("Clique em: Sol", ScreenText.Format("gaze.click", ScreenText.Name("SunButton")));
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
