
using MediaOverlayServer.Properties;
using MediaOverlayServer.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace MediaOverlayServer.Server
{
    public class TwineBotWebServer
    {
        private HttpListener HTTPListenServer { get; set; } = new();

        private Queue<string> OverlayPages { get; set; } = new();

        public TwineBotWebServer()
        {
            if (OptionFlags.MediaOverlayPort == 0)
            {
                Random random = new();
                int port = random.Next(1024, 65536);

                while (!IsFree(port))
                {
                    port++;
                }

                Settings.Default.MediaOverlayPort = port;
                OptionFlags.SetSettings();
            }
        }



        // ports: 1 - 65535
        private bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray();
            return openPorts.All(openPort => openPort != port);
        }

        public void StartServer()
        {


            HTTPListenServer.Start();

            new Thread(new ThreadStart(ServerSendAlerts)).Start();
        }

        private void ServerSendAlerts()
        {
            while (HTTPListenServer.IsListening)
            {
                HttpListenerContext context = HTTPListenServer.GetContext();
                HttpListenerRequest request = context.Request;
                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";

                lock (OverlayPages)
                {
                    if(OverlayPages.Count > 0)
                    {
                        string test = OverlayPages.Peek();
                    }
                }
                
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
        }

        internal void StopServer()
        {
            if (HTTPListenServer.IsListening)
            {
                HTTPListenServer.Stop();
            }
        }

    }
}
