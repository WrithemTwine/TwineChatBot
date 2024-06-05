using StreamerBotLib.Static;

using System.Net.Http;
using System.Reflection;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    // derived from: https://github.com/TwitchLib/TwitchLib.Api/blob/master/TwitchLib.Api/Services/LiveStreamMonitorService.cs

    public class ExtLiveStreamMonitorService : ExtApiService<GetStreamsResponse> //: LiveStreamMonitorService(api, checkIntervalInSeconds, maxStreamRequestCountPerRequest)
    {
        public event EventHandler AccessTokenUnauthorized;

        /// <summary>
        /// A cache with streams that are currently live.
        /// </summary>
        public Dictionary<string, Stream> LiveStreams { get; } = new Dictionary<string, Stream>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The maximum amount of streams to collect per request.
        /// </summary>
        public int MaxStreamRequestCountPerRequest { get; }

        /// <summary>
        /// Invoked when a monitored stream went online.
        /// </summary>
        public event EventHandler<OnStreamOnlineArgs> OnStreamOnline;
        /// <summary>
        /// Invoked when a monitored stream went offline.
        /// </summary>
        public event EventHandler<OnStreamOfflineArgs> OnStreamOffline;
        /// <summary>
        /// Invoked when a monitored stream was already online, but is updated with it's latest information (might be the same).
        /// </summary>
        public event EventHandler<OnStreamUpdateArgs> OnStreamUpdate;

        public DateTime InstanceDate { get; set; }

        /// <summary>
        /// The constructor from the LiveStreamMonitorService
        /// </summary>
        /// <exception cref="ArgumentNullException">When the <paramref name="api"/> is null.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="checkIntervalInSeconds"/> is lower than one second.</exception> 
        /// <exception cref="ArgumentException">When the <paramref name="maxStreamRequestCountPerRequest"/> is less than 1 or more than 100.</exception> 
        /// <param name="api">The api used to query information.</param>
        /// <param name="checkIntervalInSeconds"></param>
        /// <param name="maxStreamRequestCountPerRequest"></param>
        public ExtLiveStreamMonitorService(ITwitchAPI api, int checkIntervalInSeconds = 60, int maxStreamRequestCountPerRequest = 100, DateTime instanceDate = default) :
            base(api, checkIntervalInSeconds)
        {
            if (maxStreamRequestCountPerRequest < 1 || maxStreamRequestCountPerRequest > 100)
                throw new ArgumentException("Twitch doesn't support less than 1 or more than 100 streams per request.", nameof(maxStreamRequestCountPerRequest));

            MaxStreamRequestCountPerRequest = maxStreamRequestCountPerRequest;

            InstanceDate = instanceDate;
        }

        public void ClearCache()
        {
            LiveStreams.Clear();

            _nameBasedMonitor?.ClearCache();

            _nameBasedMonitor = null;
            _idBasedMonitor = null;
        }

        //public void SetChannelsById(List<string> channelsToMonitor)
        //{
        //    SetChannels(channelsToMonitor);

        //    _monitor = IdBasedMonitor;
        //}

        //public void SetChannelsByName(List<string> channelsToMonitor)
        //{
        //    SetChannels(channelsToMonitor);

        //    _monitor = NameBasedMonitor;
        //}

        protected override async Task OnServiceTimerTick()
        {
            try
            {
                await base.OnServiceTimerTick();
                await UpdateLiveStreamersAsync();

                // suspected multiple instances produces duplicative output events
            }
            catch (BadScopeException)
            {
                AccessTokenUnauthorized?.Invoke(this, new());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void UpdateToken(string accesstoken)
        {
            _api.Settings.AccessToken = accesstoken;
        }

        public async Task UpdateLiveStreamersAsync(bool callEvents = true)
        {
            var result = await GetLiveStreamersAsync();

            foreach (var channel in ChannelsToMonitor)
            {
                var liveStream = result.FirstOrDefault(await _monitor.CompareStream(channel));

                if (liveStream != null)
                {
                    HandleLiveStreamUpdate(channel, liveStream, callEvents);
                }
                else
                {
                    HandleOfflineStreamUpdate(channel, callEvents);
                }
            }
        }

        private void HandleLiveStreamUpdate(string channel, Stream liveStream, bool callEvents)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchLiveBot, $"Performing stream update with instance dated {InstanceDate}.");

            var wasAlreadyLive = LiveStreams.ContainsKey(channel);

            LiveStreams[channel] = liveStream;

            if (!callEvents)
                return;

            if (!wasAlreadyLive)
            {
                OnStreamOnline?.Invoke(this, new OnStreamOnlineArgs { Channel = channel, Stream = liveStream });
            }
            else
            {
                OnStreamUpdate?.Invoke(this, new OnStreamUpdateArgs { Channel = channel, Stream = liveStream });
            }
        }

        private void HandleOfflineStreamUpdate(string channel, bool callEvents)
        {
            var wasAlreadyLive = LiveStreams.TryGetValue(channel, out var cachedLiveStream);

            if (!wasAlreadyLive)
                return;

            LiveStreams.Remove(channel);

            if (!callEvents)
                return;

            OnStreamOffline?.Invoke(this, new OnStreamOfflineArgs { Channel = channel, Stream = cachedLiveStream });
        }

        private async Task<List<Stream>> GetLiveStreamersAsync()
        {
            var livestreamers = new List<Stream>();
            var pages = Math.Ceiling((double)ChannelsToMonitor.Count / MaxStreamRequestCountPerRequest);

            for (var i = 0; i < pages; i++)
            {
                var selectedSet = ChannelsToMonitor.Skip(i * MaxStreamRequestCountPerRequest).Take(MaxStreamRequestCountPerRequest).ToList();
                GetStreamsResponse resultset;

                resultset = await _monitor.ActionAsync(
                        (c, param) => _api.Helix.Streams.GetStreamsAsync(first: (int)param[0], userIds: (List<string>)param[1], accessToken: (string)param[2]),
                        null,
                        [selectedSet.Count, selectedSet, _api.Settings.AccessToken]);

                if (resultset.Streams == null)
                    continue;

                livestreamers.AddRange(resultset.Streams);
            }
            return livestreamers;
        }
    }
}
