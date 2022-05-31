using MediaOverlayServer.Communication;
using MediaOverlayServer.Models;
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

            _clientPipe.ReceivedOverlayEvent += _clientPipe_ReceivedOverlayEvent;
        }

        private void _clientPipe_ReceivedOverlayEvent(object? sender, OverlayActionType e)
        {
            _httpServer.SendAlert(new OverlayPage() { OverlayType = e.OverlayType.ToString(), OverlayHyperText = ProcessHyperText.ProcessOverlay(e) });
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
