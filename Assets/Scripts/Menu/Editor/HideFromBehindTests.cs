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

    [Test]
    public void KeepsOwnMaterial_LeavesPortraitShadersInPlace()
    {
        Shader sprite = Shader.Find(HideFromBehind.SpriteShader);
        Shader gray = Shader.Find("Custom/MenuGrayscale");
        Shader fallback = Shader.Find("Sprites/Default");
        Assert.IsNotNull(sprite);
        Assert.IsNotNull(gray);

        Material front = new Material(sprite);
        Material shade = new Material(gray);
        Material other = new Material(fallback);
        try
        {
            Assert.IsTrue(HideFromBehind.KeepsOwnMaterial(front));
            Assert.IsTrue(HideFromBehind.KeepsOwnMaterial(shade));
            Assert.IsFalse(HideFromBehind.KeepsOwnMaterial(other));
            Assert.IsFalse(HideFromBehind.KeepsOwnMaterial(null));
        }
        finally
        {
            Object.DestroyImmediate(front);
            Object.DestroyImmediate(shade);
            Object.DestroyImmediate(other);
        }
    }
}
