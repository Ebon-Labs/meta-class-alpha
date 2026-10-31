using System.Collections.Generic;
using UnityEngine;

public sealed class PassengerHold
{
    readonly List<Passenger> passengers = new List<Passenger>();

    public void Hold(Transform subject, string passengerTag)
    {
        if (subject == null)
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag(passengerTag);
        for (int i = 0; i < players.Length; i++)
        {
            Transform player = players[i].transform;
            if (player == subject)
                continue;

            Rigidbody body = players[i].GetComponent<Rigidbody>();
            bool wasKinematic = body != null && body.isKinematic;
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = MoveToEdge.HoldKinematic(traveling: true, wasKinematic);
            }

            passengers.Add(new Passenger(player, player.parent, body, wasKinematic));
            player.SetParent(subject, true);
        }
    }

    public void Release()
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
            passenger.Body.isKinematic = MoveToEdge.HoldKinematic(traveling: false, passenger.WasKinematic);
        }

        passengers.Clear();
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
