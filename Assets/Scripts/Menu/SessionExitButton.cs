using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SessionExitButton : MonoBehaviour
{
    public const string Label = "SAIR";
    public const int SortingOrder = 6000;

    static SessionExitButton current;

    public static bool Shows(string sceneName)
    {
        return !string.IsNullOrEmpty(sceneName) && sceneName != MenuReturn.MenuScene;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Shows(scene.name))
        {
            if (current != null)
                Destroy(current.gameObject);
            return;
        }

        if (current != null)
            return;

        var host = new GameObject("SessionExit");
        DontDestroyOnLoad(host);
        current = host.AddComponent<SessionExitButton>();
        current.CreateButton();
        CameraEditorController.ReleaseCursor();
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }

    public void Press()
    {
        MenuReturn.Leave();
    }

    void CreateButton()
    {
        var canvasObject = new GameObject("ExitCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        var buttonObject = new GameObject("Sair");
        buttonObject.transform.SetParent(canvasObject.transform, false);
        var rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -24f);
        rect.sizeDelta = new Vector2(160f, 64f);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(Press);

        var labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);
        var labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var label = labelObject.AddComponent<Text>();
        label.text = Label;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.fontSize = 28;
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
