using StreamerBotLib.Models;

namespace StreamerBotLib.Models.Events
{
    public class AddNewMultiChannelUserEventArgs(LiveUser liveUser) : EventArgs
    {
        public LiveUser LiveUser { get; set; } = liveUser;
    }
}
