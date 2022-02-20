using System;

namespace StreamerBotLib.Events
{
    public class TimerCommandsEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int RepeatMsg { get; set; }
    }
}
