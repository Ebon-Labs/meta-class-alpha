using NUnit.Framework;

public class BodyBoundsTests
{
    [Test]
    public void MissingBody_HasNoBounds()
    {
        Assert.IsFalse(BodyBounds.TryGet(null, out _));
    }
}
