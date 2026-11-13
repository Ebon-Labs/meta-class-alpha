using NUnit.Framework;
using UnityEngine;

public class CameraEditorControllerTests
{
    [Test]
    public void LocalLook_BuildsTheEulerAnglesAssignedToThePlayer()
    {
        float yaw = 0f;
        float pitch = 0f;
        EditorMouseLook.Accumulate(ref yaw, ref pitch, mouseX: 1f, mouseY: -1f, sensitivity: 10f);
        Vector3 euler = EditorMouseLook.Euler(yaw, pitch);

        Assert.AreEqual(10f, euler.x, 0.0001f);
        Assert.AreEqual(10f, euler.y, 0.0001f);
        Assert.AreEqual(0f, euler.z, 0.0001f);
    }

    [Test]
    public void ClampPitch_StopsAtFortyFiveUpAndDown()
    {
        Assert.AreEqual(20f, CameraEditorController.ClampPitch(20f), 0.001f);
        Assert.AreEqual(45f, CameraEditorController.ClampPitch(80f), 0.001f);
        Assert.AreEqual(-45f, CameraEditorController.ClampPitch(-75f), 0.001f);

        Quaternion look = Quaternion.Euler(80f, 25f, 0f);
        Quaternion clamped = CameraEditorController.ClampLookPitch(look);

        Assert.AreEqual(45f, AvatarHeadYaw.LookPitch(clamped), 0.05f);
        Assert.AreEqual(25f, AvatarHeadYaw.LookYaw(clamped), 0.05f);

        Quaternion tilted = Quaternion.Euler(80f, 25f, 12f);
        Quaternion kept = CameraEditorController.ClampLookPitch(tilted);
        Assert.AreEqual(12f, AvatarHeadYaw.LookRoll(kept), 0.05f);
        Assert.Less(Quaternion.Angle(CameraEditorController.ClampLookPitch(Quaternion.Euler(20f, 10f, 0f)), Quaternion.Euler(20f, 10f, 0f)), 0.05f);
    }

    [Test]
    public void Menu_RestoresTheCommonClick()
    {
        Assert.IsTrue(CameraEditorController.ShowsCommonClick(CameraEditorController.MenuScene));
        Assert.IsFalse(CameraEditorController.ShowsCommonClick("Tutorial"));
        Assert.IsFalse(CameraEditorController.ShowsCommonClick("SolarSystem"));
    }
}
