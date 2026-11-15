using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class PostChannelMessageEventArgs : BotPlatformEventArgs
    {
        public int RepeatMsg { get; set; } = 0;
        public bool Announcement { get; set; } = false;
        public string Msg { get; set; }
    }
}
