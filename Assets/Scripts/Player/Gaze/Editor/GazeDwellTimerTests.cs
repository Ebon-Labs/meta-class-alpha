using NUnit.Framework;

public class GazeDwellTimerTests
{
    [Test]
    public void Advance_AddsDeltaTime()
    {
        Assert.AreEqual(1.5f, GazeDwellTimer.Advance(1f, 0.5f), 0.0001f);
    }

    [Test]
    public void ThreeSeconds_CompletesAndFillsTheSlider()
    {
        float counter = 0f;
        counter = GazeDwellTimer.Advance(counter, 3f);

        Assert.IsTrue(GazeDwellTimer.IsComplete(counter, 3f));
        Assert.AreEqual(1f, GazeDwellTimer.SliderFraction(counter, 3f), 0.0001f);
    }

    [Test]
    public void ZeroClickTime_DoesNotDivide()
    {
        Assert.AreEqual(0f, GazeDwellTimer.SliderFraction(1f, 0f), 0.0001f);
    }

    [Test]
    public void Loader_ShowsOnlyWhileFocusedOnABasicObject()
    {
        Assert.IsTrue(GazeDwellTimer.ShowLoader(true, true));
        Assert.IsFalse(GazeDwellTimer.ShowLoader(true, false));
        Assert.IsFalse(GazeDwellTimer.ShowLoader(false, true));
    }
}
