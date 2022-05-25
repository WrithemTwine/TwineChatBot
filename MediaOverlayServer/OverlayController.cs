using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOverlayServer
{
    internal class OverlayController
    {
        internal OverlaySvcClientPipe _clientPipe;
        internal TwineBotWebServer _webServer;

        internal OverlayController()
        {
            _clientPipe = new();
            _webServer = new();
        }

    }
}
