using NUnit.Framework;

public class MainScreenTests
{
    [Test]
    public void FadeDuration_HasAMinimum()
    {
        Assert.AreEqual(0.01f, MainScreen.ClampFadeDuration(0f), 0.0001f);
        Assert.AreEqual(0.01f, MainScreen.ClampFadeDuration(-2f), 0.0001f);
        Assert.AreEqual(3f, MainScreen.ClampFadeDuration(3f), 0.0001f);
    }

    [Test]
    public void FadeAlpha_GoesFromZeroToOne()
    {
        Assert.AreEqual(0f, MainScreen.FadeAlpha(0f, 3f), 0.0001f);
        Assert.AreEqual(0.5f, MainScreen.FadeAlpha(1.5f, 3f), 0.0001f);
        Assert.AreEqual(1f, MainScreen.FadeAlpha(3f, 3f), 0.0001f);
        Assert.AreEqual(1f, MainScreen.FadeAlpha(9f, 3f), 0.0001f);
    }

    [Test]
    public void TintProperty_PrefersTintThenTintColorThenColor()
    {
        Assert.AreEqual(MainScreen.TintPropertyKind.Tint, MainScreen.ResolveTintKind(true, true));
        Assert.AreEqual(MainScreen.TintPropertyKind.TintColor, MainScreen.ResolveTintKind(false, true));
        Assert.AreEqual(MainScreen.TintPropertyKind.Color, MainScreen.ResolveTintKind(false, false));
    }
}
