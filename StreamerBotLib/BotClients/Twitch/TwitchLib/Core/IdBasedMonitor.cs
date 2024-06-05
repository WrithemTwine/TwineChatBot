using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Interfaces;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Core
{
    public class IdBasedMonitor<T>(ITwitchAPI api) : CoreMonitor<T>(api)
    {
        public override async Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list)
        {
            return await action.Invoke(Channel, list);
        }

        public override Task<Func<Stream, bool>> CompareStream(string Channel, string accessToken = null)
        {
            return Task.FromResult(new Func<Stream, bool>(stream => stream.UserId == Channel));
        }
    }
}
