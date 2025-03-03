using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public class TwitchShoutOutUsersEventArgs(LiveUser user)
    {
        public LiveUser User { get; set; } = user;
    }
}
