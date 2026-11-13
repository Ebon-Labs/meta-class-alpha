using NUnit.Framework;
using UnityEngine;

public class ZBounceTests
{
    [Test]
    public void Offset_StartsAtRestAndPeaksAtHalfThePeriod()
    {
        Assert.AreEqual(0f, ZBounce.Offset(0f, ZBounce.Amplitude, ZBounce.Period), 0.0001f);
        Assert.AreEqual(ZBounce.Amplitude, ZBounce.Offset(ZBounce.Period * 0.5f, ZBounce.Amplitude, ZBounce.Period), 0.0001f);
        Assert.AreEqual(0f, ZBounce.Offset(ZBounce.Period, ZBounce.Amplitude, ZBounce.Period), 0.0001f);
    }

    [Test]
    public void ShownElapsed_RestsUntilTheObjectStaysVisible()
    {
        Assert.AreEqual(0f, ZBounce.ShownElapsed(0.4f, 0.02f, shown: false, wasShown: true), 0.0001f);
        Assert.AreEqual(0f, ZBounce.ShownElapsed(0f, 0.02f, shown: true, wasShown: false), 0.0001f);
    }

    [Test]
    public void ShownElapsed_KeepsRunningWhileVisible()
    {
        float elapsed = ZBounce.ShownElapsed(0.5f, 0.1f, shown: true, wasShown: true);

        Assert.AreEqual(0.6f, elapsed, 0.0001f);
    }

    [Test]
    public void Place_MovesOnlyTheZAxis()
    {
        Vector3 place = ZBounce.Place(new Vector3(0.34f, 0.21f, 3.36f), ZBounce.Amplitude);

        Assert.AreEqual(0.34f, place.x, 0.0001f);
        Assert.AreEqual(0.21f, place.y, 0.0001f);
        Assert.AreEqual(4.36f, place.z, 0.0001f);
    }

    [Test]
    public void IsShown_RequiresAnEnabledVisibleRenderer()
    {
        Assert.IsTrue(ZBounce.IsShown(active: true, rendererEnabled: true, rendererVisible: true));
        Assert.IsFalse(ZBounce.IsShown(active: true, rendererEnabled: true, rendererVisible: false));
        Assert.IsFalse(ZBounce.IsShown(active: true, rendererEnabled: false, rendererVisible: true));
    }
}
