
using System;
using System.Collections.Generic;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services;

namespace ChatBot_Net5.Clients
{
    public class IOModuleTwitch_LiveMonitorSvc : IOModule
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        internal LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        public bool IsMultiLiveBotActive { get; set; }
        public bool IsMultiConnected { get; set; }
        public IOModuleTwitch_LiveMonitorSvc()
        {
            ChatClientName = "TwitchLiveMonitorService";
            IsStarted = false;
            IsStopped = true;
        }

        internal void ConnectLiveMonitorService()
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
        internal void SetLiveMonitorChannels(List<string> ChannelList)
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
            if (!IsStarted || !IsStopped)
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

        /// <summary>
        /// Stop the LiveMonitor Service
        /// </summary>
        public override bool StopBot()
        {
            if (!IsStopped && IsStarted)
            {
                LiveStreamMonitor.Stop();
                IsStarted = false;
                IsStopped = true;
                HandlersAdded = false;
                InvokeBotStopped();
            }
            return true;
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
