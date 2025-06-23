namespace StreamerBotLib.Models.Events
{
    using StreamerBotLib.Models.Enums;

    public class BotStartStopEventArgs : EventArgs
    {
        public Bots BotName { get; set; }
        public bool Started { get; set; }
        public bool Stopped { get; set; }
    }
}
