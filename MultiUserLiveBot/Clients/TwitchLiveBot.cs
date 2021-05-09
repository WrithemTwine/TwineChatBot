
using MultiUserLiveBot.Data;
using MultiUserLiveBot.Properties;

using System;
using System.Collections.Generic;

using TwitchLib.Api;
using TwitchLib.Api.Core;
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


        public DataManager DataManage { get; set; }

        public TwitchLiveBot()
        {
            ChatClientName = "Twitch";
            RefreshSettings();
        }

        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        { 

            string msg = Settings.Default.LiveMsg != "" ? Settings.Default.LiveMsg : "#user is now live streaming #category - #title! Come join and say hi at: #url";

            Dictionary<string, string> dictionary = new()
            {
                { "#user", e.Stream.UserName },
                { "#category", e.Stream.GameName },
                { "#title", e.Stream.Title },
                { "#url", "https://www.twitch.tv/" + e.Stream.UserName }
            };

            // true posted new event, false did not post
            bool PostedLive = DataManage.PostStreamDate(e.Stream.UserName, e.Stream.StartedAt.ToLocalTime());

            if (PostedLive)
            {
                // false if the date didn't match, true if an event matches
                bool MultiLive = DataManage.CheckStreamDate(e.Stream.UserName, e.Stream.StartedAt.ToLocalTime());

                if ((Settings.Default.PostMultiLive && MultiLive) || !MultiLive)
                {
                    foreach (Uri u in DataManage.GetDiscordLinks())
                    {
                        DiscordWebhook.SendLiveMessage(u, GoLiveWindow.ParseReplace(msg, dictionary)).Wait();
                    }
                }
            }
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

        public bool StartBot(List<string> ChannelList)
        {
            Connect(ChannelList);
            LiveStreamMonitor?.Start();
            return true;
        }

        public override bool StopBot()
        {
            if (LiveStreamMonitor?.Enabled==true)
            {
                LiveStreamMonitor.Stop();
            }
            return true;
        }
    }
}
