using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotFollowerSvc : TwitchBotsBase
    {
        /// <summary>
        /// Registers if service restart is from the access token is refreshed
        /// </summary>
        public bool RestartRefreshAccessToken { get; private set; } = false;

        /// <summary>
        /// Listens for new followers.
        /// </summary>
        public ExtFollowerService FollowerService { get; private set; }

        public TwitchBotFollowerSvc()
        {
            BotClientName = Bots.TwitchFollowBot;
            //twitchTokenBot.BotAccessTokenChanged += TwitchTokenBot_BotAccessTokenChanged;
        }

        private void TwitchTokenBot_BotAccessTokenChanged(object sender, EventArgs e)
        {
            FollowerService?.UpdateAccessToken(TwitchAccessToken);
        }

        /// <summary>
        /// Establish all of the services attached to this Twitch client. Override parameters allow connecting to another stream, such as directly to the streamer channel because any actions to add 'followers' will fail if not directly connected.
        /// </summary>
        /// <param name="ClientName">Override the Twitch Bot account name, another name used for connecting to a different channel (such as directly to streamer)</param>
        /// <param name="TwitchToken">Override the Twitch Bot token used and to connect to the <para>ClientName</para> channel with a specific token just for changing followers.</param>
        private void ConnectFollowerService(string ClientName = null, string TwitchToken = null)
        {
            if (FollowerService == null)
            {
                FollowerService = new ExtFollowerService(
                     new TwitchAPI(settings: new ApiSettings() { AccessToken = TwitchAccessToken, ClientId = TwitchClientID }),
                    (int)Math.Round(TwitchFrequencyFollowerTime, 0));

                FollowerService.SetChannelsByName([ClientName ?? TwitchChannelName]);

                // check if http access unauthorized exception; usually means expired access token
                FollowerService.AccessTokenUnauthorized += FollowerService_AccessTokenUnauthorized;
            }
            else
            {
                FollowerService.UpdateAccessToken(TwitchAccessToken);
            }
        }

        private void FollowerService_AccessTokenUnauthorized(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchFollowBot, "Checking tokens.");
            twitchTokenBot.CheckToken();
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
                    IsStarted = true;
                    FollowerService?.Start();
                    RestartRefreshAccessToken = true; // record reason is from refreshing token
                    IsStopped = false;
                    InvokeBotStarted();
                }
                return true;
            }
            catch (BadRequestException)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                twitchTokenBot.CheckToken();
                return false;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                IsStarted = false;
                IsStopped = true;
                InvokeBotFailedStart();
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
                    RestartRefreshAccessToken = false;
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

        /// <summary>
        /// Retrieve all followers to the streamer's channel and add to the database.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetAllFollowersBulkAsync()
        {
            return await FollowerService.GetAllFollowersBulkAsync(TwitchChannelName);
        }

        /// <summary>
        /// Retrieve all followers for the provided channel name.
        /// </summary>
        /// <returns>A list of all new followers to the streamer's monitored channel, since last checked.</returns>
        public async Task<List<ChannelFollower>> GetAllFollowersAsync()
        {
            return await FollowerService.GetAllFollowersAsync(TwitchChannelName);
        }

        /// <summary>
        /// App level command to stop all bots for exiting the application.
        /// </summary>
        /// <returns></returns>
        public override bool ExitBot()
        {
            if (IsStarted)
            {
                StopBot();
            }
            return base.ExitBot();
        }
    }
}
