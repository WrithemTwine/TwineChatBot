﻿using StreamerBotLib.Enums;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Api.Helix.Models.Raids.StartRaid;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    internal class HelixApiService
    {
        public event EventHandler UnauthorizedToken;

        // Streamer Id-Token API
        internal static TwitchAPI StreamerAPI { get; private set; }

        // Bot-Any Account-Id-Token API
        internal static TwitchAPI BotAPI { get; private set; }

        public static async Task<GetChattersResponse> TestGetChatAsync(string clientId, string accesstoken, string channelId, string moderatorId)
        {
            TwitchAPI testChat = new(settings: new ApiSettings() { AccessToken = accesstoken, ClientId = clientId });
            return await testChat.Helix.Chat.GetChattersAsync(channelId, moderatorId, accessToken: accesstoken);
        }

        public HelixApiService(ApiSettings BotApi, ApiSettings StreamerApi = null)
        {
            BotAPI = new(settings: BotApi);
            StreamerAPI = StreamerApi == null ? BotAPI : new(settings: StreamerApi);
        }

        private async Task<GetUsersResponse> GetUsersAsync(string UserName = null, string Id = null)
        {
            if (UserName != null)
            {
                return await BotAPI.Helix.Users.GetUsersAsync(logins: [UserName]);
            }
            else if (Id != null)
            {
                return await BotAPI.Helix.Users.GetUsersAsync(ids: [Id]);
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
                catch (BadRequestException)
                {
                    UnauthorizedToken?.Invoke(this, new());
                    break; // have to break to set the token
                }
                catch (BadScopeException)
                {
                    UnauthorizedToken?.Invoke(this, new());
                    break; // have to break to set the token
                }
                catch // backoff request
                {
                }
                loop++;
            }

            return result;
        }

        internal async Task<DateTime> GetUserCreatedAt(string UserName = null, string UserId = null)
        {
            return (await GetUsersAsync(UserName: UserName, Id: UserId))?.Users.FirstOrDefault().CreatedAt ?? DateTime.MinValue;
        }

        internal async Task<GetChannelInformationResponse> GetChannelInformationAsync(string UserId = null, string UserName = null)
        {
            string Id = UserId ?? await GetUserId(UserName);

            if (Id != null)
            {
                return await BotAPI.Helix.Channels.GetChannelInformationAsync(Id);
            }

            return null;
        }

        internal async Task<GetCustomRewardsResponse> GetChannelPointInformationAsync(string UserId = null, string UserName = null)
        {
            string Id = UserId ?? await GetUserId(UserName);

            if (Id != null)
            {
                return await StreamerAPI.Helix.ChannelPoints.GetCustomRewardAsync(Id);
            }
            return null;
        }

        internal async Task<GetChattersResponse> GetChatters(string channelId, string botId, int first = 100, string after = null)
        {
            if (channelId != null && botId != null)
            {
                return await BotAPI.Helix.Chat.GetChattersAsync(channelId, botId, first, after);
            }
            return null;
        }

        internal async Task<BanUserResponse> BanUser(string UserId = null, string UserName = null, int forDuration = 0, BanReasons banReason = BanReasons.UnsolicitedSpam)
        {
            string Id = UserId ?? await GetUserId(UserName);

            if (Id != null)
            {
                BanUserRequest userRequest = new() { UserId = UserId ?? await GetUserId(UserName), Duration = forDuration, Reason = banReason.ToString() };

                userRequest.Duration = forDuration is < 0 or > 1209600
                    ? throw new ArgumentOutOfRangeException(nameof(forDuration), "Duration is only allowed between 1 to 1,209,600 seconds.")
                    : forDuration > 0 ? forDuration : null;

                if (userRequest != null)
                {
                    return await BotAPI.Helix.Moderation.BanUserAsync(TwitchBotsBase.TwitchChannelId, TwitchBotsBase.TwitchBotUserId, userRequest);
                }
            }
            return null;
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

            await StreamerAPI.Helix.Channels.ModifyChannelInformationAsync(UserId, request);
        }

        internal async Task<GetGamesResponse> GetGameId(List<string> GameId = null, List<string> GameName = null)
        {
            if (GameId != null || GameName != null)
            {
                return await BotAPI.Helix.Games.GetGamesAsync(GameId, GameName);
            }
            return null;
        }

        internal async Task<StartRaidResponse> StartRaid(string FromId, string ToUserId = null, string ToUserName = null)
        {
            string ToId = ToUserId ?? await GetUserId(ToUserName);

            if (ToId != null)
            {
                return await StreamerAPI.Helix.Raids.StartRaidAsync(FromId, ToId);
            }
            return null;
        }

        internal async Task CancelRaid(string FromId)
        {
            if (FromId != null)
            {
                await StreamerAPI.Helix.Raids.CancelRaidAsync(FromId);
            }
        }

        internal async Task<GetStreamsResponse> GetStreams(string UserId = null, string UserName = null)
        {
            string Id = UserId ?? await GetUserId(UserName);

            if (Id != null)
            {
                return await BotAPI.Helix.Streams.GetStreamsAsync(userIds: [Id]);
            }
            return null;
        }

    }
}
