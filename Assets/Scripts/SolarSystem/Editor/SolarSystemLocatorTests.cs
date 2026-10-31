using NUnit.Framework;

public class SolarSystemLocatorTests
{
    [Test]
    public void KnownSunNames_AreRecognized()
    {
        Assert.IsTrue(SolarSystemLocator.IsKnownSunName("Sun Sphere"));
        Assert.IsTrue(SolarSystemLocator.IsKnownSunName("sun"));
        Assert.IsTrue(SolarSystemLocator.IsKnownSunName("SUN"));
        Assert.IsFalse(SolarSystemLocator.IsKnownSunName("Sphere.009"));
        Assert.IsFalse(SolarSystemLocator.IsKnownSunName(null));
    }

    [Test]
    public void MissingTransform_HasNoRadius()
    {
        Assert.AreEqual(0f, SolarSystemLocator.EstimateRadius(null), 0.0001f);
    }
}
