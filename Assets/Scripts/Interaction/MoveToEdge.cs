using UnityEngine;

public class MoveToEdge : MonoBehaviour
{
    public const float DefaultDuration = 7f;
    public const string PassengerTag = "Player";
    public const string GroundName = "Ground";

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

    [SerializeField] GameObject subject;
    [SerializeField] GameObject destination;
    [SerializeField] GameObject ground;
    [SerializeField] bool returnHome;
    [SerializeField] float duration = DefaultDuration;

    readonly PassengerHold passengers = new PassengerHold();
    Vector3 origin;
    Vector3 start;
    Vector3 fallbackForward;
    float bottomOffset;
    float elapsed;
    bool originCaptured;
    bool moving;
    Collider block;
    ObjectBasicController appearance;

    void Awake()
    {
        if (!returnHome || subject == null)
            return;

        origin = subject.transform.position;
        originCaptured = true;
        block = GetComponent<Collider>();
        appearance = GetComponent<ObjectBasicController>();
        ApplyAvailability();
    }

    public void OnPointerClick()
    {
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
        if (!BodyBounds.TryGet(destination.transform, out _))
            return;

        TravelClaim.Claim(this, CancelTravel);
        bottomOffset = BottomOffset(subject);
        if (!moving)
            passengers.Hold(subject.transform, PassengerTag);

        start = subject.transform.position;
        fallbackForward = subject.transform.forward;
        elapsed = 0f;
        moving = true;
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
        if (subject == null || destination == null || !BodyBounds.TryGet(destination.transform, out Bounds bounds))
        {
            Finish();
            return;
        }

        elapsed += Time.deltaTime;
        Vector3 end = TeleportPlacement.EdgePosition(bounds, start, fallbackForward);
        end = RestOnGround(end);
        subject.transform.position = EdgeApproach.Move(start, end, elapsed, duration);
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
        if (subject == null || !originCaptured || AtOrigin())
            return;

        TravelClaim.Claim(this, CancelTravel);
        if (!moving)
            passengers.Hold(subject.transform, PassengerTag);

        start = subject.transform.position;
        elapsed = 0f;
        moving = true;
    }

    void UpdateReturn()
    {
        if (moving && subject != null)
        {
            elapsed += Time.deltaTime;
            subject.transform.position = EdgeApproach.Move(start, origin, elapsed, duration);
            if (duration <= 0f || elapsed >= duration)
                Finish();
        }

        ApplyAvailability();
    }

    bool AtOrigin()
    {
        return subject != null && originCaptured && IsAtOrigin(subject.transform.position, origin, OriginTolerance);
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
        moving = false;
        passengers.Release();
        TravelClaim.Release(this);
    }

    Vector3 RestOnGround(Vector3 position)
    {
        GameObject floor = ground != null ? ground : GameObject.Find(GroundName);
        if (floor == null || !BodyBounds.TryGet(floor.transform, out Bounds groundBounds))
            return position;

        return EdgeApproach.RestOn(position, bottomOffset, groundBounds.max.y, 0f);
    }

    static float BottomOffset(GameObject body)
    {
        if (body == null || !BodyBounds.TryGet(body.transform, out Bounds bounds))
            return 0f;

        return body.transform.position.y - bounds.min.y;
    }
}
