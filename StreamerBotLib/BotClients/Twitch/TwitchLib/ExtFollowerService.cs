using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services.Events.FollowerService;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ExtFollowerService : ExtApiService<GetChannelFollowersResponse>
    {
        public event EventHandler<OnNewFollowersDetectedArgs> OnBulkFollowsUpdate;

        private void BulkFollowsUpdate(string ChannelName, IEnumerable<ChannelFollower> follows)
        {
            OnBulkFollowsUpdate?.Invoke(this, new() { Channel = ChannelName, NewFollowers = new(follows) });
        }

        /// <summary>
        /// Retrieves all followers for the streamer/watched channel, and is asynchronous
        /// </summary>
        /// <param name="ChannelName">The channel to retrieve the followers</param>
        /// <returns>An async task with a list of 'Follow' objects.</returns>
        public async Task<bool> GetAllFollowersBulkAsync(string ChannelName)
        {
            try
            {
                int MaxFollowers = 100; // use the max for bulk retrieve
                Channels followers = new(_api.Settings, new BypassLimiter(), new TwitchHttpClient());
                string channelId = (await _api.Helix.Users.GetUsersAsync(logins: new() { ChannelName })).Users.FirstOrDefault()?.Id;

                GetChannelFollowersResponse followsResponse = await followers.GetChannelFollowersAsync(first: MaxFollowers, broadcasterId: channelId);

                BulkFollowsUpdate(ChannelName, followsResponse.Data);

                while (followsResponse?.Data.Length == MaxFollowers && followsResponse?.Pagination.Cursor != null) // loop until the last response is less than 100; each retrieval provides 100 items at a time
                {
                    followsResponse = await followers.GetChannelFollowersAsync(after: followsResponse.Pagination.Cursor, first: MaxFollowers, broadcasterId: channelId);
                    BulkFollowsUpdate(ChannelName, followsResponse.Data);
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }

            return true;
        }

        /// <summary>
        /// Retrieves all followers for the streamer/watched channel, and is asynchronous
        /// </summary>
        /// <param name="ChannelName">The channel to retrieve the followers</param>
        /// <returns>An async task with a list of 'Follow' objects.</returns>
        public async Task<List<ChannelFollower>> GetAllFollowersAsync(string ChannelName)
        {
            List<ChannelFollower> allfollows = new();

            try
            {
                int MaxFollowers = 100; // use the max for bulk retrieve
                Channels followers = new(_api.Settings, new BypassLimiter(), new TwitchHttpClient());
                string channelId = (await _api.Helix.Users.GetUsersAsync(logins: new() { ChannelName })).Users.FirstOrDefault()?.Id;

                GetChannelFollowersResponse followsResponse = await followers.GetChannelFollowersAsync(first: MaxFollowers, broadcasterId: channelId);

                allfollows.AddRange(followsResponse.Data);

                while (followsResponse?.Data.Length == MaxFollowers && followsResponse?.Pagination.Cursor != null) // loop until the last response is less than 100; each retrieval provides 100 items at a time
                {
                    followsResponse = await followers.GetChannelFollowersAsync(after: followsResponse.Pagination.Cursor, first: MaxFollowers, broadcasterId: channelId);
                    allfollows.AddRange(followsResponse.Data);
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }

            return allfollows;
        }

        // ----------------------------------------------------------------------------
        // from TwitchLib: https://github.com/TwitchLib/TwitchLib.Api/blob/2ea61c70225c0c15d7def7a5808837191e33d778/TwitchLib.Api/Services/FollowerService.cs
        // modified to fit into TwineStreamer bot code structure and handle internet disconnection related exceptions

        private readonly Dictionary<string, DateTime> _lastFollowerDates = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly bool _invokeEventsOnStartup;

        /// <summary>
        /// The current known followers for each channel.
        /// </summary>
        public Dictionary<string, List<ChannelFollower>> KnownFollowers { get; } = new Dictionary<string, List<ChannelFollower>>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The amount of followers queried per request.
        /// </summary>
        public int QueryCountPerRequest { get; }
        /// <summary>
        /// The maximum amount of followers cached per channel.
        /// </summary>
        public int CacheSize { get; }

        /// <summary>
        /// Event which is called when new followers are detected.
        /// </summary>
        public event EventHandler<OnNewFollowersDetectedArgs> OnNewFollowersDetected;

        /// <summary>
        /// FollowerService constructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the <paramref name="api"/> is null.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="checkIntervalInSeconds"/> is lower than one second.</exception> 
        /// <exception cref="ArgumentException">When the <paramref name="queryCountPerRequest" /> is less than 1 or more than 100 followers per request.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="cacheSize" /> is less than the queryCountPerRequest.</exception>
        /// <param name="api">The api to use for querying followers.</param>
        /// <param name="checkIntervalInSeconds">How often new followers should be queried.</param>
        /// <param name="queryCountPerRequest">The amount of followers to query per request.</param>
        /// <param name="cacheSize">The maximum amount of followers to cache per channel.</param>
        /// <param name="invokeEventsOnStartup">Whether to invoke the update events on startup or not.</param>
        public ExtFollowerService(ITwitchAPI api, int checkIntervalInSeconds = 60, int queryCountPerRequest = 100, int cacheSize = 1000, bool invokeEventsOnStartup = false) :
            base(api, checkIntervalInSeconds)
        {
            if (queryCountPerRequest < 1 || queryCountPerRequest > 100)
                throw new ArgumentException("Twitch doesn't support less than 1 or more than 100 followers per request.", nameof(queryCountPerRequest));

            if (cacheSize < queryCountPerRequest)
                throw new ArgumentException($"The cache size must be at least the size of the {nameof(queryCountPerRequest)} parameter.", nameof(cacheSize));

            QueryCountPerRequest = queryCountPerRequest;
            CacheSize = cacheSize;
            _invokeEventsOnStartup = invokeEventsOnStartup;
        }

        /// <summary>
        /// Clears the existing cache.
        /// </summary>
        public void ClearCache()
        {
            KnownFollowers.Clear();

            _lastFollowerDates.Clear();

            _nameBasedMonitor?.ClearCache();

            _nameBasedMonitor = null;
            _idBasedMonitor = null;
        }

        /// <summary>
        /// Updates the followerservice with the latest followers. Automatically called internally when service is started.
        /// </summary>
        /// <param name="callEvents">Whether to invoke the update events or not.</param>
        public async Task UpdateLatestFollowersAsync(bool callEvents = true)
        {
            if (ChannelsToMonitor == null)
                return;

            foreach (var channel in ChannelsToMonitor)
            {
                List<ChannelFollower> newFollowers;
                var latestFollowers = await GetLatestFollowersAsync(channel);

                if (latestFollowers.Count == 0)
                    return;

                if (!KnownFollowers.TryGetValue(channel, out var knownFollowers))
                {
                    newFollowers = latestFollowers;
                    KnownFollowers[channel] = latestFollowers.Take(CacheSize).ToList();
                    _lastFollowerDates[channel] = DateTime.Parse(latestFollowers.Last().FollowedAt);

                    if (!_invokeEventsOnStartup)
                        return;
                }
                else
                {
                    var existingFollowerIds = new HashSet<string>(knownFollowers.Select(f => f.UserId));
                    var latestKnownFollowerDate = _lastFollowerDates[channel];
                    newFollowers = new List<ChannelFollower>();

                    foreach (var follower in latestFollowers)
                    {
                        if (!existingFollowerIds.Add(follower.UserId)) continue;

                        if (DateTime.Parse(follower.FollowedAt) < latestKnownFollowerDate) continue;

                        newFollowers.Add(follower);
                        latestKnownFollowerDate = DateTime.Parse(follower.FollowedAt);
                        knownFollowers.Add(follower);
                    }

                    existingFollowerIds.Clear();
                    existingFollowerIds.TrimExcess();

                    // prune cache so we don't use too much space unnecessarily
                    if (knownFollowers.Count > CacheSize)
                        knownFollowers.RemoveRange(0, knownFollowers.Count - CacheSize);

                    if (newFollowers.Count <= 0)
                        return;

                    _lastFollowerDates[channel] = latestKnownFollowerDate;
                }

                if (!callEvents)
                    return;

                OnNewFollowersDetected?.Invoke(this, new OnNewFollowersDetectedArgs { Channel = channel, NewFollowers = newFollowers });
            }
        }

        protected override async Task OnServiceTimerTick()
        {
            await base.OnServiceTimerTick();
            await UpdateLatestFollowersAsync();
        }

        private async Task<List<ChannelFollower>> GetLatestFollowersAsync(string channel)
        {
            try
            {
                var resultset = await _monitor.ActionAsync((c, param) => _api.Helix.Channels.GetChannelFollowersAsync(first: (int)param[0], broadcasterId: c), channel, QueryCountPerRequest);
                return resultset.Data.Reverse().ToList();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                return new();
            }

        }
    }

}

