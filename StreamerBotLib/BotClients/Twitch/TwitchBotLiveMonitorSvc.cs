using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System.Net.Http;
using System.Reflection;

using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotLiveMonitorSvc : TwitchBotsBase
    {
        // TODO: add mechanism to check the multilive listing to ensure those channels exist on Twitch. now with UserIds, convert their name & refer to their ID instead for user online checking

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

        /// <summary>
        /// Database connection to the other channels the streamer is monitoring to determine if the user went live.
        /// </summary>
        public IDataManager MultiLiveDataManager { get; private set; }

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
                LiveStreamMonitor = new(BotsTwitch.TwitchBotUserSvc.HelixAPIStreamerToken, (int)Math.Round(TwitchFrequencyLiveNotifyTime, 0));

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
        public void SetLiveMonitorChannels(List<string> ChannelList)
        {
            string s = "";
            lock (s)
            {
                List<string> ChannelsToMonitor = [];

                if (IsStarted)
                {
                    ChannelsToMonitor.UniqueAdd(TwitchChannelName);
                }

                if (IsMultiConnected)
                {
                    ChannelsToMonitor.UniqueAddRange(ChannelList);
                }

                LiveStreamMonitor?.SetChannelsByName(ChannelsToMonitor);
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
        public void MultiConnect(IDataManager datamanage)
        {
            if (MultiLiveDataManager == default)
            {
                MultiLiveDataManager = datamanage;
                MultiLiveDataManager.UpdatedMonitoringChannels += MultiLiveDataManager_UpdatedMonitoringChannels;
            }

            IsMultiConnected = true;
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
                SetLiveMonitorChannels(MultiLiveDataManager.GetMultiChannelNames());
                // TODO: localize the multilive bot data
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

        /// <summary>
        /// Send notification messages based on stream went live.
        /// </summary>
        /// <param name="e"></param>
        public void SendMultiLiveMsg(OnStreamOnlineArgs e)
        {
            if (IsMultiLiveBotActive)
            {
                DateTime CurrTime = e.Stream.StartedAt.ToLocalTime();

                // true posted new event, false did not post
                bool PostedLive = MultiLiveDataManager.PostMultiStreamDate(e.Stream.UserId, e.Stream.UserName, Platform.Twitch, CurrTime);

                if (PostedLive)
                {
                    bool MultiLive = MultiLiveDataManager.CheckMultiStreamDate(e.Channel, Platform.Twitch, CurrTime);

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = OptionFlags.MsgLive ?? "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = new()
                        {
                            { "#user", e.Stream.UserName },
                            { "#category", e.Stream.GameName },
                            { "#title", e.Stream.Title },
                            { "#url", e.Stream.UserName }
                        };

                        foreach (Tuple<WebhooksSource, Uri> u in MultiLiveDataManager.GetMultiWebHooks())
                        {
                            if (u.Item1 == WebhooksSource.Discord)
                            {
                                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(msg, dictionary), VariableParser.BuildPlatformUrl(e.Stream.UserName, Platform.Twitch));
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
