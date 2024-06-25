using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.Events
{
    public class OnDataCollectionUpdatedEventArgs(string TableName) : EventArgs
    {
        public string DatabaseModelName { get; set; } = TableName;
    }
}
