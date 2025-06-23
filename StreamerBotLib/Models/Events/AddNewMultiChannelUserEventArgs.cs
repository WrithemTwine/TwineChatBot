
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models;

    public class AddNewMultiChannelUserEventArgs(LiveUser liveUser) : EventArgs
    {
        public LiveUser LiveUser { get; set; } = liveUser;
    }
}
