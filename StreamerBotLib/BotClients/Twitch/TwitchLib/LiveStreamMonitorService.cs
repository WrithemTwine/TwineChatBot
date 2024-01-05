﻿using StreamerBotLib.Static;

using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ExtLiveStreamMonitorService : LiveStreamMonitorService
    {
        public event EventHandler AccessTokenUnauthorized;

        public ExtLiveStreamMonitorService(ITwitchAPI api,
                                           int checkIntervalInSeconds = 60,
                                           int maxStreamRequestCountPerRequest = 100) : base(api, checkIntervalInSeconds, maxStreamRequestCountPerRequest)
        {
        }

        protected override async Task OnServiceTimerTick()
        {
            try
            {
                await base.OnServiceTimerTick();
                await UpdateLiveStreamersAsync();
            }
            catch (HttpRequestException hrEx)
            {
                if (hrEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    AccessTokenUnauthorized?.Invoke(this, new());
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }


    }
}
