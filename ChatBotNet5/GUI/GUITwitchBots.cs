using ChatBot_Net5.BotClients;

namespace ChatBot_Net5.GUI
{
    public class GUITwitchBots
    {
        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public static TwitchBotChatClient TwitchIO { get; private set; } = new();
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();
        public static TwitchBotClipSvc TwitchClip { get; private set; } = new();
        public static TwitchBotUserSvc TwitchUsers { get; private set; } = new();

        public GUITwitchBots() { }
    }
}
