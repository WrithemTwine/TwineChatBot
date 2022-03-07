using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Interfaces;


namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ClipMonitorService : ExtApiService<GetClipsResponse>
    {
        // derived from: https://github.com/TwitchLib/TwitchLib.Api/blob/c960198b067e3559b7579d0e9714b6aa9eb86a1d/TwitchLib.Api/Services/FollowerService.cs and associated classes

        private readonly Dictionary<string, string> _lastClipsDates = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The current known followers for each channel.
        /// </summary>
        public Dictionary<string, List<Clip>> KnownClips { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The amount of followers queried per request.
        /// </summary>
        public int QueryCountPerRequest { get; }
        /// <summary>
        /// The maximum amount of followers cached per channel.
        /// </summary>
        public int CacheSize { get; }

        public event EventHandler<OnNewClipsDetectedArgs> OnNewClipFound;

        public ClipMonitorService(ITwitchAPI api, int checkIntervalInSeconds = 60, int queryCountPerRequest = 100, int cacheSize = 1000) : base(api, checkIntervalInSeconds)
        {
            if (queryCountPerRequest is < 1 or > 100)
            {
                throw new ArgumentException("Twitch doesn't support less than 1 or more than 100 followers per request.", nameof(queryCountPerRequest));
            }

            if (cacheSize < queryCountPerRequest)
            {
                throw new ArgumentException($"The cache size must be at least the size of the {nameof(queryCountPerRequest)} parameter.", nameof(cacheSize));
            }

            QueryCountPerRequest = queryCountPerRequest;
            CacheSize = cacheSize;
        }

        /// <summary>
        /// Clears the existing cache.
        /// </summary>
        public void ClearCache()
        {
            KnownClips.Clear();

            _lastClipsDates.Clear();

            _nameBasedMonitor?.ClearCache();

            _nameBasedMonitor = null;
            _idBasedMonitor = null;
        }

        public async Task MonitorNewClips(bool callevents = true)
        {
            if (ChannelsToMonitor == null)
            {
                return;
            }

            foreach (string channel in ChannelsToMonitor)
            {
                List<Clip> newClips;
                List<Clip> latestClips = await GetLatestClipsAsync(channel);

                if (latestClips.Count == 0) { continue; }

                if (!KnownClips.TryGetValue(channel, out List<Clip> Clipsknown))
                {
                    newClips = latestClips;
                    KnownClips[channel] = latestClips.Take(CacheSize).ToList();
                    _lastClipsDates[channel] = latestClips.Last().CreatedAt;
                }
                else
                {
                    HashSet<string> existingClips = new(Clipsknown.Select(f => f.Id));
                    string latestKnownClipDate = _lastClipsDates[channel];
                    newClips = new();

                    foreach(Clip clip in latestClips)
                    {
                        if (!existingClips.Add(clip.Id) || DateTime.Parse(clip.CreatedAt, CultureInfo.CurrentCulture) < DateTime.Parse(latestKnownClipDate, CultureInfo.CurrentCulture))
                        {
                            continue;
                        }

                        newClips.Add(clip);
                        latestKnownClipDate = clip.CreatedAt;
                        Clipsknown.Add(clip);
                    }
                    existingClips.Clear();
                    existingClips.TrimExcess();

                    if (Clipsknown.Count > CacheSize)
                    {
                        Clipsknown.RemoveRange(0, Clipsknown.Count - CacheSize);
                    }

                    if (newClips.Count <= 0)
                    {
                        continue;
                    }
                }

                if (callevents)
                {
                    OnNewClipFound?.Invoke(this, new OnNewClipsDetectedArgs() { Channel = channel, Clips = newClips});
                }
            }
        }

        public async Task<List<Clip>> GetAllClipsAsync(string ChannelName)
        {
            async Task<GetClipsResponse> Clips(string Channel, int queryCount, string after = null)
            {
                return after == null ? await _monitor.ActionAsync((c, param) => _api.Helix.Clips.GetClipsAsync(first: (int) param[0], broadcasterId: c),
                Channel, new object[] { queryCount }) : await _monitor.ActionAsync((c, param) => _api.Helix.Clips.GetClipsAsync(first: (int) param[1], broadcasterId: c, after: (string)param[0]),
                Channel, new object[] { queryCount, after });
            }

            List<Clip> allClips = new();
            string cursor = null;

            async Task<int> AddClip(string Channel, int queryCount, string after = null)
            {
                GetClipsResponse getClips = await Clips(Channel, queryCount, after);
                allClips.AddRange(getClips.Clips);
                cursor = getClips.Pagination.Cursor;
                return getClips.Clips.Length;
            }

            int count = 0;

            while (allClips.Count == 0)
            {
                try
                {
                    count = await AddClip(ChannelName, 100);
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }

            while (count == 100)
            {
                try
                {
                    count = await AddClip(ChannelName, 100, cursor);
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                }
            }

            return allClips;
        }

        protected override async Task OnServiceTimerTick()
        {
            await base.OnServiceTimerTick();
            await MonitorNewClips();
        }

        private async Task<List<Clip>> GetLatestClipsAsync(string channel)
        {
            GetClipsResponse resultset = await _monitor.ActionAsync((c, param) => _api.Helix.Clips.GetClipsAsync(first: (int)param[0], broadcasterId: c),
                channel, new object[] { QueryCountPerRequest });

            return resultset.Clips.Reverse().ToList();
        }
    }
}
