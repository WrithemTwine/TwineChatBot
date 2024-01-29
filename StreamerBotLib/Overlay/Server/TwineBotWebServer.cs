using StreamerBotLib.Enums;
using StreamerBotLib.Overlay.Communication;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Interfaces;
using StreamerBotLib.Static;

using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace StreamerBotLib.Overlay.Server
{
    public class TwineBotWebServer
    {
        /// <summary>
        /// Server maintained in this class.
        /// </summary>
        private static HttpListener HTTPListenServer { get; set; } = new();
        /// <summary>
        /// Holds a Task for all of the async listeners.
        /// </summary>
        private static Task ProcessPages;

        /// <summary>
        /// The alert pages collection.
        /// </summary>
        private static List<IOverlayPageReadOnly> OverlayPages { get; set; } = [];
        /// <summary>
        /// The ticker pages collection.
        /// </summary>
        private static List<IOverlayPageReadOnly> TickerPages { get; set; } = [];

        /// <summary>
        /// Instantiate and initialize a new object.
        /// </summary>
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

        /// <summary>
        /// Checks if the specific port is free on the system. Can't conflict with existing port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <returns>The provided port or next port determined availabe within the system.</returns>
        public static int ValidatePort(int port)
        {
            while (!IsFree(port))
            {
                port++;
            }

            return port;
        }

        /// <summary>
        /// Reviews the existing ports, finds whether the provided port is already open.
        /// Can't open a connection to a port already in use.
        /// </summary>
        /// <param name="port">The port to check within the system.</param>
        /// <returns>False if the port is already in use, true if port is free.</returns>
        // ports: 1 - 65535
        private static bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray();
            return openPorts.All(openPort => openPort != port);
        }

        /// <summary>
        /// Adds the port prefixes and starts the HTTP server.
        /// </summary>
        public static void StartServer()
        {
            HTTPListenServer.Prefixes.Clear();
            foreach (string P in PrefixGenerator.GetPrefixes())
            {
                HTTPListenServer.Prefixes.Add(P);
            }

            ThreadManager.CreateThreadStart(() =>
            {
                HTTPListenServer.Start();
                ProcessPages = new(() => ServerSendAlerts());
                ProcessPages.Start();
            });
        }

        /// <summary>
        /// Adds an alert webpage to send out to any connected webpage.
        /// </summary>
        /// <param name="overlayPage">Contains the type of alert and the html text to send.</param>
        public static void SendAlert(IOverlayPageReadOnly overlayPage)
        {
            if (HTTPListenServer.IsListening)
            {
                lock (OverlayPages)
                {
                    OverlayPages.Add(overlayPage);
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"http server - Overlay alert, {overlayPage.OverlayType}, added and awaiting to be served.");
                }
            }
        }

        /// <summary>
        /// Clear and replace all TickerPages when the user changes the visual ticker appearance.
        /// </summary>
        /// <param name="UpdateTickerPages">All of the new ticker pages to replace the current ticker pages.</param>
        public static void UpdateTicker(IEnumerable<IOverlayPageReadOnly> UpdateTickerPages)
        {
            lock (TickerPages)
            {
                TickerPages.Clear();
                TickerPages.AddRange(UpdateTickerPages);
            }
        }

        public static void UpdateTicker(IOverlayPageReadOnly UpdateTickerPage)
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

        /// <summary>
        /// Defines an async listener action for the http server, which then actually serves the Content to any connected browsers. 
        /// Also, determines if the URL is for the tickers or alerts, and uses the respective collections for the outgoing html text.
        /// 
        /// While the bot is still active and the server is 'listening' for requests, method spins a while loop to add listeners as they
        /// get used from requests, and waits 200ms per spin.
        /// 
        /// The current listener count is: <see cref="PrefixGenerator.LinkCount"/> + 5 listeners.
        /// </summary>
        private static void ServerSendAlerts()
        {
            string Lock = "";

            int ResponseCount = 0;

            Action ResponseListen = new(() =>
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

                        //LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"http server - received request, {request.RawUrl}");

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

                        //LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"http server - finished sending request.");

                        // must close the output stream.
                        output.Close();

                        lock (Lock)
                        {
                            ResponseCount--; // listener is finished, lower active listener count
                        }
                    }
                    catch { }
                }, HTTPListenServer);
            });

            // spin until application is stopped or the server is stopped
            while (OptionFlags.ActiveToken && HTTPListenServer.IsListening)
            {
                // keep adding listeners until there are 5 more than the served URLs, counting off base 0
                if (ResponseCount < PrefixGenerator.LinkCount + 5)
                {
                    lock (Lock) // lock, cause the counter is across different threads
                    {
                        ResponseCount++; // increase count for active listeners
                    }
                    ThreadManager.CreateThreadStart(() => ResponseListen.Invoke());
                    //LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"http server - Adding more http server listening threads, now {ResponseCount}.");
                }
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Looks like it stops the server. Then, waits for all of the server listeners to finish sending.
        /// </summary>
        public static void StopServer()
        {
            if (HTTPListenServer.IsListening)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"http server - Overlay http server stopping.");

                HTTPListenServer.Stop();
                ProcessPages.Wait();
            }
        }

    }
}
