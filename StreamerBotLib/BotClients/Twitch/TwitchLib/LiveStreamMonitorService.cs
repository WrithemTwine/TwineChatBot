using StreamerBotLib.Static;

using System.Net.Http;
using System.Reflection;

using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib
{
    public class ExtLiveStreamMonitorService(ITwitchAPI api,
                                       int checkIntervalInSeconds = 60,
                                       int maxStreamRequestCountPerRequest = 100,
                                       DateTime instanceDate = default) : LiveStreamMonitorService(api, checkIntervalInSeconds, maxStreamRequestCountPerRequest)
    {
        public event EventHandler AccessTokenUnauthorized;

        public DateTime InstanceDate { get; } = instanceDate;

        protected override async Task OnServiceTimerTick()
        {
            try
            {
                await base.OnServiceTimerTick();
                await UpdateLiveStreamersAsync();

                // suspected multiple instances produces duplicative output events
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchLiveBot, $"Processing a Timer Tick on instance dated {InstanceDate}.");
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
