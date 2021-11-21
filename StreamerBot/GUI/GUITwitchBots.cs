using StreamerBot.BotClients;
using StreamerBot.BotClients.Twitch;

using System;

namespace StreamerBot.GUI
{
    public class GUITwitchBots : GUIBotBase
    {
        /// <summary>
        /// Specifically Twitch Lib chat bot.
        /// </summary>
        public TwitchBotChatClient TwitchIO { get; private set; }
        public TwitchBotFollowerSvc TwitchFollower { get; private set; }
        public TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; }
        public TwitchBotClipSvc TwitchClip { get; private set; }

        public GUITwitchBots()
        {
            TwitchIO = BotsTwitch.TwitchBotChatClient;
            TwitchFollower = BotsTwitch.TwitchFollower;
            TwitchClip = BotsTwitch.TwitchBotClipSvc;
            TwitchLiveMonitor = BotsTwitch.TwitchLiveMonitor;

            TwitchIO.OnBotStarted += TwitchIO_OnBotStarted;
            TwitchIO.OnBotStopped += TwitchIO_OnBotStopped;

            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchFollower.OnBotStopped += TwitchFollower_OnBotStopped;

            TwitchLiveMonitor.OnBotStarted += TwitchLiveMonitor_OnBotStarted;
            TwitchLiveMonitor.OnBotStopped += TwitchLiveMonitor_OnBotStopped;

            TwitchClip.OnBotStarted += TwitchClip_OnBotStarted;
            TwitchClip.OnBotStopped += TwitchClip_OnBotStopped;
        }

        public void Send(string msg)
        {
            TwitchIO.Send(msg);
        }

        private void TwitchLiveMonitor_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted( new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchLiveMonitor_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchFollower_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchFollower_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchIO_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

        private void TwitchIO_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchClip_OnBotStopped(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStopped(new() { BotName = currBot.BotClientName, Stopped = currBot.IsStopped });
        }

        private void TwitchClip_OnBotStarted(object sender, EventArgs e)
        {
            TwitchBotsBase currBot = sender as TwitchBotsBase;
            BotStarted(new() { BotName = currBot.BotClientName, Started = currBot.IsStarted });
        }

    }
}
