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
    [Tooltip("Shows or hides every InteractionOrb under this SolarSystem.")]
    [SerializeField] bool showInteractionOrbs = true;

    [Tooltip("Shows or hides every PlanetLabel under this SolarSystem.")]
    [SerializeField] bool showPlanetLabels = true;

    float _appliedTimeScale = -1f;
    float _appliedDistanceScale = -1f;
    bool _appliedShowInteractionOrbs;
    bool _appliedShowPlanetLabels;

    /// <summary>Maximum Earth orbit radius in scene units (scientific scale).</summary>
    public float MaxDistanceScale => PlanetScientificCatalog.EarthOrbitSceneUnits;

    void OnValidate()
    {
        distanceScale = Mathf.Clamp(distanceScale, 0.01f, MaxDistanceScale);
    }

    void Start()
    {
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

                if (distanceChanged && motion.IsOrbitEnabled)
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
            if (child == null || !child.name.StartsWith("InteractionOrb"))
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
