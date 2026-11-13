using NUnit.Framework;
using UnityEngine;

public class FloorCarryTests
{
    [Test]
    public void Position_FollowsATranslationBetweenPlanets()
    {
        Vector3 player = new Vector3(2f, 1.6f, -3f);
        Vector3 from = new Vector3(10f, 0f, 4f);
        Vector3 to = new Vector3(-40f, 8f, 12f);

        Vector3 carried = FloorCarry.Position(player, from, Quaternion.identity, to, Quaternion.identity);

        Assert.AreEqual(to + (player - from), carried);
    }

    [Test]
    public void Position_KeepsThePlayersOffsetFromTheFloor()
    {
        Vector3 player = new Vector3(0f, 1.6f, 4f);
        Quaternion turn = Quaternion.AngleAxis(-90f, Vector3.up);

        Vector3 carried = FloorCarry.Position(player, Vector3.zero, Quaternion.identity, Vector3.zero, turn);

        Assert.AreEqual(turn * player, carried);
    }

    [Test]
    public void Rotation_TurnsThePlayerWithTheFloor()
    {
        Quaternion turn = Quaternion.AngleAxis(-45f, Vector3.up);
        Quaternion carried = FloorCarry.Rotation(Quaternion.identity, Quaternion.identity, turn);

        Assert.AreEqual(0f, Quaternion.Angle(turn, carried), 0.001f);
    }

    [Test]
    public void YawDegrees_IsTheFloorsClockwiseStep()
    {
        Quaternion from = Quaternion.identity;
        Quaternion to = Quaternion.AngleAxis(-45f, Vector3.up);

        Assert.AreEqual(-45f, FloorCarry.YawDegrees(from, to), 0.001f);
    }
}
