using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;

namespace TwineStreamerBot.MediaOverlayServer
{
    internal class OverlaySvcClientPipe
    {
        internal event EventHandler<OverlayActionType> ReceivedOverlayEvent;
        internal event EventHandler<bool> PipeReceivedStoppedEvent;

        private StreamReader ReadFromPipe;

        private NamedPipeClientStream OverlayPipe;

        internal OverlaySvcClientPipe()
        {
            OverlayPipe = new(".", PublicConstants.PipeName, PipeDirection.In,PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        }

        internal async void StartPipeWatch()
        {
            await OverlayPipe.ConnectAsync();
            ReadFromPipe = new(OverlayPipe);

            new Thread(new ThreadStart(() => WatchPipe())).Start();
        }

        internal void WatchPipe()
        {
            //BinaryFormatter SerializedMsg = new BinaryFormatter();

            while (OptionFlags.ActiveToken && OverlayPipe.IsConnected)
            {
                //                // reading from a named system pipe
                //#pragma warning disable SYSLIB0011 // Type or member is obsolete
                //                OverlayActionType received = (OverlayActionType)SerializedMsg.Deserialize(OverlayPipe);
                //#pragma warning restore SYSLIB0011 // Type or member is obsolete

                //                if(received.HashCode == received.GetHashCode())
                //                {
                //                    foundReceivedOverlayEvent(received);
                //                }

                while (!ReadFromPipe.EndOfStream)
                {
                    string? curr = ReadFromPipe.ReadLine();
                }
            }

            PipeReceivedStoppedEvent?.Invoke(this, true);
            ReadFromPipe.Close();
        }

        internal void foundReceivedOverlayEvent(OverlayActionType overlayAction)
        {
            ReceivedOverlayEvent?.Invoke(this, overlayAction);
        }
    }
}
