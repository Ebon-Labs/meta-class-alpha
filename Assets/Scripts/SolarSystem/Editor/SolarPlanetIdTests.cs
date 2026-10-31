using System;
using NUnit.Framework;

public class SolarPlanetIdTests
{
    [Test]
    public void Catalog_CoversMercuryThroughNeptune()
    {
        SolarPlanetId[] ids = (SolarPlanetId[])Enum.GetValues(typeof(SolarPlanetId));
        Assert.AreEqual(8, ids.Length);
        Assert.AreEqual(SolarPlanetId.Mercury, ids[0]);
        Assert.AreEqual(SolarPlanetId.Neptune, ids[ids.Length - 1]);
    }
}
