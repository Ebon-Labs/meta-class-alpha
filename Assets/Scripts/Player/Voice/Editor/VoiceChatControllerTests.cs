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

    [Test]
    public void VoiceChunk_StaysInsideOneTransmitWindow()
    {
        Assert.AreEqual(1600, VoiceChatController.MaxVoiceSamples(16000, 100));
        Assert.AreEqual(3000, VoiceChatController.MaxVoiceSamples(48000, 100));
    }

    [Test]
    public void VoiceReadStart_KeepsTheNewestSamples()
    {
        Assert.AreEqual(14500, VoiceChatController.VoiceReadStart(100, 16000, 1600, 16000));
    }
}
