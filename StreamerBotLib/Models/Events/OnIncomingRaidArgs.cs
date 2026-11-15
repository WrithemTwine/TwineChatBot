using StreamerBotLib.Models.Data;
using StreamerBotLib.Models.Users;

namespace StreamerBotLib.Models.Events
{
    public class OnIncomingRaidArgs : BotPlatformEventArgs
    {
        public LiveUser LiveUser { get; set; }
        public DateTime RaidTime { get; set; }
        public int ViewerCount { get; set; }
        public CategoryData Category { get; set; }
    }
}
