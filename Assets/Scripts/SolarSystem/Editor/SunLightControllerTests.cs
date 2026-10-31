using NUnit.Framework;

public class SunLightControllerTests
{
    [Test]
    public void SunAliases_MatchRegardlessOfCase()
    {
        string[] names = { "Sun Sphere", "Sun", "SUN" };
        Assert.IsTrue(SunLightController.IsSunName("sun sphere", names));
        Assert.IsFalse(SunLightController.IsSunName("Earth", names));
    }
}
