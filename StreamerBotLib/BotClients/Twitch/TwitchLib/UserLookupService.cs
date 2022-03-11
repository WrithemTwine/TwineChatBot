﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class UserLookupService : ApiService
    {
        public UserLookupService(ITwitchAPI api, int checkIntervalInSeconds = 60) : base(api, checkIntervalInSeconds)
        {
        }

        public async Task<string> GetUserId(string UserName)
        {
            string result = (await _api.Helix.Users.GetUsersAsync(logins: new List<string> { UserName })).Users.FirstOrDefault()?.Id ?? null;
            return result;
        }

        public async Task<GetChannelInformationResponse> GetChannelInformationAsync(string UserId = null, string UserName = null)
        {
            if (UserId != null)
            {
                return await _api.Helix.Channels.GetChannelInformationAsync(UserId);
            }
            else if (UserName != null)
            {
                string UserId_result = await GetUserId(UserName);
                return UserId_result != null ? await _api.Helix.Channels.GetChannelInformationAsync(UserId_result) : null;
            }

            return null;
        }

        public async Task<GetCustomRewardsResponse> GetChannelPointInformationAsync(string UserId = null, string UserName = null)
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