using NUnit.Framework;

public class ScreenControlButtonTests
{
    sealed class FakeScreen : IScreenPlayback
    {
        public string Last;

        public void Play()
        {
            Last = "play";
        }

        public void Pause()
        {
            Last = "pause";
        }

        public void Stop()
        {
            Last = "stop";
        }
    }

    [Test]
    public void Execute_ForwardsPlayPauseAndStop()
    {
        FakeScreen screen = new FakeScreen();

        ScreenControlButton.Execute(ScreenControlButton.Action.Play, screen);
        Assert.AreEqual("play", screen.Last);

        ScreenControlButton.Execute(ScreenControlButton.Action.Pause, screen);
        Assert.AreEqual("pause", screen.Last);

        ScreenControlButton.Execute(ScreenControlButton.Action.Stop, screen);
        Assert.AreEqual("stop", screen.Last);
    }

    [Test]
    public void Execute_IgnoresAMissingScreen()
    {
        Assert.DoesNotThrow(() => ScreenControlButton.Execute(ScreenControlButton.Action.Play, null));
    }
}
