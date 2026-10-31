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

        // Checks if the device parameters are stored and scans them if not.
        if (!Api.HasDeviceParams())
        {
            Api.ScanDeviceParams();
        }
    }

    public void Update()
    {
        if (Api.IsGearButtonPressed)
        {
            Api.ScanDeviceParams();
        }

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
