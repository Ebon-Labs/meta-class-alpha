using TMPro;
using UnityEngine;

public static class PlayerName
{
    public const int MinimumLength = 2;
    public const float DisabledAlpha = 0.3f;
    public const string InputName = "NameTextInput";

    static string chosen = "";

    public static string Chosen => chosen ?? "";

    public static bool Ready(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length >= MinimumLength;
    }

    public static float Alpha(string name)
    {
        return Ready(name) ? 1f : DisabledAlpha;
    }

    public static bool Gates(string objectName)
    {
        return objectName == CarouselMenu.CreateName
            || objectName == CarouselMenu.EnterName
            || objectName == CarouselMenu.TutorialName;
    }

    public static bool Allows(string objectName, string name)
    {
        return !Gates(objectName) || Ready(name);
    }

    public static string Current()
    {
        GameObject input = GameObject.Find(InputName);
        if (input == null)
            return "";

        return CreateRoomButton.Alias(input.GetComponent<TMP_InputField>());
    }

    public static void Remember()
    {
        GameObject input = GameObject.Find(InputName);
        if (input == null)
            return;

        chosen = CreateRoomButton.Alias(input.GetComponent<TMP_InputField>()) ?? "";
    }

    public static string Remembered()
    {
        Remember();
        return Chosen;
    }

    public static void Apply(TMP_Text label, string name)
    {
        if (label == null)
            return;

        float alpha = Alpha(name);
        Color color = label.color;
        color.a = alpha;
        label.color = color;
        label.alpha = alpha;
    }

    public static void Refresh()
    {
        string name = Current();
        Apply(Label(CarouselMenu.CreateName), name);
        Apply(Label(CarouselMenu.EnterName), name);
        Apply(Label(CarouselMenu.TutorialName), name);
    }

    static TMP_Text Label(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target == null ? null : target.GetComponent<TMP_Text>();
    }
}
