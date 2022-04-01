using StreamerBotLib.Enums;

using System;

namespace StreamerBotLib.Events
{
    public class BanUserRequestEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public int Duration { get; set; }
        public BanReason BanReason { get; set; }
    }
}
