using Mirror;
using Mirror.Discovery;
using UnityEngine;

[DisallowMultipleComponent]
public class LanRoomBeacon : MonoBehaviour
{
    public enum Role
    {
        Search,
        Host,
        Idle
    }

    NetworkDiscovery discovery;
    Role role = Role.Idle;

    public static Role Choose(bool serverActive, bool clientActive)
    {
        if (serverActive)
            return Role.Host;

        if (clientActive)
            return Role.Idle;

        return Role.Search;
    }

    public static void Ensure(NetworkDiscovery source)
    {
        if (source == null)
            return;

        LanRoomBeacon beacon = source.GetComponent<LanRoomBeacon>();
        if (beacon == null)
            beacon = source.gameObject.AddComponent<LanRoomBeacon>();

        beacon.discovery = source;
    }

    void Awake()
    {
        if (discovery == null)
            discovery = GetComponent<NetworkDiscovery>();
    }

    void Update()
    {
        if (discovery == null || !NetworkDiscovery.SupportedOnThisPlatform)
            return;

        Role next = Choose(NetworkServer.active, NetworkClient.active);
        if (next == role)
            return;

        role = next;
        if (next == Role.Host)
            SolarSystemThumbHost.Advertise(discovery);
        else if (next == Role.Search)
            discovery.StartDiscovery();
        else
            discovery.StopDiscovery();
    }
}
