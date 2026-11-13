using NUnit.Framework;
using UnityEngine;

public class VrEntryPromptTests
{
    [Test]
    public void Covers_SolarSystemAndTutorialOnly()
    {
        Assert.IsTrue(VrEntryPrompt.Covers(VrEntryPrompt.SolarSystemScene));
        Assert.IsTrue(VrEntryPrompt.Covers(VrEntryPrompt.TutorialScene));
        Assert.IsFalse(VrEntryPrompt.Covers("Menu"));
    }

    [Test]
    public void StillShowing_LastsFiveSeconds()
    {
        Assert.AreEqual(5f, VrEntryPrompt.DurationSeconds);
        Assert.IsTrue(VrEntryPrompt.StillShowing(now: 1f, hideAt: 6f));
        Assert.IsFalse(VrEntryPrompt.StillShowing(now: 6f, hideAt: 6f));
    }

    [Test]
    public void FitSize_KeepsTheImageInsideTheView()
    {
        Vector2 wide = VrEntryPrompt.FitSize(viewAspect: 2f, imageAspect: 1f, distance: 1f, fieldOfViewDegrees: 90f);
        Vector2 tall = VrEntryPrompt.FitSize(viewAspect: 0.5f, imageAspect: 1f, distance: 1f, fieldOfViewDegrees: 90f);

        Assert.AreEqual(wide.y, wide.x, 0.001f);
        Assert.Less(wide.x, 2f * Mathf.Tan(45f * Mathf.Deg2Rad) * 2f);
        Assert.AreEqual(tall.x, tall.y, 0.001f);
    }

    [Test]
    public void RestoreEnabled_TurnsTheLocalPlayerCameraBackOn()
    {
        Assert.IsTrue(VrEntryPrompt.RestoreEnabled(wasEnabled: false, isLocalPlayer: true));
        Assert.IsFalse(VrEntryPrompt.RestoreEnabled(wasEnabled: false, isLocalPlayer: false));
        Assert.IsTrue(VrEntryPrompt.RestoreEnabled(wasEnabled: true, isLocalPlayer: false));
    }
}
