using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class AddNewMultiChannelUserEventArgs(LiveUser liveUser) : BotPlatformEventArgs
    {
        public LiveUser LiveUser { get; set; } = liveUser;
    }
}
