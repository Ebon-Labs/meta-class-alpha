using UnityEngine;

[ExecuteAlways]
public class SunLightController : MonoBehaviour
{
    [SerializeField] Transform sunTransform;
    [SerializeField] Light sunLight;
    [SerializeField] string[] sunObjectNames = { "Sun Sphere", "Sun", "SUN" };
    [SerializeField] float lightRange = 2500f;
    [SerializeField] float lightIntensity = 4f;

    void Reset()
    {
        sunLight = GetComponent<Light>();
        ConfigureSunLight();
    }

    void Awake()
    {
        if (sunLight == null)
            sunLight = GetComponent<Light>();

        ConfigureSunLight();
    }

    void LateUpdate()
    {
        if (sunLight == null)
            sunLight = GetComponent<Light>();

        Transform sun = ResolveSunTransform();
        if (sun == null || sunLight == null)
            return;

        transform.position = sun.position;
    }

    void ConfigureSunLight()
    {
        if (sunLight == null)
            return;

        sunLight.type = LightType.Point;
        sunLight.color = new Color(1f, 0.96f, 0.84f);
        sunLight.intensity = lightIntensity;
        sunLight.range = lightRange;
        sunLight.useColorTemperature = true;
        sunLight.colorTemperature = 5778f;
    }

    Transform ResolveSunTransform()
    {
        if (sunTransform != null)
            return sunTransform;

        foreach (string objectName in sunObjectNames)
        {
            GameObject found = GameObject.Find(objectName);
            if (found != null)
                return found.transform;
        }

        GameObject solarSystem = GameObject.Find("SolarSystem");
        if (solarSystem == null)
            return null;

        foreach (Transform child in solarSystem.GetComponentsInChildren<Transform>(true))
        {
            if (IsSunName(child.name, sunObjectNames))
                return child;
        }

        return solarSystem.transform;
    }

    public static bool IsSunName(string objectName, string[] names)
    {
        return SceneNameMatch.EqualsAny(objectName, names);
    }
}
