using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Reflection;

using TwitchLib.Api.Core.Exceptions;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotLiveMonitorSvc : TwitchBotsBase
    {
        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public ExtLiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        /// <summary>
        /// To get MultiLive channels for monitoring live stream
        /// </summary>
        private static IDataManagerReadOnly DataManageReadOnly = SystemsController.DataManage;

        public TwitchBotLiveMonitorSvc()
        {
            BotClientName = Bots.TwitchMultiBot;

            DataManageReadOnly.UpdatedMonitoringChannels += DataManageReadOnly_UpdatedMonitoringChannels;
        }

        private void DataManageReadOnly_UpdatedMonitoringChannels(object sender, EventArgs e)
        {
            SetLiveMonitorChannels();
        }

        /// <summary>
        /// Build the LiveStreamMonitor service, add available multichannel user ids, and start the service.
        /// </summary>
        public override void StartBot()
        {
            try
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotSendChat, "Starting bot.");

                if (IsActive is null or false)
                {
                    ConnectLiveMonitorService();
                    if (SetLiveMonitorChannels())
                    {
                        LiveStreamMonitor.Start();
                        IsActive = true;
                        InvokeBotStarted();
                    }
                    else
                    {
                        InvokeBotFailedStart();
                    }
                }
            }
            catch (TokenExpiredException)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Livestream bot starting - checking tokens.");
                tokenBot.CheckToken();
                if (SetLiveMonitorChannels())
                {
                    LiveStreamMonitor.Start();
                    IsActive = true;
                    InvokeBotStarted();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                InvokeBotFailedStart();
            }
        }

        /// <summary>
        /// Build the live service with the client ID and access token.
        /// </summary>
        private void ConnectLiveMonitorService()
        {
            if (LiveStreamMonitor == null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotSendChat, "Creating new Livestream instance.");
                LiveStreamMonitor = new(tokenBot.StreamerHelixApi, (int)Math.Round(OptionFlags.TwitchGoLiveFrequency, 0));

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotSendChat, $"Using {(int)Math.Round(OptionFlags.TwitchGoLiveFrequency, 0)} seconds live-check frequency.");

                // check if there is an unauthorized http access exception; we have an expired token
                LiveStreamMonitor.AccessTokenUnauthorized += LiveStreamMonitor_AccessTokenUnauthorized;
            }
        }

        private void LiveStreamMonitor_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Livestream - checking tokens.");
            tokenBot.CheckToken();
        }

        private bool SetLiveMonitorChannels()
        {
            LiveStreamMonitor.ChannelsToMonitor?.Clear(); // remove any existing to avoid duplication

            List<string> ReviewIds = DataManageReadOnly.GetMultiChannelIds(Platform.Twitch);
            if (ReviewIds != null && ReviewIds.Count > 0)
            {
                LiveStreamMonitor.SetChannelsById(ReviewIds);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stop the LiveMonitor Service, including a watch on other channels.
        /// </summary>
        public override void StopBot()
        {
            try
            {
                if (IsActive == true)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchMultiLiveBot, "Stopping bot.");

                    LiveStreamMonitor?.Stop();
                    IsActive = false;
                    InvokeBotStopped();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Stops the bot and prepares to exit.
        /// </summary>
        /// <returns>true when the bot is stopped.</returns>
        public override bool ExitBot()
        {
            if (IsActive == true)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchMultiLiveBot, "Stopping and exiting bot.");

                StopBot();
            }
            LiveStreamMonitor = null;
            return base.ExitBot();
        }
    }
}
