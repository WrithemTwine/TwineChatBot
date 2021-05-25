using System;

namespace ChatBot_Net5.Events
{
    public class UpTimeCommandArgs : EventArgs
    {
        public string User { get; set; }
        public string Message { get; set; }
    }
}
