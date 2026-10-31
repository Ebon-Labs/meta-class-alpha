public static class GazeFocusRules
{
    public const string EnvironmentTag = "Environment";
    public const string TeleportableTag = "Teleportable";
    public const string PlanetTag = "Planet";

    public static bool BlocksNewFocus(bool isEnvironment, bool pointerDisabled, bool teleportableAtEdge)
    {
        return isEnvironment || pointerDisabled || teleportableAtEdge;
    }

    public static bool ShouldClearFocus(bool pointerDisabled, bool teleportableAtEdge)
    {
        return pointerDisabled || teleportableAtEdge;
    }

    public static bool AcceptsPointerMessage(string tag, string[] interactiveTags)
    {
        if (tag == null || interactiveTags == null)
            return false;

        return System.Array.IndexOf(interactiveTags, tag) >= 0;
    }
}
