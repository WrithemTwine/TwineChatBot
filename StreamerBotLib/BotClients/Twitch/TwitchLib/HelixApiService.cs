using StreamerBotLib.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Core;
using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.Interfaces;
using TwitchLib.Api.Core.RateLimiter;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Api.Helix.Models.Raids.StartRaid;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class HelixApiService
    {
        // TODO: consider adding code to determine if the internet or Twitch servers are not available (throws exception); and backoff trying to perform action; maybe add a revisit task to try again after a certain amount of time

        // Streamer Id-Token API
        private ChannelPoints ChannelPoints { get; set; }
        private Channels Channels { get; set; } // ModifyChannelInformation
        private Raids Raids { get; set; }

        // Bot-Any Account-Id-Token API
        private Users Users { get; set; }
        private Moderation Moderation { get; set; }
        private Streams Streams { get; set; }
        private Games Games { get; set; }
        /// <summary>
        /// Read-only get channel info
        /// </summary>
        private Channels GetChannels { get; set; }

        private readonly IRateLimiter limiter;
        private readonly IHttpCallHandler httpCallHandler;

        public HelixApiService(ApiSettings BotApi, ApiSettings StreamerApi = null)
        {
            limiter = new BypassLimiter();
            httpCallHandler = new TwitchHttpClient();

            SetBotApiSettings(BotApi);
            SetStreamerApiSettings(StreamerApi ?? BotApi);
        }

        internal void SetBotApiSettings(ApiSettings apiSettings)
        {
            Users = new(apiSettings, limiter, httpCallHandler);
            Moderation = new(apiSettings, limiter, httpCallHandler);
            Streams = new(apiSettings, limiter, httpCallHandler);
            Games = new(apiSettings, limiter, httpCallHandler);
            GetChannels = new(apiSettings, limiter, httpCallHandler);
        }

        internal void SetStreamerApiSettings(ApiSettings apiSettings)
        {
            ChannelPoints = new(apiSettings, limiter, httpCallHandler);
            Channels = new(apiSettings, limiter, httpCallHandler);
            Raids = new(apiSettings, limiter, httpCallHandler);
        }

        private async Task<GetUsersResponse> GetUsersAsync(string UserName = null, string Id = null)
        {
            if (UserName != null)
            {
                return await Users.GetUsersAsync(logins: new List<string> { UserName });
            }
            else if (Id != null)
            {
                return await Users.GetUsersAsync(ids: new List<string> { Id });
            }
            return null;
        }

        internal async Task<string> GetUserId(string UserName)
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

        internal async Task<DateTime> GetUserCreatedAt(string UserName = null, string UserId = null)
        {
            return (await GetUsersAsync(UserName: UserName, Id: UserId))?.Users.FirstOrDefault().CreatedAt ?? DateTime.MinValue;
        }

        internal async Task<GetChannelInformationResponse> GetChannelInformationAsync(string UserId = null, string UserName = null)
        {
            if (UserId != null || UserName != null)
            {
                return await GetChannels.GetChannelInformationAsync(UserId ?? await GetUserId(UserName));
            }

            return null;
        }

        internal async Task<GetCustomRewardsResponse> GetChannelPointInformationAsync(string UserId = null, string UserName = null)
        {
            if (UserId != null || UserName != null)
            {
                return await ChannelPoints.GetCustomRewardAsync(UserId ?? await GetUserId(UserName));
            }
            return null;
        }

        internal async Task<BanUserResponse> BanUser(string UserId = null, string UserName = null, int forDuration = 0, BanReasons banReason = BanReasons.UnsolicitedSpam)
        {
            BanUserRequest userRequest = new() { UserId = UserId ?? await GetUserId(UserName), Duration = forDuration, Reason = banReason.ToString() };

            userRequest.Duration = forDuration is < 0 or > 1209600
                ? throw new ArgumentOutOfRangeException(nameof(forDuration), "Duration is only allowed between 1 to 1,209,600 seconds.")
                : forDuration > 0 ? forDuration : null;

            return await Moderation.BanUserAsync(TwitchBotsBase.TwitchChannelId, TwitchBotsBase.TwitchBotUserId, userRequest);
        }

        internal async Task ModifyChannelInformation(string UserId, string GameId = null, string BroadcasterLanguage = null, string Title = null, int Delay = -1)
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

            await Channels.ModifyChannelInformationAsync(UserId, request);
        }

        internal async Task<GetGamesResponse> GetGameId(List<string> GameId = null, List<string> GameName = null)
        {
            return await Games.GetGamesAsync(GameId, GameName);
        }

        internal async Task<StartRaidResponse> StartRaid(string FromId, string ToUserId = null, string ToUserName = null)
        {
            if (ToUserId != null || ToUserName != null)
            {
                return await Raids.StartRaidAsync(FromId, ToUserId ?? await GetUserId(ToUserName));
            }
            return null;
        }

        internal async Task CancelRaid(string FromId)
        {
            await Raids.CancelRaidAsync(FromId);
        }

        internal async Task<GetStreamsResponse> GetStreams(string UserId = null, string UserName = null)
        {
            if (UserId != null || UserName != null)
            {
                return await Streams.GetStreamsAsync(userIds: new() { UserId ?? await GetUserId(UserName) });
            }
            return null;
        }

    }
}
