using StreamerBot.BotClients;
using StreamerBot.BotClients.Twitch;

using System;

namespace StreamerBot.GUI
{
    public class GUITwitchBots : GUIBotBase
    {
        public event EventHandler OnLiveStreamStarted;
        public event EventHandler OnLiveStreamUpdated;

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
            TwitchFollower.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchLiveMonitor.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchLiveMonitor.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchClip.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchBot_OnBotStopped;

            TwitchBotPubSub.OnBotStarted += TwitchBot_OnBotStarted;
            TwitchBotPubSub.OnBotStopped += TwitchBot_OnBotStopped;
        }

        public void Send(string msg)
        {
            TwitchIO.Send(msg);
        }

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted( new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });

            TwitchLiveMonitor.LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;
            TwitchLiveMonitor.LiveStreamMonitor.OnStreamUpdate += LiveStreamMonitor_OnStreamUpdate;
        }

        private void LiveStreamMonitor_OnStreamUpdate(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamUpdateArgs e)
        {
            OnLiveStreamUpdated?.Invoke(this, new());
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOnlineArgs e)
        {
            OnLiveStreamStarted?.Invoke(this, new());
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

    }
}
