using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class VrEntryPrompt : MonoBehaviour
{
    public const float DurationSeconds = 5f;
    public const string SolarSystemScene = "SolarSystem";
    public const string TutorialScene = "Tutorial";
    public const string PosterResource = "VrEntryPoster";
    public const string WelcomeResource = "TutorialWelcome";

    static VrEntryPrompt current;

    float hideAt;
    bool finished;
    bool frozeBody;
    bool bodyWasKinematic;
    Rigidbody frozenBody;

    public static bool Covers(string sceneName)
    {
        return sceneName == SolarSystemScene || sceneName == TutorialScene;
    }

    public static bool StillShowing(float now, float hideAt)
    {
        return now < hideAt;
    }

    public static bool ShouldPlayWelcome(string sceneName, bool alreadyPlayed)
    {
        return sceneName == TutorialScene && !alreadyPlayed;
    }

    public static bool RestoreEnabled(bool wasEnabled, bool isLocalPlayer)
    {
        return wasEnabled || isLocalPlayer;
    }

    public static Vector2 FitSize(float viewAspect, float imageAspect, float distance, float fieldOfViewDegrees)
    {
        float viewHeight = 2f * distance * Mathf.Tan(fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad);
        float viewWidth = viewHeight * Mathf.Max(0.01f, viewAspect);
        if (imageAspect <= 0f)
            return new Vector2(viewWidth, viewHeight);

        if (viewAspect > imageAspect)
            return new Vector2(viewHeight * imageAspect, viewHeight);

        return new Vector2(viewWidth, viewWidth / imageAspect);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Covers(scene.name))
            return;

        if (current != null)
            Destroy(current.gameObject);

        var host = new GameObject("VrEntryPrompt");
        current = host.AddComponent<VrEntryPrompt>();
    }

    void Awake()
    {
        hideAt = Time.unscaledTime + DurationSeconds;
        CreateOverlay(LoadPoster());
    }

    void OnEnable()
    {
        Application.onBeforeRender += ShowPlayerBeforeRender;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= ShowPlayerBeforeRender;
    }

    void LateUpdate()
    {
        if (!StillShowing(Time.unscaledTime, hideAt))
        {
            Finish();
            return;
        }

        ShowPlayer();
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }

    Texture2D LoadPoster()
    {
        VrEntryPoster posterAsset = Resources.Load<VrEntryPoster>(PosterResource);
        return posterAsset != null ? posterAsset.Image : null;
    }

    void CreateOverlay(Texture2D image)
    {
        var canvasObject = new GameObject("VrPromptCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        canvasObject.AddComponent<CanvasScaler>();

        var background = new GameObject("Background");
        background.transform.SetParent(canvasObject.transform, false);
        var backdrop = background.AddComponent<Image>();
        backdrop.color = Color.black;
        Stretch(background.GetComponent<RectTransform>());

        if (image == null)
            return;

        var picture = new GameObject("Picture");
        picture.transform.SetParent(background.transform, false);
        var raw = picture.AddComponent<RawImage>();
        raw.texture = image;
        raw.raycastTarget = false;
        Stretch(picture.GetComponent<RectTransform>());

        var fitter = picture.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = image.height > 0 ? image.width / (float)image.height : 1f;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    void ShowPlayerBeforeRender()
    {
        if (!StillShowing(Time.unscaledTime, hideAt))
            return;

        ShowPlayer();
    }

    void ShowPlayer()
    {
        NetworkIdentity identity = FindLocalIdentity();
        if (identity == null)
            return;

        EnableLocalView(identity);
        FreezeLocalPlayer(identity);
    }

    void Finish()
    {
        if (finished)
            return;

        finished = true;
        Application.onBeforeRender -= ShowPlayerBeforeRender;
        ReleaseLocalPlayer();

        NetworkIdentity identity = FindLocalIdentity();
        PlaceLocalPlayer(identity);
        EnableLocalView(identity);
        PlayWelcome(gameObject.scene.name);
        Destroy(gameObject);
    }

    static void PlayWelcome(string sceneName)
    {
        if (!ShouldPlayWelcome(sceneName, alreadyPlayed: false))
            return;

        TutorialWelcome welcome = Resources.Load<TutorialWelcome>(WelcomeResource);
        AudioClip clip = welcome != null ? welcome.Clip : null;
        if (clip == null)
            return;

        EnsureListener();

        var host = new GameObject("TutorialWelcomeVoice");
        AudioSource source = host.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0f;
        source.loop = false;
        source.playOnAwake = false;
        source.Play();
        Destroy(host, clip.length);
    }

    static void EnsureListener()
    {
        NetworkIdentity identity = FindLocalIdentity();
        if (identity == null)
            return;

        AudioListener listener = identity.GetComponentInChildren<AudioListener>(true);
        if (listener != null)
            listener.enabled = true;
    }

    void FreezeLocalPlayer(NetworkIdentity identity)
    {
        Rigidbody body = identity != null ? identity.GetComponent<Rigidbody>() : null;
        if (body == null)
            return;

        if (!frozeBody)
        {
            frozenBody = body;
            bodyWasKinematic = body.isKinematic;
            frozeBody = true;
        }

        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        body.isKinematic = true;
    }

    void ReleaseLocalPlayer()
    {
        if (!frozeBody || frozenBody == null)
            return;

        frozenBody.isKinematic = bodyWasKinematic;
        frozeBody = false;
    }

    static void PlaceLocalPlayer(NetworkIdentity identity)
    {
        if (identity == null)
            return;

        ClassController room = ClassController.ResolveExisting(FindFirstObjectByType<ClassController>());
        if (room == null)
            return;

        room.PlacePlayer(identity.transform, IsHostPlayer(identity));
    }

    static void EnableLocalView(NetworkIdentity identity)
    {
        if (identity == null)
            return;

        Camera playerCamera = identity.GetComponentInChildren<Camera>(true);
        if (playerCamera == null)
            return;

        playerCamera.enabled = true;
        playerCamera.targetDisplay = 0;

        Canvas canvas = identity.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
            canvas.enabled = true;
    }

    static NetworkIdentity FindLocalIdentity()
    {
        if (NetworkClient.active && NetworkClient.localPlayer != null)
            return NetworkClient.localPlayer;

        NetworkIdentity[] identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < identities.Length; i++)
        {
            if (identities[i] != null && identities[i].isLocalPlayer)
                return identities[i];
        }

        return null;
    }

    static bool IsHostPlayer(NetworkIdentity identity)
    {
        if (identity == null || !NetworkServer.active)
            return false;

        return identity.connectionToClient != null
            && NetworkServer.localConnection != null
            && identity.connectionToClient == NetworkServer.localConnection;
    }
}
