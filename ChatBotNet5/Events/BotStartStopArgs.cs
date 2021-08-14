using System;

namespace ChatBot_Net5.Events
{
    public class BotStartStopArgs : EventArgs
    {
        public Enum.Bots BotName { get; set; }
        public bool Started { get; set; }
        public bool Stopped { get; set; }
    }
}
