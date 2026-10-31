using NUnit.Framework;
using UnityEngine;

public class EditorMouseLookTests
{
    [Test]
    public void Accumulate_YawFollowsMouseX_AndPitchInvertsMouseY()
    {
        float yaw = 10f;
        float pitch = 5f;
        EditorMouseLook.Accumulate(ref yaw, ref pitch, mouseX: 2f, mouseY: 3f, sensitivity: 4f);

        Assert.AreEqual(18f, yaw, 0.0001f);
        Assert.AreEqual(-7f, pitch, 0.0001f);
    }

    [Test]
    public void Euler_MapsPitchToX_AndYawToY()
    {
        Vector3 euler = EditorMouseLook.Euler(yaw: 30f, pitch: 12f);

        Assert.AreEqual(12f, euler.x, 0.0001f);
        Assert.AreEqual(30f, euler.y, 0.0001f);
        Assert.AreEqual(0f, euler.z, 0.0001f);
    }
}
