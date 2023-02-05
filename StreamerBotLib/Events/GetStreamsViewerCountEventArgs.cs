using System;

namespace StreamerBotLib.Events
{
    public class GetStreamsEventArgs : EventArgs
    {
        public int ViewerCount { get; set; }
    }
}
