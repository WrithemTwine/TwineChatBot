using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;

namespace ChatBot_Net5.BotClients.TwitchLib.Core
{
    public class IdBasedMonitor<T> : CoreMonitor<T>
    {
        public IdBasedMonitor(ITwitchAPI api) : base(api) { }

        //public override Task<GetClipsResponse> GetClipsAsync(string channel, int queryCount)
        //{
        //    return _api.Helix.Clips.GetClipsAsync(first: queryCount, broadcasterId: channel);
        //}

        public override async Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list)
        {
            return await action.Invoke(Channel, list);
        }
    }
}
