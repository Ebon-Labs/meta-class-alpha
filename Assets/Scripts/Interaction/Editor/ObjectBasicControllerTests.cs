using NUnit.Framework;

public class ObjectBasicControllerTests
{
    [Test]
    public void SharedMaterial_IsAdoptedWhenItDiffersFromTheInactiveSlot()
    {
        Assert.IsTrue(ObjectBasicController.ShouldAdoptSharedMaterial(hasSharedMaterial: true, inactiveMissingOrDifferent: true));
        Assert.IsFalse(ObjectBasicController.ShouldAdoptSharedMaterial(hasSharedMaterial: false, inactiveMissingOrDifferent: true));
        Assert.IsFalse(ObjectBasicController.ShouldAdoptSharedMaterial(hasSharedMaterial: true, inactiveMissingOrDifferent: false));
    }

    [Test]
    public void Gaze_SelectsTheHighlightSlot()
    {
        Assert.IsTrue(ObjectBasicController.UseGazedMaterial(true));
        Assert.IsFalse(ObjectBasicController.UseGazedMaterial(false));
    }
}
