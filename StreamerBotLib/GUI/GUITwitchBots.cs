
using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System.Reflection;

namespace StreamerBotLib.GUI
{
    public class GUITwitchBots : GUIBotBase
    {
        public static event EventHandler OnLiveStreamStarted;
        public static event EventHandler OnLiveStreamUpdated;
        public static event EventHandler OnFollowerBotStarted;
        public static event EventHandler OnLiveStreamStopped;

        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public static TwitchBotEventSubChatClient TwitchBotEventSubChatClient { get; private set; } = BotsTwitch.TwitchBotEventSubChatClient;
        public static TwitchBotSendChatClient TwitchBotSendChatClient { get; private set; } = BotsTwitch.TwitchBotSendChatClient;
        public static TwitchBotLiveMonitorSvc TwitchBotLiveMonitorSvc { get; private set; } = BotsTwitch.TwitchBotLiveMonitorSvc;
        public static TwitchBotClipSvc TwitchClip { get; private set; } = BotsTwitch.TwitchBotClipSvc;
        public static TwitchHelixBot TwitchHelixBot { get; private set; } = BotsTwitch.TwitchHelixBot;
        public static TwitchStreamerEventSubBotScopes TwitchStreamerEventSubBot { get; private set; } = BotsTwitch.TwitchStreamerEventSubBotScopes;
        public static TwitchStreamerEventSubBotNoScopes TwitchStreamerEventSubBotNoScopes { get; private set; } = BotsTwitch.TwitchStreamerEventSubBotNoScopes;
        public GUITwitchBots()
        {
            TwitchBotEventSubChatClient.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotEventSubChatClient.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchBotEventSubChatClient.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchBotLiveMonitorSvc.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotLiveMonitorSvc.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchBotLiveMonitorSvc.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchBotLiveMonitorSvc.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchClip.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchClip.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchStreamerEventSubBot.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchStreamerEventSubBot.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchStreamerEventSubBot.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchStreamerEventSubBotNoScopes.OnNewLiveStreamStarted += TwitchStreamerEventSubBot_OnNewLiveStreamStarted;
            TwitchStreamerEventSubBotNoScopes.NewStreamOnline += TwitchStreamerEventSubBot_NewStreamOnline; ;
            TwitchStreamerEventSubBotNoScopes.NewChannelUpdate += TwitchStreamerEventSubBot_NewChannelUpdate; ;
            TwitchStreamerEventSubBotNoScopes.NewStreamOffline += TwitchStreamerEventSubBot_NewStreamOffline; ;

            BotsTwitch.RaidCompleted += Twitch_RaidCompleted;
        }

        private void TwitchStreamerEventSubBot_OnNewLiveStreamStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now online.");

            OnLiveStreamStarted?.Invoke(this, new());
        }

        public static void Send(string msg)
        {
            TwitchBotSendChatClient.Send(msg);
        }

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
        }

        private void TwitchStreamerEventSubBot_NewStreamOnline(object sender, BotClients.Twitch.TwitchLib.Events.EventSub.NewStreamOnlineEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now online.");

            OnLiveStreamStarted?.Invoke(this, new());
        }

        private void TwitchStreamerEventSubBot_NewChannelUpdate(object sender, BotClients.Twitch.TwitchLib.Events.EventSub.NewChannelUpdateEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now updated.");

            OnLiveStreamUpdated?.Invoke(this, new());
        }

        private void TwitchStreamerEventSubBot_NewStreamOffline(object sender, BotClients.Twitch.TwitchLib.Events.EventSub.NewStreamOfflineEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now offline.");

            OnLiveStreamStopped?.Invoke(this, new());
        }

        private void Twitch_RaidCompleted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIBotComs, "Raid now reported complete.");

            TwitchStreamerEventSubBot_NewStreamOffline(this, new(null));
        }

        private void TwitchBot_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsActive == true });
        }

        private void TwitchBot_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsActive == true });
        }

        private void TwitchBot_OnBotFailedStart(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotFailedStart(new() { BotName = currBot.BotClientName, Started = currBot.IsActive == true });
        }

        #region Events
        public static void RegisterGetCategory(EventHandler<OnGetChannelGameNameEventArgs> GetCategoryEvent)
        {
            TwitchHelixBot.GetChannelGameName += GetCategoryEvent;
        }

        public static void RegisterChannelPoints(EventHandler<OnGetChannelPointsEventArgs> GetPointsEvent)
        {
            TwitchHelixBot.GetChannelPoints += GetPointsEvent;
        }

        public static void GetUserGameCategory(string UserName)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIBotComs, "Sending request for user game category.");

            _ = TwitchHelixBot.GetUserGameCategory(UserName: UserName);
        }


        public static void GetChannelPoints(string UserName)
        {
            _ = TwitchHelixBot.GetUserCustomRewards(UserName: UserName);
        }

        #endregion
    }
}
