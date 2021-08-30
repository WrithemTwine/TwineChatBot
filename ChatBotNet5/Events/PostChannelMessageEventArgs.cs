using System;

namespace ChatBot_Net5.Events
{
    public class PostChannelMessageEventArgs : EventArgs
    {
        public string Msg { get; set; }
    }
}
