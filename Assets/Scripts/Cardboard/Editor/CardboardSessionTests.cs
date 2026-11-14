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
    public void Scan_OnlyWhenParamsAreMissingAndNotYetPrompted()
    {
        Assert.IsTrue(CardboardSession.ShouldScan(hasParams: false, alreadyPrompted: false));
        Assert.IsFalse(CardboardSession.ShouldScan(hasParams: true, alreadyPrompted: false));
        Assert.IsFalse(CardboardSession.ShouldScan(hasParams: false, alreadyPrompted: true));
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
