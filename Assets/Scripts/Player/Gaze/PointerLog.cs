public static class PointerLog
{
    public static string FocusName(string targetName, string parentName, bool parentIsPlanet)
    {
        if (!parentIsPlanet)
            return ScreenText.Name(targetName);

        string name = ScreenText.Name(parentName);
        if (IsSunOrMoon(parentName) || IsSunOrMoon(name))
            return name;

        return ScreenText.Format("gaze.planet", name);
    }

    static bool IsSunOrMoon(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string shown = ScreenText.Name(value);
        if (shown == ScreenText.Phrase("body.Sun") || shown == ScreenText.Phrase("body.Moon"))
            return true;

        string text = value.Trim();
        return EqualsBody(text, "Sun") || EqualsBody(text, "Moon");
    }

    static bool EqualsBody(string value, string body)
    {
        return string.Equals(value, body, System.StringComparison.OrdinalIgnoreCase);
    }

    public static string Click(string targetName)
    {
        return ScreenText.Format("gaze.click", ScreenText.Name(targetName));
    }
}
