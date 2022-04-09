using System;

namespace StreamerBotLib.Events
{
    public class UpdatedDataRowArgs : EventArgs
    {
        public bool RowChanged { get; set; }
    }
}
