
using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

namespace StreamerBotLib.GUI
{
    public class GUITwitchBots : GUIBotBase
    {
        public static event EventHandler OnFollowerBotStarted;

        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        //public static TwitchBotEventSubChatClient TwitchBotEventSubChatClient { get; private set; } = BotsTwitch.TwitchBotEventSubChatClient;
        public static TwitchBotSendChatClient TwitchBotSendChatClient { get; private set; } = BotsTwitch.TwitchBotSendChatClient;
        public static TwitchEventSub TwitchEventSubBot { get; private set; } = BotsTwitch.TwitchEventSubBot;
        public static TwitchEventSub TwitchEventSubStreamer { get; private set; } = BotsTwitch.TwitchEventSubStreamer;
        public static TwitchBotLiveMonitorSvc TwitchBotLiveMonitorSvc { get; private set; } = BotsTwitch.TwitchBotLiveMonitorSvc;
        public static TwitchBotClipSvc TwitchClip { get; private set; } = BotsTwitch.TwitchBotClipSvc;
        public static TwitchHelixBot TwitchHelixBot { get; private set; } = BotsTwitch.TwitchHelixBot;
        //public static TwitchStreamerEventSubBotScopes TwitchStreamerEventSubBotScopes { get; private set; } = BotsTwitch.TwitchStreamerEventSubBotScopes;
        //public static TwitchStreamerEventSubBotNoScopes TwitchStreamerEventSubBotNoScopes { get; private set; } = BotsTwitch.TwitchStreamerEventSubBotNoScopes;
        public GUITwitchBots()
        {
            LogWriter.DebugLog(".ctor_GUITwitchBots", DebugLogTypes.GUIBotComs, "Building the GUITwitchBots.");

            //TwitchBotEventSubChatClient.OnBotStarted += TwitchBot_OnBotStarted;
            //TwitchBotEventSubChatClient.OnBotStopped += TwitchBot_OnBotStopped;
            //TwitchBotEventSubChatClient.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchEventSubBot.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchEventSubBot.OnBotFailedStart += TwitchBot_OnBotFailedStart;
            TwitchEventSubBot.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchEventSubStreamer.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchEventSubStreamer.OnBotFailedStart += TwitchBot_OnBotFailedStart;
            TwitchEventSubStreamer.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchBotLiveMonitorSvc.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotLiveMonitorSvc.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchBotLiveMonitorSvc.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchClip.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchClip.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            //TwitchStreamerEventSubBotScopes.OnBotStarted += TwitchBot_OnBotStarted;
            //TwitchStreamerEventSubBotScopes.OnBotStopped += TwitchBot_OnBotStopped;
            //TwitchStreamerEventSubBotScopes.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            //TwitchStreamerEventSubBotNoScopes.OnBotStarted += TwitchBot_OnBotStarted;
            //TwitchStreamerEventSubBotNoScopes.OnBotStopped += TwitchBot_OnBotStopped;
            //TwitchStreamerEventSubBotNoScopes.OnBotFailedStart += TwitchBot_OnBotFailedStart;
        }

        public static void Send(string msg)
        {
            LogWriter.DebugLog("Send", DebugLogTypes.GUIBotComs, $"Sending a message, {msg}, to the chat.");
            TwitchBotSendChatClient.Send(msg);
        }

        private void TwitchBot_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            LogWriter.DebugLog("TwitchBot_OnBotStarted", DebugLogTypes.GUIBotComs, $"Bot started, {currBot.BotClientName}.");
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsActive == true });
        }

        private void TwitchBot_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            LogWriter.DebugLog("TwitchBot_OnBotStopped", DebugLogTypes.GUIBotComs, $"Bot stopped, {currBot.BotClientName}.");
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsActive == true });
        }

        private void TwitchBot_OnBotFailedStart(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            LogWriter.DebugLog("TwitchBot_OnBotFailedStart", DebugLogTypes.GUIBotComs, $"Bot failed to start, {currBot.BotClientName}.");
            BotFailedStart(new() { BotName = currBot.BotClientName, Started = currBot.IsActive == true });
        }

        #region Events
        public static void RegisterChannelPoints(EventHandler<OnGetChannelPointsEventArgs> GetPointsEvent)
        {
            LogWriter.DebugLog("RegisterChannelPoints", DebugLogTypes.GUIBotComs, "Registering the channel points event.");
            TwitchHelixBot.GetChannelPoints += GetPointsEvent;
        }

        public static void GetChannelPoints(string UserName)
        {
            LogWriter.DebugLog("GetChannelPoints", DebugLogTypes.GUIBotComs, $"Getting channel points for {UserName}.");
            _ = TwitchHelixBot.GetUserCustomRewards(UserName: UserName);
        }

        #endregion
    }
}
