#if UsePipes
#define UtilizePipeIPC // use the NamedPipe Server/Client mechanism
#else
#define UseGUIDLL
#endif

using StreamerBotLib.Overlay.Models;

using StreamerBotLib.Overlay.Server;

namespace StreamerBotLib.Overlay.Control
{
    public class OverlayController
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
        public OverlayController()
        {
            _httpServer = new();
        }

        public void SendAlert(OverlayPage overlayPage)
        {
            _httpServer.SendAlert(overlayPage);
        }
#endif

        public void StartServer()
        {
            _httpServer.StartServer();
        }

        public void StopServer()
        {
            _httpServer.StopServer();
        }

    }
}
