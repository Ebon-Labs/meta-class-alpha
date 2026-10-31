using NUnit.Framework;

public class UILoggerTests
{
    [Test]
    public void DisplayText_KeepsTheMessage()
    {
        Assert.AreEqual("Planeta: Mars", UILogger.DisplayText("Planeta: Mars"));
    }

    [Test]
    public void DisplayText_TurnsNullIntoEmpty()
    {
        Assert.AreEqual("", UILogger.DisplayText(null));
    }
}
