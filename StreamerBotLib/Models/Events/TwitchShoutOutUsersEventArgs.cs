
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models;
    public class TwitchShoutOutUsersEventArgs(LiveUser user)
    {
        public LiveUser User { get; set; } = user;
    }
}
