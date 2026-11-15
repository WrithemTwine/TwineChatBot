using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class BotStartStopEventArgs : BotPlatformEventArgs
    {
        public bool Started { get; set; }
        public bool Stopped { get; set; }
    }
}
