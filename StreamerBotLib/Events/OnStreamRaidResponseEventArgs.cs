using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.Events
{
    public class OnStreamRaidResponseEventArgs : EventArgs
    {
        public string ToChannel { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMature { get; set; }
    }
}
