using UnityEngine;

public static class SolarSystemLocator
{
    static readonly string[] SunNames = { "Sun Sphere", "Sun", "SUN" };

    public static Transform FindSun()
    {
        for (int i = 0; i < SunNames.Length; i++)
        {
            GameObject found = GameObject.Find(SunNames[i]);
            if (found != null)
                return found.transform;
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
