using UnityEngine;

[DisallowMultipleComponent]
public class PlanetCloudLayer : MonoBehaviour
{
    [SerializeField] Material cloudMaterial;
    [SerializeField] float shellScale = 1.01f;
    [SerializeField] string cloudChildName = "Clouds";

    void Awake()
    {
        EnsureCloudShell();
    }

    void EnsureCloudShell()
    {
        if (cloudMaterial == null)
            return;

        Transform existing = transform.Find(cloudChildName);
        if (existing != null)
            return;

        MeshFilter sourceFilter = FindSurfaceMeshFilter();
        if (sourceFilter == null || sourceFilter.sharedMesh == null)
            return;

        DisableExistingCloudRenderers();

        GameObject clouds = new GameObject(cloudChildName);
        clouds.transform.SetParent(sourceFilter.transform, false);
        clouds.transform.localPosition = Vector3.zero;
        clouds.transform.localRotation = Quaternion.identity;
        clouds.transform.localScale = Vector3.one * shellScale;
        clouds.layer = gameObject.layer;

        MeshFilter cloudFilter = clouds.AddComponent<MeshFilter>();
        cloudFilter.sharedMesh = sourceFilter.sharedMesh;

        MeshRenderer cloudRenderer = clouds.AddComponent<MeshRenderer>();
        cloudRenderer.sharedMaterial = cloudMaterial;
        cloudRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cloudRenderer.receiveShadows = false;
    }

    MeshFilter FindSurfaceMeshFilter()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(true);
        MeshFilter fallback = null;

        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter filter = filters[i];
            if (filter == null || filter.sharedMesh == null)
                continue;

            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial == cloudMaterial)
                continue;

            if (fallback == null)
                fallback = filter;

            if (renderer != null && renderer.enabled)
                return filter;
        }

        return fallback;
    }

    void DisableExistingCloudRenderers()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (renderer == null || renderer.sharedMaterial == null)
                continue;

            if (renderer.sharedMaterial == cloudMaterial)
                renderer.enabled = false;
        }
    }
}
