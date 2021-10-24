using StreamerBot.BotClients.Twitch;
using StreamerBot.Interfaces;

namespace StreamerBot.BotClients
{
    public class BotsTwitch : IBotTypes
    {
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();

        public BotsTwitch()
        {

        }

        public void Send(string s)
        {
            
        }
    }
}
