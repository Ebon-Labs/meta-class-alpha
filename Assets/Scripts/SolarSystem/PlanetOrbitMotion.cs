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
    [Tooltip("Planets follow SolarSystemMotionController distanceScale. Moons keep the radius authored around their planet.")]
    [SerializeField] bool useSolarDistanceScale = true;
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
    Vector3 _visualCenterOffset;
    float _clearanceRadius;
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

    public const string InteractionMarkerPrefix = "InteractionMarkerOrb";

    public bool IsOrbitEnabled => enableOrbit;
    public bool UseSolarDistanceScale => useSolarDistanceScale;
    public SolarPlanetId PlanetProfile => planetProfile;

    public static bool LocksChildPose(string childName)
    {
        return !string.IsNullOrEmpty(childName) && childName.StartsWith(InteractionMarkerPrefix);
    }

    public static bool AllowsSpin(bool orbitEnabled, bool spinEnabled)
    {
        return spinEnabled && !orbitEnabled;
    }

    void Awake()
    {
        if (applyScientificProfileOnAwake)
            ApplyScientificProfile();

        // Capture scene pose first — never alter parent/child locals on start.
        _initialLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;
        CacheChildLocalPoses();

        // Orbit lives on Wrapper; spin lives on the planet child. Never both.
        enableSpin = AllowsSpin(enableOrbit, enableSpin);

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
        if (RoomSimulationClock.TryRead(out float clock))
        {
            ApplySharedClock(clock);
            return;
        }

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

    void ApplySharedClock(float clock)
    {
        if (enableOrbit)
        {
            if (orbitCenter == null)
                ResolveOrbitCenter();
            if (orbitCenter == null)
                return;
            if (!_orbitInitialized)
                CacheOrbitOffsetAroundSun();

            _orbitAngleDegrees = PlanetOrbitCalculator.AngleAtClock(orbitalPeriodDays, clock);
            ApplyOrbitPosition();
            return;
        }

        if (!enableSpin)
            return;

        _spinAngleDegrees = PlanetSpinCalculator.AngleAtClock(siderealDayHours, clock);
        transform.localPosition = _initialLocalPosition;
        transform.localRotation = PlanetSpinCalculator.EvaluateLocalRotation(_baseLocalRotation, _spinAngleDegrees);
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
        if (!enableOrbit || !useSolarDistanceScale)
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
        _visualCenterOffset = VisualCenterOffset();
        _clearanceRadius = ensureOrbitClearsSun ? ClearanceRadius() : 0f;

        Vector3 visualCenter = transform.position + _visualCenterOffset;
        Vector3 toBody = visualCenter - orbitCenter.position;
        float radius = PlanetOrbitCalculator.ResolveRadius(
            orbitRadius,
            toBody.magnitude,
            _clearanceRadius,
            ensureOrbitClearsSun);
        _orbitOffsetFromCenter = PlanetOrbitCalculator.BuildOffsetFromCenter(toBody, radius);
        orbitRadius = _orbitOffsetFromCenter.magnitude;
        _orbitInitialized = true;
        ApplyOrbitPosition();
    }

    void RefreshOrbitOffsetMagnitude()
    {
        float radius = PlanetOrbitCalculator.ResolveRadius(
            orbitRadius,
            orbitRadius,
            _clearanceRadius,
            ensureOrbitClearsSun);
        orbitRadius = radius;

        if (_orbitOffsetFromCenter.sqrMagnitude < 0.0001f)
            _orbitOffsetFromCenter = Vector3.right * orbitRadius;
        else
            _orbitOffsetFromCenter = _orbitOffsetFromCenter.normalized * orbitRadius;
    }

    Vector3 VisualCenterOffset()
    {
        Renderer body = BodyRenderer();
        if (body == null)
            return Vector3.zero;

        return body.bounds.center - transform.position;
    }

    float ClearanceRadius()
    {
        float centerRadius = CenterRadius(orbitCenter);
        float bodyRadius = 0f;
        Renderer body = BodyRenderer();
        if (body != null)
        {
            bodyRadius = PlanetOrbitCalculator.RadiusFromBounds(
                body.bounds.center,
                body.bounds.center,
                body.bounds.extents);
        }

        return PlanetOrbitCalculator.MinimumClearRadius(centerRadius, bodyRadius, sunClearanceMultiplier);
    }

    Renderer BodyRenderer()
    {
        PlanetOrbitMotion[] motions = GetComponentsInChildren<PlanetOrbitMotion>(true);
        for (int i = 0; i < motions.Length; i++)
        {
            PlanetOrbitMotion motion = motions[i];
            if (motion == null || motion == this || motion.IsOrbitEnabled)
                continue;

            Renderer renderer = motion.GetComponent<Renderer>();
            if (renderer != null)
                return renderer;
        }

        return GetComponent<Renderer>();
    }

    static float CenterRadius(Transform center)
    {
        if (center == null)
            return 0f;

        SphereCollider sphere = center.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Vector3 scale = center.lossyScale;
            float largest = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            return sphere.radius * largest;
        }

        Renderer renderer = center.GetComponent<Renderer>();
        if (renderer == null)
            return 0f;

        return PlanetOrbitCalculator.RadiusFromBounds(center.position, renderer.bounds.center, renderer.bounds.extents);
    }

    void UpdateOrbit(float dt)
    {
        float degreesPerSecond = PlanetOrbitCalculator.DegreesPerSecond(orbitalPeriodDays, timeScale);
        _orbitAngleDegrees = PlanetOrbitCalculator.AdvanceAngle(_orbitAngleDegrees, degreesPerSecond, dt);
        ApplyOrbitPosition();
    }

    void ApplyOrbitPosition()
    {
        Vector3 orbitPoint = PlanetOrbitCalculator.EvaluatePosition(
            orbitCenter.position,
            _orbitOffsetFromCenter,
            _orbitAngleDegrees,
            orbitAxis);

        // The mesh center, not the wrapper pivot, stays on the circle.
        // Translation only — never touch rotation so RectTransform children keep locals.
        transform.position = PlanetOrbitCalculator.PivotForVisualCenter(orbitPoint, _visualCenterOffset);
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
        // Only InteractionMarkerOrb is pose-locked. PlanetLabel drives its own
        // billboard rotation and distance-independent scale each LateUpdate.
        return child != null && LocksChildPose(child.name);
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
