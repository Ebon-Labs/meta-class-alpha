using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class MainScreen : MonoBehaviour
{
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int TintId = Shader.PropertyToID("_Tint");
    static readonly int TintColorId = Shader.PropertyToID("_TintColor");

    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] Renderer screenRenderer;
    [SerializeField] float fadeDuration = 3f;

    Material material;
    int tintProperty;
    Color tint;
    Coroutine tintFade;

    void Awake()
    {
        if (screenRenderer == null)
            screenRenderer = FindScreenRenderer();
        if (videoPlayer == null)
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        material = screenRenderer.material;
        tintProperty = ResolveTintProperty(material);
        EnableFade(material);

        tint = material.GetColor(tintProperty);
        tint.a = 0f;
        material.SetColor(tintProperty, tint);
    }

    void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.started += OnVideoStarted;
    }

    void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.started -= OnVideoStarted;
    }

    void OnDestroy()
    {
        if (material != null)
            Destroy(material);
    }

    public void Play()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.Play();
    }

    public void Pause()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.Pause();
    }

    public void Stop()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.Stop();
    }

    void OnVideoStarted(VideoPlayer source)
    {
        if (tintFade != null)
            StopCoroutine(tintFade);

        tintFade = StartCoroutine(FadeTint());
    }

    System.Collections.IEnumerator FadeTint()
    {
        tint.a = 0f;
        material.SetColor(tintProperty, tint);

        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tint.a = Mathf.Clamp01(elapsed / duration);
            material.SetColor(tintProperty, tint);
            yield return null;
        }

        tint.a = 1f;
        material.SetColor(tintProperty, tint);
        tintFade = null;
    }

    Renderer FindScreenRenderer()
    {
        Transform screen = transform.Find("MainScreen");
        if (screen != null)
        {
            Renderer renderer = screen.GetComponent<Renderer>();
            if (renderer != null)
                return renderer;
        }

        return GetComponentInChildren<Renderer>(true);
    }

    static int ResolveTintProperty(Material source)
    {
        if (source.HasProperty(TintId))
            return TintId;
        if (source.HasProperty(TintColorId))
            return TintColorId;
        return ColorId;
    }

    static void EnableFade(Material source)
    {
        if (!source.HasProperty("_Mode"))
            return;

        source.SetFloat("_Mode", 2f);
        source.SetOverrideTag("RenderType", "Transparent");
        source.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        source.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        source.SetInt("_ZWrite", 0);
        source.DisableKeyword("_ALPHATEST_ON");
        source.EnableKeyword("_ALPHABLEND_ON");
        source.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        source.renderQueue = (int)RenderQueue.Transparent;
    }
}
