using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Interfaces;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Core
{
    public abstract class CoreMonitor<T>(ITwitchAPI api)
    {
        protected readonly ITwitchAPI _api = api;

        public delegate Task<T> HelixAsync(string Channel, params object[] list);

        public abstract Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list);

        public abstract Task<Func<Stream, bool>> CompareStream(string Channel, string accessToken = null);
    }
}
