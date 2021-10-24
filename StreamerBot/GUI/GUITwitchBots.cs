using StreamerBot.BotClients;
using StreamerBot.BotClients.Twitch;

namespace StreamerBot.GUI
{
    public class GUITwitchBots
    {
        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        //public static TwitchBotChatClient TwitchIO { get; private set; } = new();
        public TwitchBotFollowerSvc TwitchFollower { get; private set; }
        //public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();
        //public static TwitchBotClipSvc TwitchClip { get; private set; } = new();
        //public static TwitchBotUserSvc TwitchUsers { get; private set; } = new();

        public GUITwitchBots()
        {

            TwitchFollower = BotsTwitch.TwitchFollower;
        
        }
    }
}
