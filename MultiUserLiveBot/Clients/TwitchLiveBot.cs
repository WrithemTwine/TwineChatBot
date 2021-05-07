using ChatBot_Net5.Clients;

using MultiUserLiveBot.Properties;

using System;
using System.Collections.Generic;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace MultiUserLiveBot.Clients
{
    public class TwitchLiveBot : IOModule
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public static LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        public event EventHandler<LiveAlertArgs> OnLiveNotification;

        private void LiveAlertHandler(Stream stream)
        {
            OnLiveNotification?.Invoke(this, new() { ChannelStream = stream });
        }

        public TwitchLiveBot()
        {
            ChatClientName = "Twitch";
            RefreshSettings();
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            LiveAlertHandler(e.Stream);
        }

        public bool Connect(List<string> ChannelList)
        {
            ApiSettings apilive = new() { AccessToken = AccessToken, ClientId = ClientID };
            LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(FrequencyLiveNotifyTime, 0));
            LiveStreamMonitor.SetChannelsByName(ChannelList);

            LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;

            return true;
        }

        public override bool RefreshSettings()
        {
            SaveParams();
            AccessToken = Settings.Default.TwitchAccessToken;
            BotUserName = Settings.Default.TwitchBotUserName;
            ClientID = Settings.Default.TwitchClientID;
            FrequencyLiveNotifyTime = Settings.Default.TwitchGoLiveFrequency;
            RefreshToken = Settings.Default.TwitchRefreshToken;
            RefreshDate = Settings.Default.TwitchRefreshDate;
            return true;
        }

        public override bool StartBot()
        {
            LiveStreamMonitor.Start();
            return true;
        }

        public override bool StopBot()
        {
            LiveStreamMonitor.Stop();
            return true;
        }
    }
}
