using System;

namespace StreamerBot.Events
{
    public class PostChannelMessageEventArgs : EventArgs
    {
        public string Msg { get; set; }
    }
}
