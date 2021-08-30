using System;

namespace ChatBot_Net5.Events
{
    public class TimerCommandsEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
