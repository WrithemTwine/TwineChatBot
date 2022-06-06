#if UsePipes
#define UtilizePipeIPC // use the NamedPipe Server/Client mechanism
#else
#define UseGUIDLL
#endif


#if UtilizePipeIPC
using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;

using MediaOverlayServer.Enums;
using MediaOverlayServer.Models;
using MediaOverlayServer.Static;

namespace MediaOverlayServer.Communication
{
    internal class OverlaySvcClientPipe
    {
        internal event EventHandler<OverlayActionType> ReceivedOverlayEvent;
        internal event EventHandler<bool> PipeReceivedStoppedEvent;

        private StreamReader ReadFromPipe;

        private NamedPipeClientStream OverlayPipe;

        internal OverlaySvcClientPipe()
        {
            OverlayPipe = new(".", PublicConstants.PipeName, PipeDirection.In, PipeOptions.Asynchronous);

            StartPipeWatchAsync();
        }

        internal async void StartPipeWatchAsync()
        {
            await OverlayPipe.ConnectAsync();
            ReadFromPipe = new(OverlayPipe);

            new Thread(new ThreadStart(() => WatchPipe())).Start();
        }

        internal async void WatchPipe()
        {
            while (OptionFlags.ActiveToken)
            {
                if (OverlayPipe.IsConnected)
                {
                    if (ReadFromPipe.Peek() != -1)
                    {
                        while (!ReadFromPipe.EndOfStream)
                        {
                            try
                            {
                                OverlayActionType curr = OverlayActionType.FromString(ReadFromPipe.ReadLine());

                                if (curr.OverlayType != OverlayTypes.None)
                                {
                                    foundReceivedOverlayEvent(curr);
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                LogWriter.LogException(ex, Method: MethodBase.GetCurrentMethod().Name);

                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
                else
                {
                    await OverlayPipe.ConnectAsync();
                }
            }

            PipeReceivedStoppedEvent?.Invoke(this, true);
            ReadFromPipe.Close();
            OverlayPipe.Close();
        }

        internal void foundReceivedOverlayEvent(OverlayActionType overlayAction)
        {
            ReceivedOverlayEvent?.Invoke(this, overlayAction);
        }
    }
}
#endif

