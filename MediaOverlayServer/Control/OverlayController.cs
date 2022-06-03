#if UsePipes
#define UtilizePipeIPC // use the NamedPipe Server/Client mechanism
#else
#define UseGUIDLL
#endif

using MediaOverlayServer.Communication;
using MediaOverlayServer.Models;
using MediaOverlayServer.Server;

namespace MediaOverlayServer.Control
{
    internal class OverlayController
    {
        internal TwineBotWebServer _httpServer;


#if UtilizePipeIPC
        internal OverlaySvcClientPipe _clientPipe;

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
#elif UseGUIDLL
        internal OverlayController()
        {
            _httpServer = new();
        }

        public void ReceivedOverlayEvent(object? sender, OverlayActionType e)
        {
            _httpServer.SendAlert(new OverlayPage() { OverlayType = e.OverlayType.ToString(), OverlayHyperText = ProcessHyperText.ProcessOverlay(e) });
        }
#endif

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
