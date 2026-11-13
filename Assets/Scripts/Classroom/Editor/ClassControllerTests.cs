using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class ClassControllerTests
{
    [Test]
    public void StandAboveBounds_UsesTheTopOfTheSurfacePlusEyeHeight()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));
        Vector3 stand = ClassController.StandAboveBounds(bounds, 1.6f);

        Assert.AreEqual(0f, stand.x, 0.0001f);
        Assert.AreEqual(2.6f, stand.y, 0.0001f);
        Assert.AreEqual(0f, stand.z, 0.0001f);
    }

    [Test]
    public void StandAboveAnchor_RisesOnWorldUp()
    {
        Vector3 stand = ClassController.StandAboveAnchor(new Vector3(1f, 2f, 3f), 1.6f);

        Assert.AreEqual(1f, stand.x, 0.0001f);
        Assert.AreEqual(3.6f, stand.y, 0.0001f);
        Assert.AreEqual(3f, stand.z, 0.0001f);
    }

    [Test]
    public void SettingChanges_IgnoresTheSameValueUnlessForced()
    {
        Assert.IsFalse(ClassController.SettingChanges(force: false, applied: true, current: true));
        Assert.IsTrue(ClassController.SettingChanges(force: false, applied: false, current: true));
        Assert.IsTrue(ClassController.SettingChanges(force: true, applied: true, current: true));
    }

    [Test]
    public void ResolveExisting_UsesTheLocatedControllerWhenThereIsNoInstance()
    {
        Assert.IsNull(ClassController.ResolveExisting(null));
    }

    [Test]
    public void TurnAroundOwnY_SpinsFortyFiveDegreesOnLocalUp()
    {
        Vector3 facing = ClassController.TurnAroundOwnY(Quaternion.identity, ClassController.ClockwiseStepDegrees) * Vector3.forward;
        float axis = Mathf.Sin(45f * Mathf.Deg2Rad);

        Assert.AreEqual(45f, ClassController.ClockwiseStepDegrees);
        Assert.AreEqual(axis, facing.x, 0.0001f);
        Assert.AreEqual(0f, facing.y, 0.0001f);
        Assert.AreEqual(axis, facing.z, 0.0001f);
    }

    [Test]
    public void TurnStep_ReachesTheStepInThreeSeconds()
    {
        Quaternion from = Quaternion.identity;
        Quaternion to = ClassController.TurnAroundOwnY(from, ClassController.ClockwiseStepDegrees);
        float axis = Mathf.Sin(45f * Mathf.Deg2Rad);

        Assert.AreEqual(3f, ClassController.TurnDuration);
        Assert.AreEqual(0f, Quaternion.Angle(ClassController.TurnStep(from, to, 0f, ClassController.TurnDuration), from), 0.001f);

        Vector3 facing = ClassController.TurnStep(from, to, ClassController.TurnDuration, ClassController.TurnDuration) * Vector3.forward;
        Assert.AreEqual(axis, facing.x, 0.001f);
        Assert.AreEqual(0f, facing.y, 0.001f);
        Assert.AreEqual(axis, facing.z, 0.001f);
    }

    [Test]
    public void Orbit_TurnsAroundTheHostAreaAndLeavesThatPointStill()
    {
        Vector3 pivot = new Vector3(2f, 1f, 3f);
        Quaternion yaw = ClassController.ClockwiseYaw(ClassController.ClockwiseStepDegrees);
        Vector3 still = ClassController.Orbit(pivot, pivot, yaw);

        Assert.AreEqual(pivot.x, still.x, 0.001f);
        Assert.AreEqual(pivot.y, still.y, 0.001f);
        Assert.AreEqual(pivot.z, still.z, 0.001f);

        Vector3 room = pivot + new Vector3(0f, 0f, 4f);
        Vector3 swung = ClassController.Orbit(room, pivot, yaw);
        float along = 4f * Mathf.Sin(45f * Mathf.Deg2Rad);

        Assert.AreEqual(pivot.x + along, swung.x, 0.001f);
        Assert.AreEqual(pivot.y, swung.y, 0.001f);
        Assert.AreEqual(pivot.z + along, swung.z, 0.001f);
    }

    [Test]
    public void TutorialWithoutAnArea_StandsOnTheGround()
    {
        Transform ground = new GameObject(ClassController.GroundName).transform;
        Transform host = new GameObject("HostArea").transform;
        try
        {
            Assert.AreEqual(ground, ClassController.AreaForRoleOrGround(isHost: true, null, null, ClassController.TutorialScene, ground));
            Assert.AreEqual(host, ClassController.AreaForRoleOrGround(isHost: true, host, null, ClassController.TutorialScene, ground));
            Assert.IsNull(ClassController.AreaForRoleOrGround(isHost: true, null, null, "SolarSystem", ground));
        }
        finally
        {
            Object.DestroyImmediate(ground.gameObject);
            Object.DestroyImmediate(host.gameObject);
        }
    }

    [Test]
    public void HostGoesToHostAreaAndClientGoesToGuestArea()
    {
        Transform host = new GameObject("HostArea").transform;
        Transform guest = new GameObject("GuestArea").transform;
        try
        {
            Assert.AreEqual(host, ClassController.AreaForRole(isHost: true, host, guest));
            Assert.AreEqual(guest, ClassController.AreaForRole(isHost: false, host, guest));
        }
        finally
        {
            Object.DestroyImmediate(host.gameObject);
            Object.DestroyImmediate(guest.gameObject);
        }
    }

    [Test]
    public void AnimateSingleStep_OnlyTheNextTurnPlaysTheMotion()
    {
        Assert.IsTrue(ClassController.AnimateSingleStep(0, 1));
        Assert.IsFalse(ClassController.AnimateSingleStep(0, 4));
        Assert.IsFalse(ClassController.AnimateSingleStep(2, 2));
    }
}
