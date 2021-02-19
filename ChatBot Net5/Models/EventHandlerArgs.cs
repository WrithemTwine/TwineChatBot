using System;

namespace ChatBot_Net5.Models
{
    public class OnBeginUserDataChangedEventArgs : EventArgs
    {
        public bool Start { get; set; } = true;
    }

    public class OnEndUserDataChangedEventArgs : EventArgs
    {
        public bool End { get; set; } =true;
    }
}
