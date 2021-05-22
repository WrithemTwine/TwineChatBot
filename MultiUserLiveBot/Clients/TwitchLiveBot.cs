using MultiUserLiveBot.Clients.TwitchLib;
using MultiUserLiveBot.Data;
using MultiUserLiveBot.Properties;

using System;
using System.Collections.Generic;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace MultiUserLiveBot.Clients
{
    public partial class TwitchLiveBot : IOModule
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public static ExtLiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        /// <summary>
        /// The backend database object for the bot to store data.
        /// </summary>
        public DataManager DataManage { get; set; } = new();

        /// <summary>
        /// Instantiate new bot object and retrieve the settings values into the bot properties.
        /// </summary>
        public TwitchLiveBot()
        {
            ChatClientName = "Twitch";
            RefreshSettings();
        }

        /// <summary>
        /// Event to handle when the bot finds a stream now online.
        /// </summary>
        /// <param name="sender">Object invoking the event.</param>
        /// <param name="e">The params for the event, stream details.</param>
        private void LiveStreamMonitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            // true posted new event, false did not post
            bool PostedLive = DataManage.PostStreamDate(e.Stream.UserName, e.Stream.StartedAt.ToLocalTime());

            if (PostedLive)
            {
                string msg = Settings.Default.LiveMsg != "" ? Settings.Default.LiveMsg : "#user is now live streaming #category - #title! Come join and say hi at: #url";

                Dictionary<string, string> dictionary = new()
                {
                    { "#user", e.Stream.UserName },
                    { "#category", e.Stream.GameName },
                    { "#title", e.Stream.Title },
                    { "#url", "https://www.twitch.tv/" + e.Stream.UserName }
                };

                // false if the date didn't match, true if an event matches
                bool MultiLive = DataManage.CheckStreamDate(e.Stream.UserName, e.Stream.StartedAt.ToLocalTime());

                if ((Settings.Default.PostMultiLive && MultiLive) || !MultiLive)
                {
                    LogEntry(GoLiveWindow.ParseReplace(msg, dictionary), e.Stream.StartedAt);
                    foreach (Tuple<string, Uri> u in DataManage.GetWebLinks())
                    {
#if !DEBUG
                        if (u.Item1 == "Discord")
                        {
                            DiscordWebhook.SendLiveMessage(u.Item2, GoLiveWindow.ParseReplace(msg, dictionary)).Wait();
                        }
#endif
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
            LiveStreamMonitor = new ExtLiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(FrequencyLiveNotifyTime, 0));
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
            SaveParams();
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
        /// <param name="ChannelList">The list of strings containing the names of the channels to monitor.</param>
        /// <returns>true: for the bot beings started.</returns>
        public bool StartBot(List<string> ChannelList)
        {
            Connect(ChannelList);
            LiveStreamMonitor?.Start();
            LogEntry(string.Format("Bot started and monitoring {0} channels." , ChannelList.Count.ToString()) , DateTime.Now);
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
                LogEntry("Bot stopped.", DateTime.Now);
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
                    LogEntry(string.Format("Channels updated! Was monitoring {0} channels, now monitoring {1} channels.", LiveStreamMonitor.ChannelsToMonitor.Count.ToString(), channels.Count.ToString()), DateTime.Now);

                    LiveStreamMonitor.SetChannelsByName(channels);
                    return true;
                }
                else
                {
                    LogEntry($"There are no channels to monitor. Please add channels to the table.", DateTime.Now);
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
