﻿using System.Threading.Tasks;
using System.Timers;

namespace ChatBot_Net5.BotClients.TwitchLib.Core
{
    internal class ServiceTimer : Timer
    {
        public int IntervalInSeconds { get; }

        public delegate Task ServiceTimerTick();

        private readonly ServiceTimerTick _serviceTimerTickAsyncCallback;

        public ServiceTimer(ServiceTimerTick serviceTimerTickAsyncCallback, int intervalInSeconds = 60)
        {
            _serviceTimerTickAsyncCallback = serviceTimerTickAsyncCallback;
            Interval = intervalInSeconds * 1000;
            IntervalInSeconds = intervalInSeconds;
            Elapsed += async (sender, e) => await TimerElapsedAsync(sender, e);
        }

        private async Task TimerElapsedAsync(object sender, ElapsedEventArgs e)
        {
            await _serviceTimerTickAsyncCallback();
        }
    }
}