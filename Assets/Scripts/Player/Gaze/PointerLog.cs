public static class PointerLog
{
    public static string FocusName(string targetName, string parentName, bool parentIsPlanet)
    {
        if (parentIsPlanet)
            return "Planeta: " + parentName;

        return targetName ?? "";
    }

    public static string Click(string targetName)
    {
        return "Clique em: " + targetName;
    }
}
