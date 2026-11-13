using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RegroupButton : MonoBehaviour
{
    public const string ObjectName = "RegroupButton";

    public static bool ShouldMove(bool isHostPlayer)
    {
        return !isHostPlayer;
    }

    public static bool IsTarget(Transform hit)
    {
        for (Transform current = hit; current != null; current = current.parent)
        {
            if (current.name == ObjectName || current.GetComponent<RegroupButton>() != null)
                return true;
        }

        return false;
    }

    public void OnPointerClick()
    {
        if (RoomSync.RequestRegroup())
            return;

        PlaceLocalGuests();
    }

    public static void PlaceLocalGuests()
    {
        ClassController room = ClassController.ResolveExisting(FindFirstObjectByType<ClassController>());
        if (room == null)
            return;

        CameraPointerController[] players = FindObjectsByType<CameraPointerController>(FindObjectsSortMode.None);
        var bodies = new List<Transform>(players.Length);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
                bodies.Add(players[i].transform);
        }

        room.PlaceGuests(bodies, IsHostBody);
    }

    static bool IsHostBody(Transform body)
    {
        if (body == null || !Mirror.NetworkServer.active || Mirror.NetworkClient.localPlayer == null)
            return false;

        return body == Mirror.NetworkClient.localPlayer.transform;
    }
}
