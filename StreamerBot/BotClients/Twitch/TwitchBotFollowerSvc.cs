using StreamerBot.BotClients.Twitch.TwitchLib;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotFollowerSvc : TwitchBotsBase
    {
        /// <summary>
        /// Listens for new followers.
        /// </summary>
        public ExtFollowerService FollowerService { get; private set; }

        public TwitchBotFollowerSvc()
        {
            BotClientName = Enum.Bots.TwitchFollowBot;
        }

        /// <summary>
        /// Establish all of the services attached to this Twitch client. Override parameters allow connecting to another stream, such as directly to the streamer channel because any actions to add 'followers' will fail if not directly connected.
        /// </summary>
        /// <param name="ClientName">Override the Twitch Bot account name, another name used for connecting to a different channel (such as directly to streamer)</param>
        /// <param name="TwitchToken">Override the Twitch Bot token used and to connect to the <para>ClientName</para> channel with a specific token just for changing followers.</param>
        public void ConnectFollowerService(string ClientName = null, string TwitchToken = null )
        {
            if(IsStarted)
            {
                FollowerService.Stop();
            }

            RefreshSettings();
            ApiSettings apifollow = new() { AccessToken = TwitchToken ?? TwitchAccessToken, ClientId = ClientName ?? TwitchClientID };
            FollowerService = new ExtFollowerService(new TwitchAPI(null, null, apifollow, null), (int)Math.Round(TwitchFrequencyFollowerTime, 0));
            FollowerService.SetChannelsByName(new List<string>() { ClientName ?? TwitchChannelName });
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override bool StartBot()
        {
            try
            {
                if (IsStopped || !IsStarted)
                {
                    ConnectFollowerService();
                    FollowerService?.Start();
                    IsStarted = true;
                    IsStopped = false;
                    InvokeBotStarted();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
            return false;
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override bool StopBot()
        {
            try
            {
                if (IsStarted)
                {
                    FollowerService?.Stop();
                    IsStarted = false;
                    IsStopped = true;
                    InvokeBotStopped();
                    FollowerService = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
            return false;
        }

        public async Task<List<Follow>> GetAllFollowersAsync()
        {
            return await FollowerService.GetAllFollowers(TwitchChannelName);
        }

        public override bool ExitBot()
        {
            FollowerService?.Stop();
            FollowerService = null;
            return base.ExitBot();
        }

        // Twitch.tv disabled this feature. https://blog.twitch.tv/en/2021/06/28/deprecation-of-create-and-delete-follows-api-endpoints/
        //public void FollowBack(string ToUserName)
        //{
        //    new Thread(new ThreadStart(() =>
        //    {
        //        FollowerService.CreateUserFollowerName(TwitchChannelName, ToUserName, OptionFlags.TwitchAddFollowerNotification);
        //    })).Start();
        //}
    }
}
