using System;
using System.Data;

namespace StreamerBotLib.Events
{
    public class UpdatedDataRowArgs : EventArgs
    {
        public DataRow UpdatedDataRow { get; set; }
    }
}
