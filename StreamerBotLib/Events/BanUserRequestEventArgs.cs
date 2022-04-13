using StreamerBotLib.Enums;

using System;

namespace StreamerBotLib.Events
{
    public class BanUserRequestEventArgs : EventArgs
    {
        public Bots Source { get; set; }
        public string UserName { get; set; }
        public int Duration { get; set; }
        public BanReasons BanReason { get; set; }
    }
}
