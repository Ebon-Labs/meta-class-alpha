using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLabel : MonoBehaviour
{
    TextMeshPro text;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();
        Show("");
    }

    public void Show(string name)
    {
        if (text == null)
            text = GetComponent<TextMeshPro>();

        if (text == null)
            return;

        text.text = ShownText(name);
    }

    public static string ShownText(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        return name.Trim();
    }
}
