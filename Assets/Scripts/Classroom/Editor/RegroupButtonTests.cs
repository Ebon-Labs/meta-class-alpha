using NUnit.Framework;
using UnityEngine;

public class RegroupButtonTests
{
    [Test]
    public void ShouldMove_LeavesTheHostAndTakesTheGuests()
    {
        Assert.IsFalse(RegroupButton.ShouldMove(isHostPlayer: true));
        Assert.IsTrue(RegroupButton.ShouldMove(isHostPlayer: false));
    }

    [Test]
    public void PlaceGuests_MovesClientsOntoTheGuestAreaAndLeavesTheHost()
    {
        var roomObject = new GameObject("Room");
        var guestArea = new GameObject("GuestArea");
        var host = new GameObject("HostPlayer");
        var guest = new GameObject("GuestPlayer");
        try
        {
            ClassController room = roomObject.AddComponent<ClassController>();
            guestArea.transform.SetParent(room.transform);
            guestArea.transform.position = new Vector3(4f, 0f, 2f);
            host.transform.position = Vector3.zero;
            guest.transform.position = new Vector3(9f, 0f, 9f);

            int moved = room.PlaceGuests(
                new[] { host.transform, guest.transform },
                body => body.name == "HostPlayer");

            Assert.AreEqual(1, moved);
            Assert.AreEqual(Vector3.zero, host.transform.position);
            Vector3 stand = ClassController.StandAboveAnchor(guestArea.transform.position, 1.6f);
            Assert.AreEqual(stand.x, guest.transform.position.x, 0.001f);
            Assert.AreEqual(stand.y, guest.transform.position.y, 0.001f);
            Assert.AreEqual(stand.z, guest.transform.position.z, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(roomObject);
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(guest);
        }
    }
}
