using NUnit.Framework;

public class PointerLogTests
{
    [Test]
    public void PlanetParent_UsesTheParentName()
    {
        Assert.AreEqual("Planeta: Terra", PointerLog.FocusName("Portal", "Earth", true));
    }

    [Test]
    public void SunAndMoon_OmitThePlanetPrefix()
    {
        Assert.AreEqual("Sol", PointerLog.FocusName("Button", "Sun", true));
        Assert.AreEqual("Lua", PointerLog.FocusName("Button", "Moon", true));
        Assert.AreEqual("Sol", PointerLog.FocusName("Button", "SUN", true));
    }

    [Test]
    public void OtherTargets_UseTheirOwnName()
    {
        Assert.AreEqual("Treasure", PointerLog.FocusName("Treasure", "Interactive", false));
        Assert.AreEqual("", PointerLog.FocusName(null, null, false));
    }

    [Test]
    public void Click_PrefixesTheTargetName()
    {
        Assert.AreEqual("Clique em: Portal", PointerLog.Click("Portal"));
    }
}
