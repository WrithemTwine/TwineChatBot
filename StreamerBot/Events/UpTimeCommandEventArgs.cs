using System;

namespace StreamerBot.Events
{
    public class UpTimeCommandEventArgs : EventArgs
    {
        public string User { get; set; }
        public string Message { get; set; }
    }
}
