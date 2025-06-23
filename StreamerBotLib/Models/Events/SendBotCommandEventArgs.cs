
namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models;
    public class SendBotCommandEventArgs : EventArgs
    {
        public CmdMessage CmdMessage { get; set; }
    }
}
