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
    public void StopScale_SunIsFarther_EarthAndMarsAreCloser()
    {
        Assert.AreEqual(MoveToEdge.SunStopScale, MoveToEdge.StopScale("Sun"));
        Assert.AreEqual(5f, MoveToEdge.StopScale("Sol"));
        Assert.AreEqual(MoveToEdge.EarthStopScale, MoveToEdge.StopScale("Earth "), 0.0001f);
        Assert.AreEqual(MoveToEdge.EarthStopScale, MoveToEdge.StopScale("EarthWrapper"), 0.0001f);
        Assert.AreEqual(MoveToEdge.MarsStopScale, MoveToEdge.StopScale("MarsWrapper"), 0.0001f);
        Assert.AreEqual(1f, MoveToEdge.StopScale("Jupiter"));
        Assert.AreEqual(1f, MoveToEdge.StopScale("InfoArea"));
        Assert.AreEqual(1f, MoveToEdge.StopScale("SolarSystem"));
    }

    [Test]
    public void ApplyStopScale_ChangesDistanceAndKeepsTheParkHeight()
    {
        Vector3 stop = new Vector3(30f, MoveToEdge.AstroParkHeight, 0f);

        Vector3 farther = MoveToEdge.ApplyStopScale(Vector3.zero, stop, MoveToEdge.SunStopScale);
        Assert.AreEqual(150f, farther.x, 0.001f);
        Assert.AreEqual(MoveToEdge.AstroParkHeight, farther.y, 0.001f);

        Vector3 closer = MoveToEdge.ApplyStopScale(Vector3.zero, stop, MoveToEdge.EarthStopScale);
        Assert.AreEqual(10f, closer.x, 0.001f);
        Assert.AreEqual(MoveToEdge.AstroParkHeight, closer.y, 0.001f);
    }

    [Test]
    public void StopBody_UsesThePlanetAncestor()
    {
        var earth = new GameObject("EarthWrapper");
        var info = new GameObject("InfoArea");
        info.transform.SetParent(earth.transform, false);
        try
        {
            Assert.AreSame(earth.transform, MoveToEdge.StopBody(info.transform));
        }
        finally
        {
            Object.DestroyImmediate(earth);
        }
    }

    [Test]
    public void ShouldCarry_FollowsTravelAndTheLastStep()
    {
        Assert.IsTrue(MoveToEdge.ShouldCarry(turning: false, traveling: true, moved: false));
        Assert.IsTrue(MoveToEdge.ShouldCarry(turning: false, traveling: false, moved: true));
        Assert.IsFalse(MoveToEdge.ShouldCarry(turning: false, traveling: false, moved: false));
    }
}
