using System;

namespace ChatBot_Net5.Events
{
    internal class TimerCommandsEventArgs : EventArgs
    {
        internal string Message { get; set; }
    }
}
