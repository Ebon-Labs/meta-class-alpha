using NUnit.Framework;

public class LanRoomBeaconTests
{
    [Test]
    public void Host_AdvertisesInsteadOfSearching()
    {
        Assert.AreEqual(LanRoomBeacon.Role.Host, LanRoomBeacon.Choose(serverActive: true, clientActive: true));
        Assert.AreEqual(LanRoomBeacon.Role.Host, LanRoomBeacon.Choose(serverActive: true, clientActive: false));
    }

    [Test]
    public void IdleClient_SearchesUntilItJoins()
    {
        Assert.AreEqual(LanRoomBeacon.Role.Search, LanRoomBeacon.Choose(serverActive: false, clientActive: false));
        Assert.AreEqual(LanRoomBeacon.Role.Idle, LanRoomBeacon.Choose(serverActive: false, clientActive: true));
    }
}
