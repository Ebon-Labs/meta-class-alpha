using NUnit.Framework;

public class PlanetLabelTests
{
    [Test]
    public void OverrideText_WinsOverTheProfile()
    {
        Assert.AreEqual("Lua", PlanetLabel.ResolveLabelText("Lua", "Earth", "Earth", true));
    }

    [Test]
    public void OrbitingProfile_WinsOverASpinningParent()
    {
        Assert.AreEqual("Earth", PlanetLabel.ResolveLabelText("  ", "Earth", "Moon", true));
    }

    [Test]
    public void FallbackProfile_IsUsedWhenNothingIsOrbiting()
    {
        Assert.AreEqual("Moon", PlanetLabel.ResolveLabelText(null, null, "Moon", true));
    }

    [Test]
    public void MissingProfile_LeavesTheExistingLabelAlone()
    {
        Assert.IsNull(PlanetLabel.ResolveLabelText("", null, null, false));
    }
}
