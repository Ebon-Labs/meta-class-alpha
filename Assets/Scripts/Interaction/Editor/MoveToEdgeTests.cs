using NUnit.Framework;
using UnityEngine;

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
    public void RouteKey_UsesSiblingIndexAndParentNames()
    {
        var sun = new GameObject("Sun");
        var portal = new GameObject("Portal");
        portal.transform.SetParent(sun.transform, false);
        try
        {
            Assert.AreEqual("0:Sun/0:Portal", MoveToEdge.RouteKey(portal.transform));
        }
        finally
        {
            Object.DestroyImmediate(sun);
        }
    }

    [Test]
    public void Wrapper_IsThePlanetHolderOrTheSun()
    {
        Assert.IsTrue(MoveToEdge.IsWrapper("EarthWrapper"));
        Assert.IsTrue(MoveToEdge.IsWrapper("JupterWrapper"));
        Assert.IsTrue(MoveToEdge.IsWrapper("SunWrapper"));
        Assert.IsFalse(MoveToEdge.IsWrapper("Sun"));
        Assert.IsFalse(MoveToEdge.IsWrapper("Sol"));
        Assert.IsFalse(MoveToEdge.IsWrapper("Sun Light"));
        Assert.IsFalse(MoveToEdge.IsWrapper("InfoArea"));
        Assert.IsFalse(MoveToEdge.IsWrapper("SolarSystem"));
    }

    [Test]
    public void WrapperOf_UsesSunWrapperInsteadOfTheSunObject()
    {
        var sunWrapper = new GameObject("SunWrapper");
        var sun = new GameObject("Sun");
        var info = new GameObject("InfoArea");
        try
        {
            sun.transform.SetParent(sunWrapper.transform, false);
            info.transform.SetParent(sun.transform, false);

            Assert.AreSame(sunWrapper.transform, MoveToEdge.WrapperOf(info.transform));
        }
        finally
        {
            Object.DestroyImmediate(sunWrapper);
        }
    }

    [Test]
    public void WrapperOf_UsesTheNearestPlanetHolder()
    {
        var earth = new GameObject("EarthWrapper");
        var info = new GameObject("InfoArea");
        info.transform.SetParent(earth.transform, false);
        try
        {
            Assert.AreSame(earth.transform, MoveToEdge.WrapperOf(info.transform));
        }
        finally
        {
            Object.DestroyImmediate(earth);
        }
    }

    [Test]
    public void ReferenceOnEdge_PutsTheClassFaceOnTheWrapperFace()
    {
        var wrapper = new Bounds(Vector3.zero, new Vector3(20f, 20f, 20f));
        Vector3 from = new Vector3(100f, 0f, 0f);

        Vector3 reference = MoveToEdge.ReferenceOnEdge(wrapper, new Vector3(2f, 2f, 2f), Vector3.zero, from);

        Assert.AreEqual(12f, reference.x, 0.001f);
        Assert.AreEqual(0f, reference.y, 0.001f);
        Assert.AreEqual(0f, reference.z, 0.001f);
    }

    [Test]
    public void WithGroundHeight_ChangesOnlyY()
    {
        Vector3 parked = MoveToEdge.WithGroundHeight(new Vector3(12f, 40f, -3f), hasGround: true, groundY: 0f);

        Assert.AreEqual(12f, parked.x, 0.001f);
        Assert.AreEqual(0f, parked.y, 0.001f);
        Assert.AreEqual(-3f, parked.z, 0.001f);
        Assert.AreEqual(2f, MoveToEdge.WithGroundHeight(new Vector3(1f, 2f, 3f), hasGround: false, groundY: 0f).y, 0.001f);
    }

    [Test]
    public void ShouldCarry_FollowsTravelAndTheLastStep()
    {
        Assert.IsTrue(MoveToEdge.ShouldCarry(turning: false, traveling: true, moved: false));
        Assert.IsTrue(MoveToEdge.ShouldCarry(turning: false, traveling: false, moved: true));
        Assert.IsFalse(MoveToEdge.ShouldCarry(turning: false, traveling: false, moved: false));
    }
}
