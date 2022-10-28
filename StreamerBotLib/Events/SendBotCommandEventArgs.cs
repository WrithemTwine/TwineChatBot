using StreamerBotLib.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.Events
{
    public class SendBotCommandEventArgs : EventArgs
    {
        public CmdMessage CmdMessage { get; set; }
    }
}
