using NUnit.Framework;
using UnityEngine;

public class PlanetCloudLayerTests
{
    [Test]
    public void Shell_IsAUniformScale()
    {
        Vector3 scale = PlanetCloudLayer.ShellLocalScale(1.01f);
        Assert.AreEqual(1.01f, scale.x, 0.0001f);
        Assert.AreEqual(1.01f, scale.y, 0.0001f);
        Assert.AreEqual(1.01f, scale.z, 0.0001f);
    }

    [Test]
    public void CloudMaterial_RequiresBothAMaterialAndAMatch()
    {
        Assert.IsTrue(PlanetCloudLayer.UsesCloudMaterial(true, true));
        Assert.IsFalse(PlanetCloudLayer.UsesCloudMaterial(true, false));
        Assert.IsFalse(PlanetCloudLayer.UsesCloudMaterial(false, true));
    }
}
