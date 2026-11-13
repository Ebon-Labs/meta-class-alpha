using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraEditorController : NetworkBehaviour
{
    public const string MenuScene = "Menu";
    public const float MaxPitchUpDegrees = 45f;
    public const float MaxPitchDownDegrees = 45f;

    public float sensitivity = 5f;

    float mouseX;
    float mouseY;

    public static bool ShowsCommonClick(string sceneName)
    {
        return sceneName == MenuScene;
    }

    public static void ReleaseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static float ClampPitch(float pitch)
    {
        return Mathf.Clamp(pitch, -MaxPitchUpDegrees, MaxPitchDownDegrees);
    }

    public static Quaternion ClampLookPitch(Quaternion look)
    {
        float pitch = AvatarHeadYaw.LookPitch(look);
        float clamped = ClampPitch(pitch);
        if (Mathf.Abs(Mathf.DeltaAngle(pitch, clamped)) <= 0.01f)
            return look;
        return Quaternion.Euler(clamped, AvatarHeadYaw.LookYaw(look), AvatarHeadYaw.LookRoll(look));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ShowsCommonClick(scene.name))
            ReleaseCursor();
    }

    void Start()
    {
        if (!Application.isEditor)
            return;

        print("We are running this from inside of the editor!");
    }

    void Update()
    {
        if (!isLocalPlayer || !Application.isEditor)
            return;

        EditorMouseLook.Accumulate(ref mouseX, ref mouseY, Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), sensitivity);
        mouseY = ClampPitch(mouseY);
        FloorCarry.ConsumeYawDegrees();

        Camera camera = GetComponentInChildren<Camera>();
        Transform look = camera != null ? camera.transform : transform;
        look.localRotation = Quaternion.Euler(mouseY, mouseX, 0f);
    }

    void LateUpdate()
    {
        if (!isLocalPlayer)
            return;

        Camera camera = GetComponentInChildren<Camera>();
        if (camera == null)
            return;

        Transform look = camera.transform;
        Quaternion clamped = ClampLookPitch(look.localRotation);
        if (clamped != look.localRotation)
            look.localRotation = clamped;
    }
}
