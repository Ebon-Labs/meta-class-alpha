using UnityEngine;

public static class ControlButtonAppearance
{
    public static T Select<T>(bool useSecond, T first, T second) where T : class
    {
        return useSecond ? second : first;
    }

    public static void Apply(ObjectBasicController appearance, Renderer fallback, Material icon, bool createInstances)
    {
        if (icon == null)
            return;

        if (appearance != null)
        {
            appearance.InactiveMaterial = icon;
            if (createInstances)
                appearance.RefreshMaterials();
            return;
        }

        if (!createInstances || fallback == null)
            return;

        fallback.sharedMaterial = icon;
    }
}
