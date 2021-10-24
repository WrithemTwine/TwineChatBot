using StreamerBot.BotClients;
using StreamerBot.Systems;

using System.Collections.ObjectModel;

namespace StreamerBot.BotIOController
{
    public class BotController
    {
        public SystemsController Systems { get; private set; }
        public Collection<IOModule> IOModuleList { get; private set; } = new();

        #region Bot Clients
        public static BotsTwitch BotsTwitch { get; private set; } = new();

        #endregion


        public BotController()
        {

        }
    }
}
