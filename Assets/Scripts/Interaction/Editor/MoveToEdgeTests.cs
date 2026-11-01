using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class MoveToEdgeTests
{
    [Test]
    public void DefaultDuration_IsSevenSeconds()
    {
        Assert.AreEqual(7f, MoveToEdge.DefaultDuration);
    }

    [Test]
    public void HoldKinematic_StaysKinematicWhileTraveling()
    {
        Assert.IsTrue(MoveToEdge.HoldKinematic(traveling: true, wasKinematic: false));
        Assert.IsFalse(MoveToEdge.HoldKinematic(traveling: false, wasKinematic: false));
        Assert.IsTrue(MoveToEdge.HoldKinematic(traveling: false, wasKinematic: true));
    }

    [Test]
    public void AtOrigin_MatchesTheCapturedPosition()
    {
        Vector3 origin = new Vector3(1f, 2f, 3f);

        Assert.IsTrue(MoveToEdge.IsAtOrigin(origin, origin, MoveToEdge.OriginTolerance));
        Assert.IsFalse(MoveToEdge.IsAtOrigin(origin + new Vector3(1f, 0f, 0f), origin, MoveToEdge.OriginTolerance));
    }

    [Test]
    public void Available_OnlyAwayFromOrigin()
    {
        Assert.IsFalse(MoveToEdge.IsAvailable(atOrigin: true));
        Assert.IsTrue(MoveToEdge.IsAvailable(atOrigin: false));
    }

    [Test]
    public void Radius_IsHalfTheWidestSide()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(4f, 10f, 2f));

        Assert.AreEqual(5f, MoveToEdge.Radius(bounds), 0.0001f);
    }

    [Test]
    public void Landing_ParksAtHeightEight()
    {
        Vector3 above = MoveToEdge.Landing(Vector3.zero, 5f, new Vector3(0f, 3f, 4f));
        Vector3 below = MoveToEdge.Landing(Vector3.zero, 5f, new Vector3(0f, -3f, 4f));

        Assert.AreEqual(8f, MoveToEdge.AstroParkHeight, 0.0001f);
        Assert.AreEqual(0f, above.x, 0.0001f);
        Assert.AreEqual(8f, above.y, 0.0001f);
        Assert.AreEqual(4f, above.z, 0.0001f);
        Assert.AreEqual(0f, below.x, 0.0001f);
        Assert.AreEqual(8f, below.y, 0.0001f);
        Assert.AreEqual(4f, below.z, 0.0001f);
    }

    [Test]
    public void Center_IsTheObjectTaggedClassCenter()
    {
        GameObject center = new GameObject("Class Center");
        try
        {
            center.tag = MoveToEdge.CenterTag;

            Assert.AreEqual(center.transform, MoveToEdge.FindCenter());
        }
        finally
        {
            Object.DestroyImmediate(center);
        }
    }
}
