﻿using StreamerBot.BotClients.Twitch.TwitchLib;
using StreamerBot.Enums;
using StreamerBot.Events;
using StreamerBot.Properties;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.ChannelPoints.GetCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
        private UserLookupService userLookupService;
        private bool IsInitalized;

        public event EventHandler<OnGetChannelGameNameEventArgs> GetChannelGameName;
        public event EventHandler<OnGetChannelPointsEventArgs> GetChannelPoints;

        public TwitchBotUserSvc()
        {
            BotClientName = Bots.TwitchUserBot;
        }

        // TODO: implement !setcategory including the API calls
        // TODO: implement !settitle including the API calls

        public void ConnectUserService(string ClientName = null, string TwitchToken = null)
        {
            DefaultSettingValueAttribute defaultSetting = null;

            foreach (MemberInfo m in from MemberInfo m in typeof(Settings).GetProperties()
                                     where m.Name == "TwitchClientID"
                                     select m)
            {
                defaultSetting = (DefaultSettingValueAttribute)m.GetCustomAttribute(typeof(DefaultSettingValueAttribute));
            }

            if (Settings.Default.TwitchClientID != defaultSetting.Value)
            {
                userLookupService = null;

                RefreshSettings();
                ApiSettings api = new() { AccessToken = TwitchToken ?? TwitchAccessToken, ClientId = ClientName ?? TwitchClientID };
                userLookupService = new(new TwitchAPI(null, null, api, null), (int)Math.Round(TwitchFrequencyClipTime, 0));
            }
        }

        public string GetUserGameCategory(string UserId = null, string UserName = null)
        {
            ChannelInformation channelInformation = GetUserInfo(UserId: UserId, UserName: UserName)?.Data[0];
            string gameName = channelInformation.GameName ?? "N/A";
            string gameId = channelInformation.GameId ?? "N/A";

            PostEvent_GetChannelGameName(gameName, gameId);

            return gameName;
        }

        private void PostEvent_GetChannelGameName(string foundGameName, string foundGameId)
        {
            GetChannelGameName?.Invoke(this, new OnGetChannelGameNameEventArgs() { GameName = foundGameName, GameId = foundGameId });
        }

        public GetChannelInformationResponse GetUserInfo(string UserId = null, string UserName = null)
        {
            ConnectUserService();
            GetChannelInformationResponse user = userLookupService.GetChannelInformation(UserId: UserId, UserName: UserName).Result;
            return user;
        }

        public string GetUserId(string UserName)
        {
            ConnectUserService();
            string result = userLookupService.GetUserId(UserName).Result;
            return result;
        }

        #region StreamerChannel Client Id and Request UserId must be the same
        /// <summary>
        /// Aware of whether to use the bot user client Id or streamer client Id, due to API calls requiring the client Id of the streaming channel to retrieve the data.
        /// </summary>
        private void ChooseConnectUserService()
        {
            string SettingsClientId = "";
            string ClientId = "";
            string OauthToken = "";

            RefreshSettings();

            if (OptionFlags.TwitchStreamerUseToken)
            {
                SettingsClientId = "TwitchStreamClientId";
                ClientId = OptionFlags.TwitchStreamClientId;
                OauthToken = OptionFlags.TwitchStreamOauthToken;
            }
            else
            {
                SettingsClientId = "TwitchClientID";
                ClientId = Settings.Default.TwitchClientID;
                OauthToken = Settings.Default.TwitchAccessToken;
            }


            DefaultSettingValueAttribute defaultSetting = null;

            foreach (MemberInfo m in from MemberInfo m in typeof(Settings).GetProperties()
                                     where m.Name == SettingsClientId
                                     select m)
            {
                defaultSetting = (DefaultSettingValueAttribute)m.GetCustomAttribute(typeof(DefaultSettingValueAttribute));
            }

            if (ClientId != defaultSetting.Value)
            {
                userLookupService = null;

                ApiSettings api = new() { AccessToken = OauthToken, ClientId = ClientId };
                userLookupService = new(new TwitchAPI(null, null, api, null));
            }
        }

        public List<string> GetUserCustomRewards(string UserId = null, string UserName = null)
        {
            GetCustomRewardsResponse getCustom = GetCustomRewardsId(UserId: UserId, UserName: UserName);

            List<string> CustomRewardsList = new();
            CustomRewardsList.AddRange(getCustom.Data.Select(cr => cr.Title));
            CustomRewardsList.Sort();
            PostEvent_GetCustomRewards(CustomRewardsList);

            return CustomRewardsList;
        }

        private void PostEvent_GetCustomRewards(List<string> CustomRewardsList)
        {
            GetChannelPoints?.Invoke(this, new OnGetChannelPointsEventArgs() { ChannelPointNames = CustomRewardsList });
        }

        public GetCustomRewardsResponse GetCustomRewardsId(string UserId = null, string UserName = null)
        {
            ChooseConnectUserService();

            GetCustomRewardsResponse points = userLookupService.GetChannelPointInformation(UserId: UserId, UserName: UserName).Result;
            return points;
        }

        #endregion
    }
}
