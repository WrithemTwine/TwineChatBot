using StreamerBotLib.Enums;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotLiveMonitorSvc : TwitchBotsBase
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        public bool IsMultiLiveBotActive { get; set; }
        public bool IsMultiConnected { get; set; }
        public TwitchBotLiveMonitorSvc()
        {
            BotClientName = Bots.TwitchLiveBot;
            IsStarted = false;
            IsStopped = true;
        }

        public void ConnectLiveMonitorService()
        {
            if (IsStarted)
            {
                LiveStreamMonitor?.Stop();
            }
            RefreshSettings();
            ApiSettings apilive = new() { AccessToken = TwitchAccessToken, ClientId = TwitchClientID };
            LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(TwitchFrequencyLiveNotifyTime, 0));
        }

        /// <summary>
        /// Adds and updates the channels to monitor for if the streamer goes live.
        /// </summary>
        /// <param name="ChannelList">The channel names to monitor - the chat bot channel will automatically add to this monitor list.</param>
        public void SetLiveMonitorChannels(List<string> ChannelList)
        {
            string s = "";
            lock (s)
            {
                List<string> ChannelsToMonitor = new();

                if (IsStarted)
                {
                    ChannelsToMonitor.Add(TwitchChannelName);
                }

                if (IsMultiConnected)
                {
                    ChannelsToMonitor.AddRange(ChannelList);
                }

                LiveStreamMonitor.SetChannelsByName(ChannelsToMonitor);
            }
        }

        /// <summary>
        /// Start the LiveMonitor Service
        /// </summary>
        public override bool StartBot()
        {
            try
            {
                if (IsStopped || !IsStarted)
                {
                    if (LiveStreamMonitor == null)
                    {
                        ConnectLiveMonitorService();
                    }

                    IsStarted = true;
                    IsStopped = false;

                    SetLiveMonitorChannels(new());

                    LiveStreamMonitor.Start();
                    InvokeBotStarted();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        /// <summary>
        /// Stop the LiveMonitor Service
        /// </summary>
        public override bool StopBot()
        {
            try
            {
                if (IsStarted)
                {
                    LiveStreamMonitor.Stop();
                    LiveStreamMonitor = null;
                    IsStarted = false;
                    IsStopped = true;
                    HandlersAdded = false;
                    InvokeBotStopped();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        public override bool ExitBot()
        {
            if (IsStarted)
            {
                StopBot();
            }
            LiveStreamMonitor = null;
            return base.ExitBot();
        }

        #region MultiLive Bot

        public static MultiUserLiveBot.Data.DataManager MultiLiveDataManager { get; private set; } = new();

        private const int maxlength = 8000;

        public static string MultiLiveStatusLog { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public void MultiConnect()
        {
            MultiLiveDataManager.LoadData();
            IsMultiConnected = true;
        }

        public void MultiDisconnect()
        {
            StopMultiLive();
            IsMultiConnected = false;
            NotifyPropertyChanged(nameof(MultiLiveDataManager));
        }

        public void UpdateChannels()
        {
            if (IsMultiLiveBotActive)
            {
                MultiLiveDataManager.SaveData();
                SetLiveMonitorChannels(MultiLiveDataManager.GetChannelNames());
            }
            else
            {
                SetLiveMonitorChannels(new());
            }
            // TODO: localize the multilive bot data
            LogEntry(string.Format(CultureInfo.CurrentCulture, "MultiLive Bot started and monitoring {0} channels.", LiveStreamMonitor.ChannelsToMonitor.Count.ToString(CultureInfo.CurrentCulture)), DateTime.Now.ToLocalTime());
        }

        public void StartMultiLive()
        {
            if (IsMultiConnected && !IsMultiLiveBotActive)
            {
                IsMultiLiveBotActive = true;
                UpdateChannels();
            }
        }

        public void StopMultiLive()
        {
            if (IsMultiConnected && IsMultiLiveBotActive)
            {
                IsMultiLiveBotActive = false;
                UpdateChannels();
                LogEntry("MultiLive Bot stopped.", DateTime.Now.ToLocalTime());
            }
        }

        public void SendMultiLiveMsg(OnStreamOnlineArgs e)
        {
            if (IsMultiLiveBotActive)
            {
                // true posted new event, false did not post
                bool PostedLive = MultiLiveDataManager.PostStreamDate(e.Stream.UserName, e.Stream.StartedAt);

                if (PostedLive)
                {
                    bool MultiLive = MultiLiveDataManager.CheckStreamDate(e.Channel, e.Stream.StartedAt);

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
                            { "#url", Resources.TwitchHomepage + e.Stream.UserName }
                        };

                        LogEntry(VariableParser.ParseReplace(msg, dictionary), e.Stream.StartedAt);
                        foreach (Tuple<string, Uri> u in MultiLiveDataManager.GetWebLinks())
                        {
                            if (u.Item1 == "Discord")
                            {
                                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(msg, dictionary));
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
            if (MultiLiveStatusLog.Length + dateTime.ToString().Length + data.Length + 2 >= maxlength)
            {
                MultiLiveStatusLog = MultiLiveStatusLog[MultiLiveStatusLog.IndexOf('\n')..];
            }

            MultiLiveStatusLog += dateTime.ToString() + " " + data + "\n";

            NotifyPropertyChanged(nameof(MultiLiveStatusLog));
        }

        #endregion
    }
}
