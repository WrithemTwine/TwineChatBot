using StreamerBotLib.Enums;

namespace StreamerBotLib.Events
{
    public class InvalidAccessTokenEventArgs : EventArgs
    {
        public Platform Platform { get; set; }
        public string BotType { get; set; }

        public InvalidAccessTokenEventArgs(Platform platform, string botType)
        {
            Platform = platform;
            BotType = botType;
        }
    }
}
