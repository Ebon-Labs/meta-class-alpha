using NUnit.Framework;

public class PlayerLabelTests
{
    [Test]
    public void ShownText_UsesTheChosenNameAndHidesAnEmptyOne()
    {
        Assert.AreEqual("Ana", PlayerLabel.ShownText("Ana"));
        Assert.AreEqual("Ana", PlayerLabel.ShownText("  Ana  "));
        Assert.AreEqual("", PlayerLabel.ShownText(""));
        Assert.AreEqual("", PlayerLabel.ShownText("   "));
        Assert.AreEqual("", PlayerLabel.ShownText(null));
    }
}
