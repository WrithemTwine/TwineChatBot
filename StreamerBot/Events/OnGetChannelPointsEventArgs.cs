using System;
using System.Collections.Generic;

namespace StreamerBot.Events
{
    public class OnGetChannelPointsEventArgs : EventArgs
    {
        public List<string> ChannelPointNames { get; set; }
    }
}
