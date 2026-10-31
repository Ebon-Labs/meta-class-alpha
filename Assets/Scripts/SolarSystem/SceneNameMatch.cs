using System;

public static class SceneNameMatch
{
    public static bool Matches(string candidate, string name)
    {
        if (candidate == null || name == null)
            return false;

        return candidate.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EqualsAny(string candidate, string[] names)
    {
        if (candidate == null || names == null)
            return false;

        for (int i = 0; i < names.Length; i++)
        {
            if (Matches(candidate, names[i]))
                return true;
        }

        return false;
    }
}
