using System;

namespace StreamerBotLib.Events
{
    public class OnStreamRaidResponseEventArgs : EventArgs
    {
        public string ToChannel { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMature { get; set; }
    }
}
