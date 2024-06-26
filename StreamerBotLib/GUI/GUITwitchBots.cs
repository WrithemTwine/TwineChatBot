﻿
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
        public static TwitchBotChatClient TwitchChat { get; private set; }
        public static TwitchBotFollowerSvc TwitchFollower { get; private set; }
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; }
        public static TwitchBotClipSvc TwitchClip { get; private set; }
        public static TwitchBotUserSvc TwitchBotUserSvc { get; private set; }
        public static TwitchBotPubSub TwitchBotPubSub { get; private set; }

        public GUITwitchBots()
        {
            TwitchChat = BotsTwitch.TwitchBotChatClient;
            TwitchFollower = BotsTwitch.TwitchFollower;
            TwitchClip = BotsTwitch.TwitchBotClipSvc;
            TwitchLiveMonitor = BotsTwitch.TwitchLiveMonitor;
            TwitchBotUserSvc = BotsTwitch.TwitchBotUserSvc;
            TwitchBotPubSub = BotsTwitch.TwitchBotPubSub;

            TwitchChat.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchChat.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchChat.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchFollower.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchFollower.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchFollower.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchLiveMonitor.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchLiveMonitor.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchLiveMonitor.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchClip.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchClip.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            TwitchBotPubSub.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotPubSub.OnBotStopped += TwitchBot_OnBotStopped;
            TwitchBotPubSub.OnBotFailedStart += TwitchBot_OnBotFailedStart;

            BotsTwitch.RaidCompleted += Twitch_RaidCompleted;
        }

        public static void Send(string msg)
        {
            TwitchChat.Send(msg);
        }

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            TwitchLiveMonitor.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
            TwitchLiveMonitor.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
            TwitchLiveMonitor.LiveStreamMonitor.OnStreamOffline += LiveStreamMonitor_OnStreamOffline;
        }

        private void Twitch_RaidCompleted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIBotComs, "Raid now reported complete.");

            LiveStreamMonitor_OnStreamOffline(this, new());
        }

        private void LiveStreamMonitor_OnStreamOffline(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOfflineArgs e)
        {
            if (OptionFlags.TwitchChannelName == e.Channel || TwitchBotsBase.TwitchChannelId == e.Channel) // ensure monitoring other channels doesn't alert GUI status change
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now offline.");

                OnLiveStreamStopped?.Invoke(this, new());
            }
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamUpdateArgs e)
        {
            if (OptionFlags.TwitchChannelName == e.Channel || TwitchBotsBase.TwitchChannelId == e.Channel) // ensure monitoring other channels doesn't alert GUI status change
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now updated.");

                OnLiveStreamUpdated?.Invoke(this, new());
            }
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOnlineArgs e)
        {
            if (OptionFlags.TwitchChannelName == e.Channel || TwitchBotsBase.TwitchChannelId == e.Channel) // ensure monitoring other channels doesn't alert GUI status change
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBots, "Notify GUI stream is now online.");

                OnLiveStreamStarted?.Invoke(this, new());
            }
        }

        private void TwitchBot_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchBot_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            OnFollowerBotStarted?.Invoke(this, new());
        }

        private void TwitchBot_OnBotFailedStart(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotFailedStart(new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        #region Events
        public static void RegisterGetCategory(EventHandler<OnGetChannelGameNameEventArgs> GetCategoryEvent)
        {
            TwitchBotUserSvc.GetChannelGameName += GetCategoryEvent;
        }

        public static void RegisterChannelPoints(EventHandler<OnGetChannelPointsEventArgs> GetPointsEvent)
        {
            TwitchBotUserSvc.GetChannelPoints += GetPointsEvent;
        }

        public static void GetUserGameCategory(string UserName)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIBotComs, "Sending request for user game category.");

            _ = TwitchBotUserSvc.GetUserGameCategory(UserName: UserName);
        }


        public static void GetChannelPoints(string UserName)
        {
            _ = TwitchBotUserSvc.GetUserCustomRewards(UserName: UserName);
        }

        #endregion
    }
}
