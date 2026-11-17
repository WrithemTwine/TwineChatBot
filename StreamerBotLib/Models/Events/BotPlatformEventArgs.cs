using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class BotPlatformEventArgs : EventArgs
    {
        public Platform Platform { get; set; } = Platform.Default;
        public Bots Bots { get; set; } = Bots.Default;
    }
}
