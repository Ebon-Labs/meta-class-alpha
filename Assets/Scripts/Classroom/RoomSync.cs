using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-50)]
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
        public bool ScreenArmed;
        public int Revision;
        public float Clock;
    }

    static ServerState serverState;
    static int appliedScreen = int.MinValue;
    static RoomSync clockDriver;

    [SyncVar] bool syncedReady;
    [SyncVar] float syncedTimeScale;
    [SyncVar] float syncedDistance;
    [SyncVar] bool syncedOrbs;
    [SyncVar] bool syncedMute;
    [SyncVar] bool syncedFloor;
    [SyncVar] int syncedTurns;
    [SyncVar] int syncedScreen;
    [SyncVar] bool syncedScreenArmed;
    [SyncVar(hook = nameof(OnRevision))] int syncedRevision;
    [SyncVar(hook = nameof(OnClock))] float syncedClock;
    [SyncVar] bool syncedClassPose;
    [SyncVar] bool syncedTurning;
    [SyncVar] Vector3 syncedClassPosition;
    [SyncVar] Quaternion syncedClassRotation = Quaternion.identity;
    [SyncVar] bool syncedPlayerPose;
    [SyncVar] Vector3 syncedPlayerPosition;
    [SyncVar] Quaternion syncedPlayerRotation = Quaternion.identity;

    double clockReceivedAt;
    bool clockTimed;

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
    }

    public override void OnStopLocalPlayer()
    {
        if (Local == this)
            Local = null;
    }

    public override void OnStartServer()
    {
        if (clockDriver == null)
            clockDriver = this;
        if (this != clockDriver)
            return;
        if (!EnsureServerState())
            return;

        WriteToSyncVars();
        RoomSimulationClock.Publish(serverState.Clock);
        ApplyControls(serverState);
    }

    public override void OnStartClient()
    {
        if (isServer || !syncedReady)
            return;

        NoteClock();
        RoomSimulationClock.Publish(syncedClock);
        ApplyControls(ReadState());
    }

    public override void OnStopServer()
    {
        if (clockDriver == this)
            clockDriver = null;
        if (NetworkServer.active)
            return;

        serverState = default;
        appliedScreen = int.MinValue;
        RoomSimulationClock.Clear();
    }

    public override void OnStopClient()
    {
        if (isServer || !syncedReady)
            return;

        RoomSimulationClock.Clear();
    }

    void Update()
    {
        if (isClient && !isServer && syncedClassPose)
            ApplyClassPose();

        if (isServer && this == clockDriver)
            DriveServerClock();

        if (!isServer && isClient && syncedReady)
            RoomSimulationClock.Publish(DisplayClock());
    }

    void LateUpdate()
    {
        if (isLocalPlayer)
            PublishLocalPlayerPose();

        if (isServer && this == clockDriver)
            PublishClassPose();

        if (isClient && !isServer && !isLocalPlayer && syncedPlayerPose)
            ApplyRemotePlayerPose();
    }

    [Command]
    void CmdPublishPlayerPose(Vector3 position, Quaternion rotation)
    {
        WritePlayerPose(position, rotation);
        ApplyRemotePlayerPose(position, rotation);
    }

    void PublishLocalPlayerPose()
    {
        if (!NetworkClient.active || !NetworkClient.ready)
            return;

        if (isServer)
            WritePlayerPose(transform.position, transform.rotation);
        else
            CmdPublishPlayerPose(transform.position, transform.rotation);
    }

    void PublishClassPose()
    {
        ClassController room = ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>());
        if (room == null)
            return;

        syncedClassPosition = room.transform.position;
        syncedClassRotation = room.transform.rotation;
        syncedTurning = room.IsTurning;
        syncedClassPose = true;
    }

    void WritePlayerPose(Vector3 position, Quaternion rotation)
    {
        syncedPlayerPosition = position;
        syncedPlayerRotation = rotation;
        syncedPlayerPose = true;
    }

    void ApplyClassPose()
    {
        ClassController room = ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>());
        if (room == null)
            return;

        if (syncedTurning)
            room.HoldPassengers();

        room.transform.SetPositionAndRotation(syncedClassPosition, syncedClassRotation);

        if (!syncedTurning)
            room.ReleasePassengers();
    }

    void ApplyRemotePlayerPose()
    {
        ApplyRemotePlayerPose(syncedPlayerPosition, syncedPlayerRotation);
    }

    void ApplyRemotePlayerPose(Vector3 position, Quaternion rotation)
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.position = position;
            body.rotation = rotation;
        }

        transform.SetPositionAndRotation(position, rotation);
    }

    [Command]
    void CmdTurn()
    {
        if (!EnsureServerState())
            return;
        serverState.Turns++;
        Commit();
    }

    [Command]
    void CmdToggleTime()
    {
        if (!EnsureServerState())
            return;
        Store(ToggleTime(Capture()));
        Commit();
    }

    [Command]
    void CmdToggleDistance()
    {
        if (!EnsureServerState())
            return;
        Store(ToggleDistance(Capture()));
        Commit();
    }

    [Command]
    void CmdToggleOrbs()
    {
        if (!EnsureServerState())
            return;
        Store(ToggleOrbs(Capture()));
        Commit();
    }

    [Command]
    void CmdToggleMute()
    {
        if (!EnsureServerState())
            return;
        serverState.Mute = !serverState.Mute;
        Commit();
    }

    [Command]
    void CmdToggleFloor()
    {
        if (!EnsureServerState())
            return;
        serverState.HideFloor = !serverState.HideFloor;
        Commit();
    }

    [Command]
    void CmdScreen(int action)
    {
        if (!EnsureServerState())
            return;
        serverState.Screen = action;
        serverState.ScreenArmed = true;
        Commit();
    }

    void OnRevision(int _, int __)
    {
        if (isServer || !syncedReady)
            return;

        ApplyControls(ReadState());
    }

    void OnClock(float _, float __)
    {
        NoteClock();
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
        captured.Clock = Time.timeSinceLevelLoad * Mathf.Max(0f, captured.TimeScale);
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

    void Commit()
    {
        serverState.Revision++;
        ApplyControls(serverState);
        ClassController room = ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>());
        if (room != null)
            room.MatchTurnCount(serverState.Turns);
        RoomSimulationClock.Publish(serverState.Clock);
        if (clockDriver != null)
            clockDriver.WriteToSyncVars();
    }

    void DriveServerClock()
    {
        if (this != clockDriver)
            return;
        if (!EnsureServerState())
            return;

        serverState.Clock += Time.deltaTime * Mathf.Max(0f, serverState.TimeScale);
        WriteToSyncVars();
        RoomSimulationClock.Publish(serverState.Clock);
    }

    void WriteToSyncVars()
    {
        syncedReady = true;
        syncedTimeScale = serverState.TimeScale;
        syncedDistance = serverState.DistanceScale;
        syncedOrbs = serverState.Orbs;
        syncedMute = serverState.Mute;
        syncedFloor = serverState.HideFloor;
        syncedTurns = serverState.Turns;
        syncedScreen = serverState.Screen;
        syncedScreenArmed = serverState.ScreenArmed;
        syncedRevision = serverState.Revision;
        syncedClock = serverState.Clock;
    }

    void NoteClock()
    {
        clockReceivedAt = Time.timeAsDouble;
        clockTimed = true;
    }

    float DisplayClock()
    {
        if (!clockTimed)
            return syncedClock;

        return syncedClock + (float)(Time.timeAsDouble - clockReceivedAt) * Mathf.Max(0f, syncedTimeScale);
    }

    ServerState ReadState()
    {
        return new ServerState
        {
            TimeScale = syncedTimeScale,
            DistanceScale = syncedDistance,
            Orbs = syncedOrbs,
            Mute = syncedMute,
            HideFloor = syncedFloor,
            Turns = syncedTurns,
            Screen = syncedScreen,
            ScreenArmed = syncedScreenArmed
        };
    }

    static void ApplyControls(ServerState state)
    {
        SolarSystemMotionController motion = Object.FindFirstObjectByType<SolarSystemMotionController>();
        if (motion != null)
        {
            if (!Mathf.Approximately(motion.DistanceScale, state.DistanceScale))
                motion.SetDistanceScale(state.DistanceScale);
            if (!Mathf.Approximately(motion.TimeScale, state.TimeScale))
                motion.SetTimeScale(state.TimeScale);
            if (motion.ShowInteractionOrbs != state.Orbs)
                motion.SetShowInteractionOrbs(state.Orbs);
        }

        ClassController room = ClassController.ResolveExisting(Object.FindFirstObjectByType<ClassController>());
        if (room != null)
        {
            room.SetMuteGuests(state.Mute);
            room.SetHideFloorAreas(state.HideFloor);
        }

        if (state.ScreenArmed && state.Screen != appliedScreen)
        {
            appliedScreen = state.Screen;
            if (System.Enum.IsDefined(typeof(ScreenControlButton.Action), state.Screen))
                ScreenControlButton.Execute((ScreenControlButton.Action)state.Screen, Object.FindFirstObjectByType<MainScreen>());
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
