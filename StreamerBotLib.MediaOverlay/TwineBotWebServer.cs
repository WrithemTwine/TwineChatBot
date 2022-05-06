﻿

using Microsoft.Net.Http.Server;

using System.Linq;
using System.Net.NetworkInformation;
using System.Net;

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



        private bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
            return openPorts.All(openPort => openPort != port);
        }
    }
}
