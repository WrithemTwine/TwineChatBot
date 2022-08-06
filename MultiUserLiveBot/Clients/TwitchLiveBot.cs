using MultiUserLiveBot.Properties;

using System;
using System.Collections.Generic;
using System.Globalization;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

using StreamerBotLib.Data.MultiLive;

namespace MultiUserLiveBot.Clients
{
    public partial class TwitchLiveBot : IOModule
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public static LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        /// <summary>
        /// The backend database object for the bot to store data.
        /// </summary>
        public MultiDataManager DataManage { get; set; } = new();

        /// <summary>
        /// Instantiate new bot object and retrieve the settings values into the bot properties.
        /// </summary>
        public TwitchLiveBot()
        {
            ChatClientName = "Twitch";
            DataManage.LoadData();
            _ = RefreshSettings();
        }

        /// <summary>
        /// Event to handle when the bot finds a stream now online.
        /// </summary>
        /// <param name="sender">Object invoking the event.</param>
        /// <param name="e">The params for the event, stream details.</param>
        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            // true posted new event, false did not post
            bool PostedLive = DataManage.PostStreamDate(e.Stream.UserName, e.Stream.StartedAt);

            if (PostedLive)
            {
                string msg = Settings.Default.LiveMsg != "" ? Settings.Default.LiveMsg : "#user is now live streaming #category - #title! Come join and say hi at: #url";

                Dictionary<string, string> dictionary = new()
                {
                    { "#user", e.Stream.UserName },
                    { "#category", e.Stream.GameName },
                    { "#title", e.Stream.Title },
                    { "#url", Resources.TwitchHomepage + e.Stream.UserName }
                };

                // false if the date didn't match, true if an event matches
                bool MultiLive = DataManage.CheckStreamDate(e.Stream.UserName, e.Stream.StartedAt);

                if ((Settings.Default.PostMultiLive && MultiLive) || !MultiLive)
                {
                    LogEntry(GoLiveWindow.ParseReplace(msg, dictionary), e.Stream.StartedAt);
                    foreach (Tuple<string, Uri> u in DataManage.GetWebLinks())
                    {
                        if (u.Item1 == "Discord")
                        {
                            DiscordWebhook.SendLiveMessage(u.Item2, GoLiveWindow.ParseReplace(msg, dictionary)).Wait();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Connect the bot to first establish a connection to Twitch.
        /// </summary>
        /// <param name="ChannelList">The initial list of channels to monitor.</param>
        /// <returns>true: for establishing connection.</returns>
        public bool Connect(List<string> ChannelList)
        {
            ApiSettings apilive = new() { AccessToken = AccessToken, ClientId = ClientID };
            LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(FrequencyLiveNotifyTime, 0));
            if (ChannelList.Count > 100)
            {
                ChannelList.RemoveRange(100, ChannelList.Count - 100);
            }
            LiveStreamMonitor.SetChannelsByName(ChannelList);

            LiveStreamMonitor.OnStreamOnline += LiveStreamMonitor_OnStreamOnline;

            return true;
        }

        /// <summary>
        /// Save settings for any changes in the GUI, then refresh the settings into the bot.
        /// </summary>
        /// <returns>true: for refreshing the settings.</returns>
        public override bool RefreshSettings()
        {
            _ = SaveParams();
            AccessToken = Settings.Default.TwitchAccessToken;
            BotUserName = Settings.Default.TwitchBotUserName;
            ClientID = Settings.Default.TwitchClientID;
            FrequencyLiveNotifyTime = Settings.Default.TwitchGoLiveFrequency;
            RefreshToken = Settings.Default.TwitchRefreshToken;
            RefreshDate = Settings.Default.TwitchRefreshDate;
            return true;
        }

        /// <summary>
        /// Start the bot, which establishes the connection.
        /// </summary>
        /// <returns>true: for the bot beings started.</returns>
        public override bool StartBot()
        {
            List<string> names = DataManage.GetChannelNames();

            if (names.Count == 0)
            {
                return false;
            }

            Connect(names);
            LiveStreamMonitor?.Start();
            LogEntry(string.Format(CultureInfo.CurrentCulture, "Bot started and monitoring {0} channels.", names.Count.ToString()), DateTime.Now.ToLocalTime());
            return true;
        }

        /// <summary>
        /// Stop the bot from actively monitoring channels for going live.
        /// </summary>
        /// <returns>true: if bot is stopped; false: if bot wasn't enabled.</returns>
        public override bool StopBot()
        {
            if (LiveStreamMonitor?.Enabled == true)
            {
                LiveStreamMonitor.Stop();
                LogEntry("Bot stopped.", DateTime.Now.ToLocalTime());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the list of channels from the database and updates those channels within the bot.
        /// </summary>
        /// <returns>true: if there are channels in the database to monitor; false: there are no channels to update.</returns>
        public bool UpdateChannelList()
        {
            if (LiveStreamMonitor != null)
            {
                DataManage.SaveData();
                List<string> channels = DataManage.GetChannelNames();

                if (channels.Count > 0)
                {
                    LogEntry(data: string.Format(CultureInfo.CurrentCulture, $"Monitored {LiveStreamMonitor.ChannelsToMonitor.Count} channels updated to {channels.Count}!"), dateTime: DateTime.Now.ToLocalTime());

                    LiveStreamMonitor.SetChannelsByName(channels);
                    return true;
                }
                else
                {
                    LogEntry($"There are no channels to monitor. Please add channels to the table.", DateTime.Now.ToLocalTime());
                    LiveStreamMonitor.SetChannelsByName(new()); // empty the channel list
                    return false;
                }
            }
            return false;
        }

        public void ExitSave()
        {
            DataManage.SaveData();
            StopBot();
            SaveParams();
        }
    }
}
