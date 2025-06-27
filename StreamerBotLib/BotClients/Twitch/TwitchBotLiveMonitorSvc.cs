using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Exceptions;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotLiveMonitorSvc : TwitchBotsBase
    {
        private readonly TwitchTokenBot tokenBot;

        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        public ExtLiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        private Func<Platform, IEnumerable<string>> GetMultiChannelIds;

        internal TwitchBotLiveMonitorSvc(TwitchTokenBot TokenBot)
        {
            BotClientName = Bots.TwitchMultiBot;
            tokenBot = TokenBot;
        }

        public EventHandler SetMultiChannelIds(Func<Platform, IEnumerable<string>> GetIds)
        {
            GetMultiChannelIds = GetIds;
            return DataManageReadOnly_UpdatedMonitoringChannels;
        }

        internal void DataManageReadOnly_UpdatedMonitoringChannels(object sender, EventArgs e)
        {
            SetLiveMonitorChannels();
        }

        /// <summary>
        /// Build the LiveStreamMonitor service, add available multichannel user ids, and start the service.
        /// </summary>
        public override Task StartBot()
        {
            return Task.Run(() =>
            {
                try
                {
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchBotSendChat, "Starting bot.");

                    if (IsActive is null or false)
                    {
                        tokenBot.UpdateActiveTokens(BotType.StreamerAccount, true);
                        tokenBot.CheckToken();

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
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchTokenBot, "Livestream bot starting - checking tokens.");
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
                    LogWriter.LogException(ex, "StartBot");
                    InvokeBotFailedStart();
                }
            });
        }

        /// <summary>
        /// Build the live service with the client ID and access token.
        /// </summary>
        private void ConnectLiveMonitorService()
        {
            if (LiveStreamMonitor == null)
            {
                LogWriter.DebugLog("ConnectLiveMonitorService", DebugLogTypes.TwitchBotSendChat, "Creating new Livestream instance.");
                LiveStreamMonitor = new(tokenBot.StreamerHelixApi, (int)Math.Round(OptionFlags.TwitchGoLiveFrequency, 0));

                LogWriter.DebugLog("ConnectLiveMonitorService", DebugLogTypes.TwitchBotSendChat, $"Using {(int)Math.Round(OptionFlags.TwitchGoLiveFrequency, 0)} seconds live-check frequency.");

                // check if there is an unauthorized http access exception; we have an expired token
                LiveStreamMonitor.AccessTokenUnauthorized += LiveStreamMonitor_AccessTokenUnauthorized;
            }
        }

        private void LiveStreamMonitor_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog("LiveStreamMonitor_AccessTokenUnauthorized", DebugLogTypes.TwitchTokenBot, "Livestream - checking tokens.");
            tokenBot.CheckToken();
        }

        private bool SetLiveMonitorChannels()
        {
            LiveStreamMonitor.ChannelsToMonitor?.Clear(); // remove any existing to avoid duplication

            List<string> ReviewIds = [.. GetMultiChannelIds(Platform.Twitch)];
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
        public override Task StopBot()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (IsActive == true)
                    {
                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchMultiLiveBot, "Stopping bot.");

                        LiveStreamMonitor?.Stop();
                        IsActive = false;
                        InvokeBotStopped();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "StopBot");
                }
            });
        }

        /// <summary>
        /// Stops the bot and prepares to exit.
        /// </summary>
        /// <returns>true when the bot is stopped.</returns>
        public override Task<bool> ExitBot()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("ExitBot", DebugLogTypes.TwitchMultiLiveBot, "Stopping and exiting bot.");
                await StopBot();
                LiveStreamMonitor = null;
                return true;
            });
        }
    }
}
