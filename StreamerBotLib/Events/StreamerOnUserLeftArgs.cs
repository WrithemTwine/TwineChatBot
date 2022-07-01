using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.Events
{
    public class StreamerOnUserLeftArgs : EventArgs
    {
        public Models.LiveUser LiveUser { get; set; }
    }
}
