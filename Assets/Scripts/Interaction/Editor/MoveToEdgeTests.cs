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
}
