using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class TimerCommandsEventArgs : EventArgs
    {
        public Platform Platform { get; set; }
        public string Message { get; set; }
        public int RepeatMsg { get; set; }
        public bool Announcement { get; set; } = false;
    }
}
