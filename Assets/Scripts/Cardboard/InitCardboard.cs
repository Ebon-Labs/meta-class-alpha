using Google.XR.Cardboard;
using UnityEngine;

public class InitCardboard : MonoBehaviour
{
    public Material starsSkyboxMaterial;

    public void Start()
    {
        ApplyStarsBackground();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;

        if (CardboardSession.ShouldStoreDefault(Api.HasDeviceParams()))
            CardboardSession.StoreDefaultViewer();
    }

    public void Update()
    {
        if (Api.IsCloseButtonPressed)
        {
            Application.Quit();
        }

        if (Api.IsTriggerHeldPressed)
        {
            Api.Recenter();
        }

        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }

        #if !UNITY_EDITOR
            Api.UpdateScreenParams();
        #endif
    }

    public static CameraClearFlags ResolveClearFlags(CameraClearFlags current)
    {
        if (current == CameraClearFlags.SolidColor)
            return CameraClearFlags.Skybox;

        return current;
    }

    void ApplyStarsBackground()
    {
        if (starsSkyboxMaterial != null)
            RenderSettings.skybox = starsSkyboxMaterial;

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
            camera.clearFlags = ResolveClearFlags(camera.clearFlags);
    }
}
