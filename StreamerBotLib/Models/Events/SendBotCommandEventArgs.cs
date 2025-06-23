using StreamerBotLib.Models;

namespace StreamerBotLib.Models.Events
{
    public class SendBotCommandEventArgs : EventArgs
    {
        public CmdMessage CmdMessage { get; set; }
    }
}
