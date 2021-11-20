using System;

namespace StreamerBot.Events
{
    public class OnIncomingRaidArgs : EventArgs
    {
        public string DisplayName { get; set; }
        public DateTime RaidTime { get; set; }
        public string ViewerCount { get; set; }
        public string Category { get; set; }
    }
}
