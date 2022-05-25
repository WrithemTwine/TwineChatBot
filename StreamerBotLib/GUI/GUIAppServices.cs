using StreamerBotLib.BotClients;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamerBotLib.GUI
{
    public class GUIAppServices : GUIBotBase
    {
        public BotOverlayServer MediaOverlayServer { get; private set; }


        public GUIAppServices()
        {
            MediaOverlayServer = BotIOController.BotController.OverlayServerBot;
            MediaOverlayServer.OnBotStarted += Service_Started;
            MediaOverlayServer.OnBotStopped += Service_Stopped;
        }

        private void Service_Started(object sender, EventArgs e)
        {
            IOModule currbot = sender as IOModule;
            BotStarted(new() { BotName = currbot.BotClientName, Started = currbot.IsStarted, Stopped = currbot.IsStopped });
        }

        private void Service_Stopped(object sender, EventArgs e)
        {
            IOModule currbot = sender as IOModule;
            BotStopped(new() { BotName = currbot.BotClientName, Started = currbot.IsStarted, Stopped = currbot.IsStopped });
        }
    }
}
