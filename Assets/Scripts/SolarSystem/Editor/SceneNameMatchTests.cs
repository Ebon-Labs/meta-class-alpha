using NUnit.Framework;

public class SceneNameMatchTests
{
    [Test]
    public void Match_IgnoresCaseAndRejectsNulls()
    {
        Assert.IsTrue(SceneNameMatch.Matches("Sun Sphere", "sun sphere"));
        Assert.IsFalse(SceneNameMatch.Matches("Moon", "Sun"));
        Assert.IsFalse(SceneNameMatch.Matches(null, "Sun"));
        Assert.IsFalse(SceneNameMatch.Matches("Sun", null));
    }

    [Test]
    public void EqualsAny_AcceptsTheFirstMatchingAlias()
    {
        string[] names = { "Sun Sphere", "Sun", "SUN" };
        Assert.IsTrue(SceneNameMatch.EqualsAny("sun", names));
        Assert.IsFalse(SceneNameMatch.EqualsAny("Sphere.009", names));
        Assert.IsFalse(SceneNameMatch.EqualsAny("Sun", null));
    }
}
