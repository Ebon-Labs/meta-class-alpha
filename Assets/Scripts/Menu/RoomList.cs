using System;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomList : MonoBehaviour
{
    public const float HitPadding = 48f;
    public const float RoomTimeout = 7f;
    public const string EmptyLabel = "...";
    public const string ItemName = "RoomItem";

    public static RoomList Instance { get; private set; }

    readonly Dictionary<long, ServerResponse> rooms = new Dictionary<long, ServerResponse>();
    readonly Dictionary<long, float> seenAt = new Dictionary<long, float>();
    readonly List<long> expired = new List<long>();
    readonly List<ServerResponse> shown = new List<ServerResponse>();
    TextMeshPro template;
    NetworkDiscovery discovery;

    public static string Label(Uri uri)
    {
        if (uri == null || string.IsNullOrEmpty(uri.Host))
            return "";

        if (uri.Port <= 0)
            return uri.Host;

        return uri.Host + ":" + uri.Port;
    }

    public static Vector2 ItemPosition(int index, float lineHeight)
    {
        return new Vector2(0f, -Mathf.Max(0, index) * Mathf.Max(0f, lineHeight));
    }

    public static int Nearest(Vector2 screen, IList<Vector2> min, IList<Vector2> max, IList<Vector2> centers, float padding)
    {
        int chosen = -1;
        float nearest = float.PositiveInfinity;
        if (min == null || max == null || centers == null)
            return chosen;

        int count = Mathf.Min(min.Count, Mathf.Min(max.Count, centers.Count));
        for (int i = 0; i < count; i++)
        {
            if (!SolarSystemThumbHost.Contains(screen, min[i], max[i], padding))
                continue;

            float distance = (screen - centers[i]).sqrMagnitude;
            if (distance >= nearest)
                continue;

            nearest = distance;
            chosen = i;
        }

        return chosen;
    }

    public static bool Fresh(float seenAt, float now, float timeout)
    {
        return now - seenAt <= timeout;
    }

    public static bool JoinPressed(Vector2 screen, Camera view)
    {
        return Instance != null && Instance.TryJoin(screen, view);
    }

    void OnEnable()
    {
        Instance = this;
        template = GetComponent<TextMeshPro>();
        discovery = FindFirstObjectByType<NetworkDiscovery>();
        if (discovery == null)
            return;

        discovery.OnServerFound.AddListener(Remember);
        LanRoomBeacon.Ensure(discovery);
    }

    void Update()
    {
        if (Prune(Time.unscaledTime))
            Rebuild();
    }

    void OnDisable()
    {
        if (discovery != null)
            discovery.OnServerFound.RemoveListener(Remember);

        if (Instance == this)
            Instance = null;
    }

    void Remember(ServerResponse response)
    {
        bool changed = !rooms.TryGetValue(response.serverId, out ServerResponse existing) || existing.uri != response.uri;
        rooms[response.serverId] = response;
        seenAt[response.serverId] = Time.unscaledTime;
        if (changed)
            Rebuild();
    }

    bool Prune(float now)
    {
        expired.Clear();
        foreach (KeyValuePair<long, float> pair in seenAt)
        {
            if (!Fresh(pair.Value, now, RoomTimeout))
                expired.Add(pair.Key);
        }

        if (expired.Count == 0)
            return false;

        for (int i = 0; i < expired.Count; i++)
        {
            rooms.Remove(expired[i]);
            seenAt.Remove(expired[i]);
        }

        return true;
    }

    public bool TryJoin(Vector2 screen, Camera view)
    {
        if (view == null || shown.Count == 0)
            return false;

        var min = new List<Vector2>(shown.Count);
        var max = new List<Vector2>(shown.Count);
        var centers = new List<Vector2>(shown.Count);
        var hits = new List<int>(shown.Count);
        for (int i = 0; i < transform.childCount && i < shown.Count; i++)
        {
            if (!CarouselMenu.TryScreenBounds(transform.GetChild(i).gameObject, view, out Vector2 itemMin, out Vector2 itemMax, out Vector2 center))
                continue;

            hits.Add(i);
            min.Add(itemMin);
            max.Add(itemMax);
            centers.Add(center);
        }

        int chosen = Nearest(screen, min, max, centers, HitPadding);
        if (chosen < 0)
            return false;

        Join(shown[hits[chosen]]);
        return true;
    }

    public void Join(ServerResponse response)
    {
        NetworkManager manager = NetworkManager.singleton;
        if (manager == null || response.uri == null || NetworkClient.active)
            return;

        if (discovery != null)
            discovery.StopDiscovery();

        manager.networkAddress = response.uri.Host;
        manager.StartClient(response.uri);
    }

    void Rebuild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        shown.Clear();
        if (template != null)
            template.text = rooms.Count == 0 ? EmptyLabel : "";

        int index = 0;
        foreach (ServerResponse room in rooms.Values)
        {
            shown.Add(room);
            CreateItem(room, index);
            index++;
        }
    }

    void CreateItem(ServerResponse room, int index)
    {
        GameObject item = new GameObject(ItemName);
        item.transform.SetParent(transform, false);

        TextMeshPro text = item.AddComponent<TextMeshPro>();
        text.text = Label(room.uri);
        text.raycastTarget = false;
        if (template == null)
            return;

        text.font = template.font;
        text.fontSharedMaterial = template.fontSharedMaterial;
        text.fontSize = template.fontSize;
        text.color = template.color;
        text.alignment = template.alignment;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = template.rectTransform.anchorMin;
        rect.anchorMax = template.rectTransform.anchorMax;
        rect.pivot = template.rectTransform.pivot;
        rect.sizeDelta = template.rectTransform.sizeDelta;
        rect.anchoredPosition = ItemPosition(index, template.rectTransform.sizeDelta.y);
    }
}
