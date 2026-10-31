using System.Collections.Generic;
using UnityEngine;

public class MoveToEdge : MonoBehaviour
{
    public const float DefaultDuration = 7f;
    public const string PassengerTag = "Player";
    public const string GroundName = "Ground";

    public static bool HoldKinematic(bool traveling, bool wasKinematic)
    {
        return traveling || wasKinematic;
    }

    [SerializeField] GameObject subject;
    [SerializeField] GameObject destination;
    [SerializeField] GameObject ground;
    [SerializeField] float duration = DefaultDuration;

    readonly List<Passenger> passengers = new List<Passenger>();
    Vector3 start;
    Vector3 fallbackForward;
    float bottomOffset;
    float elapsed;
    bool moving;

    public void OnPointerClick()
    {
        Begin();
    }

    public void Begin()
    {
        if (subject == null || destination == null)
            return;
        if (!BodyBounds.TryGet(destination.transform, out _))
            return;

        bottomOffset = BottomOffset(subject);
        if (!moving)
            HoldPassengers();

        start = subject.transform.position;
        fallbackForward = subject.transform.forward;
        elapsed = 0f;
        moving = true;
    }

    void Update()
    {
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

    void Finish()
    {
        moving = false;
        ReleasePassengers();
    }

    void HoldPassengers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(PassengerTag);
        for (int i = 0; i < players.Length; i++)
        {
            Transform player = players[i].transform;
            if (player == subject.transform)
                continue;

            Rigidbody body = players[i].GetComponent<Rigidbody>();
            bool wasKinematic = body != null && body.isKinematic;
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = HoldKinematic(traveling: true, wasKinematic);
            }

            passengers.Add(new Passenger(player, player.parent, body, wasKinematic));
            player.SetParent(subject.transform, true);
        }
    }

    void ReleasePassengers()
    {
        for (int i = 0; i < passengers.Count; i++)
        {
            Passenger passenger = passengers[i];
            if (passenger.Transform == null)
                continue;

            passenger.Transform.SetParent(passenger.Parent, true);
            if (passenger.Body == null)
                continue;

            passenger.Body.linearVelocity = Vector3.zero;
            passenger.Body.angularVelocity = Vector3.zero;
            passenger.Body.isKinematic = HoldKinematic(traveling: false, passenger.WasKinematic);
        }

        passengers.Clear();
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

    readonly struct Passenger
    {
        public Passenger(Transform transform, Transform parent, Rigidbody body, bool wasKinematic)
        {
            Transform = transform;
            Parent = parent;
            Body = body;
            WasKinematic = wasKinematic;
        }

        public Transform Transform { get; }
        public Transform Parent { get; }
        public Rigidbody Body { get; }
        public bool WasKinematic { get; }
    }
}
