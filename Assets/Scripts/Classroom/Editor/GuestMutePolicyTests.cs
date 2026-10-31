using NUnit.Framework;

public class GuestMutePolicyTests
{
    [Test]
    public void Host_IsNeverMuted()
    {
        Assert.IsFalse(GuestMutePolicy.ShouldMute(muteGuests: true, isHostPlayer: true));
    }

    [Test]
    public void Guest_IsMutedOnlyWhileTheFlagIsOn()
    {
        Assert.IsTrue(GuestMutePolicy.ShouldMute(muteGuests: true, isHostPlayer: false));
        Assert.IsFalse(GuestMutePolicy.ShouldMute(muteGuests: false, isHostPlayer: false));
    }
}
