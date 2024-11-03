using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System.Reflection;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotLiveMonitorSvc : TwitchBotsBase
    {
        // TODO: consider adding mechanism to check the multilive listing to ensure those channels exist on Twitch. now with UserIds, convert their name & refer to their ID instead for user online checking

        private static IDataManagerReadOnly DataManageReadOnly;

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

        public TwitchBotLiveMonitorSvc(IDataManagerReadOnly datamanager)
        {
            BotClientName = Bots.TwitchLiveBot;
            IsStarted = false;
            IsStopped = true;

            DataManageReadOnly = datamanager;
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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Creating new Livestream instance.");
                LiveStreamMonitor = new(BotsTwitch.TwitchBotUserSvc.HelixAPIBotToken, (int)Math.Round(TwitchFrequencyLiveNotifyTime, 0), instanceDate: DateTime.Now.ToLocalTime());

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, $"Using {(int)Math.Round(TwitchFrequencyLiveNotifyTime, 0)} seconds live-check frequency.");

                // check if there is an unauthorized http access exception; we have an expired token
                LiveStreamMonitor.AccessTokenUnauthorized += LiveStreamMonitor_AccessTokenUnauthorized;
            }
            //else
            //{
            //    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Found livestream instance, updating the token.");

            //    LiveStreamMonitor.UpdateToken(TwitchAccessToken);
            //}
        }

        private void LiveStreamMonitor_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Livestream - checking tokens.");
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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Starting bot.");

                if (IsStopped || !IsStarted)
                {
                    IsInitialStart = true;
                    IsStarted = true;
                    if (LiveStreamMonitor == null)
                    {
                        ConnectLiveMonitorService();
                        IsStopped = false;
                        SetLiveMonitorChannels([]);
                    }
                    LiveStreamMonitor.Start();
                    InvokeBotStarted();
                }
                return true;
            }
            catch (global::TwitchLib.Api.Core.Exceptions.BadScopeException)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Livestream bot starting - checking tokens.");
                twitchTokenBot.CheckToken();
                LiveStreamMonitor.Start();
                InvokeBotStarted();
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
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Stopping bot.");

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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchLiveBot, "Stopping and exiting bot.");

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
                SetLiveMonitorChannels(DataManageReadOnly.GetMultiChannelNames(Platform.Twitch));
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
