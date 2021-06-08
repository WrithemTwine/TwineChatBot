using ChatBot_Net5.Clients.TwitchLib;
using ChatBot_Net5.Data;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Clients
{
    public class IOModuleTwitch_FollowerSvc : IOModule
    {
        /// <summary>
        /// Listens for new followers.
        /// </summary>
        internal ExtFollowerService FollowerService { get; private set; }

        public IOModuleTwitch_FollowerSvc()
        {
            ChatClientName = "TwitchFollowerService";
        }

        /// <summary>
        /// Establish all of the services attached to this Twitch client.
        /// </summary>
        internal void ConnectFollowerService()
        {
            RefreshSettings();
            ApiSettings apifollow = new() { AccessToken = TwitchAccessToken, ClientId = TwitchClientID };
            FollowerService = new ExtFollowerService(new TwitchAPI(null, null, apifollow, null), (int)Math.Round(TwitchFrequencyFollowerTime, 0));
            FollowerService.SetChannelsByName(new List<string>() { TwitchChannelName });
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        public override bool StartBot()
        {
            ConnectFollowerService();
            FollowerService?.Start();
            IsStarted = true;
            IsStopped = false;
            InvokeBotStarted();
            return true;
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override bool StopBot()
        {
            if (!IsStopped)
            {
                FollowerService?.Stop();
                IsStarted = false;
                IsStopped = true;
                InvokeBotStopped();
                FollowerService = null;
            }
            return true;
        }

        internal async Task<List<Follow>> GetAllFollowersAsync()
        {
            return await FollowerService.GetAllFollowers(TwitchChannelName);
        }

        public override bool ExitBot()
        {
            FollowerService?.Stop();
            FollowerService = null;
            return base.ExitBot();
        }

        internal void FollowBack(string ToUserName)
        {
            new Thread(new ThreadStart(() =>
           {
               FollowerService.CreateUserFollowerName(TwitchChannelName, ToUserName, OptionFlags.TwitchAddFollowerNotification);
           })).Start();
        }
    }
}
