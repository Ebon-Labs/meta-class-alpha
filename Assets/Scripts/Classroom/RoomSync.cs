using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomSync : NetworkBehaviour
{
    public static RoomSync Local { get; private set; }

    struct ServerState
    {
        public bool Ready;
        public float DefaultTimeScale;
        public float TimeScale;
        public float DistanceScale;
        public float MaxDistance;
        public bool Orbs;
        public bool Mute;
        public bool HideFloor;
        public int Turns;
        public int Screen;
    }

    static ServerState serverState;
    static int appliedScreen = int.MinValue;

    public static bool RequestTurn()
    {
        return Send(sync => sync.CmdTurn());
    }

    public static bool RequestToggleTime()
    {
        return Send(sync => sync.CmdToggleTime());
    }

    public static bool RequestToggleDistance()
    {
        return Send(sync => sync.CmdToggleDistance());
    }

    public static bool RequestToggleOrbs()
    {
        return Send(sync => sync.CmdToggleOrbs());
    }

    public static bool RequestToggleMute()
    {
        return Send(sync => sync.CmdToggleMute());
    }

    public static bool RequestToggleFloor()
    {
        return Send(sync => sync.CmdToggleFloor());
    }

    public static bool RequestScreen(ScreenControlButton.Action action)
    {
        return Send(sync => sync.CmdScreen((int)action));
    }

    public static SolarMotionSnapshot ToggleTime(SolarMotionSnapshot snapshot)
    {
        return snapshot.ToggleTime();
    }

    public static SolarMotionSnapshot ToggleDistance(SolarMotionSnapshot snapshot)
    {
        return snapshot.ToggleDistance();
    }

    public static SolarMotionSnapshot ToggleOrbs(SolarMotionSnapshot snapshot)
    {
        return snapshot.ToggleOrbs();
    }

    public override void OnStartLocalPlayer()
    {
        Local = this;
        CmdPull();
    }

    public override void OnStopLocalPlayer()
    {
        if (Local == this)
            Local = null;
    }

    public override void OnStartServer()
    {
        if (!EnsureServerState())
            return;
    }

    public override void OnStopServer()
    {
        if (NetworkServer.active)
            return;

        serverState = default;
        appliedScreen = int.MinValue;
    }

    [Command]
    void CmdPull()
    {
        if (!EnsureServerState())
            return;
        TargetApply(serverState.TimeScale, serverState.DistanceScale, serverState.Orbs, serverState.Mute, serverState.HideFloor, serverState.Turns, serverState.Screen);
    }

    [Command]
    void CmdTurn()
    {
        if (!EnsureServerState())
            return;
        serverState.Turns++;
        Broadcast();
    }

    [Command]
    void CmdToggleTime()
    {
        if (!EnsureServerState())
            return;
        Store(ToggleTime(Capture()));
        Broadcast();
    }

    [Command]
    void CmdToggleDistance()
    {
        if (!EnsureServerState())
            return;
        Store(ToggleDistance(Capture()));
        Broadcast();
    }

    [Command]
    void CmdToggleOrbs()
    {
        if (!EnsureServerState())
            return;
        Store(ToggleOrbs(Capture()));
        Broadcast();
    }

    [Command]
    void CmdToggleMute()
    {
        if (!EnsureServerState())
            return;
        serverState.Mute = !serverState.Mute;
        Broadcast();
    }

    [Command]
    void CmdToggleFloor()
    {
        if (!EnsureServerState())
            return;
        serverState.HideFloor = !serverState.HideFloor;
        Broadcast();
    }

    [Command]
    void CmdScreen(int action)
    {
        if (!EnsureServerState())
            return;
        serverState.Screen = action;
        Broadcast();
    }

    [ClientRpc]
    void RpcApply(float timeScale, float distanceScale, bool orbs, bool mute, bool hideFloor, int turns, int screen)
    {
        Apply(timeScale, distanceScale, orbs, mute, hideFloor, turns, screen);
    }

    [TargetRpc]
    void TargetApply(float timeScale, float distanceScale, bool orbs, bool mute, bool hideFloor, int turns, int screen)
    {
        Apply(timeScale, distanceScale, orbs, mute, hideFloor, turns, screen);
    }

    static bool Send(System.Action<RoomSync> command)
    {
        if (Local == null || !NetworkClient.active || !NetworkClient.ready)
            return false;

        command(Local);
        return true;
    }

    static bool EnsureServerState()
    {
        if (serverState.Ready)
            return true;

        if (Object.FindFirstObjectByType<SolarSystemMotionController>() == null
            && ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>()) == null)
            return false;

        ServerState captured = CaptureScene();
        captured.Ready = true;
        serverState = captured;
        return true;
    }

    static ServerState CaptureScene()
    {
        ServerState state = default;
        SolarSystemMotionController motion = Object.FindFirstObjectByType<SolarSystemMotionController>();
        if (motion != null)
        {
            state.DefaultTimeScale = motion.DefaultTimeScale;
            state.TimeScale = motion.TimeScale;
            state.DistanceScale = motion.DistanceScale;
            state.MaxDistance = motion.MaxDistanceScale;
            state.Orbs = motion.ShowInteractionOrbs;
        }

        ClassController room = ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>());
        if (room != null)
        {
            state.Mute = room.MuteGuests;
            state.HideFloor = room.HideFloorAreas;
        }

        return state;
    }

    SolarMotionSnapshot Capture()
    {
        return new SolarMotionSnapshot(
            serverState.DefaultTimeScale,
            serverState.TimeScale,
            serverState.DistanceScale,
            serverState.Orbs,
            true,
            serverState.MaxDistance);
    }

    static void Store(SolarMotionSnapshot snapshot)
    {
        serverState.TimeScale = snapshot.TimeScale;
        serverState.DistanceScale = snapshot.DistanceScale;
        serverState.Orbs = snapshot.InteractionOrbsEnabled;
    }

    void Broadcast()
    {
        RpcApply(serverState.TimeScale, serverState.DistanceScale, serverState.Orbs, serverState.Mute, serverState.HideFloor, serverState.Turns, serverState.Screen);
    }

    static void Apply(float timeScale, float distanceScale, bool orbs, bool mute, bool hideFloor, int turns, int screen)
    {
        SolarSystemMotionController motion = Object.FindFirstObjectByType<SolarSystemMotionController>();
        if (motion != null)
        {
            motion.SetDistanceScale(distanceScale);
            motion.SetTimeScale(timeScale);
            motion.SetShowInteractionOrbs(orbs);
        }

        ClassController room = ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>());
        if (room != null)
        {
            room.SetMuteGuests(mute);
            room.SetHideFloorAreas(hideFloor);
            room.MatchTurnCount(turns);
        }

        if (screen != appliedScreen)
        {
            appliedScreen = screen;
            if (System.Enum.IsDefined(typeof(ScreenControlButton.Action), screen))
                ScreenControlButton.Execute((ScreenControlButton.Action)screen, Object.FindFirstObjectByType<MainScreen>());
        }

        RefreshButtons();
    }

    static void RefreshButtons()
    {
        Refresh<OrbitControlButton>(button => button.RefreshAppearance());
        Refresh<DistanceControlButton>(button => button.RefreshAppearance());
        Refresh<MarkerControlButton>(button => button.RefreshAppearance());
        Refresh<MuteControlButton>(button => button.RefreshAppearance());
        Refresh<GroundControlButton>(button => button.RefreshAppearance());
    }

    static void Refresh<T>(System.Action<T> refresh) where T : Object
    {
        T[] buttons = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
                refresh(buttons[i]);
        }
    }
}
