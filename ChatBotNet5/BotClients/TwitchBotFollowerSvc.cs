using ChatBot_Net5.BotClients.TwitchLib;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.BotClients
{
    public class TwitchBotFollowerSvc : TwitchBots
    {
        /// <summary>
        /// Listens for new followers.
        /// </summary>
        internal ExtFollowerService FollowerService { get; private set; }

        public TwitchBotFollowerSvc()
        {
            BotClientName = "TwitchFollowerService";
        }

        /// <summary>
        /// Establish all of the services attached to this Twitch client. Override parameters allow connecting to another stream, such as directly to the streamer channel because any actions to add 'followers' will fail if not directly connected.
        /// </summary>
        /// <param name="ClientName">Override the Twitch Bot account name, another name used for connecting to a different channel (such as directly to streamer)</param>
        /// <param name="TwitchToken">Override the Twitch Bot token used and to connect to the <para>ClientName</para> channel with a specific token just for changing followers.</param>
        internal void ConnectFollowerService(string ClientName = null, string TwitchToken = null )
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
                ConnectFollowerService();
                FollowerService?.Start();
                IsStarted = true;
                IsStopped = false;
                InvokeBotStarted();

                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        public override bool StopBot()
        {
            try
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
            catch { }
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

        public void FollowBack(string ToUserName)
        {
            new Thread(new ThreadStart(() =>
            {
                FollowerService.CreateUserFollowerName(TwitchChannelName, ToUserName, OptionFlags.TwitchAddFollowerNotification);
            })).Start();
        }
    }
}
