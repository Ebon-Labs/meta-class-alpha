using NUnit.Framework;
using UnityEngine;

public class AvatarHeadYawTests
{
    [Test]
    public void ClampYaw_StopsAtFortyFiveDegreesEachWay()
    {
        Assert.AreEqual(0f, AvatarHeadYaw.ClampYaw(0f, AvatarHeadYaw.LimitDegrees), 0.001f);
        Assert.AreEqual(30f, AvatarHeadYaw.ClampYaw(30f, AvatarHeadYaw.LimitDegrees), 0.001f);
        Assert.AreEqual(45f, AvatarHeadYaw.ClampYaw(90f, AvatarHeadYaw.LimitDegrees), 0.001f);
        Assert.AreEqual(-45f, AvatarHeadYaw.ClampYaw(-80f, AvatarHeadYaw.LimitDegrees), 0.001f);
    }

    [Test]
    public void Follow_KeepsTheHeadInsideFortyFiveAndTurnsTheBodyPastIt()
    {
        AvatarHeadYaw.YawPose inside = AvatarHeadYaw.Follow(0f, 30f, AvatarHeadYaw.LimitDegrees);
        Assert.AreEqual(0f, inside.Body, 0.001f);
        Assert.AreEqual(30f, inside.Head, 0.001f);

        AvatarHeadYaw.YawPose past = AvatarHeadYaw.Follow(0f, 90f, AvatarHeadYaw.LimitDegrees);
        Assert.AreEqual(45f, past.Body, 0.001f);
        Assert.AreEqual(45f, past.Head, 0.001f);

        AvatarHeadYaw.YawPose held = AvatarHeadYaw.Follow(past.Body, 20f, AvatarHeadYaw.LimitDegrees);
        Assert.AreEqual(45f, held.Body, 0.001f);
        Assert.AreEqual(-25f, held.Head, 0.001f);

        AvatarHeadYaw.YawPose opposite = AvatarHeadYaw.Follow(45f, -40f, AvatarHeadYaw.LimitDegrees);
        Assert.AreEqual(5f, opposite.Body, 0.001f);
        Assert.AreEqual(-45f, opposite.Head, 0.001f);
    }

    [Test]
    public void LookYaw_UsesOnlyTheHorizontalTurn()
    {
        float level = AvatarHeadYaw.LookYaw(Quaternion.Euler(0f, 35f, 0f));
        float pitched = AvatarHeadYaw.LookYaw(Quaternion.Euler(60f, 35f, 0f));

        Assert.AreEqual(35f, level, 0.05f);
        Assert.AreEqual(level, pitched, 0.05f);
    }

    [Test]
    public void WithYaw_TurnsAroundThePlayerUpAxis()
    {
        Quaternion rest = Quaternion.Euler(10f, 0f, 0f);
        Vector3 axis = Vector3.right;
        Quaternion posed = AvatarHeadYaw.WithYaw(rest, 45f, axis);
        Quaternion expected = Quaternion.AngleAxis(45f, axis) * rest;

        Assert.Less(Quaternion.Angle(posed, expected), 0.05f);
        Assert.Greater(Quaternion.Angle(posed, Quaternion.AngleAxis(45f, Vector3.up) * rest), 20f);
    }

    [Test]
    public void UpAxis_FollowsThePlayerVerticalInTheBoneParent()
    {
        var player = new GameObject("Player");
        var neck = new GameObject("neck");
        neck.transform.SetParent(player.transform, false);
        neck.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        try
        {
            Vector3 axis = AvatarHeadYaw.UpAxis(neck.transform, player.transform);
            Assert.Less(Vector3.Angle(axis, Vector3.right), 0.5f);
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void FindBone_SkipsTheCameraHead()
    {
        var root = new GameObject("Player");
        var cameraHead = new GameObject("Head");
        var avatar = new GameObject(AvatarHeadYaw.BodyName);
        var bone = new GameObject(AvatarHeadYaw.BoneName);
        cameraHead.transform.SetParent(root.transform, false);
        avatar.transform.SetParent(root.transform, false);
        bone.transform.SetParent(avatar.transform, false);
        cameraHead.AddComponent<Camera>();
        try
        {
            Assert.AreEqual(bone.transform, AvatarHeadYaw.FindBone(root.transform, AvatarHeadYaw.BoneName));
            Assert.AreEqual(avatar.transform, AvatarHeadYaw.FindBody(root.transform, bone.transform));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
