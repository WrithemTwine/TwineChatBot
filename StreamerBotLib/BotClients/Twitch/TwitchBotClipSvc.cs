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
        private static TwitchTokenBot twitchTokenBot;

        public ClipMonitorService ClipMonitorService { get; set; }

        public TwitchBotClipSvc()
        {
            BotClientName = Bots.TwitchClipBot;
        }

        /// <summary>
        /// Sets the Twitch Token bot used for the automatic refreshing access token.
        /// </summary>
        /// <param name="tokenBot">An instance of the token bot, to use the same token bot across chat bots.</param>
        internal override void SetTokenBot(TwitchTokenBot tokenBot)
        {
            twitchTokenBot = tokenBot;
        }

        /// <summary>
        /// Rebuilds the clip service.
        /// </summary>
        /// <param name="ClientName">Channel to monitor.</param>
        /// <param name="TwitchToken">Access token, if applicable</param>
        private void ConnectClipService(string ClientName = null, string TwitchToken = null)
        {
            if (ClipMonitorService == null)
            {
                ClipMonitorService = new(BotsTwitch.TwitchBotUserSvc.HelixAPIBotToken, (int)Math.Ceiling(TwitchFrequencyClipTime));
                ClipMonitorService.SetChannelsByName([ClientName ?? TwitchChannelName]);

                ClipMonitorService.AccessTokenUnauthorized += ClipMonitorService_AccessTokenUnauthorized;
            }
            else
            {
                ClipMonitorService.UpdateAccessToken(TwitchAccessToken);
            }
        }

        private void ClipMonitorService_AccessTokenUnauthorized(object sender, EventArgs e)
        {
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
                twitchTokenBot.CheckToken();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                IsStarted = false;
                IsStopped = true;
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
                    ClipMonitorService = null;
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

        public async Task<List<Clip>> GetAllClipsAsync(string ChannelName = null)
        {
            return await ClipMonitorService.GetAllClipsAsync(ChannelName ?? TwitchChannelName);
        }

        public void CreateClip()
        {
            _ = ClipMonitorService.CreateClip(TwitchChannelId);
        }
    }
}
