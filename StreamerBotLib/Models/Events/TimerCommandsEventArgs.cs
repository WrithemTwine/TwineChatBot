namespace StreamerBotLib.Models.Events
{
    public class TimerCommandsEventArgs : BotPlatformEventArgs
    {
        public string Message { get; set; }
        public int RepeatMsg { get; set; }
        public bool Announcement { get; set; } = false;
    }
}
