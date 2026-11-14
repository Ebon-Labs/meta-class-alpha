using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class InfoArea : MonoBehaviour
{
    static readonly List<InfoArea> active = new List<InfoArea>();
    const string LoremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

    [TextArea(3, 8)]
    [SerializeField] string body = LoremIpsum;

    SphereCollider volume;
    bool localPlayerInside;

    void Awake()
    {
        volume = GetComponent<SphereCollider>();
        volume.isTrigger = true;
    }

    void OnEnable()
    {
        if (!active.Contains(this))
            active.Add(this);
    }

    void OnDisable()
    {
        active.Remove(this);

        if (!localPlayerInside)
            return;

        localPlayerInside = false;
        FindLocalPanel()?.Hide(this);
    }

    void Update()
    {
        SceneInfoPanel panel = FindLocalPanel();
        bool inside = panel != null && Contains(panel.transform.position);

        if (inside == localPlayerInside)
            return;

        localPlayerInside = inside;

        if (panel == null)
            return;

        if (inside)
            panel.Show(this, body);
        else
            panel.Hide(this);
    }

    public static Transform PlanetOf(Transform area)
    {
        Transform cursor = area;
        while (cursor != null)
        {
            if (cursor.CompareTag(GazeFocusRules.PlanetTag))
                return cursor;

            Transform parent = cursor.parent;
            if (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (child.CompareTag(GazeFocusRules.PlanetTag))
                        return child;
                }
            }

            cursor = parent;
        }

        return null;
    }

    public static bool BlocksPlanet(Transform hit, Transform planet)
    {
        if (hit == null || planet == null)
            return false;

        return hit == planet || hit.IsChildOf(planet);
    }

    public static bool AllowsRay(Transform hit, Vector3 observer)
    {
        if (hit == null)
            return false;

        for (int i = 0; i < active.Count; i++)
        {
            InfoArea area = active[i];
            if (area == null || !area.Contains(observer))
                continue;

            if (BlocksPlanet(hit, PlanetOf(area.transform)))
                return false;
        }

        return true;
    }

    public static bool TryClosestHit(RaycastHit[] hits, Vector3 observer, out RaycastHit closest)
    {
        closest = default;
        bool found = false;
        float best = float.PositiveInfinity;
        if (hits == null)
            return false;

        for (int i = 0; i < hits.Length; i++)
        {
            Transform hit = hits[i].transform;
            Transform collider = hits[i].collider != null ? hits[i].collider.transform : null;
            bool blocked = !AllowsRay(hit, observer) || (collider != null && !AllowsRay(collider, observer));
            if (hits[i].distance >= best || blocked)
                continue;

            best = hits[i].distance;
            closest = hits[i];
            found = true;
        }

        return found;
    }

    public static bool ContainsLocalPoint(Vector3 localPoint, Vector3 center, float radius)
    {
        float safeRadius = Mathf.Max(0.001f, radius);
        return (localPoint - center).sqrMagnitude <= safeRadius * safeRadius;
    }

    bool Contains(Vector3 worldPoint)
    {
        if (volume == null)
            return false;

        Vector3 local = transform.InverseTransformPoint(worldPoint);
        return ContainsLocalPoint(local, volume.center, volume.radius);
    }

    static SceneInfoPanel FindLocalPanel()
    {
        if (!NetworkClient.active || NetworkClient.localPlayer == null)
            return null;

        return NetworkClient.localPlayer.GetComponent<SceneInfoPanel>();
    }
}
