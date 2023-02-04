using StreamerBotLib.Models;

using System;

namespace StreamerBotLib.Events
{
    public class SendBotCommandEventArgs : EventArgs
    {
        public CmdMessage CmdMessage { get; set; }
    }
}
