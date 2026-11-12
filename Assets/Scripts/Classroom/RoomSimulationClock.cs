public static class RoomSimulationClock
{
    public static bool Active { get; private set; }
    public static float Time { get; private set; }

    public static bool TryRead(out float time)
    {
        time = Time;
        return Active;
    }

    public static void Publish(float time)
    {
        Active = true;
        Time = time;
    }

    public static void Clear()
    {
        Active = false;
        Time = 0f;
    }
}
