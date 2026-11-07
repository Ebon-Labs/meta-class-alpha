using NUnit.Framework;
using UnityEngine;

public class HideFromBehindTests
{
    [Test]
    public void Cull_HidesTheBackOfTheMenuSprite()
    {
        Shader shader = Shader.Find(HideFromBehind.SpriteShader);
        Assert.IsNotNull(shader);

        Material material = new Material(shader);
        try
        {
            material.SetFloat("_CullMode", 0f);
            HideFromBehind.Cull(material);

            Assert.AreEqual(HideFromBehind.CullBack, material.GetFloat("_CullMode"));
        }
        finally
        {
            Object.DestroyImmediate(material);
        }
    }
}
