using System.Collections.Concurrent;

using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Interfaces;

namespace StreamerBotLib.BotClients.Twitch.TwitchLib.Core
{
    public class NameBasedMonitor<T>(ITwitchAPI api) : CoreMonitor<T>(api)
    {
        private readonly ConcurrentDictionary<string, string> _channelToId = new(StringComparer.OrdinalIgnoreCase);

        public override async Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list)
        {
            if (!_channelToId.TryGetValue(Channel, out var channelId))
            {
                channelId = (await _api.Helix.Users.GetUsersAsync(logins: [Channel])).Users.FirstOrDefault()?.Id;
                _channelToId[Channel] = channelId ?? throw new InvalidOperationException($"No channel with the name \"{Channel}\" could be found.");
            }

            return await action.Invoke(channelId, list);
        }

        public void ClearCache()
        {
            _channelToId.Clear();
        }

        public override async Task<Func<Stream, bool>> CompareStream(string Channel, string accessToken = null)
        {
            if (!_channelToId.TryGetValue(Channel, out var channelId))
            {
                channelId = (await _api.Helix.Users.GetUsersAsync(logins: [Channel])).Users.FirstOrDefault()?.Id;
                _channelToId[Channel] = channelId ?? throw new InvalidOperationException($"No channel with the name \"{Channel}\" could be found.");
            }

            return stream => stream.Id == channelId;
        }
    }
}
