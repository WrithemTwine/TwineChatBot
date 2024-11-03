using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public class AddNewMultiChannelUserEventArgs(LiveUser liveUser) : EventArgs
    {
        public LiveUser LiveUser { get; set; } = liveUser;
    }
}
