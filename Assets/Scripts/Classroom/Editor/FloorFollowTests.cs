using NUnit.Framework;
using UnityEngine;

public class FloorFollowTests
{
    [Test]
    public void ParentUnderTheRoom_LocksThePassenger()
    {
        var room = new GameObject("Class");
        var player = new GameObject("Player");
        player.transform.SetParent(room.transform, true);
        try
        {
            Assert.IsTrue(FloorFollow.ParentLocksPassenger(player.transform, room.transform));
            Assert.IsFalse(FloorFollow.ParentLocksPassenger(room.transform, room.transform));
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(room);
        }
    }

    [Test]
    public void Detach_KeepsTheWorldPositionOffTheScaledRoom()
    {
        var room = new GameObject("Class");
        room.transform.localScale = new Vector3(50f, 50f, 50f);
        var player = new GameObject("Player");
        Vector3 world = new Vector3(3f, 1.6f, -2f);
        player.transform.position = world;
        player.transform.SetParent(room.transform, true);
        try
        {
            FloorFollow.DetachPassenger(player.transform);

            Assert.IsNull(player.transform.parent);
            Assert.AreEqual(world, player.transform.position);
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(room);
        }
    }
}
