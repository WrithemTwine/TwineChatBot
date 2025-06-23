using StreamerBotLib.Models;

namespace StreamerBotLib.Models.Events
{
    public class OnIncomingRaidArgs : EventArgs
    {
        public LiveUser LiveUser { get; set; }
        public DateTime RaidTime { get; set; }
        public int ViewerCount { get; set; }
        public CategoryData Category { get; set; }
    }
}
