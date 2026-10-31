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
    [SerializeField] bool interactionOrbsEnabled = false;

    [Tooltip("Shows or hides every PlanetLabel under this SolarSystem.")]
    [SerializeField] bool showPlanetLabels = true;

    /// <summary>Factor applied by OrbitButton to leave the default pace.</summary>
    public const float IncreasedTimeScaleMultiplier = SolarMotionSnapshot.IncreasedTimeScaleMultiplier;

    float _defaultTimeScale = 4320f;
    float _appliedTimeScale = -1f;
    float _appliedDistanceScale = -1f;
    bool _appliedShowInteractionOrbs;
    bool _appliedShowPlanetLabels;

    /// <summary>Earth orbit radius used by the close-distance view.</summary>
    public const float NearDistanceScale = SolarMotionSnapshot.NearDistanceScale;

    /// <summary>Maximum Earth orbit radius in scene units (scientific scale).</summary>
    public float MaxDistanceScale => PlanetScientificCatalog.EarthOrbitSceneUnits;

    public float DistanceScale => distanceScale;

    public bool IsNearDistance => Capture().IsNearDistance;

    public void ToggleDistanceScale()
    {
        Store(Capture().ToggleDistance());
        ApplyToPlanets(force: true);
    }

    public void SetDistanceScale(float scale)
    {
        Store(Capture().WithDistance(scale));
        ApplyToPlanets(force: true);
    }

    public bool ShowInteractionOrbs => interactionOrbsEnabled;

    public void ToggleShowInteractionOrbs()
    {
        Store(Capture().ToggleOrbs());
        ApplyToPlanets(force: true);
    }

    public void SetShowInteractionOrbs(bool show)
    {
        Store(Capture().WithOrbs(show));
        ApplyToPlanets(force: true);
    }

    public float TimeScale => timeScale;

    public bool IsIncreasedTimeScale => Capture().IsIncreasedTimeScale;

    public void ToggleTimeScale()
    {
        Store(Capture().ToggleTime());
        ApplyToPlanets(force: true);
    }

    public void SetTimeScale(float scale)
    {
        Store(Capture().WithTime(scale));
        ApplyToPlanets(force: true);
    }

    void Awake()
    {
        _defaultTimeScale = timeScale;
    }

    void OnValidate()
    {
        distanceScale = Capture().Clamped().DistanceScale;
    }

    void Start()
    {
        // Near distance keeps the orbits stopped. Common distance keeps the captured default pace.
        Store(Capture().StoppedIfNear());

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
        bool orbsChanged = force || _appliedShowInteractionOrbs != interactionOrbsEnabled;
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
        _appliedShowInteractionOrbs = interactionOrbsEnabled;
        _appliedShowPlanetLabels = showPlanetLabels;
    }

    void ApplyInteractionOrbVisibility()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform child = transforms[i];
            if (child == null || !PlanetOrbitMotion.LocksChildPose(child.name))
                continue;

            GameObject orb = child.gameObject;
            if (orb.activeSelf != interactionOrbsEnabled)
                orb.SetActive(interactionOrbsEnabled);
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

    SolarMotionSnapshot Capture()
    {
        return new SolarMotionSnapshot(
            _defaultTimeScale,
            timeScale,
            distanceScale,
            interactionOrbsEnabled,
            showPlanetLabels,
            MaxDistanceScale);
    }

    void Store(SolarMotionSnapshot snapshot)
    {
        timeScale = snapshot.TimeScale;
        distanceScale = snapshot.DistanceScale;
        interactionOrbsEnabled = snapshot.InteractionOrbsEnabled;
        showPlanetLabels = snapshot.ShowPlanetLabels;
    }
}
