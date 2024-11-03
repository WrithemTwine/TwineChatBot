using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public class OnIncomingRaidArgs : EventArgs
    {
        public string DisplayName { get; set; }
        public DateTime RaidTime { get; set; }
        public string ViewerCount { get; set; }
        public CategoryData Category { get; set; }
    }
}
