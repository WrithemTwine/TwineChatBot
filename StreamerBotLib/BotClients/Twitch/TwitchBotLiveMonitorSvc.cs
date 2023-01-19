using StreamerBotLib.Data.MultiLive;
using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

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
        public MultiDataManager MultiLiveDataManager { get; private set; }

        public TwitchBotLiveMonitorSvc()
        {
            BotClientName = Bots.TwitchLiveBot;
            IsStarted = false;
            IsStopped = true;

        }

        private void MultiLiveDataManager_UpdatedMonitoringChannels(object sender, EventArgs e)
        {
            if (LiveStreamMonitor != null)
            {
                UpdateChannels();
            }
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
                    ChannelsToMonitor.UniqueAdd(TwitchChannelName);
                }

                if (IsMultiConnected)
                {
                    ChannelsToMonitor.UniqueAddRange(ChannelList);
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
                    StopMultiLive();
                    LiveStreamMonitor?.Stop();
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

        public void MultiConnect()
        {
            if (MultiLiveDataManager == null)
            {
                MultiLiveDataManager = new();
                MultiLiveDataManager.UpdatedMonitoringChannels += MultiLiveDataManager_UpdatedMonitoringChannels;
            }

            MultiLiveDataManager.LoadData();
            IsMultiConnected = true;
        }

        public void MultiDisconnect()
        {
            StopMultiLive();
            IsMultiConnected = false;
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
            MultiLiveDataManager.LogEntry(string.Format(CultureInfo.CurrentCulture, "MultiLive Bot started and monitoring {0} channels.", LiveStreamMonitor.ChannelsToMonitor.Count.ToString(CultureInfo.CurrentCulture)), DateTime.Now.ToLocalTime());
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
                if (OptionFlags.ActiveToken)
                {
                    UpdateChannels();
                }
                MultiLiveDataManager.LogEntry("MultiLive Bot stopped.", DateTime.Now.ToLocalTime());
            }
        }

        public void SendMultiLiveMsg(OnStreamOnlineArgs e)
        {
            if (IsMultiLiveBotActive)
            {
                DateTime CurrTime = e.Stream.StartedAt.ToLocalTime();

                // true posted new event, false did not post
                bool PostedLive = MultiLiveDataManager.PostStreamDate(e.Stream.UserName, CurrTime);

                if (PostedLive)
                {
                    bool MultiLive = MultiLiveDataManager.CheckStreamDate(e.Channel, CurrTime);

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
                            { "#url", e.Stream.UserName }
                        };

                        MultiLiveDataManager.LogEntry(VariableParser.ParseReplace(msg, dictionary), CurrTime);
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

        #endregion
    }
}
