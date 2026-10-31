using NUnit.Framework;

public class GazeFocusRulesTests
{
    [Test]
    public void EnvironmentPointerLockAndEdge_BlockANewFocus()
    {
        Assert.IsTrue(GazeFocusRules.BlocksNewFocus(true, false, false));
        Assert.IsTrue(GazeFocusRules.BlocksNewFocus(false, true, false));
        Assert.IsTrue(GazeFocusRules.BlocksNewFocus(false, false, true));
        Assert.IsFalse(GazeFocusRules.BlocksNewFocus(false, false, false));
    }

    [Test]
    public void SameTarget_ClearsFocusWhenPointerIsLockedOrAlreadyOnTheEdge()
    {
        Assert.IsTrue(GazeFocusRules.ShouldClearFocus(true, false));
        Assert.IsTrue(GazeFocusRules.ShouldClearFocus(false, true));
        Assert.IsFalse(GazeFocusRules.ShouldClearFocus(false, false));
    }

    [Test]
    public void PointerMessages_GoOnlyToInteractiveTags()
    {
        string[] tags = { "Interactive", "Teleportable", "Player" };

        Assert.IsTrue(GazeFocusRules.AcceptsPointerMessage("Teleportable", tags));
        Assert.IsFalse(GazeFocusRules.AcceptsPointerMessage("Environment", tags));
        Assert.IsFalse(GazeFocusRules.AcceptsPointerMessage(null, tags));
        Assert.IsFalse(GazeFocusRules.AcceptsPointerMessage("Interactive", null));
    }
}
