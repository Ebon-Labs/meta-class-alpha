using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class SolarSystemMotionController : MonoBehaviour
{
    [Header("Time")]
    [Tooltip("Applied to every PlanetOrbitMotion under SolarSystem.")]
    [SerializeField] float timeScale = 4320f;

    [Header("Distance")]
    [Tooltip("Earth orbit radius used now. Clamped to MaxDistanceScale. Other planets keep AU proportions.")]
    [SerializeField] float distanceScale = PlanetScientificCatalog.EarthOrbitSceneUnits;

    [Header("Interaction")]
    [Tooltip("Shows or hides every InteractionMarkerOrb under this SolarSystem.")]
    [SerializeField] bool showInteractionOrbs = true;

    [Tooltip("Shows or hides every PlanetLabel under this SolarSystem.")]
    [SerializeField] bool showPlanetLabels = true;

    /// <summary>Factor applied by OrbitButton to leave the default pace.</summary>
    public const float IncreasedTimeScaleMultiplier = 1000f;

    float _defaultTimeScale = 4320f;
    float _appliedTimeScale = -1f;
    float _appliedDistanceScale = -1f;
    bool _appliedShowInteractionOrbs;
    bool _appliedShowPlanetLabels;

    /// <summary>Earth orbit radius used by the close-distance view.</summary>
    public const float NearDistanceScale = 3f;

    /// <summary>Maximum Earth orbit radius in scene units (scientific scale).</summary>
    public float MaxDistanceScale => PlanetScientificCatalog.EarthOrbitSceneUnits;

    public float DistanceScale => distanceScale;

    public bool IsNearDistance => Mathf.Approximately(distanceScale, NearDistanceScale);

    public void ToggleDistanceScale()
    {
        SetDistanceScale(IsNearDistance ? MaxDistanceScale : NearDistanceScale);
    }

    public void SetDistanceScale(float scale)
    {
        bool wasNear = IsNearDistance;
        distanceScale = scale;
        if (wasNear != IsNearDistance)
            timeScale = IsNearDistance ? 0f : _defaultTimeScale;

        ApplyToPlanets(force: true);
    }

    public bool ShowInteractionOrbs => showInteractionOrbs;

    public void ToggleShowInteractionOrbs()
    {
        SetShowInteractionOrbs(!showInteractionOrbs);
    }

    public void SetShowInteractionOrbs(bool show)
    {
        showInteractionOrbs = show;
        ApplyToPlanets(force: true);
    }

    public float TimeScale => timeScale;

    public bool IsIncreasedTimeScale =>
        Mathf.Approximately(timeScale, _defaultTimeScale * IncreasedTimeScaleMultiplier);

    public void ToggleTimeScale()
    {
        SetTimeScale(IsIncreasedTimeScale
            ? _defaultTimeScale
            : _defaultTimeScale * IncreasedTimeScaleMultiplier);
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
        ApplyToPlanets(force: true);
    }

    void Awake()
    {
        _defaultTimeScale = timeScale;
    }

    void OnValidate()
    {
        distanceScale = Mathf.Clamp(distanceScale, 0.01f, MaxDistanceScale);
    }

    void Start()
    {
        // Near distance keeps the orbits stopped. Common distance keeps the captured default pace.
        if (IsNearDistance)
            timeScale = 0f;

        // After PlanetOrbitMotion.Awake captured child poses / orbit offsets.
        ApplyToPlanets(force: true);
    }

    void LateUpdate()
    {
        ApplyToPlanets(force: false);
    }

    [ContextMenu("Apply To Planets")]
    public void ApplyToPlanets()
    {
        ApplyToPlanets(force: true);
    }

    void ApplyToPlanets(bool force)
    {
        OnValidate();

        bool timeChanged = force || !Mathf.Approximately(_appliedTimeScale, timeScale);
        bool distanceChanged = force || !Mathf.Approximately(_appliedDistanceScale, distanceScale);
        bool orbsChanged = force || _appliedShowInteractionOrbs != showInteractionOrbs;
        bool labelsChanged = force || _appliedShowPlanetLabels != showPlanetLabels;
        if (!timeChanged && !distanceChanged && !orbsChanged && !labelsChanged)
            return;

        if (timeChanged || distanceChanged)
        {
            PlanetOrbitMotion[] motions = GetComponentsInChildren<PlanetOrbitMotion>(true);
            for (int i = 0; i < motions.Length; i++)
            {
                PlanetOrbitMotion motion = motions[i];
                if (motion == null)
                    continue;

                if (timeChanged)
                    motion.SetTimeScale(timeScale);

                if (distanceChanged && motion.IsOrbitEnabled && motion.UseSolarDistanceScale)
                    motion.SetOrbitDistanceScale(distanceScale);
            }
        }

        if (orbsChanged)
            ApplyInteractionOrbVisibility();

        if (labelsChanged)
            ApplyPlanetLabelVisibility();

        _appliedTimeScale = timeScale;
        _appliedDistanceScale = distanceScale;
        _appliedShowInteractionOrbs = showInteractionOrbs;
        _appliedShowPlanetLabels = showPlanetLabels;
    }

    void ApplyInteractionOrbVisibility()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform child = transforms[i];
            if (child == null || !child.name.StartsWith("InteractionMarkerOrb"))
                continue;

            GameObject orb = child.gameObject;
            if (orb.activeSelf != showInteractionOrbs)
                orb.SetActive(showInteractionOrbs);
        }
    }

    void ApplyPlanetLabelVisibility()
    {
        PlanetLabel[] labels = GetComponentsInChildren<PlanetLabel>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            PlanetLabel planetLabel = labels[i];
            if (planetLabel == null)
                continue;

            planetLabel.SetVisible(showPlanetLabels);
        }
    }
}
