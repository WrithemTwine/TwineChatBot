
using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.Events;

namespace StreamerBotLib.GUI
{
    public class GUITwitchBots : GUIBotBase
    {
        public static event EventHandler OnFollowerBotStarted;

        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public static TwitchBotEventSubChatClient TwitchBotEventSubChatClient { get; private set; } = BotsTwitch.TwitchBotEventSubChatClient;
        public static TwitchBotSendChatClient TwitchBotSendChatClient { get; private set; } = BotsTwitch.TwitchBotSendChatClient;
        public static TwitchBotLiveMonitorSvc TwitchBotLiveMonitorSvc { get; private set; } = BotsTwitch.TwitchBotLiveMonitorSvc;
        public static TwitchBotClipSvc TwitchClip { get; private set; } = BotsTwitch.TwitchBotClipSvc;
        public static TwitchHelixBot TwitchHelixBot { get; private set; } = BotsTwitch.TwitchHelixBot;
        public static TwitchStreamerEventSubBotScopes TwitchStreamerEventSubBotScopes { get; private set; } = BotsTwitch.TwitchStreamerEventSubBotScopes;
        public static TwitchStreamerEventSubBotNoScopes TwitchStreamerEventSubBotNoScopes { get; private set; } = BotsTwitch.TwitchStreamerEventSubBotNoScopes;
        public GUITwitchBots()
        {
            TwitchBotEventSubChatClient.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotEventSubChatClient.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchBotEventSubChatClient.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchBotLiveMonitorSvc.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotLiveMonitorSvc.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchBotLiveMonitorSvc.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchClip.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchClip.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchStreamerEventSubBotScopes.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchStreamerEventSubBotScopes.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchStreamerEventSubBotScopes.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchStreamerEventSubBotNoScopes.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchStreamerEventSubBotNoScopes.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchStreamerEventSubBotNoScopes.OnBotFailedStart += TwitchBot_OnBotFailedStart;
        }

        public static void Send(string msg)
        {
            TwitchBotSendChatClient.Send(msg);
        }

        private void TwitchBot_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsActive == true });
        }

        private void TwitchBot_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsActive == true });
        }

        private void TwitchBot_OnBotFailedStart(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotFailedStart(new() { BotName = currBot.BotClientName, Started = currBot.IsActive == true });
        }

        #region Events
        public static void RegisterChannelPoints(EventHandler<OnGetChannelPointsEventArgs> GetPointsEvent)
        {
            TwitchHelixBot.GetChannelPoints += GetPointsEvent;
        }

        public static void GetChannelPoints(string UserName)
        {
            _ = TwitchHelixBot.GetUserCustomRewards(UserName: UserName);
        }

        #endregion
    }
}
