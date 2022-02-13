﻿using StreamerBot.BotClients.Twitch.TwitchLib;
using StreamerBot.Enums;
using StreamerBot.Events;
using StreamerBot.Properties;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
        private UserLookupService userLookupService;

        public event EventHandler<OnGetChannelGameNameEventArgs> GetChannelGameName;
        public event EventHandler<OnGetChannelPointsEventArgs> GetChannelPoints;

        public TwitchBotUserSvc()
        {
            BotClientName = Bots.TwitchUserBot;
        }

        // TODO: implement !setcategory including the API calls
        // TODO: implement !settitle including the API calls

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

            if(!OptionFlags.CheckSettingIsDefault(SettingsClientId, ClientId))
            {
                userLookupService = null;

                ApiSettings api = new() { AccessToken = OauthToken, ClientId = ClientId };
                userLookupService = new(new TwitchAPI(null, null, api, null));
            }
        }

        #region ClientId can be different between Bot and Channel

        public string GetUserGameCategory(string UserId = null, string UserName = null)
        {
            ChannelInformation channelInformation = GetUserInfo(UserId: UserId, UserName: UserName)?.Data[0];
            string gameName = channelInformation?.GameName ?? "N/A";
            string gameId = channelInformation?.GameId ?? "N/A";

            PostEvent_GetChannelGameName(gameName, gameId);

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
