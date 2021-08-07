﻿using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;

namespace ChatBot_Net5.BotClients.TwitchLib.Core
{
    internal abstract class CoreMonitor<T>
    {
        protected readonly ITwitchAPI _api;

        public delegate Task<T> HelixAsync(string Channel, params object[] list);

        public abstract Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list);

        protected CoreMonitor(ITwitchAPI api)
        {
            _api = api;
        }

    }
}