using StreamerBot.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBot.BotClients.Twitch.TwitchLib
{
    public class UserLookupService : ApiService
    {
        public UserLookupService(ITwitchAPI api, int checkIntervalInSeconds = 60) : base(api, checkIntervalInSeconds)
        {
        }


        public async Task<string> GetUserId(string UserName)
        {
            string result = (await _api.Helix.Users.GetUsersAsync(logins: new List<string> { UserName })).Users.FirstOrDefault()?.Id;
            return result;
        }

        public async Task<GetChannelInformationResponse> GetChannelInformation(string UserId = null, string UserName = null)
        {
            if (UserId != null)
            {
                return await _api.Helix.Channels.GetChannelInformationAsync(UserId);
            }
            else if (UserName != null)
            {
                return await _api.Helix.Channels.GetChannelInformationAsync(await GetUserId(UserName));
            }

            return null;
        }

        public async Task<GetCustomRewardsResponse> GetChannelPointInformation(string UserId = null, string UserName = null)
        {
            if (UserId != null)
            {
                return await _api.Helix.ChannelPoints.GetCustomRewardAsync(UserId);
            }
            else if (UserName != null)
            {
                return await _api.Helix.ChannelPoints.GetCustomRewardAsync(await GetUserId(UserName));
            }

            return null;
        }
    }
}
