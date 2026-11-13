using Mirror;
using Mirror.Discovery;
using UnityEngine;

[DisallowMultipleComponent]
public class SolarSystemThumbHost : MonoBehaviour
{
    public const float HitPadding = 48f;
    public const string ObjectName = "SolarSystemThumb";
    public const string SolarSystemScene = "Assets/Scenes/SolarSystem.unity";

    public static bool CanStartHost(bool clientConnected, bool clientActive, bool serverActive, bool webGl)
    {
        return !clientConnected && !clientActive && !serverActive && !webGl;
    }

    public static bool FacesCamera(Vector3 canvasForward, Vector3 cameraForward)
    {
        return Vector3.Dot(canvasForward, cameraForward) > 0.35f;
    }

    public static bool Contains(Vector2 screen, Vector2 min, Vector2 max, float padding)
    {
        Vector2 margin = new Vector2(padding, padding);
        min -= margin;
        max += margin;
        return screen.x >= min.x && screen.x <= max.x && screen.y >= min.y && screen.y <= max.y;
    }

    public static Vector2 OrthoPixels(float worldWidth, float worldHeight, float orthographicSize, float pixelHeight)
    {
        float pixels = pixelHeight / (Mathf.Max(0.0001f, orthographicSize) * 2f);
        return new Vector2(Mathf.Abs(worldWidth) * pixels, Mathf.Abs(worldHeight) * pixels);
    }

    public static bool Hits(GameObject target, Vector2 screen, Camera view)
    {
        if (target == null || view == null || !FacesCamera(target.transform.forward, view.transform.forward))
            return false;

        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screen, view))
            return true;

        if (rect != null && CoversOrtho(rect, screen, view))
            return true;

        if (CarouselMenu.TryScreenBounds(target, view, out Vector2 min, out Vector2 max, out _)
            && Contains(screen, min, max, HitPadding)
            && CoversScreen(min, max))
            return true;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (CarouselMenu.TryScreenBounds(renderers[i].gameObject, view, out Vector2 glyphMin, out Vector2 glyphMax, out _)
                && Contains(screen, glyphMin, glyphMax, HitPadding))
                return true;
        }

        return false;
    }

    public static bool CoversScreen(Vector2 min, Vector2 max)
    {
        return max.x - min.x >= 24f && max.y - min.y >= 24f;
    }

    public static bool CoversOrtho(RectTransform rect, Vector2 screen, Camera view)
    {
        if (rect == null || view == null || !view.orthographic)
            return false;

        Vector3 projected = view.WorldToScreenPoint(rect.position);
        if (projected.z < 0f)
            return false;

        Vector2 size = rect.rect.size;
        Vector3 scale = rect.lossyScale;
        Vector2 pixels = OrthoPixels(size.x * scale.x, size.y * scale.y, view.orthographicSize, view.pixelHeight);
        if (!CoversScreen(Vector2.zero, pixels))
            return false;

        Vector2 half = pixels * 0.5f;
        Vector2 center = projected;
        return Contains(screen, center - half, center + half, HitPadding);
    }

    public static bool Press(Vector2 screen, Camera view)
    {
        return Hits(GameObject.Find(ObjectName), screen, view);
    }

    public static void StartHostSession()
    {
        StartHostScene(SolarSystemScene);
    }

    public static void StartHostScene(string scene)
    {
        bool webGl = Application.platform == RuntimePlatform.WebGLPlayer;
        if (!CanStartHost(NetworkClient.isConnected, NetworkClient.active, NetworkServer.active, webGl))
            return;

        NetworkManager manager = NetworkManager.singleton;
        if (manager == null)
            manager = Object.FindFirstObjectByType<NetworkManager>();
        if (manager == null)
            return;

        if (!string.IsNullOrEmpty(scene))
            manager.onlineScene = scene;

        manager.StartHost();
        Advertise(manager.GetComponent<NetworkDiscovery>());
    }

    public void Host()
    {
        StartHostSession();
    }

    public static void Advertise(NetworkDiscovery discovery)
    {
        if (discovery == null || !NetworkDiscovery.SupportedOnThisPlatform)
            return;

        discovery.AdvertiseServer();
    }
}
