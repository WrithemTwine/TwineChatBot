using ChatBot_Net5.Clients;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController : INotifyPropertyChanged
    {
        public MultiUserLiveBot.Data.DataManager MultiLiveDataManager { get; private set; }

        private const int maxlength = 8000;

        public string MultiLiveStatusLog { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public void MultiConnect()
        {
            MultiLiveDataManager = new();
            TwitchLiveMonitor.IsMultiConnected = true;
            NotifyPropertyChanged(nameof(MultiLiveDataManager));
        }

        public void MultiDisconnect()
        {
            StopMultiLive();
            MultiLiveDataManager = null;
            TwitchLiveMonitor.IsMultiConnected = false;
            NotifyPropertyChanged(nameof(MultiLiveDataManager));
        }

        public void UpdateChannels()
        {
            if (TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                MultiLiveDataManager.SaveData();
                TwitchLiveMonitor.SetLiveMonitorChannels(MultiLiveDataManager.GetChannelNames());
            }
            else
            {
                TwitchLiveMonitor.SetLiveMonitorChannels(new());
            }
            LogEntry(string.Format("MultiLive Bot started and monitoring {0} channels.", TwitchLiveMonitor.LiveStreamMonitor.ChannelsToMonitor.Count.ToString()), DateTime.Now);
        }

        public void StartMultiLive()
        {
            if (TwitchLiveMonitor.IsMultiConnected && !TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                TwitchLiveMonitor.IsMultiLiveBotActive = true;
                UpdateChannels();
                LogEntry(string.Format("MultiLive Bot started and monitoring {0} channels.", TwitchLiveMonitor.LiveStreamMonitor.ChannelsToMonitor.Count.ToString()), DateTime.Now);
            }
        }

        public void StopMultiLive()
        {
            if (TwitchLiveMonitor.IsMultiConnected && TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                TwitchLiveMonitor.IsMultiLiveBotActive = false;
                UpdateChannels();
                LogEntry("MultiLive Bot stopped.", DateTime.Now);
            }
        }

        internal void SendMultiLiveMsg(OnStreamOnlineArgs e)
        {
            if (TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                // true posted new event, false did not post
                bool PostedLive = MultiLiveDataManager.PostStreamDate(e.Stream.UserName, e.Stream.StartedAt.ToLocalTime());

                if (PostedLive)
                {

                    bool MultiLive = MultiLiveDataManager.CheckStreamDate(e.Channel, e.Stream.StartedAt.ToLocalTime());

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = OptionFlags.LiveMsg ?? "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = new()
                        {
                            { "#user", e.Stream.UserName },
                            { "#category", e.Stream.GameName },
                            { "#title", e.Stream.Title },
                            { "#url", "https://www.twitch.tv/" + e.Stream.UserName }
                        };

                        LogEntry(ParseReplace(msg, dictionary), e.Stream.StartedAt.ToLocalTime());
                        foreach (Tuple<string, Uri> u in MultiLiveDataManager.GetWebLinks())
                        {
                            if (u.Item1 == "Discord")
                            {
                                DiscordWebhook.SendLiveMessage(u.Item2, ParseReplace(msg, dictionary)).Wait();
                                Stats.AddDiscord();
                            }
                        }
                    }
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="data">The string of the message.</param>
        /// <param name="dateTime">The time of the event.</param>
        public void LogEntry(string data, DateTime dateTime)
        {
            if (MultiLiveStatusLog.Length + dateTime.ToLocalTime().ToString().Length + data.Length + 2 >= maxlength)
            {
                MultiLiveStatusLog = MultiLiveStatusLog[MultiLiveStatusLog.IndexOf('\n')..];
            }

            MultiLiveStatusLog += dateTime.ToLocalTime().ToString() + " " + data + "\n";

            NotifyPropertyChanged(nameof(MultiLiveStatusLog));
        }
    }
}
