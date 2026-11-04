using NUnit.Framework;
using UnityEngine;

public class LogoBounceTests
{
    [Test]
    public void Offset_StartsAtRestAndPeaksAtHalfThePeriod()
    {
        Assert.AreEqual(0f, LogoBounce.Offset(0f, LogoBounce.Amplitude, LogoBounce.Period), 0.0001f);
        Assert.AreEqual(LogoBounce.Amplitude, LogoBounce.Offset(LogoBounce.Period * 0.5f, LogoBounce.Amplitude, LogoBounce.Period), 0.0001f);
        Assert.AreEqual(0f, LogoBounce.Offset(LogoBounce.Period, LogoBounce.Amplitude, LogoBounce.Period), 0.0001f);
    }

    [Test]
    public void Offset_IsHalfwayUpAtAQuarterPeriod()
    {
        float offset = LogoBounce.Offset(LogoBounce.Period * 0.25f, LogoBounce.Amplitude, LogoBounce.Period);

        Assert.AreEqual(LogoBounce.Amplitude * 0.5f, offset, 0.0001f);
    }

    [Test]
    public void Place_LiftsOnlyTheVerticalAxis()
    {
        Vector2 place = LogoBounce.Place(new Vector2(1.5f, 2.57f), 0.35f);

        Assert.AreEqual(1.5f, place.x, 0.0001f);
        Assert.AreEqual(2.92f, place.y, 0.0001f);
    }
}
