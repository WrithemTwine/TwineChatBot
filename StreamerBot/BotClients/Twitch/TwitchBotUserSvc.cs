using StreamerBot.BotClients.Twitch.TwitchLib;
using StreamerBot.Enums;
using StreamerBot.Events;
using StreamerBot.Properties;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
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
                if (IsStarted)
                {
                    userLookupService.Stop();
                }

                RefreshSettings();
                ApiSettings apiclip = new() { AccessToken = TwitchToken ?? TwitchAccessToken, ClientId = ClientName ?? TwitchClientID };
                userLookupService = new(new TwitchAPI(null, null, apiclip, null), (int)Math.Round(TwitchFrequencyClipTime, 0));

                IsInitalized = true;
            }
        }

        public string GetUserGameCategoryId(string UserId)
        {
            ChannelInformation channelInformation = GetUserInfoId(UserId)?.Data[0];
            string gameName = channelInformation.GameName ?? "N/A";
            string gameId = channelInformation.GameId ?? "N/A";

            PostEvent_GetChannelGameName(gameName, gameId);

            return gameName;
        }

        public string GetUserGameCategoryName(string UserName)
        {
            ChannelInformation channelInformation = GetUserInfoName(UserName)?.Data[0];

            string gameName = channelInformation.GameName ?? "N/A";
            string gameId = channelInformation.GameId ?? "N/A";

            PostEvent_GetChannelGameName(gameName, gameId);

            return gameName;
        }

        private void PostEvent_GetChannelGameName(string foundGameName, string foundGameId)
        {
            GetChannelGameName?.Invoke(this, new OnGetChannelGameNameEventArgs() { GameName = foundGameName, GameId = foundGameId });
        }

        public GetChannelInformationResponse GetUserInfoId(string UserId)
        {
            if (!IsInitalized)
            {
                ConnectUserService();
            }

            GetChannelInformationResponse user = userLookupService.GetChannelInformation(UserId: UserId).Result;
            return user;
        }

        public GetChannelInformationResponse GetUserInfoName(string UserName)
        {
            if (!IsInitalized)
            {
                ConnectUserService();
            }

            GetChannelInformationResponse user = userLookupService.GetChannelInformation(UserName: UserName).Result;
            return user;
        }

        public List<string> GetUserCustomRewardsId(string UserId)
        {
            GetCustomRewardsResponse getCustom = GetCustomRewardsId(UserId);

            List<string> CustomRewardsList = new();
            CustomRewardsList.AddRange(getCustom.Data.Select(cr => cr.Title));
            PostEvent_GetCustomRewards(CustomRewardsList);

            return CustomRewardsList;
        }

        public List<string> GetUserCustomRewardsName(string UserName)
        {
            GetCustomRewardsResponse getCustom = GetCustomRewardsName(UserName);

            List<string> CustomRewardsList = new();
            CustomRewardsList.AddRange(getCustom.Data.Select(cr => cr.Title));
            PostEvent_GetCustomRewards(CustomRewardsList);

            return CustomRewardsList;
        }

        private void PostEvent_GetCustomRewards(List<string> CustomRewardsList)
        {
            GetChannelPoints?.Invoke(this, new OnGetChannelPointsEventArgs() { ChannelPointNames = CustomRewardsList });
        }

        public GetCustomRewardsResponse GetCustomRewardsId(string UserId)
        {
            if (!IsInitalized)
            {
                ConnectUserService();
            }

            GetCustomRewardsResponse points = userLookupService.GetChannelPointInformation(UserId: UserId).Result;
            return points;
        }

        public GetCustomRewardsResponse GetCustomRewardsName(string UserName)
        {
            if (!IsInitalized)
            {
                ConnectUserService();
            }

            GetCustomRewardsResponse points = userLookupService.GetChannelPointInformation(UserName: UserName).Result;
            return points;
        }

        public async Task<string> GetUserId(string UserName)
        {
            if (!IsInitalized)
            {
                ConnectUserService();
            }

            return await userLookupService.GetUserId(UserName);
        }

    }
}
