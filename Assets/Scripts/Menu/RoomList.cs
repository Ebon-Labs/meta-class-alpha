using System;
using System.Collections.Generic;
using System.Text;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomList : MonoBehaviour
{
    public const float HitPadding = 48f;
    public const float RoomTimeout = 7f;

    public static RoomList Instance { get; private set; }

    readonly Dictionary<long, ServerResponse> rooms = new Dictionary<long, ServerResponse>();
    readonly Dictionary<long, float> seenAt = new Dictionary<long, float>();
    readonly List<long> expired = new List<long>();
    readonly List<ServerResponse> shown = new List<ServerResponse>();
    readonly List<string> labels = new List<string>();
    TMP_InputField field;
    TMP_Text body;
    NetworkDiscovery discovery;

    public static string Label(Uri uri)
    {
        if (uri == null || string.IsNullOrEmpty(uri.Host))
            return "";

        if (uri.Port <= 0)
            return uri.Host;

        return uri.Host + ":" + uri.Port;
    }

    public static string Label(string alias, Uri uri)
    {
        if (!string.IsNullOrEmpty(alias))
            return alias;

        return Label(uri);
    }

    public static string Body(IList<string> labels)
    {
        if (labels == null || labels.Count == 0)
            return "";

        var text = new StringBuilder();
        for (int i = 0; i < labels.Count; i++)
        {
            if (i > 0)
                text.Append('\n');

            text.Append(labels[i] ?? "");
        }

        return text.ToString();
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
            if (!ImageMenuThumb.Contains(screen, min[i], max[i], padding))
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

    public static bool CanJoinAsGuest(bool serverActive, bool clientActive)
    {
        return !serverActive && !clientActive;
    }

    public static bool JoinPressed(Vector2 screen, Camera view)
    {
        return Instance != null && Instance.TryJoin(screen, view);
    }

    void OnEnable()
    {
        Instance = this;
        field = GetComponent<TMP_InputField>();
        body = field != null ? field.textComponent : GetComponentInChildren<TMP_Text>(true);
        PrepareField();
        Rebuild();
        discovery = FindFirstObjectByType<NetworkDiscovery>();
        if (discovery == null)
            return;

        discovery.OnServerFound.AddListener(Remember);
        LanRoomBeacon.Ensure(discovery);
    }

    void PrepareField()
    {
        if (field == null)
            return;

        field.readOnly = true;
        field.resetOnDeActivation = false;
        field.onFocusSelectAll = false;
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
        bool changed = !rooms.TryGetValue(response.serverId, out ServerResponse existing)
            || existing.uri != response.uri
            || existing.alias != response.alias;
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
        if (view == null || shown.Count == 0 || body == null)
            return false;

        body.ForceMeshUpdate();
        int line = LineUnder(body, screen, view, HitPadding);
        if (line < 0 || line >= shown.Count)
            return false;

        Join(shown[line]);
        return true;
    }

    public static int LineUnder(TMP_Text text, Vector2 screen, Camera view, float padding)
    {
        if (text == null || view == null || text.textInfo == null)
            return -1;

        var min = new List<Vector2>();
        var max = new List<Vector2>();
        var centers = new List<Vector2>();
        var lines = new List<int>();
        int count = text.textInfo.lineCount;
        for (int i = 0; i < count; i++)
        {
            if (text.textInfo.lineInfo[i].characterCount <= 0)
                continue;

            if (!TryLineBounds(text, i, view, out Vector2 itemMin, out Vector2 itemMax, out Vector2 center))
                continue;

            lines.Add(i);
            min.Add(itemMin);
            max.Add(itemMax);
            centers.Add(center);
        }

        int chosen = Nearest(screen, min, max, centers, padding);
        return chosen < 0 ? -1 : lines[chosen];
    }

    public static bool TryLineBounds(TMP_Text text, int line, Camera view, out Vector2 min, out Vector2 max, out Vector2 center)
    {
        min = max = center = default;
        if (text == null || text.textInfo == null || line < 0 || line >= text.textInfo.lineCount)
            return false;

        TMP_LineInfo info = text.textInfo.lineInfo[line];
        Vector3 bottomLeft = text.rectTransform.TransformPoint(new Vector3(info.lineExtents.min.x, info.lineExtents.min.y, 0f));
        Vector3 bottomRight = text.rectTransform.TransformPoint(new Vector3(info.lineExtents.max.x, info.lineExtents.min.y, 0f));
        Vector3 topLeft = text.rectTransform.TransformPoint(new Vector3(info.lineExtents.min.x, info.lineExtents.max.y, 0f));
        Vector3 topRight = text.rectTransform.TransformPoint(new Vector3(info.lineExtents.max.x, info.lineExtents.max.y, 0f));
        return CarouselMenu.ScreenBounds(view, new[] { bottomLeft, bottomRight, topLeft, topRight }, out min, out max, out center);
    }

    public void Join(ServerResponse response)
    {
        NetworkManager manager = NetworkManager.singleton;
        if (manager == null || response.uri == null)
            return;

        if (!CanJoinAsGuest(NetworkServer.active, NetworkClient.active))
            return;

        if (discovery != null)
            discovery.StopDiscovery();

        manager.networkAddress = response.uri.Host;
        PlayerPortraitChoice.Apply(manager);
        manager.StartClient(response.uri);
    }

    void Rebuild()
    {
        shown.Clear();
        labels.Clear();
        foreach (ServerResponse room in rooms.Values)
        {
            shown.Add(room);
            labels.Add(Label(room.alias, room.uri));
        }

        Show(Body(labels));
    }

    void Show(string text)
    {
        if (field != null)
        {
            field.text = text;
            return;
        }

        if (body != null)
            body.text = text;
    }
}
