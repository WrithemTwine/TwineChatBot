using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace MultiUserLiveBot.Clients.TwitchLib
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
            catch { }
        }

 
    }
}
