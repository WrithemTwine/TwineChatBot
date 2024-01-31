using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Chat.GetChatters;
using TwitchLib.Api.Helix.Models.Raids.StartRaid;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
        private readonly static string TokenLock = "Lock";

        private static HelixApiService HelixApiCalls;

        /// <summary>
        /// Contains the Helix API utilizing the Channel-Streamer access token
        /// </summary>
        public TwitchAPI HelixAPIStreamerToken
        {
            get
            {
                if (HelixApiService.StreamerAPI == null)
                {
                    ChooseConnectUserService();
                }

                return HelixApiService.StreamerAPI;
            }
        }
        /// <summary>
        /// Contains the Helix API utilizing the Bot account access token
        /// </summary>
        public TwitchAPI HelixAPIBotToken
        {
            get
            {
                if (HelixApiService.StreamerAPI == null)
                {
                    ChooseConnectUserService();
                }

                return HelixApiService.StreamerAPI;
            }
        }

        /// <summary>
        /// Reports Game Category Name from querying a channel
        /// </summary>
        public event EventHandler<OnGetChannelGameNameEventArgs> GetChannelGameName;
        public event EventHandler<OnGetChannelPointsEventArgs> GetChannelPoints;
        public event EventHandler<OnStreamRaidResponseEventArgs> StartRaidEventResponse;
        public event EventHandler<GetStreamsEventArgs> GetStreamsViewerCount;
        public event EventHandler CancelRaidEvent;

        public TwitchBotUserSvc()
        {
            BotClientName = Bots.TwitchUserBot;
            twitchTokenBot.BotAccessTokenChanged += TwitchTokenBot_BotAccessTokenChanged;
            twitchTokenBot.StreamerAccessTokenChanged += TwitchTokenBot_StreamerAccessTokenChanged;
        }

        #region Token Bot ops

        private void TwitchTokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            lock (TokenLock)
            {
                if (HelixApiCalls != null)
                {
                    HelixAPIStreamerToken.Settings.AccessToken = TwitchStreamerAccessToken;
                }
                else
                {
                    ChooseConnectUserService();
                }
                ResetTokenMode = false;
            }
        }

        private void TwitchTokenBot_BotAccessTokenChanged(object sender, EventArgs e)
        {
            lock (TokenLock)
            {
                if (HelixApiCalls != null)
                {
                    HelixAPIBotToken.Settings.AccessToken = TwitchAccessToken;
                }
                else
                {
                    ChooseConnectUserService();
                }
                ResetTokenMode = false;
            }
        }

        #endregion

        /// <summary>
        /// Aware of whether to use the bot user client Id or streamer client Id, due to API calls requiring the client Id of the streaming channel to retrieve the data.
        /// </summary>
        /// <param name="UseStreamToken">Specify whether the Streamer's Token is required to access Channel Data</param>
        private void ChooseConnectUserService()
        {
            if (HelixApiCalls == null)
            {
                ApiSettings BotApiSettings = null;
                ApiSettings StreamerApiSettings = null;

                string ClientId;
                string OauthToken;

                // verify and, if necessary, refresh the access tokens

                if (OptionFlags.TwitchStreamerUseToken)
                {
                    ClientId = TwitchStreamClientId;
                    OauthToken = TwitchStreamerAccessToken;
                    if (ClientId != null && OauthToken != null)
                    {
                        StreamerApiSettings = new() { AccessToken = OauthToken, ClientId = ClientId };
                    }
                }

                ClientId = TwitchClientID;
                OauthToken = TwitchAccessToken;
                if (ClientId != null && OauthToken != null)
                {
                    BotApiSettings = new() { ClientId = ClientId, AccessToken = OauthToken };
                }

                if (BotApiSettings != null
                    && ((OptionFlags.TwitchStreamerUseToken && StreamerApiSettings != null)
                       || !OptionFlags.TwitchStreamerUseToken))
                {
                    HelixApiCalls = new(BotApiSettings, StreamerApiSettings);
                    HelixApiCalls.UnauthorizedToken += HelixApiCalls_UnauthorizedToken;

                    if (TwitchChannelId == null && TwitchBotUserId == null)
                    {
                        SetIds();
                    }
                }
            }
        }

        private static void HelixApiCalls_UnauthorizedToken(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
            twitchTokenBot.CheckToken();
        }

        /// <summary>
        /// Helper method to consolidate try/catch related to testing for auth code token expiration and needing another access token.
        /// </summary>
        /// <typeparam name="T">The data return for the API call.</typeparam>
        /// <param name="MethodName">Name of the method for exception logging.</param>
        /// <param name="func">The function to perform.</param>
        /// <returns>The results of the function call.</returns>
        private T PerformAction<T>(string MethodName, Func<T> func)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Performing a user-service action.");

            try
            {
                ChooseConnectUserService();
                return func.Invoke();
            }
            catch (UnauthorizedAccessException ex)
            {
                LogWriter.LogException(ex, MethodName);
                LogWriter.DebugLog(MethodName, DebugLogTypes.TwitchBotUserSvc, "Exception found. Attempting to update token.");

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                twitchTokenBot.CheckToken();
                while (ResetTokenMode) { }

                LogWriter.DebugLog(MethodName, DebugLogTypes.TwitchBotUserSvc, "Attempting to again perform user-service action.");

                return func.Invoke();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodName);

                return default;
            }
        }

        internal void SetIds(string StreamerChannelId = null, string BotChannelId = null)
        {
            if (StreamerChannelId != null)
            {
                TwitchChannelId = StreamerChannelId;
            }

            if (BotChannelId != null)
            {
                TwitchBotUserId = BotChannelId;
            }

            if (HelixApiCalls != null)
            {
                ThreadManager.CreateThreadStart(() =>
                {
                    if (StreamerChannelId == null && BotChannelId == null && TwitchChannelName != null)
                    {
                        TwitchBotUserId = GetUserId(TwitchBotUserName);
                        TwitchChannelId = GetUserId(TwitchChannelName);
                    }
                });
            }
        }

        #region ClientId can be different between Bot and Channel

        public void BanUser(string BannedUserName, BanReasons banReason, int Duration = 0)
        {
            _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixApiCalls.BanUser(UserName: BannedUserName, forDuration: Duration, banReason: banReason)?.Result;
            });
        }

        /// <summary>
        /// Retrieves the Game Category for a channel. Performs event <code>Post_GetChannelGameName</code> when <paramref name="UserName"/> equals the TwitchChannelName.
        /// </summary>
        /// <param name="UserId">References the Channel to get the Game Category.</param>
        /// <param name="UserName">References the Channel, converts to UserId, to get the Game Category.</param>
        /// <returns>The Game Category for the requested channel.</returns>
        public string GetUserGameCategory(string UserId = null, string UserName = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Checking for update to game category.");

            ChannelInformation channelInformation = GetUserInfo(UserId: UserId, UserName: UserName)?.Data[0];
            string gameName = channelInformation?.GameName ?? "N/A";
            string gameId = channelInformation?.GameId ?? "N/A";

            if (UserName == TwitchChannelName)
            {
                PostEvent_GetChannelGameName(gameName, gameId);
            }

            return gameName;
        }

        public GetChannelInformationResponse GetUserInfo(string UserId = null, string UserName = null)
        {
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixApiCalls.GetChannelInformationAsync(UserId: UserId, UserName: UserName)?.Result;
            });
        }

        public string GetUserId(string UserName)
        {
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixApiCalls.GetUserId(UserName)?.Result;
            });
        }

        /// <summary>
        /// Retrieve the Id value for the provided game.
        /// </summary>
        /// <param name="GameName">The name of the game to retrieve the Id.</param>
        /// <returns>The Twitch Id of the game.</returns>
        public string GetGameId(string GameName)
        {
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixApiCalls.GetGameId(GameName: [GameName]).Result.Games[0].Id;
            });
        }

        /// <summary>
        /// Retrieve the current viewer count of the specified channel (user) name.
        /// Results are through the "GetStreamsViewerCount" event.
        /// </summary>
        /// <param name="UserName">The channel user name to get the current viewer count.</param>
        public void GetViewerCount(string UserName)
        {
            _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                GetStreamsResponse getStreamsResponse = HelixApiCalls.GetStreams(UserName: UserName).Result;
                GetCurrentViewerCount(getStreamsResponse);
                return getStreamsResponse;
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
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                string channelId = TwitchChannelId, botId = TwitchBotUserId;

                List<Chatter> currChat = [];
                string cursor = null;

                do
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Getting a list of current chatters.");

                    GetChattersResponse curr = HelixApiCalls.GetChatters(channelId, botId, after: cursor).Result;
                    cursor = curr.Pagination?.Cursor;
                    currChat.AddRange(curr.Data);
                }
                while (!string.IsNullOrEmpty(cursor));

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, $"Finished getting a list of {currChat.Count} current chatters.");

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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Getting the current account's creation date.");


            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixApiCalls.GetUserCreatedAt(UserName, UserId).Result;
            });
        }

        #endregion

        #region StreamerChannel Client Id and Request UserId must be the same

        public GetCustomRewardsResponse GetCustomRewardsId(string UserId = null, string UserName = null)
        {
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                return HelixApiCalls?.GetChannelPointInformationAsync(UserId: UserId, UserName: UserName).Result;
            });
        }

        public List<string> GetUserCustomRewards(string UserId = null, string UserName = null)
        {
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
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                _ = HelixApiCalls?.ModifyChannelInformation(TwitchChannelId, Title: Title);
                return true;
            });
        }

        public bool SetChannelCategory(string CategoryName = null, string CategoryId = null)
        {
            return PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                CategoryId ??= GetGameId(CategoryName);
                _ = HelixApiCalls?.ModifyChannelInformation(TwitchChannelId, GameId: CategoryId);
                return true;
            });
        }

        public void RaidChannel(string ToChannelName)
        {
            if (ToChannelName == null)
            {
                return;
            }
            else
            {
                _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
                {
                    OptionFlags.TwitchOutRaidStarted = true;
                    StartRaidResponse response = HelixApiCalls?.StartRaid(TwitchChannelId, ToUserName: ToChannelName).Result;
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
            _ = PerformAction(MethodBase.GetCurrentMethod().Name, () =>
            {
                HelixApiCalls?.CancelRaid(TwitchChannelId);
                NotifyCancelRaid();
                return true;
            });
        }

        #endregion

        #region process events

        private void PostEvent_GetChannelGameName(string foundGameName, string foundGameId)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Posting game category update through 'GetChannelGameName' event.");

            GetChannelGameName?.Invoke(this, new OnGetChannelGameNameEventArgs() { GameName = foundGameName, GameId = foundGameId });
        }

        private void PostEvent_GetCustomRewards(List<string> CustomRewardsList)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Posting custom reward list through 'GetCustomRewards' event.");

            GetChannelPoints?.Invoke(this, new OnGetChannelPointsEventArgs() { ChannelPointNames = CustomRewardsList });
        }
        private void GetCurrentViewerCount(GetStreamsResponse getStreamsResponse)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Posting current viewer count through 'GetCurrentViewerCount' event.");

            GetStreamsViewerCount?.Invoke(this, new() { ViewerCount = getStreamsResponse?.Streams[0]?.ViewerCount ?? 0 });
        }
        private void NotifyStartRaidResponse(string ToChannelName, StartRaidResponse response)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Posting the start of a raid to another channel through the 'StartRaidEventResponse' event.");

            StartRaidEventResponse?.Invoke(this, new()
            {
                ToChannel = ToChannelName,
                CreatedAt = response.Data[0].CreatedAt,
                IsMature = response.Data[0].IsMature
            });
        }
        private void NotifyCancelRaid()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchBotUserSvc, "Posting to cancel current raid through 'CancelRaidEvent' event.");

            CancelRaidEvent?.Invoke(this, new());
        }

        #endregion
    }
}
