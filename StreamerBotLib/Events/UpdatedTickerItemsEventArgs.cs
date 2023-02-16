using StreamerBotLib.Models;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.Events
{
    public class UpdatedTickerItemsEventArgs : EventArgs
    {
        public List<TickerItem> TickerItems { get; set; }
    }
}
