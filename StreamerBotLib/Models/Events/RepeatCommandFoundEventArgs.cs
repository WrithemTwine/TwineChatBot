using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    internal class RepeatCommandFoundEventArgs : EventArgs
    {
        public string Command { get; set; }
        public Platform platform { get; set; }
    }
}
