using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ExtFollowerService : FollowerService
    {
        public ExtFollowerService(ITwitchAPI api, int checkIntervalInSeconds = 60, int queryCountPerRequest = 100, int cacheSize = 300) : base(api, checkIntervalInSeconds, queryCountPerRequest, cacheSize)
        {
        }

        /// <summary>
        /// Retrieves all followers for the streamer/watched channel, and is asynchronous
        /// </summary>
        /// <param name="ChannelName">The channel to retrieve the followers</param>
        /// <returns>An async task with a list of 'Follow' objects.</returns>
        public async Task<List<Follow>> GetAllFollowersAsync(string ChannelName)
        {
            List<Follow> allfollows = new();

            try
            {
                int MaxFollowers = 100; // use the max for bulk retrieve
                Users followers = new(_api.Settings, new BypassLimiter(), new TwitchHttpClient());
                string channelId = (await _api.Helix.Users.GetUsersAsync(logins: new() { ChannelName })).Users.FirstOrDefault()?.Id;

                GetUsersFollowsResponse followsResponse = await followers.GetUsersFollowsAsync(first: MaxFollowers, toId: channelId);

                allfollows.AddRange(followsResponse.Follows);

                while (followsResponse?.Follows.Length == MaxFollowers && followsResponse?.Pagination.Cursor != null) // loop until the last response is less than 100; each retrieval provides 100 items at a time
                {
                    followsResponse = await followers.GetUsersFollowsAsync(after: followsResponse.Pagination.Cursor, first: MaxFollowers, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }

            return allfollows;
        }
    }
}

