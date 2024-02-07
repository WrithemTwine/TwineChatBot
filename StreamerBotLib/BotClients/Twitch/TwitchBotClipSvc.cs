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
                ClipMonitorService = new(BotsTwitch.TwitchBotUserSvc.HelixAPIBotToken, (int)Math.Ceiling(TwitchFrequencyClipTime));
                ClipMonitorService.SetChannelsById([TwitchChannelId]);

                ClipMonitorService.AccessTokenUnauthorized += ClipMonitorService_AccessTokenUnauthorized;
            }
        }

        private void ClipMonitorService_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
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
                    ConnectClipService();
                    IsStarted = true;
                    ClipMonitorService?.Start();
                    IsStopped = false;
                    InvokeBotStarted();
                }
                return true;
            }
            catch (BadRequestException)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
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
                    ClipMonitorService?.Stop();
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
                StopBot();
            }
            return base.ExitBot();
        }

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            return await ClipMonitorService.GetAllClipsAsync(TwitchChannelId);
        }

        public void CreateClip()
        {
            _ = ClipMonitorService.CreateClip(TwitchChannelId);
        }
    }
}
