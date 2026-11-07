using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CreateRoomButton : MonoBehaviour
{
    public const int MinimumLength = 5;
    public const float DisabledAlpha = 0.3f;
    public const string ObjectName = "CreateRoomButton";
    public const string InputName = "TextInput";
    public const string Caption = "Entrar";

    TextMeshPro label;

    public static bool Ready(string alias)
    {
        return !string.IsNullOrEmpty(alias) && alias.Length >= MinimumLength;
    }

    public static float Alpha(string alias)
    {
        return Ready(alias) ? 1f : DisabledAlpha;
    }

    public static Color WithAlpha(Color color, string alias)
    {
        color.a = Alpha(alias);
        return color;
    }

    public static string Alias(TMP_InputField field)
    {
        if (field == null || field.text == null)
            return "";

        return field.text;
    }

    public static string CurrentAlias()
    {
        GameObject input = GameObject.Find(InputName);
        if (input == null)
            return "";

        return Alias(input.GetComponent<TMP_InputField>());
    }

    public static bool IsCaption(string text)
    {
        return text != null && text.Trim() == Caption;
    }

    public static TextMeshPro FindLabel()
    {
        TextMeshPro named = LabelOn(GameObject.Find(ObjectName));
        if (named != null)
            return named;

        TextMeshPro[] labels = Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && IsCaption(labels[i].text))
                return labels[i];
        }

        return null;
    }

    public static void Apply(TMP_Text label, string alias)
    {
        if (label == null)
            return;

        float alpha = Alpha(alias);
        Color color = label.color;
        color.a = alpha;
        label.color = color;
        label.alpha = alpha;
    }

    public static void Refresh()
    {
        Apply(FindLabel(), CurrentAlias());
    }

    public static bool Press(Vector2 screen, Camera view)
    {
        TextMeshPro label = FindLabel();
        if (label == null || !SolarSystemThumbHost.Hits(label.gameObject, screen, view))
            return false;

        string alias = CurrentAlias();
        if (Ready(alias))
            Open(alias);

        return true;
    }

    public static void Open(string alias)
    {
        if (!Ready(alias))
            return;

        bool webGl = Application.platform == RuntimePlatform.WebGLPlayer;
        if (!SolarSystemThumbHost.CanStartHost(NetworkClient.isConnected, NetworkClient.active, NetworkServer.active, webGl))
            return;

        NetworkDiscovery discovery = Object.FindFirstObjectByType<NetworkDiscovery>();
        if (discovery != null)
            discovery.roomAlias = alias;

        SolarSystemThumbHost.StartHostSession();
    }

    void Awake()
    {
        label = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        if (label == null)
            label = GetComponent<TextMeshPro>();

        Apply(label, CurrentAlias());
    }

    static TextMeshPro LabelOn(GameObject target)
    {
        return target == null ? null : target.GetComponent<TextMeshPro>();
    }
}
