public static class GuestMutePolicy
{
    public static bool ShouldMute(bool muteGuests, bool isHostPlayer)
    {
        return muteGuests && !isHostPlayer;
    }
}
