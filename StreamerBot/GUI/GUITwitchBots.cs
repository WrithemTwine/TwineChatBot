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
        //public static TwitchBotChatClient TwitchIO { get; private set; } = new();
        public TwitchBotFollowerSvc TwitchFollower { get; private set; }
        //public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();
        //public static TwitchBotClipSvc TwitchClip { get; private set; } = new();
        //public static TwitchBotUserSvc TwitchUsers { get; private set; } = new();

        public string TwitchLiveMonitor { get; set; }
        public string TwitchIO { get; set; }
        public string TwitchClip { get; set; }

        public GUITwitchBots()
        {
            TwitchFollower = BotsTwitch.TwitchFollower;

            TwitchFollower.OnBotStarted += TwitchFollower_OnBotStarted;
            TwitchFollower.OnBotStopped += TwitchFollower_OnBotStopped;
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
