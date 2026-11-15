using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class StreamerOnUserJoinedArgs : BotPlatformEventArgs
    {
        public LiveUser LiveUser { get; set; }
    }
}
