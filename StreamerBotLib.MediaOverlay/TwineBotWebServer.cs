

using Microsoft.Net.Http.Server;

namespace StreamerBotLib.MediaOverlay
{
    public class TwineBotWebServer
    {
        private WebListener WebListener { get; set; }

        public TwineBotWebServer()
        {
            WebListener = new(new());

            WebListener.Settings.Authentication.AllowAnonymous = true;
        }
    }
}
