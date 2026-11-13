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
    public void LookPitch_UsesOnlyTheVerticalTurnWithinTheCameraLimits()
    {
        Assert.AreEqual(30f, AvatarHeadYaw.LookPitch(Quaternion.Euler(30f, 0f, 0f)), 0.05f);
        Assert.AreEqual(-20f, AvatarHeadYaw.LookPitch(Quaternion.Euler(-20f, 40f, 0f)), 0.05f);
        Assert.AreEqual(45f, CameraEditorController.ClampPitch(80f), 0.001f);
        Assert.AreEqual(-45f, CameraEditorController.ClampPitch(-70f), 0.001f);
    }

    [Test]
    public void LookRoll_UsesOnlyTheSideTiltAndStopsAtFortyFive()
    {
        Assert.AreEqual(25f, AvatarHeadYaw.LookRoll(Quaternion.Euler(0f, 0f, 25f)), 0.05f);
        Assert.AreEqual(-15f, AvatarHeadYaw.LookRoll(Quaternion.Euler(10f, 40f, -15f)), 0.05f);
        Assert.AreEqual(45f, AvatarHeadYaw.ClampYaw(70f, AvatarHeadYaw.LimitDegrees), 0.001f);
        Assert.AreEqual(-45f, AvatarHeadYaw.ClampYaw(-80f, AvatarHeadYaw.LimitDegrees), 0.001f);
    }

    [Test]
    public void Nod_LooksUpOnlyUp_AndDownOnlyDown()
    {
        Quaternion rest = Quaternion.identity;
        Vector3 localUp = Vector3.up;
        Vector3 localForward = Vector3.forward;

        Quaternion down = AvatarHeadYaw.Nod(rest, 30f, localUp, localForward);
        Vector3 lowered = down * localForward;
        Assert.Less(lowered.y, -0.2f);
        Assert.AreEqual(0f, lowered.x, 0.001f);
        Assert.AreEqual(0f, AvatarHeadYaw.LookYaw(down), 0.05f);

        Quaternion up = AvatarHeadYaw.Nod(rest, -30f, localUp, localForward);
        Vector3 raised = up * localForward;
        Assert.Greater(raised.y, 0.2f);
        Assert.AreEqual(0f, raised.x, 0.001f);
        Assert.AreEqual(0f, AvatarHeadYaw.LookYaw(up), 0.05f);
    }

    [Test]
    public void Nod_OnTheBoneAxes_StaysInTheUpForwardPlane()
    {
        Quaternion rest = Quaternion.identity;
        Vector3 localUp = Vector3.right;
        Vector3 localForward = -Vector3.up;
        Vector3 ear = AvatarHeadYaw.EarAxis(localUp, localForward);

        Vector3 lowered = AvatarHeadYaw.Nod(rest, 30f, localUp, localForward) * localForward;
        Assert.AreEqual(0f, Vector3.Dot(lowered, ear), 0.001f);
        Assert.Less(Vector3.Dot(lowered, localUp), -0.2f);

        Vector3 raised = AvatarHeadYaw.Nod(rest, -30f, localUp, localForward) * localForward;
        Assert.AreEqual(0f, Vector3.Dot(raised, ear), 0.001f);
        Assert.Greater(Vector3.Dot(raised, localUp), 0.2f);
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

    [Test]
    public void StopIndependentMotion_DisablesImportedClips()
    {
        var root = new GameObject("Player");
        var body = new GameObject("Avatar");
        body.transform.SetParent(root.transform, false);
        Animator animator = body.AddComponent<Animator>();
        Animation clip = body.AddComponent<Animation>();
        try
        {
            clip.playAutomatically = true;
            AvatarHeadYaw.StopIndependentMotion(root);
            Assert.IsFalse(animator.enabled);
            Assert.IsNull(animator.runtimeAnimatorController);
            Assert.IsFalse(clip.enabled);
            Assert.IsFalse(clip.playAutomatically);
            Assert.IsNull(clip.clip);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
