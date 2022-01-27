using StreamerBot.BotClients.Twitch.TwitchLib;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Properties;

using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotUserSvc : TwitchBotsBase
    {
        private UserLookupService userLookupService;
        private bool IsInitalized;

        public event EventHandler<OnGetChannelGameNameEventArgs> GetChannelGameName;

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
    }
}
