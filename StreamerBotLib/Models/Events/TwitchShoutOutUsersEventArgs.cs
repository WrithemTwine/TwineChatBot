namespace StreamerBotLib.Models.Events
{
    public class TwitchShoutOutUsersEventArgs(LiveUser user)
    {
        public LiveUser User { get; set; } = user;
    }
}
