
using System.Threading.Tasks;

using TwitchLib.Api.Interfaces;

namespace StreamerBot.BotClients.Twitch.TwitchLib.Core
{
    public class IdBasedMonitor<T> : CoreMonitor<T>
    {
        public IdBasedMonitor(ITwitchAPI api) : base(api) { }

        public override async Task<T> ActionAsync(HelixAsync action, string Channel, params object[] list)
        {
            return await action.Invoke(Channel, list);
        }
    }
}
