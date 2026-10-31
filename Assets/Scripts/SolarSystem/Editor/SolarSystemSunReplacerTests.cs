using NUnit.Framework;

public class SolarSystemSunReplacerTests
{
    readonly string[] oldNames = { "SUN", "Sun", "Sphere.009" };

    [Test]
    public void OldSun_IsReplaceableWhenItIsASeparateObject()
    {
        Assert.IsTrue(SolarSystemSunReplacer.IsReplaceableSun("Sphere.009", oldNames, false, false, false));
        Assert.IsTrue(SolarSystemSunReplacer.IsReplaceableSun("sun", oldNames, false, false, false));
    }

    [Test]
    public void RootNewSunAndItsChildren_AreKept()
    {
        Assert.IsFalse(SolarSystemSunReplacer.IsReplaceableSun("SUN", oldNames, true, false, false));
        Assert.IsFalse(SolarSystemSunReplacer.IsReplaceableSun("SUN", oldNames, false, true, false));
        Assert.IsFalse(SolarSystemSunReplacer.IsReplaceableSun("SUN", oldNames, false, false, true));
        Assert.IsFalse(SolarSystemSunReplacer.IsReplaceableSun("Moon", oldNames, false, false, false));
    }
}
