using NUnit.Framework;
using UnityEngine;

public class InitCardboardTests
{
    [Test]
    public void SolidColorCamera_UsesSkybox()
    {
        Assert.AreEqual(CameraClearFlags.Skybox, InitCardboard.ResolveClearFlags(CameraClearFlags.SolidColor));
    }

    [Test]
    public void ExistingSkyboxAndDepth_StayUnchanged()
    {
        Assert.AreEqual(CameraClearFlags.Skybox, InitCardboard.ResolveClearFlags(CameraClearFlags.Skybox));
        Assert.AreEqual(CameraClearFlags.Depth, InitCardboard.ResolveClearFlags(CameraClearFlags.Depth));
    }
}
