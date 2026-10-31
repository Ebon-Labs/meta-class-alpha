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
}
