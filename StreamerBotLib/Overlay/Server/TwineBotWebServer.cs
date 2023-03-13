using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace StreamerBotLib.Overlay.Server
{
    public class TwineBotWebServer
    {
        private HttpListener HTTPListenServer { get; set; } = new();

        private List<IOverlayPageReadOnly> OverlayPages { get; set; } = new();
        private List<IOverlayPageReadOnly> TickerPages { get; set; } = new();

        public TwineBotWebServer()
        {
            if (OptionFlags.MediaOverlayMediaPort == 0)
            {
                Random random = new();
                int port = ValidatePort(random.Next(1024, 65536));

                Settings.Default.MediaOverlayPort = port;
            }
        }

        public static int ValidatePort(int port)
        {
            while (!IsFree(port))
            {
                port++;
            }

            return port;
        }

        // ports: 1 - 65535
        private static bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray();
            return openPorts.All(openPort => openPort != port);
        }

        public void StartServer()
        {
            HTTPListenServer.Prefixes.Clear();
            foreach (string P in PrefixGenerator.GetPrefixes())
            {
                HTTPListenServer.Prefixes.Add(P);
            }
            HTTPListenServer.Start();

            ThreadManager.CreateThreadStart(() => ServerSendAlerts());
        }

        public void SendAlert(IOverlayPageReadOnly overlayPage)
        {
            if (HTTPListenServer.IsListening)
            {
                lock (OverlayPages)
                {
                    OverlayPages.Add(overlayPage);
                }
            }
        }

        /// <summary>
        /// Clear and replace all TickerPages when the user changes the visual ticker appearance.
        /// </summary>
        /// <param name="UpdateTickerPages">All of the new ticker pages to replace the current ticker pages.</param>
        public void UpdateTicker(IEnumerable<IOverlayPageReadOnly> UpdateTickerPages)
        {
            lock (TickerPages)
            {
                TickerPages.Clear();
                TickerPages.AddRange(UpdateTickerPages);
            }
        }

        public void UpdateTicker(IOverlayPageReadOnly UpdateTickerPage)
        {
            lock (TickerPages)
            {
                int idx = TickerPages.FindIndex((T) => T.OverlayType == UpdateTickerPage.OverlayType);

                if (idx >= 0)
                {
                    TickerPages[idx] = UpdateTickerPage;
                }
            }
        }

        private void ServerSendAlerts()
        {
            try
            {
                while (OptionFlags.ActiveToken && HTTPListenServer.IsListening)
                {
                    HttpListenerContext context = HTTPListenServer.GetContext();
                    HttpListenerRequest request = context.Request;
                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;
                    // Construct a response.

                    byte[] buffer;

                    if (request.RawUrl.Contains("ticker"))
                    {
                        string responseString = ProcessHyperText.DefaultPage; // "<HTML><BODY> Hello world!</BODY></HTML>";

                        lock (TickerPages)
                        {
                            if (TickerPages.Count == 1)
                            {
                                responseString = TickerPages[0].OverlayHyperText;
                            } 
                            else if(TickerPages.Count > 1)
                            {
                                foreach (var T in from T in TickerPages
                                                  where request.RawUrl.Contains(T.OverlayType)
                                                  select T)
                                {
                                    responseString = T.OverlayHyperText;
                                }
                            }
                        }

                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    }
                    else if (request.RawUrl.Contains("index.html"))
                    {
                        string RequestType = OverlayTypes.None.ToString();
                        if (!OptionFlags.MediaOverlayUseSameStyle)
                        {
                            RequestType = request.RawUrl?.Substring(1, request.RawUrl.IndexOf('/', 1)) ?? RequestType;
                        }

                        string responseString = ProcessHyperText.DefaultPage; // "<HTML><BODY> Hello world!</BODY></HTML>";

                        lock (OverlayPages)
                        {
                            if (OverlayPages.Count > 0)
                            {
                                IOverlayPageReadOnly found = null;

                                foreach (var page in OverlayPages)
                                {
                                    if (page.OverlayType == RequestType || OptionFlags.MediaOverlayUseSameStyle)
                                    {
                                        found = page;
                                    }
                                }

                                if (found != null)
                                {
                                    OverlayPages.Remove(found);
                                    responseString = found.OverlayHyperText;
                                }
                            }
                        }
                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    }
                    else
                    {
                        if (File.Exists(request.RawUrl[1..]))
                        {
                            buffer = File.ReadAllBytes(request.RawUrl[1..]);
                        }
                        else
                        {
                            buffer = System.Text.Encoding.UTF8.GetBytes("");
                        }
                    }

                    // Get a response stream and write the response to it.
                    response.ContentLength64 = buffer.Length;

                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // You must close the output stream.
                    output.Close();
                }
            }
            catch //(Exception ex)
            {
                //LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void StopServer()
        {
            if (HTTPListenServer.IsListening)
            {
                HTTPListenServer.Stop();
            }
        }

    }
}
