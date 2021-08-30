using ChatBot_Net5.BotClients.TwitchLib.Core;

using System;
using System.Collections.Generic;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace ChatBot_Net5.BotClients.TwitchLib
{
    public abstract class ExtApiService<T> : ApiService
    {
        public CoreMonitor<T> _monitor;
        public IdBasedMonitor<T> _idBasedMonitor;
        public NameBasedMonitor<T> _nameBasedMonitor;

        public IdBasedMonitor<T> IdBasedMonitor => _idBasedMonitor ?? (_idBasedMonitor = new IdBasedMonitor<T>(_api));
        public NameBasedMonitor<T> NameBasedMonitor => _nameBasedMonitor ?? (_nameBasedMonitor = new NameBasedMonitor<T>(_api));

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
