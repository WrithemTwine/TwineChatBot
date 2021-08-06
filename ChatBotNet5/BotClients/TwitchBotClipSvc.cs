using ChatBot_Net5.BotClients.TwitchLib;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Clips.GetClips;

namespace ChatBot_Net5.BotClients
{
    public class TwitchBotClipSvc : TwitchBots
    {
        internal ClipMonitorService clipMonitorService { get; set; }

        public TwitchBotClipSvc()
        {
            BotClientName = "TwitchClipBot";
        }

        internal void ConnectClipService(string ClientName = null, string TwitchToken = null)
        {
            if (IsStarted)
            {
                clipMonitorService.Stop();
            }

            RefreshSettings();
            ApiSettings apiclip = new() { AccessToken = TwitchToken ?? TwitchAccessToken, ClientId = ClientName ?? TwitchClientID };
            clipMonitorService = new(new TwitchAPI(null, null, apiclip, null), (int)Math.Round(TwitchFrequencyClipTime, 0));
            clipMonitorService.SetChannelsByName(new List<string>() { ClientName ?? TwitchChannelName });
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override bool StartBot()
        {
            try
            {
                ConnectClipService();
                clipMonitorService?.Start();
                IsStarted = true;
                IsStopped = false;
                InvokeBotStarted();

                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override bool StopBot()
        {
            try
            {
                if (!IsStopped)
                {
                    clipMonitorService?.Stop();
                    IsStarted = false;
                    IsStopped = true;
                    InvokeBotStopped();
                    clipMonitorService = null;
                }
                return true;
            }
            catch { }
            return false;
        }

        public override bool ExitBot()
        {
            clipMonitorService?.Stop();
            clipMonitorService = null;
            return base.ExitBot();
        }

        public async Task<List<Clip>> GetAllClipsAsync()
        {
            return await clipMonitorService.GetAllClipsAsync(TwitchChannelName);
        }

    }
}
