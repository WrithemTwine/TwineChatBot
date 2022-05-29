using MediaOverlayServer.Communication;
using MediaOverlayServer.Server;

namespace MediaOverlayServer.Control
{
    internal class OverlayController
    {
        internal OverlaySvcClientPipe _clientPipe;
        internal TwineBotWebServer _httpServer;

        internal OverlayController()
        {
            _clientPipe = new();
            _httpServer = new();
        }

        internal void StartServer()
        {
            _httpServer.StartServer();
        }

        internal void StopServer()
        {
            _httpServer.StopServer();
        }

    }
}
