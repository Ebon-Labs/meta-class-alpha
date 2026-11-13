using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-30)]
public class MoveToEdge : MonoBehaviour
{
    public const float DefaultDuration = 7f;
    public const float AstroParkHeight = 8f;
    public const string PassengerTag = "Player";
    public const string CenterTag = "Class Center";

    public const float OriginTolerance = 0.05f;

    public static bool HoldKinematic(bool traveling, bool wasKinematic)
    {
        return traveling || wasKinematic;
    }

    public static bool IsAtOrigin(Vector3 position, Vector3 origin, float tolerance)
    {
        float limit = Mathf.Max(0f, tolerance);
        return (position - origin).sqrMagnitude <= limit * limit;
    }

    public static bool IsAvailable(bool atOrigin)
    {
        return !atOrigin;
    }

    public static float Radius(Bounds bounds)
    {
        return Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
    }

    public static bool ShouldCarry(bool turning, bool traveling, bool moved)
    {
        return turning || traveling || moved;
    }

    public static string RouteKey(Transform transform)
    {
        if (transform == null)
            return "";

        var parts = new List<string>();
        Transform cursor = transform;
        while (cursor != null)
        {
            parts.Add(cursor.GetSiblingIndex() + ":" + cursor.name);
            cursor = cursor.parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }

    public static int RouteIndex(MoveToEdge edge)
    {
        if (edge == null)
            return -1;

        MoveToEdge[] ordered = Ordered();
        for (int i = 0; i < ordered.Length; i++)
        {
            if (ordered[i] == edge)
                return i;
        }

        return -1;
    }

    public static MoveToEdge AtRoute(int index)
    {
        MoveToEdge[] ordered = Ordered();
        if (index < 0 || index >= ordered.Length)
            return null;

        return ordered[index];
    }

    public static bool IsMoving => movingCount > 0;

    static int movingCount;

    static MoveToEdge[] Ordered()
    {
        MoveToEdge[] found = Object.FindObjectsByType<MoveToEdge>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(found, (a, b) => string.CompareOrdinal(RouteKey(a.transform), RouteKey(b.transform)));
        return found;
    }

    public static Transform FindCenter()
    {
        GameObject center = GameObject.FindWithTag(CenterTag);
        if (center == null)
            return null;

        return center.transform;
    }

    public static Vector3 Landing(Vector3 center, float radius, Vector3 from)
    {
        Vector3 outward = from - center;
        if (outward.sqrMagnitude < 0.0001f)
            outward = Vector3.up;
        else
            outward.Normalize();

        Vector3 point = center + outward * radius;
        point.y = AstroParkHeight;
        return point;
    }

    [SerializeField] GameObject subject;
    [SerializeField] GameObject destination;
    [SerializeField] bool returnHome;
    [SerializeField] float duration = DefaultDuration;

    Vector3 origin;
    Vector3 start;
    Transform referencePoint;
    Vector3 destinationCenter;
    float destinationRadius;
    float elapsed;
    bool originCaptured;
    bool moving;
    Collider block;
    ObjectBasicController appearance;

    void Awake()
    {
        if (!returnHome || subject == null)
            return;

        referencePoint = FindCenter();
        origin = ReferencePosition();
        originCaptured = true;
        block = GetComponent<Collider>();
        appearance = GetComponent<ObjectBasicController>();
        ApplyAvailability();
    }

    public void OnPointerClick()
    {
        int route = RouteIndex(this);
        if (route >= 0 && RoomSync.RequestTravel(route))
            return;

        Begin();
    }

    public void Begin()
    {
        if (returnHome)
        {
            BeginReturn();
            return;
        }

        if (subject == null || destination == null)
            return;
        if (!BodyBounds.TryGet(destination.transform, out Bounds bounds))
            return;

        destinationCenter = bounds.center;
        destinationRadius = Radius(bounds);

        TravelClaim.Claim(this, CancelTravel);
        StartMoving();
    }

    void Update()
    {
        if (returnHome)
        {
            UpdateReturn();
            return;
        }

        if (!moving)
            return;
        if (subject == null)
        {
            Finish();
            return;
        }

        elapsed += Time.deltaTime;
        Vector3 end = Landing(destinationCenter, destinationRadius, start);
        MoveSubject(EdgeApproach.Move(start, end, elapsed, duration));
        if (duration <= 0f || elapsed >= duration)
            Finish();
    }

    void OnDisable()
    {
        if (moving)
            Finish();
    }

    void BeginReturn()
    {
        if (subject == null || !originCaptured)
            return;

        referencePoint = FindCenter();
        if (AtOrigin())
            return;

        TravelClaim.Claim(this, CancelTravel);
        StartMoving();
    }

    void PlaceSubject()
    {
        referencePoint = FindCenter();
        Vector3 reference = ReferencePosition();
        start = reference;
        if (referencePoint != null)
            referencePoint.position = reference;
        MoveSubject(reference);
    }

    Vector3 ReferencePosition()
    {
        if (referencePoint == null)
            referencePoint = FindCenter();
        if (referencePoint != null)
            return referencePoint.position;

        return subject.transform.position;
    }

    void StartMoving()
    {
        if (!moving)
        {
            movingCount++;
            moving = true;
        }

        PlaceSubject();
        elapsed = 0f;
    }

    void MoveSubject(Vector3 reference)
    {
        Transform body = subject.transform;
        Vector3 offset = body.position - ReferencePosition();
        body.position = reference + offset;
    }

    void UpdateReturn()
    {
        if (moving && subject != null)
        {
            elapsed += Time.deltaTime;
            MoveSubject(EdgeApproach.Move(start, origin, elapsed, duration));
            if (duration <= 0f || elapsed >= duration)
                Finish();
        }

        ApplyAvailability();
    }

    bool AtOrigin()
    {
        return subject != null && originCaptured && IsAtOrigin(ReferencePosition(), origin, OriginTolerance);
    }

    void ApplyAvailability()
    {
        bool next = IsAvailable(AtOrigin());
        if (block != null)
            block.enabled = next;
        if (appearance != null)
            appearance.SetColor(next ? Color.white : new Color(0.35f, 0.35f, 0.35f, 1f));
    }

    void CancelTravel()
    {
        if (moving)
            Finish();
    }

    void Finish()
    {
        if (moving)
        {
            moving = false;
            movingCount--;
        }

        TravelClaim.Release(this);
    }
}
