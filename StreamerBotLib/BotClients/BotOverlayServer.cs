using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MediaOverlayServer.Enums;
using MediaOverlayServer.Static;
using MediaOverlayServer.Models;

namespace StreamerBotLib.BotClients
{
    public class BotOverlayServer : IOModule, IDisposable, IBotTypes
    {
        private string MediaOverlayProcName = PublicConstants.AssemblyName;
        private Process MediaOverlayProcess;
        private bool PauseAlerts = false;
        private bool AlertsThreadStarted = false;
        private bool ClosedByProcess = false;

        private StreamWriter WriteToPipe;
        private NamedPipeServerStream PipeServer;

        private Queue<Thread> SendAlerts { get; set; } = new();

        public int MediaItems
        {
            get
            {
                lock (SendAlerts)
                {
                    return SendAlerts.Count;
                }
            }
        }

        public event EventHandler<BotEventArgs> BotEvent;

        public event EventHandler<EventArgs> ActionQueueChanged;

        //private BinaryFormatter SerializedMsg { get; } = new BinaryFormatter();

        public BotOverlayServer()
        {
            BotClientName = Enums.Bots.MediaOverlayServer;

            PipeServer = new(PublicConstants.PipeName, PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            WriteToPipe = new(PipeServer);
        }

        private void NotifyActionQueueChanged()
        {
            ActionQueueChanged?.Invoke(this, new());
        }

        public override bool StartBot()
        {
            StartMediaOverlayServer();
            return IsStarted;
        }

        internal void StartMediaOverlayServer(bool StartProcess = true)
        {
            ThreadManager.CreateThreadStart(() =>
            {
                if (StartProcess)
                {
                    MediaOverlayProcess = new Process();
                    MediaOverlayProcess.StartInfo.FileName = MediaOverlayProcName;
                    MediaOverlayProcess.Start();
                }

                PipeServer.BeginWaitForConnection(null,null);

                IsStarted = true;
                IsStopped = false;
                SetPauseAlert(false);

                if (!AlertsThreadStarted)
                {
                    ThreadManager.CreateThreadStart(() => ProcessAlerts(), waitState: Enums.ThreadWaitStates.Wait, Priority: Enums.ThreadExitPriority.High);
                }

                InvokeBotStarted();
            });
        }

        private void CheckProcess()
        {
            Process[] processes = Process.GetProcessesByName(MediaOverlayProcName[..^4]);
            if (processes.Length == 0)
            {
                // if user closes process, detect and report service stopped
                ClosedByProcess = true;
                StopBot();
                ClosedByProcess = false;
            }
            else if (!IsStarted && IsStopped)
            {
                MediaOverlayProcess = processes[0];
                StartMediaOverlayServer(false);
            }
        }

        public void NewOverlayEventHandler(object sender, NewOverlayEventArgs e)
        {
            Send(e.OverlayAction);
        }

        private void Send(OverlayActionType overlayActionType)
        {
            Send(overlayActionType.OverlayType, overlayActionType.ActionValue, overlayActionType.UserName, overlayActionType.Message, overlayActionType.Duration, overlayActionType.MediaFile);
        }

        private void Send(OverlayTypes overlayTypes, string ActionValue, string User, string Msg, int Duration, string MediaPath)
        {
            lock (SendAlerts)
            {
                SendAlerts.Enqueue(
                   ThreadManager.CreateThread(
                       () => SendAlert(
                           new() { OverlayType = overlayTypes, Duration = Duration, UserName = User, ActionValue = ActionValue, Message = Msg, MediaFile = MediaPath }
                           )
                       )
                    );
                NotifyActionQueueChanged();
            }
        }

        private void ProcessAlerts()
        {
            AlertsThreadStarted = true; // flag, this loop has started
            while (IsStarted)
            {
                lock (SendAlerts)
                {
                    if (SendAlerts.Count > 0)
                    {
                        SendAlerts.Dequeue().Start(); // sleep inside thread action
                        NotifyActionQueueChanged();
                    }
                }

                // sleep for 1 sec before checking for next alert
                Thread.Sleep(1000);

                CheckProcess();

                while (PauseAlerts)
                {
                    Thread.Sleep(5000);
                    CheckProcess();
                }
            }
            AlertsThreadStarted = false;
        }

        /// <summary>
        /// The thread action to send the data to the server
        /// </summary>
        /// <param name="overlayActionType">Contains the data for the alert.</param>
        private void SendAlert(OverlayActionType overlayActionType)
        {
            SendToServer(overlayActionType);
            Thread.Sleep(overlayActionType.Duration * 1000); // sleep to pause and wait for the alert, to avoid collisions with next alert
        }
  
        internal void SendToServer(OverlayActionType Message)
        {
            if (PipeServer.IsConnected)
            {
                //OverlayActionType msg = new() { ActionValue = ActionValue, Message = Msg, OverlayType = overlayTypes };
                //            msg.HashCode = msg.GetHashCode();

                //            // serialize and send object over the named pipe
                //#pragma warning disable SYSLIB0011 // Type or member is obsolete
                //            SerializedMsg.Serialize(PipeServer, msg);
                //#pragma warning restore SYSLIB0011 // Type or member is obsolete

                WriteToPipe.WriteLine(Message);
            }
        }

        public void SetPauseAlert(bool Alert)
        {
            PauseAlerts = Alert;
        }

        public void SetClearAlerts()
        {
            lock (SendAlerts)
            {
                SendAlerts.Clear();
            }
        }

        public void Dispose()
        {
            MediaOverlayProcess.Dispose();
        }

        public void StopBots()
        {
            StopBot();
        }

        public override bool StopBot()
        {
            IsStarted = false;
            IsStopped = true;

            if (!ClosedByProcess && MediaOverlayProcess != null)
            {
                MediaOverlayProcess.CloseMainWindow();
                Dispose();
            }
            if (!OptionFlags.ActiveToken)
            {
                SetPauseAlert(false);
            }
            else
            {
                SetPauseAlert(true);
            }
            InvokeBotStopped();
            return IsStopped;
        }

        #region unused interface
        public override bool Send(string s)
        {
            return false;
        }

        public void GetAllFollowers()
        {
        }

        public void SetIds()
        {
        }

        void IBotTypes.Send(string s)
        {

        }

        #endregion
    }
}
