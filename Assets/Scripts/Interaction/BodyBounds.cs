using UnityEngine;

public static class BodyBounds
{
    public static bool TryGet(Transform body, out Bounds bounds)
    {
        bounds = default;
        if (body == null)
            return false;

        Renderer own = body.GetComponent<Renderer>();
        if (own != null)
        {
            bounds = own.bounds;
            return true;
        }

        Renderer[] renderers = body.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        Collider[] colliders = body.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
            return false;

        bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
            bounds.Encapsulate(colliders[i].bounds);
        return true;
    }
}
