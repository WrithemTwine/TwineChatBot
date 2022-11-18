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
using TwitchLib.Api.Helix.Models.Schedule;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.PubSub.Models.Responses;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
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

            RefreshSettings();
        }

        /// <summary>
        /// Aware of whether to use the bot user client Id or streamer client Id, due to API calls requiring the client Id of the streaming channel to retrieve the data.
        /// </summary>
        /// <param name="UseStreamToken">Specify whether the Streamer's Token is required to access Channel Data</param>
        private void ChooseConnectUserService(bool UseStreamToken = false)
        {
            string SettingsClientId;
            string ClientId;
            string OauthToken;

            RefreshSettings();

            if (OptionFlags.TwitchStreamerUseToken && UseStreamToken)
            {
                SettingsClientId = "TwitchStreamClientId";
                ClientId = OptionFlags.TwitchStreamClientId;
                OauthToken = OptionFlags.TwitchStreamOauthToken;
            }
            else
            {
                SettingsClientId = "TwitchClientID";
                ClientId = OptionFlags.TwitchBotClientId;
                OauthToken = OptionFlags.TwitchBotAccessToken;
            }

            if (!OptionFlags.CheckSettingIsDefault(SettingsClientId, ClientId))
            {
                userLookupService = null;

                ApiSettings api = new() { AccessToken = OauthToken, ClientId = ClientId };
                userLookupService = new(new TwitchAPI(null, null, api, null));
            }
        }

        public void SetIds(string StreamerChannelId=null, string BotChannelId=null)
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
            ChooseConnectUserService();
            SetIds();
            _ = userLookupService.BanUser(UserName: BannedUserName, forDuration: Duration, banReason: banReason)?.Result;
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
            ChooseConnectUserService();
            GetChannelInformationResponse user = userLookupService.GetChannelInformationAsync(UserId: UserId, UserName: UserName)?.Result;
            return user;
        }

        public string GetUserId(string UserName)
        {
            ChooseConnectUserService();
            string result = userLookupService.GetUserId(UserName)?.Result;
            return result;
        }

        public string GetGameId(string GameName)
        {
            return userLookupService.GetGameId(GameName: new() { GameName }).Result.Games[0].Id;
        }

        public void GetViewerCount(string UserName)
        {
            ChooseConnectUserService();
            GetStreamsResponse getStreamsResponse = userLookupService.GetStreams(UserName: UserName).Result;
            GetStreamsViewerCount?.Invoke(this, new() { ViewerCount = getStreamsResponse?.Streams[0]?.ViewerCount ?? 0 });
        }

        #endregion

        #region StreamerChannel Client Id and Request UserId must be the same

        public GetCustomRewardsResponse GetCustomRewardsId(string UserId = null, string UserName = null)
        {
            ChooseConnectUserService(true);

            GetCustomRewardsResponse points = null;
            try
            {
                points = userLookupService?.GetChannelPointInformationAsync(UserId: UserId, UserName: UserName).Result;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
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

            ChooseConnectUserService(true);
            try
            {
                _ = userLookupService?.ModifyChannelInformation(TwitchChannelId, Title: Title);
                result = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                result = false;
            }

            return result;
        }

        public bool SetChannelCategory(string CategoryName = null, string CategoryId = null)
        {
            bool result;

            ChooseConnectUserService(true);

            if (CategoryId == null)
            {
                CategoryId = GetGameId(CategoryName);
            }
            try
            {
                _ = userLookupService?.ModifyChannelInformation(TwitchChannelId, GameId: CategoryId);
                result = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                result = false;
            }
            return result;
        }

        public void RaidChannel(string ToChannelName)
        {
            ChooseConnectUserService(true);

            if(ToChannelName == null)
            {
                return;
            } else
            {
                try
                {
                    OptionFlags.TwitchOutRaidStarted = true;
                    StartRaidResponse response = userLookupService?.StartRaid(TwitchChannelId, ToUserName: ToChannelName).Result;
                    if (response != null)
                    {
                        StartRaidEventResponse?.Invoke(this, new() { 
                            ToChannel = ToChannelName, 
                            CreatedAt = response.Data[0].CreatedAt, 
                            IsMature = response.Data[0].IsMature 
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        public void CancelRaidChannel()
        {
            ChooseConnectUserService(true);
            try
            {
                userLookupService?.CancelRaid(TwitchChannelId);
                CancelRaidEvent?.Invoke(this, new());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
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
