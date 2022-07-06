using StreamerBotLib.Enums;
using StreamerBotLib.Models;

using System;

namespace StreamerBotLib.Events
{
    public class BanUserRequestEventArgs : EventArgs
    {
        public LiveUser User { get; set; }
        public int Duration { get; set; }
        public BanReasons BanReason { get; set; }
    }
}
