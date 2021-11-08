using StreamerBot.BotClients.Twitch;

namespace StreamerBot.BotClients
{
    public class BotsTwitch : BotsBase
    {
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();

        public BotsTwitch()
        {
            AddBot(TwitchFollower);
        }

    }
}
