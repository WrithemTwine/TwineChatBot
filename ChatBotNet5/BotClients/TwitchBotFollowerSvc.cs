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

        /// <summary>
        /// Constructs the Follower Service bot
        /// </summary>
        public TwitchBotFollowerSvc()
        {
            ChatClientName = "TwitchFollowerService";
        }

        /// <summary>
        /// Establish all of the services attached to this Twitch client.
        /// </summary>
        internal void ConnectFollowerService()
        {
            if(IsStarted)
            {
                FollowerService.Stop();
            }
            // TODO: consider follower check time different between stream is live and not live
            RefreshSettings();
            FollowerService = new ExtFollowerService(new TwitchAPI(null, null, new ApiSettings() { AccessToken = TwitchAccessToken, ClientId = TwitchClientID }, null), (int)Math.Round(TwitchFrequencyFollowerTime, 0));
            FollowerService.SetChannelsByName(new List<string>() { TwitchChannelName });
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
            catch { return false; }
        }

        /// <summary>
        /// Stop all of the services attached to the client, but the app is still running.
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
            catch { return false; }
        }

        /// <summary>
        /// Asynchronous call to get all followers for the channel.
        /// </summary>
        /// <returns>The list of 'Follow' objects for the monitored channel.</returns>
        internal async Task<List<Follow>> GetAllFollowersAsync()
        {
            return await FollowerService.GetAllFollowers(TwitchChannelName);
        }

        /// <summary>
        /// As the application exits, this stops the bot.
        /// </summary>
        /// <returns></returns>
        public override bool ExitBot()
        {
            FollowerService?.Stop();
            FollowerService = null;
            return base.ExitBot();
        }

        /// <summary>
        /// Performs creating a follow to the provided user, and the follow is currently to the bot account. The authentication token is for the bot account, and that is where the create follow must occur until another option is available to implement following for the streaming channel <- will require a separate token for the streaming channel username.
        /// </summary>
        /// <param name="ToUserName">The Twitch user to follow.</param>
        internal void FollowBack(string ToUserName)
        {
            new Thread(new ThreadStart(() =>
            {
                FollowerService.CreateUserFollowerName(TwitchBotUserName, ToUserName, OptionFlags.TwitchAddFollowerNotification);
            })).Start();
        }
    }
}
