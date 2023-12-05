#if UsePipes
#define UtilizePipeIPC // use the NamedPipe Server/Client mechanism
using System.Reflection;
#else
#define UseGUIDLL
#endif


using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.Threading;

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
        /// <summary>
        /// Interface handler, primarily for emitting events to BotController - this bot doesn't use.
        /// </summary>
        public event EventHandler<BotEventArgs> BotEvent;
        /// <summary>
        /// Alerts a change in the overlay action queue collection.
        /// </summary>
        public event EventHandler<EventArgs> ActionQueueChanged;
        /// <summary>
        /// Send overlay alert to the server.
        /// </summary>
        public event EventHandler<OverlayActionType> SendOverlayToServer;
        /// <summary>
        /// Send ticker item updates to the server.
        /// </summary>
        public event EventHandler<UpdatedTickerItemsEventArgs> SendTickerToServer;

        /// <summary>
        /// flag to pause alert processing
        /// </summary>
        private bool PauseAlerts = false;
        /// <summary>
        /// flag to specify alert sending thread start status
        /// </summary>
        private bool AlertsThreadStarted = false;
        /// <summary>
        /// tracks the open close status of the overlay server
        /// </summary>
        private bool WindowClosing = false;

        /// <summary>
        /// Queue to pipeline the alerts in an orderly and timed fashion, don't send alerts until
        /// ready (previous alert finishes).
        /// </summary>
        private Queue<Thread> SendAlerts { get; set; } = new();
        /// <summary>
        /// The alert count.
        /// </summary>
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

        /// <summary>
        /// Build and initialize object.
        /// </summary>
        public BotOverlayServer()
        {
            BotClientName = Bots.MediaOverlayServer;

            IsStarted = false;
            IsStopped = true;

            BotEvent?.Invoke(this, new() { MethodName = BotEvents.HandleBotEventEmpty.ToString() });
        }

        public void ManageStreamOnlineOfflineStatus(bool Start)
        {
            if (OptionFlags.MediaOverlayStartWithStream)
            {
                if (Start)
                {
                    _ = StartBot();
                }
                else
                {
                    _ = StopBot();
                }
            }
        }

        /// <summary>
        /// Probably starts the bot.
        /// </summary>
        /// <returns>True, the bot has started.</returns>
        public override bool StartBot()
        {
            WindowClosing = false;
            IsStopped = false;
            IsStarted = true;

            if (OverlayWindow == null)
            {
                OverlayWindow = new(OverlayWindow_UserHideWindow);
                SendOverlayToServer += OverlayWindow.GetOverlayActionReceivedHandler();
                SendTickerToServer += OverlayWindow.GetupdatedTickerReceivedHandler();
            }

            if (!AlertsThreadStarted)
            {
                ThreadManager.CreateThreadStart(() => ProcessAlerts(), waitState: ThreadWaitStates.Wait, Priority: ThreadExitPriority.High);
            }

            InvokeBotStarted();

            OverlayWindow.Show();
            return IsStarted;
        }

        /// <summary>
        /// Manages when user closes the Overlay Server window, notify & update GUI for the closed window
        /// and turns off the bot.
        /// </summary>
        /// <param name="sender">Invoking object-unused.</param>
        /// <param name="e">Payload data-unused.</param>
        private void OverlayWindow_UserHideWindow(object sender, EventArgs e)
        {
            WindowClosing = true;

            InvokeBotStopped();
            StopBot();
        }

        /// <summary>
        /// Should stop the bot.
        /// </summary>
        /// <returns>True for bot stopped.</returns>
        public override bool StopBot()
        {
            IsStarted = false;
            IsStopped = true;

            SendOverlayToServer -= OverlayWindow?.GetOverlayActionReceivedHandler();
            SendTickerToServer -= OverlayWindow?.GetupdatedTickerReceivedHandler();

            if (!WindowClosing)
            {
                OverlayWindow?.CloseApp();
            }
            OverlayWindow = null;

            InvokeBotStopped();

            return IsStopped;
        }

        #region Sending Msg mechansim

        /// <summary>
        /// Handles a new overlay action alert, sets it up in a queue to send to overlay server in an orderly manner
        /// </summary>
        /// <param name="sender">Invoking member-unused.</param>
        /// <param name="e">The data payload.</param>
        public void NewOverlayEventHandler(object sender, NewOverlayEventArgs e)
        {
            lock (SendAlerts)
            {
                SendAlerts.Enqueue(ThreadManager.CreateThread(() => SendAlert(e.OverlayAction)));
                NotifyActionQueueChanged();
            }
        }

        /// <summary>
        /// Sends ticker information to the overlay server, mainly when there's an updated change.
        /// </summary>
        /// <param name="sender">The invoking object.</param>
        /// <param name="e">The data payload to send.</param>
        public void UpdatedTickerEventHandler(object sender, UpdatedTickerItemsEventArgs e)
        {
            SendTickerToServer?.Invoke(sender, e);
        }

        /// <summary>
        /// Notify when the queue holding the alert actions has changed.
        /// </summary>
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

#if LOG_OVERLAY
            LogWriter.OverlayLog(MethodBase.GetCurrentMethod().Name, $"BotOverlayServer - sending {overlayActionType.OverlayType} to the Overlay Server.");
#endif

            Thread.Sleep(overlayActionType.Duration * 1000); // sleep to pause and wait for the alert, to avoid collisions with next alert
        }

        #endregion
#endif

        #region Alerts

        /// <summary>
        /// Spins and sends alerts to the Overlay server, waits for alert to finish before sending another
        /// </summary>
        private void ProcessAlerts()
        {
            AlertsThreadStarted = true; // flag, this loop has started
            while (IsStarted)
            {
                lock (SendAlerts)
                {
                    if (SendAlerts.Count > 0)
                    {
                        Thread Next = SendAlerts.Dequeue();
                        Next.Start();
                        Next.Join(); // sleep inside thread action, wait until it completes

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
        /// User selects a pause to hold up alerts.
        /// </summary>
        /// <param name="Alert">true or false to hold alerts</param>
        public void SetPauseAlert(bool Alert)
        {
            PauseAlerts = Alert;
        }

        /// <summary>
        /// Clear any of the pending alerts.
        /// </summary>
        public void SetClearAlerts()
        {
            lock (SendAlerts)
            {
                SendAlerts.Clear();
            }
        }

        #endregion

        #region Stop Bots

        /// <summary>
        /// Most likely, this will stop the bot.
        /// </summary>
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
