using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Api.Helix.Models.Raids.StartRaid;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Services.Events.FollowerService;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchHelixBot : TwitchBotsBase
    {

        /// <summary>
        /// Reports Game Category Name from querying a channel
        /// </summary>
        public event EventHandler<OnGetChannelGameNameEventArgs> GetChannelGameName;
        public event EventHandler<OnGetChannelPointsEventArgs> GetChannelPoints;
        public event EventHandler<OnStreamRaidResponseEventArgs> StartRaidEventResponse;
        public event EventHandler<GetStreamsEventArgs> GetStreamsViewerCount;
        public event EventHandler CancelRaidEvent;
        public event EventHandler<OnNewFollowersDetectedArgs> OnBulkFollowsUpdate;
        public event EventHandler BulkFollowsCompleted;
        public event EventHandler AccessTokenUnauthorized;

        internal TwitchHelixBot()
        {
            BotClientName = Bots.TwitchHelixBot;
        }

        private void BulkFollowsUpdate(string ChannelName, IEnumerable<ChannelFollower> follows)
        {
            OnBulkFollowsUpdate?.Invoke(this, new() { Channel = ChannelName, NewFollowers = new(follows) });
        }

        #region external-Twitch-calls Helix calls

        public static async Task<GetChattersResponse> TestGetChatAsync(string clientId, string accesstoken, string channelId, string moderatorId)
        {
            TwitchAPI testChat = new(settings: new ApiSettings() { AccessToken = accesstoken, ClientId = clientId });
            return await testChat.Helix.Chat.GetChattersAsync(channelId, moderatorId, accessToken: accesstoken);
        }

        private async Task<GetUsersResponse> GetUsersAsync(string UserName = null, string Id = null)
        {
            if (UserName != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Users.GetUsersAsync(logins: [UserName]);
            }
            else if (Id != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Users.GetUsersAsync(ids: [Id]);
            }
            return null;
        }

        private async Task<string> HelixGetUserId(string UserName)
        {
            return (await GetUsersAsync(UserName: UserName))?.Users.FirstOrDefault()?.Id ?? null;
        }

        private async Task<DateTime> HelixGetUserCreatedAt(string UserName = null, string UserId = null)
        {
            return (await GetUsersAsync(UserName: UserName, Id: UserId))?.Users.FirstOrDefault().CreatedAt ?? DateTime.MinValue;
        }

        private async Task<GetChannelInformationResponse> GetChannelInformationAsync(string UserId = null, string UserName = null)
        {
            string Id = UserId ?? await HelixGetUserId(UserName);

            if (Id != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Channels.GetChannelInformationAsync(Id);
            }

            return null;
        }

        private async Task<GetCustomRewardsResponse> GetChannelPointInformationAsync(string UserId = null, string UserName = null)
        {
            string Id = UserId ?? await HelixGetUserId(UserName);

            if (Id != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.ChannelPoints.GetCustomRewardAsync(Id);
            }
            return null;
        }

        private async Task<GetChattersResponse> GetChatters(string channelId, string botId, int first = 100, string after = null)
        {
            if (channelId != null && botId != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Chat.GetChattersAsync(channelId, botId, first, after);
            }
            return null;
        }

        private async Task<BanUserResponse> BanUser(string UserId = null, string UserName = null, int forDuration = 0, BanReasons banReason = BanReasons.UnsolicitedSpam)
        {
            string Id = UserId ?? await HelixGetUserId(UserName);

            if (Id != null)
            {
                BanUserRequest userRequest = new() { UserId = UserId ?? await HelixGetUserId(UserName), Duration = forDuration, Reason = banReason.ToString() };

                userRequest.Duration = forDuration is < 0 or > 1209600
                    ? throw new ArgumentOutOfRangeException(nameof(forDuration), "Duration is only allowed between 1 to 1,209,600 seconds.")
                    : forDuration > 0 ? forDuration : null;

                if (userRequest != null)
                {
                    return await tokenBot.StreamerHelixApi.Helix.Moderation.BanUserAsync(OptionFlags.TwitchStreamerUserId, OptionFlags.TwitchBotUserId, userRequest);
                }
            }
            return null;
        }

        private async Task ModifyChannelInformation(string UserId, string GameId = null, string BroadcasterLanguage = null, string Title = null, int Delay = -1)
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

            await tokenBot.StreamerHelixApi.Helix.Channels.ModifyChannelInformationAsync(UserId, request);
        }

        private async Task<GetGamesResponse> GetGameId(List<string> GameId = null, List<string> GameName = null)
        {
            if (GameId != null || GameName != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Games.GetGamesAsync(GameId, GameName);
            }
            return null;
        }

        private async Task<StartRaidResponse> StartRaid(string FromId, string ToUserId = null, string ToUserName = null)
        {
            string ToId = ToUserId ?? await HelixGetUserId(ToUserName);

            if (ToId != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Raids.StartRaidAsync(FromId, ToId);
            }
            return null;
        }

        private async Task CancelRaid(string FromId)
        {
            if (FromId != null)
            {
                await tokenBot.StreamerHelixApi.Helix.Raids.CancelRaidAsync(FromId);
            }
        }

        private async Task<GetStreamsResponse> GetStreams(string UserId = null, string UserName = null)
        {
            string Id = UserId ?? await HelixGetUserId(UserName);

            if (Id != null)
            {
                return await tokenBot.StreamerHelixApi.Helix.Streams.GetStreamsAsync(userIds: [Id]);
            }
            return null;
        }

        private async Task<GetChannelFollowersResponse> GetFollowers(string broadcasterId, string UserId = null, int first = 20, string after = null)
        {
            return await tokenBot.StreamerHelixApi.Helix.Channels.GetChannelFollowersAsync(broadcasterId: broadcasterId, userId: UserId, first: first, after: after);
        }

        #endregion

        #region Data Methods - Interface to Helix calls

        /// <summary>
        /// Helper method to consolidate try/catch related to testing for auth code token expiration and needing another access token.
        /// </summary>
        /// <typeparam name="T">The data return for the API call.</typeparam>
        /// <param name="MethodName">Name of the method for exception logging.</param>
        /// <param name="func">The function to perform.</param>
        /// <returns>The results of the function call.</returns>
        private T PerformAction<T>(string MethodName, Func<T> func)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Performing a user-service action.");

            try
            {
                return func.Invoke();
            }
            catch (TokenExpiredException ex)
            {
                LogWriter.LogException(ex, MethodName);
                LogWriter.DebugLog(MethodName, DebugLogTypes.TwitchHelixBot, "Exception found. Attempting to update token.");
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                tokenBot.CheckToken();

                LogWriter.DebugLog(MethodName, DebugLogTypes.TwitchHelixBot, "Attempting to again perform user-service action.");

                return func.Invoke();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodName);

                return default;
            }
        }

        public void BanUser(string BannedUserName, BanReasons banReason, int Duration = 0)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Performing a ban user operation.");

            _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return BanUser(UserName: BannedUserName, forDuration: Duration, banReason: banReason)?.Result;
            });
        }

        /// <summary>
        /// Retrieves the Game Category for a channel. Performs event <code>Post_GetChannelGameName</code> when <paramref name="UserName"/> equals the TwitchChannelName.
        /// </summary>
        /// <param name="UserId">References the Channel to get the Game Category.</param>
        /// <param name="UserName">References the Channel, converts to UserId, to get the Game Category.</param>
        /// <returns>The Game Category for the requested channel.</returns>
        public CategoryData GetUserGameCategory(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Checking for update to game category.");

            ChannelInformation channelInformation = GetUserInfo(UserId: UserId, UserName: UserName)?.Data[0];
            string gameName = channelInformation?.GameName ?? "All";
            string gameId = channelInformation?.GameId ?? "0";

            if (UserName == OptionFlags.TwitchChannelName)
            {
                PostEvent_GetChannelGameName(gameName, gameId);
            }

            return new(gameId, gameName);
        }

        public GetChannelInformationResponse GetUserInfo(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"The API call to get the channel information for {UserName}.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return GetChannelInformationAsync(UserId: UserId, UserName: UserName)?.Result;
            });
        }

        public string GetUserId(string UserName)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"Retrieving the user Id for the username, {UserName}.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixGetUserId(UserName)?.Result;
            });
        }

        /// <summary>
        /// Retrieve the Id value for the provided game.
        /// </summary>
        /// <param name="GameName">The name of the game to retrieve the Id.</param>
        /// <returns>The Twitch Id of the game.</returns>
        public string GetGameId(string GameName)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"Getting the game ID of the provided game name, {GameName}.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return GetGameId(GameName: [GameName]).Result.Data[0].IgdbId;
            });
        }

        /// <summary>
        /// Retrieve the current viewer count of the specified channel (user) name.
        /// Results are through the "GetStreamsViewerCount" event.
        /// </summary>
        /// <param name="UserName">The channel user name to get the current viewer count.</param>
        public void GetViewerCount(string UserName)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Getting the current viewer count.");

            _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                GetStreamsResponse getStreamsResponse = GetStreams(UserName: UserName).Result;
                GetCurrentViewerCount(getStreamsResponse);
                return getStreamsResponse;
            });
        }

        public GetStreamsResponse GetStreamDetail(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Getting the current stream data.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return GetStreams(UserId: UserId, UserName: UserName).Result;
            });
        }

        /// <summary>
        /// Gets the current Chatters/Viewers in the broadcaster's channel.
        /// </summary>
        /// <param name="channelId">The user Id of the broadcaster.</param>
        /// <param name="botId">The user Id of the bot account.</param>
        /// <returns>A list of Chatters detailing the current viewers of the channel.</returns>
        public List<Chatter> GetChatters()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Getting a list of current chatters.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                string channelId = OptionFlags.TwitchStreamerUserId, botId = OptionFlags.TwitchBotUserId;

                List<Chatter> currChat = [];
                string cursor = null;

                do
                {
                    GetChattersResponse curr = GetChatters(channelId, botId, after: cursor).Result;
                    cursor = curr.Pagination?.Cursor;
                    currChat.AddRange(curr.Data);
                }
                while (!string.IsNullOrEmpty(cursor));

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"Finished getting a list of {currChat.Count} current chatters.");

                return currChat;
            });
        }

        /// <summary>
        /// Gets the account create date of the specified user. Specify either parameter.
        /// </summary>
        /// <param name="UserName">The user name for user.</param>
        /// <param name="UserId">The user Id for the user.</param>
        /// <returns>The date the user created their account.</returns>
        public DateTime GetUserCreatedAt(string UserName = null, string UserId = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Getting the current account's creation date.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixGetUserCreatedAt(UserName, UserId).Result;
            });
        }

        public GetCustomRewardsResponse GetCustomRewardsId(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "API call to get a channel's point redemption rewards.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return GetChannelPointInformationAsync(UserId: UserId, UserName: UserName).Result;
            });
        }

        public List<string> GetUserCustomRewards(string UserId = null, string UserName = null)
        {
            if (UserName == OptionFlags.TwitchChannelName && OptionFlags.TwitchStreamerUserId != null) // switch the username for Id
            {
                UserName = null;
                UserId = OptionFlags.TwitchStreamerUserId;
            }

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Getting the stream's current channel point redemptions.");

            GetCustomRewardsResponse getCustom = GetCustomRewardsId(UserId: UserId, UserName: UserName);

            List<string> CustomRewardsList = [];
            if (getCustom != null)
            {
                CustomRewardsList.AddRange(getCustom.Data.Select(cr => cr.Title));
                CustomRewardsList.Sort();
                PostEvent_GetCustomRewards(CustomRewardsList);
            }
            else
            {
                CustomRewardsList.Add("");
            }

            return CustomRewardsList;
        }

        public bool SetChannelTitle(string Title)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"Setting current stream title to, {Title}.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                _ = ModifyChannelInformation(OptionFlags.TwitchStreamerUserId, Title: Title);
                return true;
            });
        }

        public bool SetChannelCategory(string CategoryName = null, string CategoryId = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"Setting the channel category to {CategoryName}.");

            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                CategoryId ??= GetGameId(CategoryName);
                _ = ModifyChannelInformation(OptionFlags.TwitchStreamerUserId, GameId: CategoryId);
                return true;
            });
        }

        public void RaidChannel(string ToChannelName)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, $"Raiding another channel, {ToChannelName}.");

            if (ToChannelName == null)
            {
                return;
            }
            else
            {
                _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
                {
                    OptionFlags.TwitchOutRaidStarted = true;
                    StartRaidResponse response = StartRaid(OptionFlags.TwitchStreamerUserId, ToUserName: ToChannelName).Result;
                    if (response != null)
                    {
                        NotifyStartRaidResponse(ToChannelName, response);
                    }
                    return true;
                });
            }
        }

        public void CancelRaidChannel()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Cancel raid.");

            _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                CancelRaid(OptionFlags.TwitchStreamerUserId).Start();
                NotifyCancelRaid();
                return true;
            });
        }

        public void GetAllFollowers()
        {
            ThreadManager.CreateThreadStart(
                new Task(async () =>
                {
                    try
                    {
                        int MaxFollowers = 100; // use the max for bulk retrieve

                        List<ChannelFollower> ResponseList = [];

                        GetChannelFollowersResponse followsResponse = await GetFollowers(broadcasterId: OptionFlags.TwitchStreamerUserId, first: MaxFollowers);

                        ResponseList.AddRange(followsResponse.Data);

                        while (followsResponse?.Data.Length == MaxFollowers && followsResponse?.Pagination.Cursor != null) // loop until the last response is less than 100; each retrieval provides 100 items at a time
                        {
                            followsResponse = await GetFollowers(after: followsResponse.Pagination.Cursor,
                                                                 first: MaxFollowers,
                                                                 broadcasterId: OptionFlags.TwitchStreamerUserId);
                            ResponseList.AddRange(followsResponse.Data);
                            Thread.Sleep(200);
                        }
                        BulkFollowsUpdate(OptionFlags.TwitchChannelName, ResponseList);
                    }
                    catch (BadTokenException)
                    {
                        AccessTokenUnauthorized?.Invoke(this, new());
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    }
                    BulkFollowsCompleted?.Invoke(this, new());
                })
            );
        }

        #region process events

        private void PostEvent_GetChannelGameName(string foundGameName, string foundGameId)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Posting game category update through 'GetChannelGameName' event.");

            GetChannelGameName?.Invoke(this, new OnGetChannelGameNameEventArgs() { GameName = foundGameName, GameId = foundGameId });
        }

        private void PostEvent_GetCustomRewards(List<string> CustomRewardsList)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Posting custom reward list through 'GetCustomRewards' event.");

            GetChannelPoints?.Invoke(this, new OnGetChannelPointsEventArgs() { ChannelPointNames = CustomRewardsList });
        }

        private void GetCurrentViewerCount(GetStreamsResponse getStreamsResponse)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Posting current viewer count through 'GetCurrentViewerCount' event.");

            GetStreamsViewerCount?.Invoke(this, new() { ViewerCount = getStreamsResponse?.Streams[0]?.ViewerCount ?? 0 });
        }

        private void NotifyStartRaidResponse(string ToChannelName, StartRaidResponse response)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Posting the start of a raid to another channel through the 'StartRaidEventResponse' event.");

            StartRaidEventResponse?.Invoke(this, new()
            {
                ToChannel = ToChannelName,
                CreatedAt = response.Data[0].CreatedAt,
                IsMature = response.Data[0].IsMature
            });
        }

        private void NotifyCancelRaid()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchHelixBot, "Posting to cancel current raid through 'CancelRaidEvent' event.");

            CancelRaidEvent?.Invoke(this, new());
        }

        #endregion

        #endregion

    }
}
