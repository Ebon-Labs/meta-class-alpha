using NUnit.Framework;

public class PointerLogTests
{
    [Test]
    public void PlanetParent_UsesTheParentName()
    {
        Assert.AreEqual("Planeta: Earth", PointerLog.FocusName("Portal", "Earth", true));
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
