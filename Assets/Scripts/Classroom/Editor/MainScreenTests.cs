using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

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

    [Test]
    public void Reveal_EnablesThePortalWhenTheVideoEnds()
    {
        GameObject portal = new GameObject("PortalOne");
        portal.SetActive(false);
        try
        {
            MainScreen.Reveal(portal);

            Assert.IsTrue(portal.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(portal);
        }
    }

    [Test]
    public void Reveal_IgnoresAMissingPortal()
    {
        MainScreen.Reveal(null);
    }

    [Test]
    public void ChoosePortal_UsesTheTutorialPortalWhenTheReferenceIsMissing()
    {
        GameObject portal = new GameObject(MainScreen.PortalOneName);
        try
        {
            Assert.AreEqual(portal, MainScreen.ChoosePortal(null, MainScreen.TutorialScene, portal));
            Assert.IsNull(MainScreen.ChoosePortal(null, "SolarSystem", portal));
        }
        finally
        {
            Object.DestroyImmediate(portal);
        }
    }

    [Test]
    public void PlaybackEnded_RequiresTheClipToReachTheEnd()
    {
        Assert.IsFalse(MainScreen.PlaybackEnded(false, false, false, 17, 17));
        Assert.IsFalse(MainScreen.PlaybackEnded(true, true, false, 17, 17));
        Assert.IsFalse(MainScreen.PlaybackEnded(true, false, true, 4, 17));
        Assert.IsTrue(MainScreen.PlaybackEnded(true, false, false, 16.9, 17));
    }
}
