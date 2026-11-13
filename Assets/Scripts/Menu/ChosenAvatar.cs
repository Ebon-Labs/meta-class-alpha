using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct AvatarChoiceMessage : NetworkMessage
{
    public bool male;
}

[DisallowMultipleComponent]
public class ChosenAvatar : MonoBehaviour
{
    public const string GirlPrefabName = "Player";
    public const string BoyPrefabName = "PlayerM";

    static ChosenAvatar current;
    static readonly Dictionary<int, bool> choices = new Dictionary<int, bool>();
    static readonly HashSet<int> waiting = new HashSet<int>();
    static bool installed;
    static bool registered;

    bool announced;

    public static bool WaitsForChoice(bool isLocal, bool hasChoice)
    {
        return !isLocal && !hasChoice;
    }

    public static bool MaleFor(bool isLocal, bool hasChoice, bool chosenMale, bool localMale)
    {
        if (hasChoice)
            return chosenMale;

        return isLocal && localMale;
    }

    public static GameObject PrefabFor(bool male, GameObject girlPrefab, GameObject boyPrefab, IList<GameObject> registeredPrefabs, GameObject fallback)
    {
        GameObject chosen = PlayerPortraitChoice.Chosen(male, girlPrefab, boyPrefab);
        if (chosen != null)
            return chosen;

        string name = male ? BoyPrefabName : GirlPrefabName;
        if (fallback != null && fallback.name == name)
            return fallback;

        if (registeredPrefabs != null)
        {
            for (int i = 0; i < registeredPrefabs.Count; i++)
            {
                GameObject prefab = registeredPrefabs[i];
                if (prefab != null && prefab.name == name)
                    return prefab;
            }
        }

        return fallback;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        if (current != null)
            return;

        var host = new GameObject("ChosenAvatar");
        DontDestroyOnLoad(host);
        current = host.AddComponent<ChosenAvatar>();
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }

    void Update()
    {
        if (!NetworkServer.active)
        {
            installed = false;
            choices.Clear();
            waiting.Clear();
        }
        else if (!installed)
        {
            NetworkServer.ReplaceHandler<AddPlayerMessage>(OnAddPlayer);
            NetworkServer.ReplaceHandler<AvatarChoiceMessage>(OnChoice);
            installed = true;
        }

        if (!NetworkClient.active)
        {
            registered = false;
            announced = false;
            return;
        }

        if (!registered)
        {
            Register(PlayerPortraitChoice.Prefab(false));
            Register(PlayerPortraitChoice.Prefab(true));
            registered = true;
        }

        if (announced || NetworkServer.active || !NetworkClient.isConnected)
            return;

        if (NetworkClient.connection == null || !NetworkClient.connection.isAuthenticated)
            return;

        NetworkClient.Send(new AvatarChoiceMessage { male = PlayerPortraitChoice.UsesMale });
        announced = true;
    }

    static void Register(GameObject prefab)
    {
        if (prefab == null)
            return;

        NetworkClient.RegisterPrefab(prefab);
    }

    static void OnChoice(NetworkConnectionToClient conn, AvatarChoiceMessage message)
    {
        if (conn == null)
            return;

        choices[conn.connectionId] = message.male;
        if (!waiting.Contains(conn.connectionId))
            return;

        waiting.Remove(conn.connectionId);
        NetworkManager manager = NetworkManager.singleton;
        if (manager != null && conn.identity == null)
            Spawn(conn, manager);
    }

    static void OnAddPlayer(NetworkConnectionToClient conn, AddPlayerMessage message)
    {
        NetworkManager manager = NetworkManager.singleton;
        if (manager == null || conn == null || conn.identity != null)
            return;

        if (manager.autoCreatePlayer && manager.playerPrefab == null)
            return;

        bool local = IsLocal(conn);
        if (local)
            choices[conn.connectionId] = PlayerPortraitChoice.UsesMale;

        if (WaitsForChoice(local, choices.ContainsKey(conn.connectionId)))
        {
            waiting.Add(conn.connectionId);
            return;
        }

        Spawn(conn, manager);
    }

    static void Spawn(NetworkConnectionToClient conn, NetworkManager manager)
    {
        bool hasChoice = choices.TryGetValue(conn.connectionId, out bool chosenMale);
        bool male = MaleFor(IsLocal(conn), hasChoice, chosenMale, PlayerPortraitChoice.UsesMale);
        GameObject prefab = PrefabFor(male, PlayerPortraitChoice.Prefab(false), PlayerPortraitChoice.Prefab(true), manager.spawnPrefabs, manager.playerPrefab);
        if (prefab == null || !prefab.TryGetComponent<NetworkIdentity>(out _))
            return;

        Transform start = manager.GetStartPosition();
        GameObject player = start != null
            ? Instantiate(prefab, start.position, start.rotation)
            : Instantiate(prefab);
        player.name = $"{prefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    static bool IsLocal(NetworkConnectionToClient conn)
    {
        return conn != null
            && NetworkServer.localConnection != null
            && conn.connectionId == NetworkServer.localConnection.connectionId;
    }
}
