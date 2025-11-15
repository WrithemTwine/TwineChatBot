using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.ClipService;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Diagnostics.CodeAnalysis;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Clips.CreateClip;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Interfaces;


namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ClipMonitorService : ExtApiService<GetClipsResponse>
    {
        // derived from: https://github.com/TwitchLib/TwitchLib.Api/blob/c960198b067e3559b7579d0e9714b6aa9eb86a1d/TwitchLib.Api/Services/FollowerService.cs and associated classes

        private readonly Dictionary<string, string> _lastClipsDates = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The current known clips for each channel.
        /// </summary>
        public Dictionary<string, List<Clip>> KnownClips { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The amount of clips queried per request.
        /// </summary>
        public int QueryCountPerRequest { get; }
        /// <summary>
        /// The maximum amount of clips cached per channel.
        /// </summary>
        public int CacheSize { get; }

        public event EventHandler<OnNewClipsDetectedArgs> OnNewClipFound;
        public event EventHandler<ExpiredTokenEventArgs> AccessTokenUnauthorized;

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

        public void UpdateAccessToken(string accessToken)
        {
            _api.Settings.AccessToken = accessToken;
        }

        /// <summary>
        /// Clears the existing cache.
        /// </summary>
        public void ClearCache()
        {
#if DEBUG
            LogWriter.DebugLog("ClearCache", Models.Enums.DebugLogTypes.SpecialPurpose, "Clear the channel clips monitor cache.");
#endif

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
                List<Clip> latestClips = null;

                latestClips = await GetLatestClipsAsync(channel);

                if (latestClips == null || latestClips.Count == 0) { continue; }

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
                    newClips = latestClips.Except(Clipsknown, new ClipsComparer()).ToList();
                    latestKnownClipDate = latestClips.Last().CreatedAt;
                    Clipsknown.AddRange(newClips);

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
                    OnNewClipFound?.Invoke(this, new OnNewClipsDetectedArgs() { Channel = channel, Clips = newClips });
                }
            }
        }

        public async Task<List<Clip>> GetAllClipsAsync(string ChannelName)
        {
            string after = null;

            List<Clip> clips = [];

            try
            {
#if DEBUG
                LogWriter.DebugLog("GetAllClipsAsync", Models.Enums.DebugLogTypes.SpecialPurpose, $"Get All Clips for the channel: {ChannelName}");
#endif

                do
                {
                    GetClipsResponse curr = await _monitor.ActionAsync((c, param) =>
                                                                    _api.Helix.Clips.GetClipsAsync(first: (int)param[0],
                                                                                                   broadcasterId: c,
                                                                                                   after: (string)param[1]),
                                                                                                   ChannelName,
                                                                                                   [QueryCountPerRequest, after]);
                    after = curr.Pagination.Cursor;
                    clips.AddRange(curr.Clips);
                } while (after != null);

                return clips;
            }
            catch (BadScopeException)
            {
                AccessTokenUnauthorized?.Invoke(this, new(null)); // ignore perform action, service tick calls this again later
                return null;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "GetAllClipsAsync");
                return null;
            }
        }

        protected override async Task OnServiceTimerTick()
        {
            try
            {
#if DEBUG
                LogWriter.DebugLog("OnServiceTimerTick", Models.Enums.DebugLogTypes.SpecialPurpose, "Service Timer Tick");
#endif

                await base.OnServiceTimerTick();
                await MonitorNewClips();
            }
            catch (BadScopeException)
            {
                AccessTokenUnauthorized?.Invoke(this, new(null));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "OnServiceTimerTick");
            }
        }

        private async Task<List<Clip>> GetLatestClipsAsync(string channel)
        {
#if DEBUG
            LogWriter.DebugLog("GetLatestClipsAsync", Models.Enums.DebugLogTypes.SpecialPurpose, $"Get latest clips for channel: {channel}");
#endif

            GetClipsResponse resultset = await _monitor.ActionAsync((c, param) => _api.Helix.Clips.GetClipsAsync(first: (int)param[0], broadcasterId: c),
                channel, [QueryCountPerRequest]);

            return resultset.Clips.Reverse().ToList();
        }

        public async Task<CreatedClipResponse> CreateClip(string channelId)
        {
            try
            {
#if DEBUG
                LogWriter.DebugLog("CreateClip", Models.Enums.DebugLogTypes.SpecialPurpose, $"Creating clip for channel ID: {channelId}");
#endif

                return await _api.Helix.Clips.CreateClipAsync(channelId);
            }
            catch (BadScopeException)
            {
                AccessTokenUnauthorized?.Invoke(this, new(() => _api.Helix.Clips.CreateClipAsync(channelId)));
                return null;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "CreateClip");
                return null;
            }
        }
    }

    internal class ClipsComparer : IEqualityComparer<Clip>
    {
        public bool Equals(Clip x, Clip y)
        {
            return (x.CreatedAt == y.CreatedAt) && (x.Id == y.Id);
        }

        public int GetHashCode([DisallowNull] Clip obj)
        {
            return obj.CreatedAt.GetHashCode();
        }
    }
}
