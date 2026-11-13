using NUnit.Framework;
using UnityEngine;

public class ImageMenuThumbTests
{
    [Test]
    public void CanStartHost_MatchesTheIdleHudButton()
    {
        Assert.IsTrue(ImageMenuThumb.CanStartHost(false, false, false, false));
    }

    [Test]
    public void CanStartHost_StaysQuietWhenASessionExistsOrWebGlCannotHost()
    {
        Assert.IsFalse(ImageMenuThumb.CanStartHost(true, false, false, false));
        Assert.IsFalse(ImageMenuThumb.CanStartHost(false, true, false, false));
        Assert.IsFalse(ImageMenuThumb.CanStartHost(false, false, true, false));
        Assert.IsFalse(ImageMenuThumb.CanStartHost(false, false, false, true));
    }

    [Test]
    public void FacesCamera_AcceptsTheSideTurnedTowardTheLens()
    {
        Assert.IsTrue(ImageMenuThumb.FacesCamera(Vector3.forward, Vector3.forward));
        Assert.IsFalse(ImageMenuThumb.FacesCamera(Vector3.right, Vector3.forward));
    }

    [Test]
    public void OrthoPixels_MapsTheThumbOntoTheGameView()
    {
        Vector2 pixels = ImageMenuThumb.OrthoPixels(2.1f, 2.16f, 5f, 1080f);

        Assert.Greater(pixels.x, 200f);
        Assert.Greater(pixels.y, 200f);
        Assert.IsTrue(ImageMenuThumb.CoversScreen(Vector2.zero, pixels));
    }

    [Test]
    public void Contains_AcceptsAPointInsideTheThumbAndRejectsOneOutside()
    {
        Vector2 min = new Vector2(10f, 20f);
        Vector2 max = new Vector2(40f, 50f);

        Assert.IsTrue(ImageMenuThumb.Contains(new Vector2(25f, 35f), min, max, 0f));
        Assert.IsFalse(ImageMenuThumb.Contains(new Vector2(5f, 35f), min, max, 0f));
        Assert.IsTrue(ImageMenuThumb.Contains(new Vector2(5f, 35f), min, max, ImageMenuThumb.HitPadding));
    }
}
