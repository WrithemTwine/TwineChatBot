using StreamerBotLib.Enums;

namespace StreamerBotLib.Events
{
    public class InvalidAccessTokenEventArgs : EventArgs
    {
        public Platform Platform { get; set; }
        public BotType BotType { get; set; }

        public InvalidAccessTokenEventArgs(Platform platform, BotType botType)
        {
            Platform = platform;
            BotType = botType;
        }
    }
}
