using System.Collections;
using Google.XR.Cardboard;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class CardboardSession : MonoBehaviour
{
    public const float MenuOrthographicSize = 5f;
    public const float MenuFieldOfView = 60f;
    public const string DefaultViewerUri =
        "https://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0rGBU9JQHegj0qEAAASEIAAEhCAABIQgAASEJYADUpXA89OggeZnc-Ej6aPlAAYAM";

    static CardboardSession current;

    int ticket;

    public static bool RunsOnThisPlayer()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    public static bool UsesViewer(string sceneName)
    {
        return VrEntryPrompt.Covers(sceneName);
    }

    public static bool ShouldStoreDefault(bool hasParams)
    {
        return !hasParams;
    }

    public static void StoreDefaultViewer()
    {
        Api.SaveDeviceParams(DefaultViewerUri);
        Api.ReloadDeviceParams();
    }

    public static void RestoreFlatCamera(Camera camera, bool orthographic, float orthographicSize, float fieldOfView)
    {
        if (camera == null)
            return;

        camera.ResetAspect();
        camera.orthographic = orthographic;
        camera.orthographicSize = orthographicSize;
        camera.fieldOfView = fieldOfView;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        if (!RunsOnThisPlayer())
            return;

        var host = new GameObject("CardboardSession");
        DontDestroyOnLoad(host);
        current = host.AddComponent<CardboardSession>();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (current == null)
            return;

        current.Apply(scene.name);
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }

    public void Apply(string sceneName)
    {
        if (UsesViewer(sceneName))
            Enter();
        else
            Leave();
    }

    void Enter()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        int mine = ++ticket;
        StartCoroutine(StartViewer(mine));
    }

    void Leave()
    {
        ticket++;
        StopAllCoroutines();

        XRManagerSettings manager = Manager();
        if (manager != null && manager.isInitializationComplete)
        {
            manager.StopSubsystems();
            manager.DeinitializeLoader();
            RestoreFlatCamera(Camera.main, true, MenuOrthographicSize, MenuFieldOfView);
        }
    }

    IEnumerator StartViewer(int mine)
    {
        XRManagerSettings manager = Manager();
        if (manager == null)
            yield break;

        if (!manager.isInitializationComplete)
            yield return manager.InitializeLoader();

        if (mine != ticket || manager.activeLoader == null)
        {
            if (manager.isInitializationComplete)
            {
                manager.StopSubsystems();
                manager.DeinitializeLoader();
            }

            yield break;
        }

        manager.StartSubsystems();
        if (ShouldStoreDefault(Api.HasDeviceParams()))
            StoreDefaultViewer();
    }

    void Update()
    {
        XRManagerSettings manager = Manager();
        if (manager == null || !manager.isInitializationComplete)
            return;

        if (Api.IsCloseButtonPressed)
        {
            MenuReturn.Leave();
            return;
        }

        if (Api.IsTriggerHeldPressed)
            Api.Recenter();

        if (Api.HasNewDeviceParams())
            Api.ReloadDeviceParams();

        Api.UpdateScreenParams();
    }

    static XRManagerSettings Manager()
    {
        return XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
    }
}
