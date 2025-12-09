using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Clips.CreateClip;
using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotClipSvc : TwitchBotsBase
    {
        private readonly TwitchTokenBot tokenBot;
        public ClipMonitorService ClipMonitorService { get; set; }

        private Action _rePerformAction; // used to hold an action to re-perform after a token refresh

        internal TwitchBotClipSvc(TwitchTokenBot TokenBot)
        {
            BotClientName = Bots.TwitchClipBot;
            tokenBot = TokenBot;

            tokenBot.StreamerAccessTokenChanged += TokenBot_StreamerAccessTokenChanged;
        }

        private void TokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            if (IsActive == true && !ClipMonitorService.Enabled)
            {
                ClipMonitorService.Start();
            }

            _rePerformAction?.Invoke();
            _rePerformAction = null;
        }

        /// <summary>
        /// Builds the clip service.
        /// </summary>
        private void ConnectClipService()
        {
            if (ClipMonitorService == null)
            {
                LogWriter.DebugLog("ConnectClipService", DebugLogTypes.TwitchClipBot, "Building clip service object.");

                ClipMonitorService = new(tokenBot.StreamerHelixApi, (int)Math.Ceiling(OptionFlags.TwitchFrequencyClipTime));
                ClipMonitorService.SetChannelsById([OptionFlags.TwitchStreamerUserId]);

                ClipMonitorService.AccessTokenUnauthorized += ClipMonitorService_AccessTokenUnauthorized;
            }
        }

        private void ClipMonitorService_AccessTokenUnauthorized(object sender, ExpiredTokenEventArgs e)
        {
            LogWriter.DebugLog("ClipMonitorService_AccessTokenUnauthorized", DebugLogTypes.TwitchClipBot, "Checking tokens.");
            _rePerformAction = e.RePerformAction;
            tokenBot.CheckToken();
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override Task StartBot()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (IsActive == null || IsActive == false)
                    {
                        tokenBot.UpdateActiveTokens(BotType.StreamerAccount, true);
                        tokenBot.CheckToken();

                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Starting bot.");

                        ConnectClipService();
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Starting service.");
                        ClipMonitorService.Start();
                        IsActive = true;
                        InvokeBotStarted();
                    }
                }
                catch (BadRequestException)
                {
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Checking tokens.");
                    tokenBot.CheckToken();
                }
                catch (Exception ex)
                {
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Caught an exception trying to start the bot.");
                    LogWriter.LogException(ex, "StartBot");
                    if (IsActive == false)
                    {
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Found the bot didn't start, notifying GUI the bot is stopped.");

                        IsActive = false;
                        InvokeBotFailedStart();
                    }
                    else
                    {
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchClipBot, "Determined bot is started, notifying GUI the bot started.");
                        IsActive = true;
                        InvokeBotStarted();
                    }
                }
            });
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override Task StopBot()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (IsActive == true)
                    {
                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchClipBot, "Stopping bot.");

                        ClipMonitorService.Stop();
                        IsActive = false;
                        InvokeBotStopped();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "StopBot");
                    IsActive = false;
                    InvokeBotStopped();
                }
            });
        }

        public override Task<bool> ExitBot()
        {
            return Task.Run(() =>
            {
                if (IsActive == true)
                {
                    LogWriter.DebugLog("ExitBot", DebugLogTypes.TwitchClipBot, "Now stopping and exiting bot.");

                    StopBot();
                }
                return true;
            });
        }

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            LogWriter.DebugLog("GetAllClipsAsync", DebugLogTypes.TwitchClipBot, "Getting all clips.");

            return await ClipMonitorService.GetAllClipsAsync(OptionFlags.TwitchStreamerUserId);
        }

        public async Task CreateClipAsync()
        {
            if (IsActive == true)
            {
                LogWriter.DebugLog("CreateClip", DebugLogTypes.TwitchClipBot, "Creating a new clip.");

                // if create clip fails due to token, the token event will re-call this method
                ClipMonitorService?.CreateClip(OptionFlags.TwitchStreamerUserId);
            }
        }
    }
}
