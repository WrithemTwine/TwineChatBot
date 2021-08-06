using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace ChatBot_Net5.BotClients.TwitchLib
{
    internal class ExtFollowerService : FollowerService
    {
        public ExtFollowerService(ITwitchAPI api, int checkIntervalInSeconds = 60, int queryCountPerRequest = 100, int cacheSize = 300) : base(api, checkIntervalInSeconds, queryCountPerRequest, cacheSize)
        {
        }

        /// <summary>
        /// Retrieves all followers for the streamer/watched channel, and is asynchronous
        /// </summary>
        /// <param name="ChannelName">The channel to retrieve the followers</param>
        /// <returns>An async task with a list of 'Follow' objects.</returns>
        public async Task<List<Follow>> GetAllFollowers(string ChannelName)
        {
            Users followers = new(_api.Settings, new BypassLimiter(), new TwitchWebRequest());

            List<Follow> allfollows = new();
            
            string channelId = (await _api.Helix.Users.GetUsersAsync(logins: new() { ChannelName })).Users.FirstOrDefault()?.Id;

            GetUsersFollowsResponse followsResponse = null;

            while (followsResponse == null) // loop until getting a response
            {
                try
                {
                    followsResponse = await followers.GetUsersFollowsAsync(first: 100, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }
                catch { }
                finally
                {
                    followsResponse = await followers.GetUsersFollowsAsync(first: 100, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }

            }

            while (followsResponse?.Follows.Length == 100) // loop until the last response is less than 100; each retrieval provides 100 items at a time
            {
                try
                {
                    followsResponse = await followers.GetUsersFollowsAsync(after: followsResponse.Pagination.Cursor, first: 100, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }
                catch { }
            }

            return allfollows;
        }

        /// <summary>
        /// Create the follow based on the user IDs
        /// </summary>
        /// <param name="from_id">The ID to follow another channel</param>
        /// <param name="to_id">The ID of the channel to follow</param>
        /// <param name="allownotification">Specifies whether to notify when the channel goes live</param>
        public async void CreateUserFollowerId(string from_id, string to_id, bool? allownotification = null)
        {
            Users followers = new(_api.Settings, new BypassLimiter(), new TwitchWebRequest());

            // get user followers to the other channel
            GetUsersFollowsResponse followsResponse = await followers.GetUsersFollowsAsync(fromId: from_id, toId: to_id);

            if (followsResponse.Follows.Length == 0) // if not following, create the follow
            {
                try
                {
                    // add the follow
                    await followers.CreateUserFollows(from_id, to_id, allownotification);
                }
                catch { }
            }
            return;
        }

        /// <summary>
        /// Accepts the user names for the 'from' and 'to' and will convert them to IDs for creating the user follow
        /// </summary>
        /// <param name="from_username">The username wanting to follow another channel</param>
        /// <param name="to_username">The username of the channel to follow</param>
        /// <param name="allownotification">Specifies whether to enable notifications when the channel goes live</param>
        public async void CreateUserFollowerName(string from_username, string to_username, bool? allownotification = null)
        {
            string fromid = (await _api.Helix.Users.GetUsersAsync(logins: new() { from_username })).Users.FirstOrDefault()?.Id;
            string toid = (await _api.Helix.Users.GetUsersAsync(logins: new() { to_username })).Users.FirstOrDefault()?.Id;
            CreateUserFollowerId(fromid, toid, allownotification);
            return;
        }

    }
}

