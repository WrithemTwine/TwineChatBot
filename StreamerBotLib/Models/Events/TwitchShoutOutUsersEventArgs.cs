using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class TwitchShoutOutUsersEventArgs(LiveUser user) : EventArgs
    {
        public LiveUser User { get; set; } = user;
    }
}
