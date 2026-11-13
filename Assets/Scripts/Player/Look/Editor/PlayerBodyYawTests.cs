using NUnit.Framework;
using UnityEngine;

public class PlayerBodyYawTests
{
    [Test]
    public void Split_KeepsOnlyYawOnTheBody()
    {
        Quaternion look = Quaternion.Euler(30f, 80f, 0f);

        PlayerBodyYaw.Split(look, out Quaternion bodyYaw, out Quaternion headPitch);

        Vector3 body = bodyYaw.eulerAngles;
        Assert.AreEqual(0f, Mathf.DeltaAngle(0f, body.x), 0.05f);
        Assert.AreEqual(0f, Mathf.DeltaAngle(80f, body.y), 0.05f);
        Assert.AreEqual(0f, Mathf.DeltaAngle(0f, body.z), 0.05f);
        Assert.Less(Quaternion.Angle(bodyYaw * headPitch, look), 0.05f);
    }
}
