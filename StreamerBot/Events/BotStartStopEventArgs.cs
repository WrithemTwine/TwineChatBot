using System;

namespace StreamerBot.Events
{
    public class BotStartStopEventArgs : EventArgs
    {
        public Enums.Bots BotName { get; set; }
        public bool Started { get; set; }
        public bool Stopped { get; set; }
    }
}
