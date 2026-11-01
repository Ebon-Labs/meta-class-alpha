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
