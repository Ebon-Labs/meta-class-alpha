using UnityEngine;

public static class SolarSystemLocator
{
    static readonly string[] SunNames = { "Sun Sphere", "Sun", "SUN" };

    public static bool IsKnownSunName(string objectName)
    {
        return SceneNameMatch.EqualsAny(objectName, SunNames);
    }

    public static Transform FindSun()
    {
        for (int i = 0; i < SunNames.Length; i++)
        {
            GameObject found = GameObject.Find(SunNames[i]);
            if (found != null)
                return found.transform;
        }

        GameObject solarSystem = GameObject.Find("SolarSystem");
        if (solarSystem == null)
            return null;

        Transform[] children = solarSystem.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (IsKnownSunName(child.name))
                return child;
        }

        return null;
    }

    public static float EstimateRadius(Transform target)
    {
        if (target == null)
            return 0f;

        Renderer renderer = target.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds.extents.magnitude;

        Vector3 scale = target.lossyScale;
        return 0.5f * Mathf.Max(scale.x, scale.y, scale.z);
    }
}
