using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-100)]
public class SolarSystemSunReplacer : MonoBehaviour
{
    [SerializeField] Transform solarSystemRoot;
    [SerializeField] string sunSphereName = "Sun Sphere";
    [SerializeField] string[] oldSunNames = { "SUN", "Sun", "Sphere.009" };

    void OnEnable()
    {
        ReplaceSun();
    }

    void ReplaceSun()
    {
        Transform root = ResolveSolarSystemRoot();
        if (root == null)
            return;

        Transform sunSphere = FindChildByName(root, sunSphereName);
        if (sunSphere == null)
            return;

        Transform oldSun = FindOldSun(root, sunSphere);
        if (oldSun == null)
            return;

        sunSphere.SetLocalPositionAndRotation(oldSun.localPosition, oldSun.localRotation);
        sunSphere.localScale = oldSun.localScale;

        if (Application.isPlaying)
            Destroy(oldSun.gameObject);
        else
            DestroyImmediate(oldSun.gameObject);
    }

    Transform ResolveSolarSystemRoot()
    {
        if (solarSystemRoot != null)
            return solarSystemRoot;

        GameObject found = GameObject.Find("SolarSystem");
        return found != null ? found.transform : null;
    }

    Transform FindOldSun(Transform root, Transform sunSphere)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (IsReplaceableSun(child.name, oldSunNames, child == root, child == sunSphere, IsUnder(child, sunSphere)))
                return child;
        }

        return null;
    }

    static Transform FindChildByName(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (SceneNameMatch.Matches(child.name, objectName))
                return child;
        }

        return null;
    }

    static bool IsUnder(Transform candidate, Transform ancestor)
    {
        Transform current = candidate;
        while (current != null)
        {
            if (current == ancestor)
                return true;

            current = current.parent;
        }

        return false;
    }

    public static bool IsReplaceableSun(string objectName, string[] oldNames, bool isRoot, bool isNewSun, bool isUnderNewSun)
    {
        if (isRoot || isNewSun || isUnderNewSun)
            return false;

        return SceneNameMatch.EqualsAny(objectName, oldNames);
    }
}
