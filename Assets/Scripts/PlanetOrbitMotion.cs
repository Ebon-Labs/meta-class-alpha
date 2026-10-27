using UnityEngine;

[DisallowMultipleComponent]
public class PlanetOrbitMotion : MonoBehaviour
{
    [SerializeField] Transform orbitCenter;
    [SerializeField] float orbitRadius;
    [SerializeField] float axialTiltDegrees = 23.44f;
    [SerializeField] float siderealDayHours = 23.934f;
    [SerializeField] float orbitalPeriodDays = 365.25f;
    [Tooltip("Simulation speed. 4320 ≈ 1 sidereal day every 20 real seconds.")]
    [SerializeField] float timeScale = 4320f;
    [SerializeField] Vector3 orbitAxis = Vector3.up;

    Vector3 _initialOffset;
    float _orbitAngleDegrees;
    float _spinAngleDegrees;
    Quaternion _axialTilt;

    void Awake()
    {
        _axialTilt = Quaternion.Euler(axialTiltDegrees, 0f, 0f);

        if (orbitCenter == null)
            orbitCenter = FindOrbitCenter();

        if (orbitCenter != null)
        {
            _initialOffset = transform.position - orbitCenter.position;
            if (orbitRadius <= 0f)
                orbitRadius = _initialOffset.magnitude;
            else if (_initialOffset.sqrMagnitude > 0.0001f)
                _initialOffset = _initialOffset.normalized * orbitRadius;
            else
                _initialOffset = Vector3.right * orbitRadius;
        }
    }

    void Update()
    {
        if (orbitCenter == null || timeScale <= 0f)
            return;

        float dt = Time.deltaTime;
        float daySeconds = Mathf.Max(0.001f, siderealDayHours) * 3600f;
        float yearSeconds = daySeconds * Mathf.Max(0.001f, orbitalPeriodDays);

        float spinDegreesPerSecond = 360f / (daySeconds / timeScale);
        float orbitDegreesPerSecond = 360f / (yearSeconds / timeScale);

        _spinAngleDegrees = Mathf.Repeat(_spinAngleDegrees + spinDegreesPerSecond * dt, 360f);
        _orbitAngleDegrees = Mathf.Repeat(_orbitAngleDegrees + orbitDegreesPerSecond * dt, 360f);

        Vector3 axis = orbitAxis.sqrMagnitude > 0.0001f ? orbitAxis.normalized : Vector3.up;
        transform.position = orbitCenter.position + Quaternion.AngleAxis(_orbitAngleDegrees, axis) * _initialOffset;
        transform.rotation = _axialTilt * Quaternion.AngleAxis(_spinAngleDegrees, Vector3.up);
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
