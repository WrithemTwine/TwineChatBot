using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System.Net.Http;
using System.Reflection;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotLiveMonitorSvc : TwitchBotsBase
    {
        // TODO: add mechanism to check the multilive listing to ensure those channels exist on Twitch. now with UserIds, convert their name & refer to their ID instead for user online checking

        public event EventHandler<MultiLiveGetChannelsEventArgs> MultiLiveGetChannels;

        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public ExtLiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        /// <summary>
        /// Notifies whether the multilive channels are part of the live stream monitored channels.
        /// </summary>
        public bool IsMultiLiveBotActive { get; set; }

        /// <summary>
        /// Notifies if the multilive channels are monitored for changes and will update the monitored channel list. 
        /// </summary>
        public bool IsMultiConnected { get; set; }
        private const string s = "";

        public TwitchBotLiveMonitorSvc()
        {
            BotClientName = Bots.TwitchLiveBot;
            IsStarted = false;
            IsStopped = true;
        }

        /// <summary>
        /// Update monitored channels when the user changes the channel list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MultiLiveDataManager_UpdatedMonitoringChannels(object sender, EventArgs e)
        {
            if (LiveStreamMonitor != null)
            {
                UpdateChannels();
            }
        }

        /// <summary>
        /// Build the live service with the client ID and access token.
        /// </summary>
        private void ConnectLiveMonitorService()
        {
            if (LiveStreamMonitor == null)
            {
                LiveStreamMonitor = new(BotsTwitch.TwitchBotUserSvc.HelixAPIBotToken, (int)Math.Round(TwitchFrequencyLiveNotifyTime, 0));

                // check if there is an unauthorized http access exception; we have an expired token
                LiveStreamMonitor.AccessTokenUnauthorized += LiveStreamMonitor_AccessTokenUnauthorized;
            }
            else
            {
                LiveStreamMonitor.UpdateToken(TwitchAccessToken);
            }
        }

        private void LiveStreamMonitor_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
            twitchTokenBot.CheckToken();
        }

        /// <summary>
        /// Adds and updates the channels to monitor for if the streamer goes live.
        /// </summary>
        /// <param name="ChannelList">The channel names to monitor - the chat bot channel will automatically add to this monitor list.</param>
        public void SetLiveMonitorChannels(List<string> ChannelIdList)
        {
            lock (s)
            {
                List<string> ChannelsToMonitor = [];

                if (IsStarted)
                {
                    ChannelsToMonitor.UniqueAdd(TwitchChannelId);
                }

                if (IsMultiConnected)
                {
                    ChannelsToMonitor.UniqueAddRange(ChannelIdList);
                }

                LiveStreamMonitor?.SetChannelsById(ChannelsToMonitor);
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
                    IsInitialStart = true;
                    IsStarted = true;
                    if (LiveStreamMonitor == null)
                    {
                        ConnectLiveMonitorService();
                    }

                    IsStopped = false;

                    SetLiveMonitorChannels([]);

                    LiveStreamMonitor.Start();
                    InvokeBotStarted();
                }
                return true;
            }
            catch (HttpRequestException hrEx)
            {
                if (hrEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                    twitchTokenBot.CheckToken();
                }
                return false;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                InvokeBotFailedStart();
                return false;
            }
        }

        /// <summary>
        /// Stop the LiveMonitor Service, including a watch on other channels.
        /// </summary>
        public override bool StopBot()
        {
            try
            {
                if (IsStarted)
                {
                    StopMultiLive();
                    LiveStreamMonitor?.Stop();
                    IsStarted = false;
                    IsStopped = true;
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

        /// <summary>
        /// Stops the bot and prepares to exit.
        /// </summary>
        /// <returns>true when the bot is stopped.</returns>
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

        /// <summary>
        /// manages the multilive monitored channels; build the client
        /// </summary>
        public EventHandler GetUpdatedChannelHandler()
        {
            IsMultiConnected = true;
            return MultiLiveDataManager_UpdatedMonitoringChannels;
        }

        /// <summary>
        /// Disconnect the multiple channel monitoring for live stream.
        /// </summary>
        public void MultiDisconnect()
        {
            StopMultiLive();
            IsMultiConnected = false;
        }

        /// <summary>
        /// Update the channels from the GUI per the user's choices, save to multilive database.
        /// </summary>
        public void UpdateChannels()
        {
            if (IsMultiLiveBotActive)
            {
                MultiLiveGetChannels?.Invoke(this, new() { Callback = SetLiveMonitorChannels });
            }
            else
            {
                SetLiveMonitorChannels([]);
            }
        }

        /// <summary>
        /// Add the additional channels to monitor for livestream status, per the user's choice
        /// </summary>
        public void StartMultiLive()
        {
            if (IsMultiConnected && !IsMultiLiveBotActive)
            {
                IsMultiLiveBotActive = true;
                UpdateChannels();
            }
        }

        /// <summary>
        /// Stop monitoring the additional channels if they went live.
        /// </summary>
        public void StopMultiLive()
        {
            if (IsMultiConnected && IsMultiLiveBotActive)
            {
                IsMultiLiveBotActive = false;
                if (OptionFlags.ActiveToken)
                {
                    UpdateChannels();
                }
            }
        }

        #endregion
    }
}
