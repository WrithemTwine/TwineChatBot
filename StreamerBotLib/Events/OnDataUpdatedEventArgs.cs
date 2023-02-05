using System;
using System.Collections.Generic;

namespace StreamerBotLib.Events
{
    public class OnDataUpdatedEventArgs : EventArgs
    {
        public List<string> UpdatedTables = new();
    }
}
