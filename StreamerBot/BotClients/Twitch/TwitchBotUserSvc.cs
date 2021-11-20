using StreamerBot.BotClients.Twitch.TwitchLib;
using StreamerBot.Enum;

using System;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
        private UserLookupService userLookupService;

        public TwitchBotUserSvc()
        {
            BotClientName = Bots.TwitchUserBot;
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
