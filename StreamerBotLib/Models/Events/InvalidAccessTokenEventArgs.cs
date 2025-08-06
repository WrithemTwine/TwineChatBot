using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.Models.Events
{
    public class InvalidAccessTokenEventArgs(Platform platform, BotType botType) : EventArgs
    {
        public Platform Platform { get; set; } = platform;
        public BotType BotType { get; set; } = botType;
    }
}
