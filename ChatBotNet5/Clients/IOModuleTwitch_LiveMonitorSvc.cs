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
        public IOModuleTwitch_LiveMonitorSvc()
        {
            ChatClientName = "TwitchLiveMonitorService";
        }

        internal void ConnectLiveMonitorService()
        {
            RefreshSettings();
            ApiSettings apilive = new() { AccessToken = TwitchAccessToken, ClientId = TwitchClientID };
            LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(TwitchFrequencyLiveNotifyTime, 0));
        }

        /// <summary>
        /// Adds and updates the channels to monitor for if the streamer goes live.
        /// </summary>
        /// <param name="ChannelList">The channel names to monitor - the chat bot channel will automatically add to this monitor list.</param>
        internal void SetLiveMonitorChannels(List<string> ChannelList)
        {
            if (!ChannelList.Contains(TwitchChannelName)) { ChannelList.Add(TwitchChannelName); }

            LiveStreamMonitor.SetChannelsByName(ChannelList);
        }

        /// <summary>
        /// Start the LiveMonitor Service
        /// </summary>
        public override bool StartBot()
        {
            ConnectLiveMonitorService();
            LiveStreamMonitor.Start();
            IsStarted = true;
            InvokeBotStarted();

            return true;
        }

        /// <summary>
        /// Stop the LiveMonitor Service
        /// </summary>
        public override bool StopBot()
        {
            LiveStreamMonitor?.Stop();
            IsStarted = false;
            InvokeBotStopped();
            return true;
        }


    }
}
