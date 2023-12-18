using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Raids.StartRaid;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
        private static TwitchTokenBot twitchTokenBot;
        private UserLookupService userLookupService;

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
        }

        #region Token Bot ops
        /// <summary>
        /// Sets the Twitch Token bot used for the automatic refreshing access token.
        /// </summary>
        /// <param name="tokenBot">An instance of the token bot, to use the same token bot across chat bots.</param>
        internal override void SetTokenBot(TwitchTokenBot tokenBot)
        {
            twitchTokenBot = tokenBot;
            twitchTokenBot.BotAccessTokenChanged += TwitchTokenBot_BotAccessTokenChanged;
            twitchTokenBot.StreamerAccessTokenChanged += TwitchTokenBot_StreamerAccessTokenChanged;
        }

        private void TwitchTokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            ChooseConnectUserService();
        }

        private void TwitchTokenBot_BotAccessTokenChanged(object sender, EventArgs e)
        {
            ChooseConnectUserService();
        }

        #endregion

        /// <summary>
        /// Aware of whether to use the bot user client Id or streamer client Id, due to API calls requiring the client Id of the streaming channel to retrieve the data.
        /// </summary>
        /// <param name="UseStreamToken">Specify whether the Streamer's Token is required to access Channel Data</param>
        private void ChooseConnectUserService(bool UseStreamToken = false)
        {
            string SettingsClientId;
            string ClientId;
            string OauthToken;

            // verify and, if necessary, refresh the access tokens

            if (OptionFlags.TwitchStreamerUseToken && UseStreamToken)
            {
                SettingsClientId = "TwitchStreamClientId";
                ClientId = OptionFlags.TwitchStreamClientId;
                OauthToken = TwitchStreamerAccessToken;
            }
            else
            {
                SettingsClientId = "TwitchClientID";
                ClientId = OptionFlags.TwitchBotClientId;
                OauthToken = TwitchAccessToken;
            }

            if (!OptionFlags.CheckSettingIsDefault(SettingsClientId, ClientId))
            {
                userLookupService = null;

                ApiSettings api = new() { AccessToken = OauthToken, ClientId = ClientId };
                userLookupService = new(new TwitchAPI(null, null, api, null));
            }
        }

        public void SetIds(string StreamerChannelId = null, string BotChannelId = null)
        {
            if (StreamerChannelId != null)
            {
                TwitchChannelId = StreamerChannelId;
            }

            if (BotChannelId != null)
            {
                TwitchBotUserId = BotChannelId;
            }

            if (TwitchChannelId == null && TwitchBotUserId == null && TwitchChannelName != null)
            {
                TwitchBotUserId = GetUserId(TwitchBotUserName);
                TwitchChannelId = GetUserId(TwitchChannelName);
            }
        }

        #region ClientId can be different between Bot and Channel

        public void BanUser(string BannedUserName, BanReasons banReason, int Duration = 0)
        {
            try
            {
                ChooseConnectUserService();
                SetIds();
                _ = userLookupService.BanUser(UserName: BannedUserName, forDuration: Duration, banReason: banReason)?.Result;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService();
                SetIds();
                _ = userLookupService.BanUser(UserName: BannedUserName, forDuration: Duration, banReason: banReason)?.Result;
            }
        }

        /// <summary>
        /// Retrieves the Game Category for a channel. Performs event <code>Post_GetChannelGameName</code> when <paramref name="UserName"/> equals the TwitchChannelName.
        /// </summary>
        /// <param name="UserId">References the Channel to get the Game Category.</param>
        /// <param name="UserName">References the Channel, converts to UserId, to get the Game Category.</param>
        /// <returns>The Game Category for the requested channel.</returns>
        public string GetUserGameCategory(string UserId = null, string UserName = null)
        {
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
            try
            {

                ChooseConnectUserService();
                GetChannelInformationResponse user = userLookupService.GetChannelInformationAsync(UserId: UserId, UserName: UserName)?.Result;
                return user;

            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService();
                GetChannelInformationResponse user = userLookupService.GetChannelInformationAsync(UserId: UserId, UserName: UserName)?.Result;
                return user;

            }
        }

        public string GetUserId(string UserName)
        {
            try
            {
                ChooseConnectUserService();
                string result = userLookupService.GetUserId(UserName)?.Result;
                return result;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService();
                string result = userLookupService.GetUserId(UserName)?.Result;
                return result;
            }
        }

        public string GetGameId(string GameName)
        {
            try
            {
                ChooseConnectUserService();
                return userLookupService.GetGameId(GameName: new() { GameName }).Result.Games[0].Id;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService();
                return userLookupService.GetGameId(GameName: new() { GameName }).Result.Games[0].Id;
            }
        }

        public void GetViewerCount(string UserName)
        {
            try
            {
                ChooseConnectUserService();
                GetStreamsResponse getStreamsResponse = userLookupService.GetStreams(UserName: UserName).Result;
                GetStreamsViewerCount?.Invoke(this, new() { ViewerCount = getStreamsResponse?.Streams[0]?.ViewerCount ?? 0 });

            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService();
                GetStreamsResponse getStreamsResponse = userLookupService.GetStreams(UserName: UserName).Result;
                GetStreamsViewerCount?.Invoke(this, new() { ViewerCount = getStreamsResponse?.Streams[0]?.ViewerCount ?? 0 });
            }
        }

        public DateTime GetUserCreatedAt(string UserName = null, string UserId = null)
        {
            try
            {
                ChooseConnectUserService();
                DateTime result = userLookupService.GetUserCreatedAt(UserName, UserId).Result;
                return result;

            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService();
                DateTime result = userLookupService.GetUserCreatedAt(UserName, UserId).Result;
                return result;
            }
        }

        #endregion

        #region StreamerChannel Client Id and Request UserId must be the same

        public GetCustomRewardsResponse GetCustomRewardsId(string UserId = null, string UserName = null)
        {
            GetCustomRewardsResponse points = null;
            try
            {
                ChooseConnectUserService(true);
                points = userLookupService?.GetChannelPointInformationAsync(UserId: UserId, UserName: UserName).Result;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService(true);
                points = userLookupService?.GetChannelPointInformationAsync(UserId: UserId, UserName: UserName).Result;
            }

            return points;
        }

        public List<string> GetUserCustomRewards(string UserId = null, string UserName = null)
        {
            GetCustomRewardsResponse getCustom = GetCustomRewardsId(UserId: UserId, UserName: UserName);

            List<string> CustomRewardsList = new();
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
            bool result;

            try
            {
                ChooseConnectUserService(true);
                _ = userLookupService?.ModifyChannelInformation(TwitchChannelId, Title: Title);
                result = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService(true);
                _ = userLookupService?.ModifyChannelInformation(TwitchChannelId, Title: Title);
                result = true;
            }

            return result;
        }

        public bool SetChannelCategory(string CategoryName = null, string CategoryId = null)
        {
            bool result;

            if (CategoryId == null)
            {
                CategoryId = GetGameId(CategoryName);
            }
            try
            {
                ChooseConnectUserService(true);
                _ = userLookupService?.ModifyChannelInformation(TwitchChannelId, GameId: CategoryId);
                result = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService(true);
                _ = userLookupService?.ModifyChannelInformation(TwitchChannelId, GameId: CategoryId);
                result = true;
            }
            return result;
        }

        public void RaidChannel(string ToChannelName)
        {
            if (ToChannelName == null)
            {
                return;
            }
            else
            {
                try
                {
                    ChooseConnectUserService(true);
                    OptionFlags.TwitchOutRaidStarted = true;
                    StartRaidResponse response = userLookupService?.StartRaid(TwitchChannelId, ToUserName: ToChannelName).Result;
                    if (response != null)
                    {
                        StartRaidEventResponse?.Invoke(this, new()
                        {
                            ToChannel = ToChannelName,
                            CreatedAt = response.Data[0].CreatedAt,
                            IsMature = response.Data[0].IsMature
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                    twitchTokenBot.CheckToken();

                    ChooseConnectUserService(true);
                    OptionFlags.TwitchOutRaidStarted = true;
                    StartRaidResponse response = userLookupService?.StartRaid(TwitchChannelId, ToUserName: ToChannelName).Result;
                    if (response != null)
                    {
                        StartRaidEventResponse?.Invoke(this, new()
                        {
                            ToChannel = ToChannelName,
                            CreatedAt = response.Data[0].CreatedAt,
                            IsMature = response.Data[0].IsMature
                        });
                    }
                }
            }
        }

        public void CancelRaidChannel()
        {
            try
            {
                ChooseConnectUserService(true);
                userLookupService?.CancelRaid(TwitchChannelId);
                CancelRaidEvent?.Invoke(this, new());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                twitchTokenBot.CheckToken();

                ChooseConnectUserService(true);
                userLookupService?.CancelRaid(TwitchChannelId);
                CancelRaidEvent?.Invoke(this, new());
            }
        }

        #endregion

        #region process events

        private void PostEvent_GetChannelGameName(string foundGameName, string foundGameId)
        {
            GetChannelGameName?.Invoke(this, new OnGetChannelGameNameEventArgs() { GameName = foundGameName, GameId = foundGameId });
        }

        private void PostEvent_GetCustomRewards(List<string> CustomRewardsList)
        {
            GetChannelPoints?.Invoke(this, new OnGetChannelPointsEventArgs() { ChannelPointNames = CustomRewardsList });
        }

        #endregion
    }
}
