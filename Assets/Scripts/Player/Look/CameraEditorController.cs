using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraEditorController : NetworkBehaviour
{
    public const string MenuScene = "Menu";

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
        mouseX += FloorCarry.ConsumeYawDegrees();

        Camera camera = GetComponentInChildren<Camera>();
        Transform look = camera != null ? camera.transform : transform;
        look.localRotation = Quaternion.Euler(mouseY, mouseX, 0f);
    }
}
