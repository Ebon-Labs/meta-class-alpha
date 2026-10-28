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

    float _appliedTimeScale = -1f;
    float _appliedDistanceScale = -1f;

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
        if (!timeChanged && !distanceChanged)
            return;

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

        _appliedTimeScale = timeScale;
        _appliedDistanceScale = distanceScale;
    }
}
