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
    public void OnFloor_KeepsThePlayerOnTheSameFloorPointAfterTheYaw()
    {
        Transform room = new GameObject("Class").transform;
        try
        {
            Vector3 standing = new Vector3(0f, 2f, 5f);
            Vector3 point = ClassController.FloorPoint(room, standing);
            room.localRotation = ClassController.TurnAroundOwnY(Quaternion.identity, ClassController.ClockwiseStepDegrees);
            Vector3 stuck = ClassController.OnFloor(room, point);
            float along = 5f * Mathf.Sin(45f * Mathf.Deg2Rad);

            Assert.AreEqual(along, stuck.x, 0.001f);
            Assert.AreEqual(2f, stuck.y, 0.001f);
            Assert.AreEqual(along, stuck.z, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(room.gameObject);
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
}
