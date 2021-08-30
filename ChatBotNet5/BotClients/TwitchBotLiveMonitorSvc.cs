
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services;

namespace ChatBot_Net5.BotClients
{
    public class TwitchBotLiveMonitorSvc : TwitchBots
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        public bool IsMultiLiveBotActive { get; set; }
        public bool IsMultiConnected { get; set; }
        public TwitchBotLiveMonitorSvc()
        {
            BotClientName = Enum.Bots.TwitchLiveBot;
            IsStarted = false;
            IsStopped = true;
        }

        public void ConnectLiveMonitorService()
        {
            if (IsStarted)
            {
                LiveStreamMonitor?.Stop();
            }
            LiveStreamMonitor = null;
            RefreshSettings();
            ApiSettings apilive = new() { AccessToken = TwitchAccessToken, ClientId = TwitchClientID };
            LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(TwitchFrequencyLiveNotifyTime, 0));
            SetLiveMonitorChannels(new());
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
                if (!ChannelList.Contains(TwitchChannelName)) { ChannelList.Add(TwitchChannelName); }

                LiveStreamMonitor.SetChannelsByName(ChannelList);
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
                    ConnectLiveMonitorService();

                    if (LiveStreamMonitor.ChannelsToMonitor.Count < 1)
                    {
                        SetLiveMonitorChannels(new() { TwitchChannelName });
                    }

                    LiveStreamMonitor.Start();
                    IsStarted = true;
                    IsStopped = false;
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
                LiveStreamMonitor?.Stop();
            }
            LiveStreamMonitor = null;
            return base.ExitBot();
        }
    }
}
