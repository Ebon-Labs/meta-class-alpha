using NUnit.Framework;
using UnityEngine;

public class TeleportPlacementTests
{
    [Test]
    public void Edge_SitsJustOutsideTheFaceFacingTheObserver()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        Vector3 edge = TeleportPlacement.EdgePosition(bounds, new Vector3(10f, 0f, 0f), Vector3.forward);

        Assert.AreEqual(0.55f, edge.x, 0.0001f);
        Assert.AreEqual(0f, edge.y, 0.0001f);
        Assert.AreEqual(0f, edge.z, 0.0001f);
    }

    [Test]
    public void ObserverAtTheCenter_UsesTheOppositeOfForward()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        Vector3 edge = TeleportPlacement.EdgePosition(bounds, Vector3.zero, Vector3.forward);

        Assert.AreEqual(0f, edge.x, 0.0001f);
        Assert.AreEqual(0f, edge.y, 0.0001f);
        Assert.AreEqual(-0.55f, edge.z, 0.0001f);
    }

    [Test]
    public void IsAtEdge_UsesAMinimumTolerance()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        Vector3 edge = new Vector3(0.55f, 0f, 0f);

        Assert.IsTrue(TeleportPlacement.IsAtEdge(edge, edge, bounds));
        Assert.IsFalse(TeleportPlacement.IsAtEdge(new Vector3(5f, 0f, 0f), edge, bounds));
    }
}
