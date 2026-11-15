using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class StreamerOnUserLeftArgs : BotPlatformEventArgs
    {
        public LiveUser LiveUser { get; set; }
    }
}
