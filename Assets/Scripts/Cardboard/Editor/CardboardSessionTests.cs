using NUnit.Framework;
using UnityEngine;

public class CardboardSessionTests
{
    [Test]
    public void ViewerScenes_AreTheClassroomScenes()
    {
        Assert.IsTrue(CardboardSession.UsesViewer("SolarSystem"));
        Assert.IsTrue(CardboardSession.UsesViewer("Tutorial"));
        Assert.IsFalse(CardboardSession.UsesViewer("Menu"));
    }

    [Test]
    public void DefaultViewer_IsStoredOnlyWhenParamsAreMissing()
    {
        Assert.IsTrue(CardboardSession.ShouldStoreDefault(hasParams: false));
        Assert.IsFalse(CardboardSession.ShouldStoreDefault(hasParams: true));
        StringAssert.StartsWith("https://google.com/cardboard/cfg?p=", CardboardSession.DefaultViewerUri);
    }

    [Test]
    public void RestoreFlatCamera_PutsTheMenuCameraBack()
    {
        var host = new GameObject("camera");
        Camera camera = host.AddComponent<Camera>();
        camera.orthographic = false;
        camera.fieldOfView = 20f;

        CardboardSession.RestoreFlatCamera(camera, true, CardboardSession.MenuOrthographicSize, CardboardSession.MenuFieldOfView);

        Assert.IsTrue(camera.orthographic);
        Assert.AreEqual(CardboardSession.MenuOrthographicSize, camera.orthographicSize);
        Assert.AreEqual(CardboardSession.MenuFieldOfView, camera.fieldOfView);
        Object.DestroyImmediate(host);
    }
}
