using NUnit.Framework;

public class BillboardScaleTests
{
    [Test]
    public void NinetyDegreeFov_ScalesWithDistanceOverScreenHeight()
    {
        float scale = BillboardScale.Uniform(
            fontSize: 100f,
            distance: 10f,
            fieldOfViewDegrees: 90f,
            screenHeight: 100f,
            parentScale: 1f);

        Assert.AreEqual(0.2f, scale, 0.0001f);
    }

    [Test]
    public void ParentScale_ShrinksTheLocalScale()
    {
        float scale = BillboardScale.Uniform(100f, 10f, 90f, 100f, parentScale: 2f);
        Assert.AreEqual(0.1f, scale, 0.0001f);
    }
}
