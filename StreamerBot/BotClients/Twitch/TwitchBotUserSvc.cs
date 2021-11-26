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
            string result = GetUserInfoId(UserId)?.Data[0].GameName ?? "N/A";

            PostEvent_GetChannelGameName(result);

            return result;
        }

        public string GetUserGameCategoryName(string UserName)
        {
            string result = GetUserInfoName(UserName)?.Data[0].GameName ?? "N/A";

            PostEvent_GetChannelGameName(result);

            return result;
        }

        private void PostEvent_GetChannelGameName(string foundGameName)
        {
            GetChannelGameName?.Invoke(this, new OnGetChannelGameNameEventArgs() { GameName = foundGameName });
        }

        public GetChannelInformationResponse GetUserInfoId(string UserId)
        {
            if (!IsInitalized) ConnectUserService();

            //userLookupService.Start();
            GetChannelInformationResponse user = userLookupService.GetChannelInformation(UserId: UserId).Result;
            //userLookupService.Stop();
            
            return user;
        }

        public GetChannelInformationResponse GetUserInfoName(string UserName)
        {
            if (!IsInitalized) ConnectUserService();

            //userLookupService.Start();
            GetChannelInformationResponse user = userLookupService.GetChannelInformation(UserName: UserName).Result;
            //userLookupService.Stop();

            return user;
        }
    }
}
