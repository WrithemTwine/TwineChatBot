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

                ClipMonitorService = new(BotsTwitch.TwitchBotUserSvc.HelixAPIBotToken, (int)Math.Ceiling(TwitchFrequencyClipTime));
                ClipMonitorService.SetChannelsById([TwitchChannelId]);

                ClipMonitorService.AccessTokenUnauthorized += ClipMonitorService_AccessTokenUnauthorized;
            }
        }

        private void ClipMonitorService_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Checking tokens.");
            twitchTokenBot.CheckToken();
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override bool StartBot()
        {
            try
            {
                if (IsStopped || !IsStarted)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Starting bot.");

                    ConnectClipService();
                    ClipMonitorService.Start();
                    IsStarted = true;
                    IsStopped = false;
                    InvokeBotStarted();
                }
                return true;
            }
            catch (BadRequestException)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Checking tokens.");
                twitchTokenBot.CheckToken();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                IsStarted = false;
                IsStopped = true;
                InvokeBotFailedStart();
            }
            return false;
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override bool StopBot()
        {
            try
            {
                if (IsStarted)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Stopping bot.");

                    ClipMonitorService.Stop();
                    IsStarted = false;
                    IsStopped = true;
                    InvokeBotStopped();
                    //ClipMonitorService = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
            return false;
        }

        public override bool ExitBot()
        {
            if (IsStarted)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Now stopping and exiting bot.");

                StopBot();
            }
            return base.ExitBot();
        }

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Getting all clips.");

            return await ClipMonitorService.GetAllClipsAsync(TwitchChannelId);
        }

        public void CreateClip()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchClipBot, "Creating a new clip.");

            _ = ClipMonitorService.CreateClip(TwitchChannelId);
        }
    }
}
