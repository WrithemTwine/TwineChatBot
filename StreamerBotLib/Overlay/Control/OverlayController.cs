#if UsePipes
#define UtilizePipeIPC // use the NamedPipe Server/Client mechanism
#else
#define UseGUIDLL
#endif

using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Models;

using StreamerBotLib.Overlay.Server;
using StreamerBotLib.Static;

using System.Collections.Generic;
using System.Reflection;

namespace StreamerBotLib.Overlay.Control
{
    /// <summary>
    /// Class to receive overlay requested data and send to the webserver.
    /// </summary>
    public class OverlayController
    {
        internal TwineBotWebServer _httpServer;
        internal TickerFormatter tickerFormatter;

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
        /// <summary>
        /// instantiates the private members
        /// </summary>
        public OverlayController()
        {
            _httpServer = new();
            tickerFormatter = new();
        }

        /// <summary>
        /// Send a popup alert to the Overlay http server.
        /// </summary>
        /// <param name="overlayPage">An object holding the html data to send to for the alert.</param>
        public void SendAlert(OverlayPage overlayPage)
        {
            _httpServer.SendAlert(overlayPage);

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Sending alert, {overlayPage.OverlayType}, for display.");

        }
#endif

        /// <summary>
        /// Starts the http server
        /// </summary>
        public void StartServer()
        {
            _httpServer.StartServer();

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Overlay http server started.");

        }

        /// <summary>
        /// Stops the http server
        /// </summary>
        public void StopServer()
        {
            _httpServer.StopServer();

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Overlay http server stopped.");
        }

        /// <summary>
        /// Specify the ticker categories the user requests to view on screen. The user
        /// doesn't have to display all of them on screen.
        /// </summary>
        /// <param name="items">A collection of the user selected items for the ticker.</param>
        public void SetTickerItems(IEnumerable<SelectedTickerItem> items)
        {
            tickerFormatter.SetTickersSelected(items);
        }

        /// <summary>
        /// Changes the ticker data and specifies the current marquee style.
        /// </summary>
        /// <param name="data">All of the data used for the tickers.</param>
        /// <param name="overlayStyles">The styles used for the ticker items, based on current style.</param>
        public void SetTickerData(IEnumerable<TickerItem> data, IEnumerable<OverlayStyle> overlayStyles)
        {
            tickerFormatter.SetTickerData(data);
            UpdateTicker(overlayStyles);
        }

        /// <summary>
        /// Change the ticker style from static, rotating, and scrolling-marquee. Need the 
        /// overlay style data to change to the new data page. Doesn't update the ticker data contents.
        /// </summary>
        /// <param name="overlayStyles">A collection of styles relevant to the current setup.</param>
        public void UpdateTicker(IEnumerable<OverlayStyle> overlayStyles)
        {
            _httpServer.UpdateTicker(tickerFormatter.GetTickerPages(overlayStyles));
        }
    }
}
