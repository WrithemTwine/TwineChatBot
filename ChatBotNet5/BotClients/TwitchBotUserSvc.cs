using ChatBot_Net5.BotClients.TwitchLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchLib.Api.Core;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace ChatBot_Net5.BotClients
{
    public class TwitchBotUserSvc : TwitchBots
    {
        public UserLookupService userLookupService { get; set; }

        public TwitchBotUserSvc()
        {
            BotClientName = Enum.Bots.TwitchUserBot;
        }

        public void ConnectUserService(string ClientName = null, string TwitchToken = null)
        {
            if (IsStarted)
            {
                userLookupService.Stop();
            }

            RefreshSettings();
            ApiSettings apiclip = new() { AccessToken = TwitchToken ?? TwitchAccessToken, ClientId = ClientName ?? TwitchClientID };
            userLookupService = new(new TwitchAPI(null, null, apiclip, null), (int)Math.Round(TwitchFrequencyClipTime, 0));
        }

        public string GetUserGameCategory(string UserId)
        {
            userLookupService.Start();
            GetChannelInformationResponse user =  userLookupService.GetChannelInformation(UserId).Result;
            userLookupService.Stop();

            return user.Data[0].GameName;
        }
    }
}
