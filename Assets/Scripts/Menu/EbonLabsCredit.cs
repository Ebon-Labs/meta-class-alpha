using UnityEngine;

[DisallowMultipleComponent]
public class EbonLabsCredit : MonoBehaviour
{
    public const string ObjectName = "EbonLabsCredit";
    public const string Url = "https://ebonlabs.pages.dev/";

    public static bool Press(Vector2 screen, Camera view)
    {
        if (!ImageMenuThumb.Hits(GameObject.Find(ObjectName), screen, view))
            return false;

        Open(Url);
        return true;
    }

    public static void Open(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        Application.OpenURL(url);
    }
}
