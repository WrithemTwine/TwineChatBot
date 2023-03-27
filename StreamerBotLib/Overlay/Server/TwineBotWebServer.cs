﻿using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace StreamerBotLib.Overlay.Server
{
    public class TwineBotWebServer
    {
        private static HttpListener HTTPListenServer { get; set; } = new();
        private static Task ProcessPages = new(() => ServerSendAlerts());

        private static List<IOverlayPageReadOnly> OverlayPages { get; set; } = new();
        private static List<IOverlayPageReadOnly> TickerPages { get; set; } = new();

        public TwineBotWebServer()
        {
            static int Assign(int CheckPort)
            {
                if (CheckPort == 0)
                {
                    Random random = new();
                    return ValidatePort(random.Next(1024, 65536));
                }
                else
                {
                    return CheckPort;
                }
            }

            OptionFlags.MediaOverlayMediaActionPort = Assign(OptionFlags.MediaOverlayMediaActionPort);
            OptionFlags.MediaOverlayMediaTickerPort = Assign(OptionFlags.MediaOverlayMediaTickerPort);
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

            ProcessPages.Start();
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

        private static void ServerSendAlerts()
        {

            while (OptionFlags.ActiveToken && HTTPListenServer.IsListening)
            {
                _ = HTTPListenServer.BeginGetContext((result) =>
                {
                    try
                    {
                        HttpListenerContext context = (result.AsyncState as HttpListener).EndGetContext(result);
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
                                else if (TickerPages.Count > 1)
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
                        // must close the output stream.
                        output.Close();
                    }
                    catch { }
                }, HTTPListenServer);
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
