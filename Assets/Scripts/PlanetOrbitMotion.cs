using UnityEngine;

[DisallowMultipleComponent]
public class PlanetOrbitMotion : MonoBehaviour
{
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
    [Tooltip("Sidereal day in hours (Earth ≈ 23.934).")]
    [SerializeField] float siderealDayHours = 23.934f;
    [Tooltip("If true, applies axial tilt once from the scene local rotation.")]
    [SerializeField] bool applyAxialTiltOnStart = true;

    [Header("Time")]
    [Tooltip("Simulation speed. 4320 ≈ 1 sidereal day every 20 real seconds.")]
    [SerializeField] float timeScale = 43200f;

    Vector3 _orbitOffsetFromCenter;
    float _orbitAngleDegrees;
    float _spinAngleDegrees;
    Quaternion _baseLocalRotation;
    Vector3 _initialLocalPosition;
    Quaternion _lockedWorldRotation;

    void Awake()
    {
        _initialLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;
        _lockedWorldRotation = transform.rotation;

        if (enableSpin && applyAxialTiltOnStart)
            _baseLocalRotation = Quaternion.Euler(axialTiltDegrees, 0f, 0f) * _baseLocalRotation;

        if (!enableOrbit)
            return;

        ResolveOrbitCenter();
        if (orbitCenter == null)
            return;

        CacheOrbitOffsetAroundSun();
    }

    void Update()
    {
        if (timeScale <= 0f)
            return;

        float dt = Time.deltaTime;
        float daySeconds = Mathf.Max(0.001f, siderealDayHours) * 3600f;

        if (enableOrbit)
        {
            if (orbitCenter == null)
                ResolveOrbitCenter();
            if (orbitCenter != null)
                UpdateOrbit(dt, daySeconds);
        }

        if (enableSpin)
            UpdateSpin(dt, daySeconds);
    }

    void ResolveOrbitCenter()
    {
        if (orbitCenter != null)
            return;

        orbitCenter = FindOrbitCenter();
    }

    void CacheOrbitOffsetAroundSun()
    {
        Vector3 toPlanet = transform.position - orbitCenter.position;
        float distance = toPlanet.magnitude;

        if (orbitRadius <= 0f)
            orbitRadius = distance;

        if (ensureOrbitClearsSun)
        {
            float minRadius = EstimateRadius(orbitCenter) * sunClearanceMultiplier
                + EstimateRadius(transform);
            if (orbitRadius < minRadius)
                orbitRadius = minRadius;
        }

        if (toPlanet.sqrMagnitude < 0.0001f)
            _orbitOffsetFromCenter = Vector3.right * orbitRadius;
        else
            _orbitOffsetFromCenter = toPlanet.normalized * orbitRadius;

        // Snap to the orbit circle around the Sun immediately.
        transform.SetPositionAndRotation(orbitCenter.position + _orbitOffsetFromCenter, _lockedWorldRotation);
    }

    void UpdateOrbit(float dt, float daySeconds)
    {
        float yearSeconds = daySeconds * Mathf.Max(0.001f, orbitalPeriodDays);
        float orbitDegreesPerSecond = 360f / (yearSeconds / timeScale);
        _orbitAngleDegrees = Mathf.Repeat(_orbitAngleDegrees + orbitDegreesPerSecond * dt, 360f);

        Vector3 axis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;
        Vector3 nextPosition = orbitCenter.position
            + Quaternion.AngleAxis(_orbitAngleDegrees, axis) * _orbitOffsetFromCenter;

        // Translate around the Sun only — Wrapper rotation stays locked.
        transform.SetPositionAndRotation(nextPosition, _lockedWorldRotation);
    }

    void UpdateSpin(float dt, float daySeconds)
    {
        float spinDegreesPerSecond = 360f / (daySeconds / timeScale);
        _spinAngleDegrees = Mathf.Repeat(_spinAngleDegrees + spinDegreesPerSecond * dt, 360f);

        transform.localPosition = _initialLocalPosition;
        transform.localRotation = _baseLocalRotation * Quaternion.AngleAxis(_spinAngleDegrees, Vector3.up);
    }

    static float EstimateRadius(Transform t)
    {
        if (t == null)
            return 0f;

        Renderer renderer = t.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds.extents.magnitude;

        Vector3 scale = t.lossyScale;
        return 0.5f * Mathf.Max(scale.x, scale.y, scale.z);
    }

    static Transform FindOrbitCenter()
    {
        string[] names = { "Sun Sphere", "Sun", "SUN" };
        for (int i = 0; i < names.Length; i++)
        {
            GameObject found = GameObject.Find(names[i]);
            if (found != null)
                return found.transform;
        }

        return null;
    }
}
