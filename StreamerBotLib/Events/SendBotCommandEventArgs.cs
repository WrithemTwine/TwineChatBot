using StreamerBotLib.Models;

namespace StreamerBotLib.Events
{
    public class SendBotCommandEventArgs : EventArgs
    {
        public CmdMessage CmdMessage { get; set; }
    }
}
