﻿using StreamerBotLib.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Api.Helix.Models.Raids.StartRaid;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class UserLookupService : ApiService
    {
        // TODO: consider adding code to determine if the internet or Twitch servers are not available (throws exception); and backoff trying to perform action; maybe add a revisit task to try again after a certain amount of time

        public UserLookupService(ITwitchAPI api, int checkIntervalInSeconds = 60) : base(api, checkIntervalInSeconds)
        {
        }

        private async Task<GetUsersResponse> GetUsersAsync(string UserName = null, string Id = null)
        {
            if (UserName != null)
            {
                return await _api.Helix.Users.GetUsersAsync(logins: new List<string> { UserName });
            }
            else if (Id != null)
            {
                return await _api.Helix.Users.GetUsersAsync(ids: new List<string> { Id });
            }
            return null;
        }

        public async Task<string> GetUserId(string UserName)
        {
            string result = null;
            int loop = 0;

            while (loop < 5 && result == null)
            {
                try
                {
                    result = (await GetUsersAsync(UserName: UserName))?.Users.FirstOrDefault()?.Id ?? null;
                }
                catch
                {
                    loop++;
                }
            }

            return result;
        }

        public async Task<DateTime> GetUserCreatedAt(string UserName = null, string UserId = null)
        {
            return (await GetUsersAsync(UserName: UserName, Id: UserId))?.Users.FirstOrDefault().CreatedAt ?? DateTime.MinValue;
        }

        public async Task<GetChannelInformationResponse> GetChannelInformationAsync(string UserId = null, string UserName = null)
        {
            if (UserId != null || UserName != null)
            {
                return await _api.Helix.Channels.GetChannelInformationAsync(UserId ?? await GetUserId(UserName));
            }

            return null;
        }

        public async Task<GetCustomRewardsResponse> GetChannelPointInformationAsync(string UserId = null, string UserName = null)
        {
            if (UserId != null || UserName != null)
            {
                return await _api.Helix.ChannelPoints.GetCustomRewardAsync(UserId ?? await GetUserId(UserName));
            }
            return null;
        }

        public async Task<BanUserResponse> BanUser(string UserId = null, string UserName = null, int forDuration = 0, BanReasons banReason = BanReasons.UnsolicitedSpam)
        {
            BanUserRequest userRequest = new() { UserId = UserId ?? await GetUserId(UserName), Duration = forDuration, Reason = banReason.ToString() };

            userRequest.Duration = forDuration is < 0 or > 1209600
                ? throw new ArgumentOutOfRangeException(nameof(forDuration), "Duration is only allowed between 1 to 1,209,600 seconds.")
                : forDuration > 0 ? forDuration : null;

            return await _api.Helix.Moderation.BanUserAsync(TwitchBotsBase.TwitchChannelId, TwitchBotsBase.TwitchBotUserId, userRequest);
        }

        public async Task ModifyChannelInformation(string UserId, string GameId = null, string BroadcasterLanguage = null, string Title = null, int Delay = -1)
        {
            ModifyChannelInformationRequest request = new();

            if (GameId != null)
            {
                request.GameId = GameId;
            }
            if (BroadcasterLanguage != null)
            {
                request.BroadcasterLanguage = BroadcasterLanguage;
            }
            if (Title != null)
            {
                request.Title = Title;
            }
            if (Delay != -1)
            {
                request.Delay = Delay;
            }

            await _api.Helix.Channels.ModifyChannelInformationAsync(UserId, request);
        }

        public async Task<GetGamesResponse> GetGameId(List<string> GameId = null, List<string> GameName = null)
        {
            return await _api.Helix.Games.GetGamesAsync(GameId, GameName);
        }

        public async Task<StartRaidResponse> StartRaid(string FromId, string ToUserId = null, string ToUserName = null)
        {
            if (ToUserId != null || ToUserName != null)
            {
                return await _api.Helix.Raids.StartRaidAsync(FromId, ToUserId ?? await GetUserId(ToUserName));
            }
            return null;
        }

        public async Task CancelRaid(string FromId)
        {
            await _api.Helix.Raids.CancelRaidAsync(FromId);
        }

        public async Task<GetStreamsResponse> GetStreams(string UserId = null, string UserName = null)
        {
            if (UserId != null || UserName != null)
            {
                return await _api.Helix.Streams.GetStreamsAsync(userIds: new() { UserId ?? await GetUserId(UserName) });
            }
            return null;
        }

    }
}
