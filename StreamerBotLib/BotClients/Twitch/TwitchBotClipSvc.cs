using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotClipSvc : TwitchBotsBase
    {
        public ClipMonitorService ClipMonitorService { get; set; }

        public TwitchBotClipSvc()
        {
            BotClientName = Bots.TwitchClipBot;
        }

        /// <summary>
        /// Builds the clip service.
        /// </summary>
        private void ConnectClipService()
        {
            if (ClipMonitorService == null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Building clip service object.");

                ClipMonitorService = new(tokenBot.StreamerHelixApi, (int)Math.Ceiling(OptionFlags.TwitchFrequencyClipTime));
                ClipMonitorService.SetChannelsById([OptionFlags.TwitchStreamerUserId]);

                ClipMonitorService.AccessTokenUnauthorized += ClipMonitorService_AccessTokenUnauthorized;
            }
        }

        private void ClipMonitorService_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Checking tokens.");
            tokenBot.CheckToken();
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override void StartBot()
        {
            try
            {
                if (IsActive == null || IsActive == false)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Starting bot.");

                    ConnectClipService();
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Starting service.");
                    ClipMonitorService.Start();
                    IsActive = true;
                    InvokeBotStarted();
                }
            }
            catch (BadRequestException)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Checking tokens.");
                tokenBot.CheckToken();
            }
            catch (Exception ex)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Caught an exception trying to start the bot.");
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                if (IsActive == false)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Found the bot didn't start, notifying GUI the bot is stopped.");

                    IsActive = false;
                    InvokeBotFailedStart();
                }
                else
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Determined bot is started, notifying GUI the bot started.");
                    IsActive = true;
                    InvokeBotStarted();
                }
            }
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override void StopBot()
        {
            try
            {
                if (IsActive == true)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Stopping bot.");

                    ClipMonitorService.Stop();
                    IsActive = false;
                    InvokeBotStopped();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                IsActive = false;
                InvokeBotStopped();
            }
        }

        public override bool ExitBot()
        {
            if (IsActive == true)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Now stopping and exiting bot.");

                StopBot();
            }
            return base.ExitBot();
        }

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Getting all clips.");

            return await ClipMonitorService.GetAllClipsAsync(OptionFlags.TwitchStreamerUserId);
        }

        public void CreateClip()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Creating a new clip.");

            _ = ClipMonitorService.CreateClip(OptionFlags.TwitchStreamerUserId);
        }
    }
}
