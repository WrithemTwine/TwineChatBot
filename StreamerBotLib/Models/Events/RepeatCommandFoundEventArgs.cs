using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    internal class RepeatCommandFoundEventArgs : EventArgs
    {
        internal string Command { get; set; }
        internal bool Announcement { get; set; }
        internal Platform platform { get; set; }
    }
}
