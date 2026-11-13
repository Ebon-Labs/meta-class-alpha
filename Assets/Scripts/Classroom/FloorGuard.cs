using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-45)]
public class FloorGuard : MonoBehaviour
{
    void Update()
    {
        FloorFollow.DetachPassengers(transform, MoveToEdge.PassengerTag);
    }
}
