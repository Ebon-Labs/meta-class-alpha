using NUnit.Framework;

public class VoiceMuteIndicatorTests
{
    [Test]
    public void Icon_ShowsOnlyWhenTheClassroomMutesGuests()
    {
        Assert.IsTrue(VoiceMuteIndicator.ShouldShowIcon(hasClassroom: true, guestsMuted: true));
        Assert.IsFalse(VoiceMuteIndicator.ShouldShowIcon(hasClassroom: true, guestsMuted: false));
        Assert.IsFalse(VoiceMuteIndicator.ShouldShowIcon(hasClassroom: false, guestsMuted: true));
    }
}
