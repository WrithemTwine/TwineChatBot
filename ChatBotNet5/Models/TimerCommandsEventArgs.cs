using System;

namespace ChatBot_Net5.Models
{
    internal class TimerCommandsEventArgs : EventArgs
    {
        internal string Message { get; set; }
    }
}
