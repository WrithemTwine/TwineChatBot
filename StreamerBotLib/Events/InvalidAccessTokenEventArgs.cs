using StreamerBotLib.Enums;

namespace StreamerBotLib.Events
{
    public class InvalidAccessTokenEventArgs(Platform platform, BotType botType) : EventArgs
    {
        public Platform Platform { get; set; } = platform;
        public BotType BotType { get; set; } = botType;
    }
}
