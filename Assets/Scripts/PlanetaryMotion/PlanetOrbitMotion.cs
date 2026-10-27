using UnityEngine;

[DisallowMultipleComponent]
public class PlanetOrbitMotion : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] SolarPlanetId planetProfile = SolarPlanetId.Earth;
    [Tooltip("Copy scientific values from PlanetScientificCatalog into this component.")]
    [SerializeField] bool applyScientificProfileOnAwake;

    [Header("Orbit (translation around the Sun)")]
    [SerializeField] bool enableOrbit = true;
    [SerializeField] Transform orbitCenter;
    [SerializeField] float orbitRadius;
    [Tooltip("Sidereal orbital period in days (Earth ≈ 365.256).")]
    [SerializeField] float orbitalPeriodDays = 365.256f;
    [SerializeField] Vector3 orbitAxis = Vector3.up;
    [Tooltip("If the current distance to the Sun is smaller than the Sun radius, push the orbit out.")]
    [SerializeField] bool ensureOrbitClearsSun = true;
    [SerializeField] float sunClearanceMultiplier = 1.35f;

    [Header("Spin (rotation)")]
    [SerializeField] bool enableSpin = true;
    [Tooltip("Obliquity / axial tilt in degrees (Earth ≈ 23.44).")]
    [SerializeField] float axialTiltDegrees = 23.44f;
    [Tooltip("Sidereal day in hours (Earth ≈ 23.934). Negative = retrograde (Venus, Uranus).")]
    [SerializeField] float siderealDayHours = 23.934f;
    [Tooltip("If true, applies axial tilt once from the scene local rotation.")]
    [SerializeField] bool applyAxialTiltOnStart = true;

    [Header("Time")]
    [Tooltip("Simulation speed. 4320 ≈ 1 Earth sidereal day every 20 real seconds.")]
    [SerializeField] float timeScale = 4320f;

    Vector3 _orbitOffsetFromCenter;
    float _orbitAngleDegrees;
    float _spinAngleDegrees;
    Quaternion _baseLocalRotation;
    Vector3 _initialLocalPosition;
    Quaternion _lockedWorldRotation;
    bool _orbitInitialized;

    public bool IsOrbitEnabled => enableOrbit;
    public SolarPlanetId PlanetProfile => planetProfile;

    void Awake()
    {
        if (applyScientificProfileOnAwake)
            ApplyScientificProfile();

        _initialLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;
        _lockedWorldRotation = transform.rotation;

        // Orbit lives on Wrapper; spin lives on the planet child. Never both.
        if (enableOrbit)
            enableSpin = false;

        if (enableSpin && applyAxialTiltOnStart)
            _baseLocalRotation = PlanetSpinCalculator.ApplyAxialTilt(_baseLocalRotation, axialTiltDegrees);

        if (!enableOrbit)
            return;

        ResolveOrbitCenter();
        if (orbitCenter == null)
        {
            Debug.LogWarning($"PlanetOrbitMotion on '{name}': orbit center (Sun) not found.", this);
            return;
        }

        CacheOrbitOffsetAroundSun();
    }

    void Update()
    {
        if (timeScale <= 0f)
            return;

        float dt = Time.deltaTime;

        if (enableOrbit)
        {
            if (orbitCenter == null)
                ResolveOrbitCenter();
            if (orbitCenter != null)
            {
                if (!_orbitInitialized)
                    CacheOrbitOffsetAroundSun();
                UpdateOrbit(dt);
            }
        }
        else if (enableSpin)
        {
            UpdateSpin(dt);
        }
    }

    public void SetTimeScale(float value)
    {
        timeScale = Mathf.Max(0f, value);
    }

    public void SetOrbitDistanceScale(float earthOrbitSceneUnits)
    {
        if (!enableOrbit)
            return;

        PlanetScientificData data = PlanetScientificCatalog.Get(planetProfile);
        orbitRadius = data.SceneOrbitRadius(Mathf.Max(0.01f, earthOrbitSceneUnits));

        if (!_orbitInitialized)
            return;

        ResolveOrbitCenter();
        if (orbitCenter == null)
            return;

        RefreshOrbitOffsetMagnitude();
    }

    [ContextMenu("Apply Scientific Profile")]
    public void ApplyScientificProfile()
    {
        PlanetScientificData data = PlanetScientificCatalog.Get(planetProfile);
        orbitalPeriodDays = data.orbitalPeriodDays;
        siderealDayHours = data.siderealDayHours;
        axialTiltDegrees = data.axialTiltDegrees;

        if (enableOrbit)
            orbitRadius = data.SceneOrbitRadius(PlanetScientificCatalog.EarthOrbitSceneUnits);
    }

    void ResolveOrbitCenter()
    {
        if (orbitCenter != null)
            return;

        orbitCenter = SolarSystemLocator.FindSun();
    }

    void CacheOrbitOffsetAroundSun()
    {
        Vector3 toPlanet = transform.position - orbitCenter.position;
        float minClearance = SolarSystemLocator.EstimateRadius(orbitCenter) * sunClearanceMultiplier
            + SolarSystemLocator.EstimateRadius(transform);

        orbitRadius = PlanetOrbitCalculator.ResolveRadius(
            orbitRadius,
            toPlanet.magnitude,
            minClearance,
            ensureOrbitClearsSun);

        _orbitOffsetFromCenter = PlanetOrbitCalculator.BuildOffsetFromCenter(toPlanet, orbitRadius);
        transform.SetPositionAndRotation(
            orbitCenter.position + _orbitOffsetFromCenter,
            _lockedWorldRotation);
        _orbitInitialized = true;
    }

    void RefreshOrbitOffsetMagnitude()
    {
        float minClearance = SolarSystemLocator.EstimateRadius(orbitCenter) * sunClearanceMultiplier
            + SolarSystemLocator.EstimateRadius(transform);

        orbitRadius = PlanetOrbitCalculator.ResolveRadius(
            orbitRadius,
            orbitRadius,
            minClearance,
            ensureOrbitClearsSun);

        if (_orbitOffsetFromCenter.sqrMagnitude < 0.0001f)
            _orbitOffsetFromCenter = Vector3.right * orbitRadius;
        else
            _orbitOffsetFromCenter = _orbitOffsetFromCenter.normalized * orbitRadius;
    }

    void UpdateOrbit(float dt)
    {
        float degreesPerSecond = PlanetOrbitCalculator.DegreesPerSecond(orbitalPeriodDays, timeScale);
        _orbitAngleDegrees = PlanetOrbitCalculator.AdvanceAngle(_orbitAngleDegrees, degreesPerSecond, dt);

        Vector3 nextPosition = PlanetOrbitCalculator.EvaluatePosition(
            orbitCenter.position,
            _orbitOffsetFromCenter,
            _orbitAngleDegrees,
            orbitAxis);

        transform.SetPositionAndRotation(nextPosition, _lockedWorldRotation);
    }

    void UpdateSpin(float dt)
    {
        float degreesPerSecond = PlanetSpinCalculator.DegreesPerSecond(siderealDayHours, timeScale);
        _spinAngleDegrees = PlanetSpinCalculator.AdvanceAngle(_spinAngleDegrees, degreesPerSecond, dt);

        transform.localPosition = _initialLocalPosition;
        transform.localRotation = PlanetSpinCalculator.EvaluateLocalRotation(
            _baseLocalRotation,
            _spinAngleDegrees);
    }
}
