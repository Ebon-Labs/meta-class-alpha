using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class GoHomeOrb : MonoBehaviour
{
    public static bool IsTarget(Transform hit)
    {
        for (Transform current = hit; current != null; current = current.parent)
        {
            if (current.name == "GoHomeOrb" || current.GetComponent<GoHomeOrb>() != null)
                return true;
        }

        return false;
    }

    public void OnPointerClick()
    {
        CameraPointerController pointer = LocalPointer();
        if (pointer == null)
            return;

        pointer.ReturnToClassArea();
    }

    static CameraPointerController LocalPointer()
    {
        if (!NetworkClient.active || NetworkClient.localPlayer == null)
            return null;

        return NetworkClient.localPlayer.GetComponent<CameraPointerController>();
    }
}
