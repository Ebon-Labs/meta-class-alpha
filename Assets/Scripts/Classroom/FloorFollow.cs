using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-20)]
public class FloorFollow : MonoBehaviour
{
    readonly List<Rigidbody> carriedBodies = new List<Rigidbody>();
    readonly List<bool> carriedWasKinematic = new List<bool>();
    Vector3 lastPosition;
    Quaternion lastRotation = Quaternion.identity;
    bool captured;

    public static bool ParentLocksPassenger(Transform passenger, Transform room)
    {
        return passenger != null && room != null && passenger != room && passenger.IsChildOf(room);
    }

    public static void DetachPassenger(Transform passenger)
    {
        passenger.SetParent(null, true);
    }

    public static void DetachPassengers(Transform room, string passengerTag)
    {
        if (room == null)
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag(passengerTag);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            Transform player = players[i].transform;
            if (!ParentLocksPassenger(player, room))
                continue;

            DetachPassenger(player);
        }
    }

    void OnEnable()
    {
        Capture();
    }

    void OnDisable()
    {
        Release();
    }

    void Update()
    {
        if (!captured)
            Capture();

        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        bool moved = position != lastPosition || rotation != lastRotation;
        ClassController room = GetComponent<ClassController>();
        bool hold = MoveToEdge.ShouldCarry(room != null && room.IsTurning, MoveToEdge.IsMoving, moved);
        if (hold)
            Freeze();
        if (moved)
            Carry(lastPosition, lastRotation, position, rotation);

        if (!hold)
            Release();

        lastPosition = position;
        lastRotation = rotation;
    }

    void Capture()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        captured = true;
    }

    void Freeze()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(MoveToEdge.PassengerTag);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            Rigidbody body = players[i].GetComponent<Rigidbody>();
            if (body == null || AlreadyCarried(body))
                continue;

            bool wasKinematic = body.isKinematic;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = MoveToEdge.HoldKinematic(traveling: true, wasKinematic);
            carriedBodies.Add(body);
            carriedWasKinematic.Add(wasKinematic);
        }
    }

    bool AlreadyCarried(Rigidbody body)
    {
        for (int i = 0; i < carriedBodies.Count; i++)
        {
            if (carriedBodies[i] == body)
                return true;
        }

        return false;
    }

    void Carry(Vector3 fromPosition, Quaternion fromRotation, Vector3 toPosition, Quaternion toRotation)
    {
        FloorCarry.AddYaw(FloorCarry.YawDegrees(fromRotation, toRotation));
        GameObject[] players = GameObject.FindGameObjectsWithTag(MoveToEdge.PassengerTag);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            Transform player = players[i].transform;
            Rigidbody body = players[i].GetComponent<Rigidbody>();
            Vector3 position = FloorCarry.Position(player.position, fromPosition, fromRotation, toPosition, toRotation);
            Quaternion rotation = FloorCarry.Rotation(player.rotation, fromRotation, toRotation);
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = position;
                body.rotation = rotation;
            }

            player.SetPositionAndRotation(position, rotation);
        }
    }

    void Release()
    {
        for (int i = 0; i < carriedBodies.Count; i++)
        {
            Rigidbody body = carriedBodies[i];
            if (body == null)
                continue;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = MoveToEdge.HoldKinematic(traveling: false, carriedWasKinematic[i]);
        }

        carriedBodies.Clear();
        carriedWasKinematic.Clear();
    }
}
