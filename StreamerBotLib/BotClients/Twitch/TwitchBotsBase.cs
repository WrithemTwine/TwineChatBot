using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotsBase : IOModule
    {
        internal static TwitchTokenBot tokenBot = new();
    }
}
