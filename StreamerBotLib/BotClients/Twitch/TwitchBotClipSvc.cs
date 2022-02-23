using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Static;

using StreamerBotLib.Enums;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
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

        public void ConnectClipService(string ClientName = null, string TwitchToken = null)
        {
            if (IsStarted)
            {
                ClipMonitorService.Stop();
            }

            RefreshSettings();
            ApiSettings apiclip = new() { AccessToken = TwitchToken ?? TwitchAccessToken, ClientId = ClientName ?? TwitchClientID };
            ClipMonitorService = new(new TwitchAPI(null, null, apiclip, null), (int)Math.Round(TwitchFrequencyClipTime, 0));
            ClipMonitorService.SetChannelsByName(new List<string>() { ClientName ?? TwitchChannelName });
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
                    ClipMonitorService?.Start();
                    IsStarted = true;
                    IsStopped = false;
                    InvokeBotStarted();
                }
                return true;
            }
            catch (Exception ex) 
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
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

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            return await ClipMonitorService.GetAllClipsAsync(TwitchChannelName);
        }

    }
}
