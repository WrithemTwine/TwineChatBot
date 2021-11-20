
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;

namespace StreamerBot.BotClients.Twitch.TwitchLib.Core
{
    public class NameBasedMonitor<T> : CoreMonitor<T>
    {
        private readonly ConcurrentDictionary<string, string> _channelToId = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public NameBasedMonitor(ITwitchAPI api) : base(api) { }

        public override async Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list)
        {
            if (!_channelToId.TryGetValue(Channel, out var channelId))
            {
                channelId = (await _api.Helix.Users.GetUsersAsync(logins: new List<string> { Channel })).Users.FirstOrDefault()?.Id;
                _channelToId[Channel] = channelId ?? throw new InvalidOperationException($"No channel with the name \"{Channel}\" could be found.");
            }
            //return await _api.Helix.Clips.GetClipsAsync(first: queryCount, broadcasterId: channelId);

            return await action.Invoke(channelId, list);
        }

        public void ClearCache()
        {
            _channelToId.Clear();
        }
    }
}
