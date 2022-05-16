using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;

using TwineStreamerBot.MediaOverlayServer;
using TwineStreamerBot.MediaOverlayServer.Enums;
using TwineStreamerBot.MediaOverlayServer.Static;

namespace StreamerBotLib.BotClients
{
    internal class BotOverlayServer : IDisposable
    {
        private string MediaOverlayProcName = TwineStreamerBot.MediaOverlayServer.App.ResourceAssembly.FullName;
        private Process MediaOverlayProcess;

        private StreamWriter WriteToPipe;
        private NamedPipeServerStream PipeServer;
        //private BinaryFormatter SerializedMsg { get; } = new BinaryFormatter();

        internal void StartMediaOverlayServer()
        {    
            PipeServer = new(PublicConstants.PipeName, PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            WriteToPipe = new(PipeServer);

            MediaOverlayProcess = new Process();
            MediaOverlayProcess.StartInfo.FileName = MediaOverlayProcName;

            MediaOverlayProcess.Start();
        }

        internal void Send(OverlayTypes overlayTypes, string ActionValue, string Msg)
        {
            if (PipeServer.IsConnected)
            {
                OverlayActionType msg = new() { ActionValue = ActionValue, Message = Msg, OverlayType = overlayTypes };
                //            msg.HashCode = msg.GetHashCode();

                //            // serialize and send object over the named pipe
                //#pragma warning disable SYSLIB0011 // Type or member is obsolete
                //            SerializedMsg.Serialize(PipeServer, msg);
                //#pragma warning restore SYSLIB0011 // Type or member is obsolete

                WriteToPipe.WriteLine(msg);
            }
        }

        internal void ExitMediaOverlayProc()
        {
            _ = MediaOverlayProcess.CloseMainWindow();
        }

        public void Dispose()
        {
            MediaOverlayProcess.Dispose();
        }

    }
}
