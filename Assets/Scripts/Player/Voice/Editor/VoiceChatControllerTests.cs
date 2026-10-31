using NUnit.Framework;

public class VoiceChatControllerTests
{
    [Test]
    public void Host_KeepsCapturingWhenGuestsAreMuted()
    {
        Assert.IsTrue(VoiceChatController.ShouldCapture(muteGuests: true, isHostPlayer: true, microphoneReady: true));
    }

    [Test]
    public void Guest_StopsCapturingWhenGuestsAreMuted()
    {
        Assert.IsFalse(VoiceChatController.ShouldCapture(muteGuests: true, isHostPlayer: false, microphoneReady: true));
    }

    [Test]
    public void ReadyMicrophone_IsRequired()
    {
        Assert.IsFalse(VoiceChatController.ShouldCapture(muteGuests: false, isHostPlayer: false, microphoneReady: false));
        Assert.IsTrue(VoiceChatController.ShouldCapture(muteGuests: false, isHostPlayer: false, microphoneReady: true));
    }
}
