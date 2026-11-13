using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MenuReturn : MonoBehaviour
{
    public const float DelaySeconds = 3f;
    public const string MenuScene = "Menu";

    float elapsed;
    bool running;

    public bool IsRunning => running;

    public enum LeaveKind
    {
        LoadMenu,
        Host,
        Client,
        Server
    }

    public static void Begin(GameObject target)
    {
        if (target == null)
            return;

        MenuReturn menuReturn = target.GetComponent<MenuReturn>();
        if (menuReturn == null || menuReturn.running)
            return;

        menuReturn.elapsed = 0f;
        menuReturn.running = true;
    }

    public static float Advance(float elapsed, float deltaTime)
    {
        if (deltaTime <= 0f)
            return elapsed;

        return elapsed + deltaTime;
    }

    public static bool IsDue(float elapsed, float delay)
    {
        return delay > 0f && elapsed >= delay;
    }

    public static LeaveKind Choose(bool hasManager, bool serverActive, bool clientActive)
    {
        if (hasManager && serverActive && clientActive)
            return LeaveKind.Host;
        if (hasManager && clientActive)
            return LeaveKind.Client;
        if (hasManager && serverActive)
            return LeaveKind.Server;
        return LeaveKind.LoadMenu;
    }

    void Update()
    {
        if (!running)
            return;

        elapsed = Advance(elapsed, Time.deltaTime);
        if (!IsDue(elapsed, DelaySeconds))
            return;

        running = false;
        Leave();
    }

    public static void Leave()
    {
        NetworkManager manager = NetworkManager.singleton;
        switch (Choose(manager != null, NetworkServer.active, NetworkClient.active))
        {
            case LeaveKind.Host:
                manager.StopHost();
                return;
            case LeaveKind.Client:
                manager.StopClient();
                return;
            case LeaveKind.Server:
                manager.StopServer();
                return;
        }

        string scene = manager != null && !string.IsNullOrEmpty(manager.offlineScene)
            ? manager.offlineScene
            : MenuScene;
        SceneManager.LoadScene(scene);
    }
}
