using NUnit.Framework;

public class MoveToEdgeTests
{
    [Test]
    public void DefaultDuration_IsSevenSeconds()
    {
        Assert.AreEqual(7f, MoveToEdge.DefaultDuration);
    }

    [Test]
    public void Shell_IsVisibleOnlyWhileTraveling()
    {
        Assert.IsTrue(MoveToEdge.ShellVisible(true));
        Assert.IsFalse(MoveToEdge.ShellVisible(false));
    }
}
