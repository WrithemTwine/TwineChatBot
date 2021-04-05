﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Core;
using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace ChatBot_Net5.Clients
{
    internal class FullFollowerService : FollowerService
    {
        public FullFollowerService(ITwitchAPI api, int checkIntervalInSeconds = 60, int queryCountPerRequest = 100, int cacheSize = 300) : base(api, checkIntervalInSeconds, queryCountPerRequest, cacheSize)
        { 
        }

        public async Task<List<Follow>> GetAllFollowers(string ChannelName)
        {
            Users followers = new(_api.Settings, new BypassLimiter(), new TwitchWebRequest());

            List<Follow> allfollows = new List<Follow>();

            string channelId = (await _api.Helix.Users.GetUsersAsync(logins: new List<string> { ChannelName })).Users.FirstOrDefault()?.Id;

            GetUsersFollowsResponse followsResponse = null;

            while (followsResponse == null)
            {
                try
                {
                    followsResponse = await followers.GetUsersFollowsAsync(first: 100, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    followsResponse = await followers.GetUsersFollowsAsync(first: 100, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }

            }

            while(followsResponse?.Follows.Length == 100)
            {
                try
                {
                    followsResponse = await followers.GetUsersFollowsAsync(after: followsResponse.Pagination.Cursor, first: 100, toId: channelId);
                    allfollows.AddRange(followsResponse.Follows);
                }
                catch (Exception ex)
                {

                }
            }

            return allfollows;
        }

    }
}