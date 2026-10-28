using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)]
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
    [Tooltip("If true, expands radius when too close to the Sun without moving on the first frame.")]
    [SerializeField] bool ensureOrbitClearsSun = true;
    [SerializeField] float sunClearanceMultiplier = 1.35f;

    [Header("Spin (rotation)")]
    [SerializeField] bool enableSpin = true;
    [Tooltip("Obliquity / axial tilt in degrees (Earth ≈ 23.44).")]
    [SerializeField] float axialTiltDegrees = 23.44f;
    [Tooltip("Sidereal day in hours (Earth ≈ 23.934). Negative = retrograde (Venus, Uranus).")]
    [SerializeField] float siderealDayHours = 23.934f;
    [Tooltip("If true, applies axial tilt once from the scene local rotation.")]
    [SerializeField] bool applyAxialTiltOnStart = false;

    [Header("Time")]
    [Tooltip("Simulation speed. 4320 ≈ 1 Earth sidereal day every 20 real seconds.")]
    [SerializeField] float timeScale = 4320f;

    Vector3 _orbitOffsetFromCenter;
    float _orbitAngleDegrees;
    float _spinAngleDegrees;
    Quaternion _baseLocalRotation;
    Vector3 _initialLocalPosition;
    bool _orbitInitialized;
    ChildLocalPose[] _childPoses;

    struct ChildLocalPose
    {
        public Transform transform;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public Vector3 anchoredPosition3D;
        public bool isRect;
    }

    public bool IsOrbitEnabled => enableOrbit;
    public SolarPlanetId PlanetProfile => planetProfile;

    void Awake()
    {
        if (applyScientificProfileOnAwake)
            ApplyScientificProfile();

        // Capture scene pose first — never alter parent/child locals on start.
        _initialLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;
        CacheChildLocalPoses();

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
        RestoreChildLocalPoses();
    }

    void Start()
    {
        // Undo anything other Awake/Start scripts may have done to children.
        RestoreChildLocalPoses();
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

    void LateUpdate()
    {
        // Hard guarantee: markers/orbs keep the exact local pose captured at start.
        RestoreChildLocalPoses();
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
        float nextRadius = data.SceneOrbitRadius(Mathf.Max(0.01f, earthOrbitSceneUnits));

        // Avoid a first-frame jump when the requested scale matches the current orbit.
        if (_orbitInitialized && Mathf.Approximately(nextRadius, orbitRadius))
            return;

        orbitRadius = nextRadius;

        if (!_orbitInitialized)
            return;

        ResolveOrbitCenter();
        if (orbitCenter == null)
            return;

        RefreshOrbitOffsetMagnitude();
        ApplyOrbitPosition();
        RestoreChildLocalPoses();
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
        // Keep the exact scene placement so InteractionOrb/planet local poses stay valid.
        Vector3 toPlanet = transform.position - orbitCenter.position;
        _orbitOffsetFromCenter = toPlanet.sqrMagnitude < 0.0001f
            ? Vector3.right * Mathf.Max(0.01f, orbitRadius)
            : toPlanet;
        orbitRadius = _orbitOffsetFromCenter.magnitude;
        _orbitInitialized = true;
    }

    void RefreshOrbitOffsetMagnitude()
    {
        // Never re-apply sun clearance after init — that jumps the Wrapper and breaks
        // authored InteractionOrb local placement under scaled parents.
        if (_orbitOffsetFromCenter.sqrMagnitude < 0.0001f)
            _orbitOffsetFromCenter = Vector3.right * orbitRadius;
        else
            _orbitOffsetFromCenter = _orbitOffsetFromCenter.normalized * orbitRadius;
    }

    void UpdateOrbit(float dt)
    {
        float degreesPerSecond = PlanetOrbitCalculator.DegreesPerSecond(orbitalPeriodDays, timeScale);
        _orbitAngleDegrees = PlanetOrbitCalculator.AdvanceAngle(_orbitAngleDegrees, degreesPerSecond, dt);
        ApplyOrbitPosition();
    }

    void ApplyOrbitPosition()
    {
        Vector3 nextPosition = PlanetOrbitCalculator.EvaluatePosition(
            orbitCenter.position,
            _orbitOffsetFromCenter,
            _orbitAngleDegrees,
            orbitAxis);

        // Translation only — never touch rotation so RectTransform children keep locals.
        transform.position = nextPosition;
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

    void CacheChildLocalPoses()
    {
        // Lock markers/orbs on the Wrapper. Do not lock the spinning Planet child.
        int lockedCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (ShouldPreserveChildPose(transform.GetChild(i)))
                lockedCount++;
        }

        _childPoses = new ChildLocalPose[lockedCount];
        int index = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!ShouldPreserveChildPose(child))
                continue;

            ChildLocalPose pose = new ChildLocalPose
            {
                transform = child,
                localPosition = child.localPosition,
                localRotation = child.localRotation,
                localScale = child.localScale,
                isRect = child is RectTransform
            };

            if (pose.isRect)
                pose.anchoredPosition3D = ((RectTransform)child).anchoredPosition3D;

            _childPoses[index++] = pose;
        }
    }

    static bool ShouldPreserveChildPose(Transform child)
    {
        if (child == null)
            return false;

        // Only InteractionOrb is pose-locked. PlanetLabel drives its own
        // billboard rotation and distance-independent scale each LateUpdate.
        return child.name.StartsWith("InteractionOrb");
    }

    void RestoreChildLocalPoses()
    {
        if (_childPoses == null)
            return;

        for (int i = 0; i < _childPoses.Length; i++)
        {
            ChildLocalPose pose = _childPoses[i];
            if (pose.transform == null)
                continue;

            if (pose.isRect)
            {
                RectTransform rect = (RectTransform)pose.transform;
                // Only drive RectTransform via anchored pose — setting localPosition too
                // fights Unity's Rect layout under a scaled/rotated parent.
                rect.localRotation = pose.localRotation;
                rect.localScale = pose.localScale;
                rect.anchoredPosition3D = pose.anchoredPosition3D;
            }
            else
            {
                pose.transform.localPosition = pose.localPosition;
                pose.transform.localRotation = pose.localRotation;
                pose.transform.localScale = pose.localScale;
            }
        }
    }
}
