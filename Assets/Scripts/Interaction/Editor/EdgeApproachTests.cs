using NUnit.Framework;
using UnityEngine;

public class EdgeApproachTests
{
    static readonly Bounds Body = new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));
    static readonly Vector3 Start = new Vector3(10f, 0f, 0f);

    [Test]
    public void AtTheStart_StaysAtTheSubject()
    {
        Vector3 position = EdgeApproach.Position(Start, Body, Vector3.forward, 0f, MoveToEdge.DefaultDuration);

        Assert.AreEqual(Start.x, position.x, 0.0001f);
        Assert.AreEqual(0f, position.y, 0.0001f);
        Assert.AreEqual(0f, position.z, 0.0001f);
    }

    [Test]
    public void AtTheEnd_SitsOnTheEdgeFacingTheStart()
    {
        Vector3 position = EdgeApproach.Position(Start, Body, Vector3.forward, MoveToEdge.DefaultDuration, MoveToEdge.DefaultDuration);

        Assert.AreEqual(1.05f, position.x, 0.0001f);
        Assert.AreEqual(0f, position.y, 0.0001f);
        Assert.AreEqual(0f, position.z, 0.0001f);
    }

    [Test]
    public void Halfway_IsTheSmoothedMidpoint()
    {
        Vector3 position = EdgeApproach.Position(Start, Body, Vector3.forward, 3.5f, MoveToEdge.DefaultDuration);

        Assert.AreEqual(Mathf.Lerp(10f, 1.05f, 0.5f), position.x, 0.0001f);
    }

    [Test]
    public void ZeroDuration_SnapsToTheEdge()
    {
        Vector3 position = EdgeApproach.Position(Start, Body, Vector3.forward, 0f, 0f);

        Assert.AreEqual(1.05f, position.x, 0.0001f);
    }

    [Test]
    public void PastTheDuration_StaysOnTheEdge()
    {
        Vector3 position = EdgeApproach.Position(Start, Body, Vector3.forward, 20f, MoveToEdge.DefaultDuration);

        Assert.AreEqual(1.05f, position.x, 0.0001f);
    }

    [Test]
    public void DefaultDuration_IsSevenSeconds()
    {
        Assert.AreEqual(7f, MoveToEdge.DefaultDuration);
    }

    [Test]
    public void RestOn_PlacesTheBottomOnTheGround()
    {
        Vector3 position = EdgeApproach.RestOn(new Vector3(4f, 10f, 3f), 1.5f, 0f, 0f);

        Assert.AreEqual(4f, position.x, 0.0001f);
        Assert.AreEqual(1.5f, position.y, 0.0001f);
        Assert.AreEqual(3f, position.z, 0.0001f);
    }
}
