
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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;

namespace StreamerBotLib.BotClients
{
    public class BotOverlayServer : IOModule, IDisposable, IBotTypes
    {
        private string MediaOverlayProcName = PublicConstants.AssemblyName;
        private Process MediaOverlayProcess;
        private bool PauseAlerts = false;
        private bool AlertsThreadStarted = false;
        private bool CheckedProcess = false;

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

            PipeServer = new(PublicConstants.PipeName, PipeDirection.Out, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            WriteToPipe = new(PipeServer);

            IsStarted = false;
            IsStopped = true;

            ThreadManager.CreateThreadStart(() => CheckProcess());
        }

        private void NotifyActionQueueChanged()
        {
#if VerboseLogging
            StreamerBotLib.Static.LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, "The action queue has changed.");
#endif

            ActionQueueChanged?.Invoke(this, new());
        }

        public override bool StartBot()
        {
            IsStopped = false;

            while (!CheckedProcess) { Thread.Sleep(500); } // spin until one check of the processes
            return IsStarted;
        }

        private void CheckProcess()
        {
            while (OptionFlags.ActiveToken)
            {
                if (!IsStopped && !IsStarted)
                {
                    Process[] processes = Process.GetProcessesByName(MediaOverlayProcName[..^4]);
                    if (processes.Length == 0)
                    { // process not found
                        MediaOverlayProcess = new Process();
                        MediaOverlayProcess.StartInfo.FileName = MediaOverlayProcName;
                        MediaOverlayProcess.Start();

                        IsStarted = true;
                        SetPauseAlert(false);
                        if (!AlertsThreadStarted)
                        {
                            ThreadManager.CreateThread(() => ProcessAlerts(), waitState: Enums.ThreadWaitStates.Wait, Priority: Enums.ThreadExitPriority.High).Start();
                        }

                    }
                    else
                    {
                        MediaOverlayProcess = processes[0];
                        IsStarted = true;
                    }

                    ThreadManager.CreateThreadStart(() => InvokeBotStarted());

                    if (!PipeServer.IsConnected)
                    {
                        PipeServer.BeginWaitForConnection(PipeConnected, null);
                    }
                    CheckedProcess = true;
                }

                Thread.Sleep(2000);
            }
        }

        private void PipeConnected(IAsyncResult ar)
        {
            PipeServer.EndWaitForConnection(ar);
            WriteToPipe.AutoFlush = true;
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

                while (PauseAlerts)
                {
                    Thread.Sleep(5000);
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
            IsStopped = true;

            if (MediaOverlayProcess != null && IsStarted)
            {
                IsStarted = false;
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
            if (PipeServer.IsConnected)
            {
                PipeServer.Disconnect();
            }
            
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
