using StreamerBotLib.Enums;

namespace StreamerBotLib.Events
{
    public class BotStartStopEventArgs : EventArgs
    {
        public Bots BotName { get; set; }
        public bool Started { get; set; }
        public bool Stopped { get; set; }
    }
}
