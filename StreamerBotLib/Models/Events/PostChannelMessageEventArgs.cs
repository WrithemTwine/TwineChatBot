using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class PostChannelMessageEventArgs : EventArgs
    {
        public Platform Platform { get; set; } = Platform.Default;
        public int RepeatMsg { get; set; } = 0;
        public bool Announcement { get; set; } = false;
        public string Msg { get; set; }
    }
}
