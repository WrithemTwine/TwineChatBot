using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchLib.Client.Events;

namespace StreamerBotLib.Events
{
    public class StreamerOnExistingUserDetectedArgs : EventArgs
    {
        public List<Models.LiveUser> Users { get; set; }
    }
}
