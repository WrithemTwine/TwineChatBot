#if UsePipes
#define UtilizePipeIPC // use the NamedPipe Server/Client mechanism
#else
#define UseGUIDLL
#endif


using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Threading;

using OptionFlags = StreamerBotLib.Static.OptionFlags;

namespace StreamerBotLib.BotClients
{
    /*
     * This class originally implemented a named pipe server to send messages to a receiving .exe operating a webserver client. 
     * Re-working this class so the webserver operates in a .dll and this class spawns the main window and subsequent server process.
     * 
     * MediaOverlayServer needs to be modified:
     * 1) as .exe - use named pipe server
     * 2) as .dll - open the main window here, directly connect the alert mechanism
     *
     */

#if UtilizePipeIPC
    public class BotOverlayServer : IOModule, IDisposable, IBotTypes
    {
#elif UseGUIDLL
    public class BotOverlayServer : IOModule, IBotTypes
    {
#endif

        public event EventHandler<BotEventArgs> BotEvent;
        public event EventHandler<EventArgs> ActionQueueChanged;
        public event EventHandler<OverlayActionType> SendOverlayToServer;
        private bool PauseAlerts = false;
        private bool AlertsThreadStarted = false;

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

#if UtilizePipeIPC
        private string MediaOverlayProcName = PublicConstants.AssemblyName;
        private Process MediaOverlayProcess;
        private bool CheckedProcess = false;

        private StreamWriter WriteToPipe;
        private NamedPipeServerStream PipeServer;

        public BotOverlayServer()
        {
            BotClientName = Enums.Bots.MediaOverlayServer;

            PipeServer = new(PublicConstants.PipeName, PipeDirection.Out, 254, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            WriteToPipe = new(PipeServer);

            IsStarted = false;
            IsStopped = true;

            ThreadManager.CreateThreadStart(() => CheckProcess());
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
          
        internal void SendToServer(OverlayActionType Message)
        {
            if (PipeServer.IsConnected)
            {
                WriteToPipe.WriteLine(Message);
            }
        }

        public void Dispose()
        {
            MediaOverlayProcess.Dispose();
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

        #region Sending Msg mechansim
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

        private void NotifyActionQueueChanged()
        {
            ActionQueueChanged?.Invoke(this, new());
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

        #endregion

#elif UseGUIDLL
        private MainWindow OverlayWindow;

        public BotOverlayServer()
        {
            BotClientName = Enums.Bots.MediaOverlayServer;

            IsStarted = false;
            IsStopped = true;
        }

        public override bool StartBot()
        {
            IsStopped = false;
            IsStarted = true;

            if (OverlayWindow == null)
            {
                OverlayWindow = new(OverlayWindow_UserHideWindow);
                SendOverlayToServer += OverlayWindow.GetOverlayActionReceivedHandler();
            }

            if (!AlertsThreadStarted)
            {
                ThreadManager.CreateThread(() => ProcessAlerts(), waitState: Enums.ThreadWaitStates.Wait, Priority: Enums.ThreadExitPriority.High).Start();
            }

            InvokeBotStarted();

            OverlayWindow.Show();
            return IsStarted;
        }

        private void OverlayWindow_UserHideWindow(object sender, EventArgs e)
        {
            InvokeBotStopped();
        }

        public override bool StopBot()
        {
            IsStarted = false;
            IsStopped = true;

            if (!OptionFlags.ActiveToken)
            {
                OverlayWindow?.CloseApp();
            }
            else
            {
                OverlayWindow?.Hide();
            }

            InvokeBotStopped();

            return IsStopped;
        }

        #region Sending Msg mechansim
        public void NewOverlayEventHandler(object sender, NewOverlayEventArgs e)
        {
            lock (SendAlerts)
            {
                SendAlerts.Enqueue(ThreadManager.CreateThread(() => SendAlert(e.OverlayAction)));
                NotifyActionQueueChanged();
            }
        }

        private void NotifyActionQueueChanged()
        {
            ActionQueueChanged?.Invoke(this, new());
        }

        /// <summary>
        /// The thread action to send the data to the server
        /// </summary>
        /// <param name="overlayActionType">Contains the data for the alert.</param>
        private void SendAlert(OverlayActionType overlayActionType)
        {
            SendOverlayToServer?.Invoke(this, overlayActionType);
            Thread.Sleep(overlayActionType.Duration * 1000); // sleep to pause and wait for the alert, to avoid collisions with next alert
        }

        #endregion
#endif

        #region Alerts

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

        #endregion

        #region Stop Bots

        public void StopBots()
        {
            StopBot();
        }

        #endregion


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
