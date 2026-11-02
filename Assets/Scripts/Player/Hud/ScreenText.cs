using System.Collections.Generic;

public static class ScreenText
{
    public const string PortugueseBrazil = "pt-BR";

    public static string Language { get; private set; } = PortugueseBrazil;

    static readonly Dictionary<string, string> Portuguese = new Dictionary<string, string>
    {
        { "gaze.planet", "Planeta: {0}" },
        { "gaze.click", "Clique em: {0}" },
        { "body.Mercury", "Mercúrio" },
        { "body.Venus", "Vênus" },
        { "body.Earth", "Terra" },
        { "body.Mars", "Marte" },
        { "body.Jupiter", "Júpiter" },
        { "body.Saturn", "Saturno" },
        { "body.Uranus", "Urano" },
        { "body.Neptune", "Netuno" },
        { "body.Moon", "Lua" },
        { "body.Sun", "Sol" },
        { "body.Planet", "Planeta" },
        { "body.Jupter", "Júpiter" },
        { "control.MercuryButton", "Mercúrio" },
        { "control.VenusButton", "Vênus" },
        { "control.EarthButton", "Terra" },
        { "control.MarsButton", "Marte" },
        { "control.JupiterButton", "Júpiter" },
        { "control.JupterButton", "Júpiter" },
        { "control.SaturnButton", "Saturno" },
        { "control.UranusButton", "Urano" },
        { "control.NeptuneButton", "Netuno" },
        { "control.SunButton", "Sol" },
        { "control.MoonButton", "Lua" },
        { "control.BackButton", "Voltar" },
        { "control.MuteButton", "Silenciar" },
        { "control.OrbitButton", "Órbita" },
        { "control.DistanceButton", "Distância" },
        { "control.MarkerButton", "Marcadores" },
        { "control.GroundButton", "Chão" },
        { "control.PlayButton", "Reproduzir" },
        { "control.PauseButton", "Pausar" },
        { "control.StopButton", "Parar" },
        { "control.GoHomeOrb", "Voltar à sala" },
        { "control.RotateClassButton", "Rotação" },
        { "control.InteractionMarkerOrb", "Marcador" }
    };

    public static void Use(string language)
    {
        Language = string.IsNullOrWhiteSpace(language) ? PortugueseBrazil : language;
    }

    public static string Phrase(string key)
    {
        return Phrase(key, Language);
    }

    public static string Phrase(string key, string language)
    {
        if (string.IsNullOrEmpty(key))
            return "";

        if (TryGet(language, key, out string text))
            return text;

        if (language != PortugueseBrazil && TryGet(PortugueseBrazil, key, out text))
            return text;

        return key;
    }

    public static string Format(string key, string value)
    {
        return Phrase(key).Replace("{0}", value ?? "");
    }

    public static string Name(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return "";

        string trimmed = source.Trim();
        string body = "body." + trimmed;
        if (Contains(body))
            return Phrase(body);

        string control = "control." + trimmed;
        if (Contains(control))
            return Phrase(control);

        return trimmed;
    }

    static bool Contains(string key)
    {
        return TryGet(Language, key, out _) || TryGet(PortugueseBrazil, key, out _);
    }

    static bool TryGet(string language, string key, out string text)
    {
        text = null;
        Dictionary<string, string> catalog = Catalog(language);
        if (catalog == null)
            return false;

        return catalog.TryGetValue(key, out text);
    }

    static Dictionary<string, string> Catalog(string language)
    {
        if (language == PortugueseBrazil)
            return Portuguese;

        return null;
    }
}
