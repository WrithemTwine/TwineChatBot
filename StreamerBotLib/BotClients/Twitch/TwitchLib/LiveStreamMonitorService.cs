using StreamerBotLib.Static;

using System.Net.Http;
using System.Reflection;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ExtLiveStreamMonitorService(ITwitchAPI api,
                                       int checkIntervalInSeconds = 60,
                                       int maxStreamRequestCountPerRequest = 100) : LiveStreamMonitorService(api, checkIntervalInSeconds, maxStreamRequestCountPerRequest)
    {
        public event EventHandler AccessTokenUnauthorized;

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

        public void UpdateToken(string accesstoken)
        {
            _api.Settings.AccessToken = accesstoken;
        }

    }
}
