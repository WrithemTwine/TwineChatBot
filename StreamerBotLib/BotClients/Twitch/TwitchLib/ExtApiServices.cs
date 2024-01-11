
using StreamerBotLib.BotClients.Twitch.TwitchLib.Core;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public abstract class ExtApiService<T> : ApiService
    {
        protected CoreMonitor<T> _monitor;
        protected IdBasedMonitor<T> _idBasedMonitor;
        protected NameBasedMonitor<T> _nameBasedMonitor;

        protected IdBasedMonitor<T> IdBasedMonitor => _idBasedMonitor ??= new(_api);
        protected NameBasedMonitor<T> NameBasedMonitor => _nameBasedMonitor ??= new(_api);

        protected ExtApiService(ITwitchAPI api, int checkIntervalInSeconds) : base(api, checkIntervalInSeconds)
        {
        }

        /// <summary>
        /// Sets the channels to monitor by id. Event's channel properties will be Ids in this case.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="channelsToMonitor"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="channelsToMonitor"/> is empty.</exception>
        /// <param name="channelsToMonitor">A list with channels to monitor.</param>
        public void SetChannelsById(List<string> channelsToMonitor)
        {
            SetChannels(channelsToMonitor);

            _monitor = IdBasedMonitor;
        }

        /// <summary>
        /// Sets the channels to monitor by name. Event's channel properties will be names in this case.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="channelsToMonitor"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="channelsToMonitor"/> is empty.</exception>
        /// <param name="channelsToMonitor">A list with channels to monitor.</param>
        public void SetChannelsByName(List<string> channelsToMonitor)
        {
            SetChannels(channelsToMonitor);

            _monitor = NameBasedMonitor;
        }

    }
}
