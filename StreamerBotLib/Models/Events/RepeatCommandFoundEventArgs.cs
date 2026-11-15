using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    internal class RepeatCommandFoundEventArgs : BotPlatformEventArgs
    {
        internal string Command { get; set; }
        internal bool Announcement { get; set; }
    }
}
