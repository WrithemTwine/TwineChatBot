using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class StreamerOnExistingUserDetectedArgs : BotPlatformEventArgs
    {
        public List<LiveUser> Users { get; set; }
    }
}
