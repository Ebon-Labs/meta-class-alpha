using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HideFromBehind : MonoBehaviour
{
    public const float CullBack = 2f;
    public const string SpriteShader = "Custom/MenuSprite";

    Material side;

    public static bool KeepsOwnMaterial(Material material)
    {
        if (material == null || material.shader == null)
            return false;

        string shader = material.shader.name;
        return shader == SpriteShader || shader == "Custom/MenuGrayscale";
    }

    public static void Cull(Material material)
    {
        if (material != null && material.HasProperty("_CullMode"))
            material.SetFloat("_CullMode", CullBack);
    }

    void LateUpdate()
    {
        if (side == null)
        {
            Shader shader = Shader.Find(SpriteShader);
            if (shader == null)
                return;

            side = new Material(shader);
            Cull(side);
        }

        Image image = GetComponent<Image>();
        if (image != null)
        {
            if (!KeepsOwnMaterial(image.material))
                image.material = side;
            else
                Cull(image.material);
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
                Cull(texts[i].fontMaterial);
        }

        TMP_SelectionCaret caret = GetComponentInChildren<TMP_SelectionCaret>(true);
        if (caret != null && caret.material != side)
            caret.material = side;
    }

    void OnDestroy()
    {
        if (side != null)
            Destroy(side);
    }
}
