
using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System;

namespace StreamerBotLib.GUI
{
    public class GUITwitchBots : GUIBotBase
    {
        public event EventHandler OnLiveStreamStarted;
        public event EventHandler OnLiveStreamUpdated;
        public event EventHandler OnFollowerBotStarted;
        public event EventHandler OnLiveStreamStopped;

        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public TwitchBotChatClient TwitchIO { get; private set; }
        public TwitchBotFollowerSvc TwitchFollower { get; private set; }
        public TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; }
        public TwitchBotClipSvc TwitchClip { get; private set; }
        public TwitchBotUserSvc TwitchBotUserSvc { get; private set; }
        public TwitchBotPubSub TwitchBotPubSub { get; private set; }

        public GUITwitchBots()
        {
            TwitchIO = BotsTwitch.TwitchBotChatClient;
            TwitchFollower = BotsTwitch.TwitchFollower;
            TwitchClip = BotsTwitch.TwitchBotClipSvc;
            TwitchLiveMonitor = BotsTwitch.TwitchLiveMonitor;
            TwitchBotUserSvc = BotsTwitch.TwitchBotUserSvc;
            TwitchBotPubSub = BotsTwitch.TwitchBotPubSub;

            TwitchIO.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchIO.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchFollower.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchFollower.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchLiveMonitor.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchLiveMonitor.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchClip.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchBotPubSub.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotPubSub.OnBotStopped += TwitchBot_OnBotStopped;

            BotsTwitch.RaidCompleted += Twitch_RaidCompleted;
        }

        public void Send(string msg)
        {
            TwitchIO.Send(msg);
        }

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });

            TwitchLiveMonitor.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
            TwitchLiveMonitor.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
            TwitchLiveMonitor.LiveStreamMonitor.OnStreamOffline += LiveStreamMonitor_OnStreamOffline;
        }

        private void Twitch_RaidCompleted(object sender, EventArgs e)
        {
            LiveStreamMonitor_OnStreamOffline(this, new());
        }

        private void LiveStreamMonitor_OnStreamOffline(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOfflineArgs e)
        {
            if (OptionFlags.TwitchChannelName == e.Channel) // ensure monitoring other channels doesn't alert GUI status change
            {
                OnLiveStreamStopped?.Invoke(this, new());
            }
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamUpdateArgs e)
        {
            if (OptionFlags.TwitchChannelName == e.Channel) // ensure monitoring other channels doesn't alert GUI status change
            {
                OnLiveStreamUpdated?.Invoke(this, new());
            }
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOnlineArgs e)
        {
            if (OptionFlags.TwitchChannelName == e.Channel) // ensure monitoring other channels doesn't alert GUI status change
            {
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


        #region Events
        public void RegisterGetCategory(EventHandler<OnGetChannelGameNameEventArgs> GetCategoryEvent)
        {
            TwitchBotUserSvc.GetChannelGameName += GetCategoryEvent;
        }

        public void RegisterChannelPoints(EventHandler<OnGetChannelPointsEventArgs> GetPointsEvent)
        {
            TwitchBotUserSvc.GetChannelPoints += GetPointsEvent;
        }

        public void GetUserGameCategory(string UserName)
        {
            _ = TwitchBotUserSvc.GetUserGameCategory(UserName: UserName);
        }


        public void GetChannelPoints(string UserName)
        {
            _ = TwitchBotUserSvc.GetUserCustomRewards(UserName: UserName);
        }

        #endregion
    }
}
