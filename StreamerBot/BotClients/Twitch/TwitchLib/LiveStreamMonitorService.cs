using StreamerBot.Static;

using System;
using System.Reflection;
using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBot.BotClients.Twitch.TwitchLib
{
    public class ExtLiveStreamMonitorService : LiveStreamMonitorService
    {
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
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

 
    }
}
