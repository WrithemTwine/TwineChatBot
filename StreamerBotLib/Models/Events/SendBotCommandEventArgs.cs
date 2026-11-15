using StreamerBotLib.Models.Data;

namespace StreamerBotLib.Models.Events
{
    public class SendBotCommandEventArgs : BotPlatformEventArgs
    {
        public CmdMessage CmdMessage { get; set; }
    }
}
