using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Overlay;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System.Reflection;

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


    public class BotOverlayServer : IOModule, IBotTypes
    {
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
        /// Sets the OverlayPage to use with the overlay service
        /// </summary>
        public event EventHandler<SetOverlayWindowEventArgs> SetOverlayWindow;

        /// <summary>
        /// flag to pause alert processing
        /// </summary>
        private bool PauseAlerts = false;
        /// <summary>
        /// flag to specify alert sending thread start status
        /// </summary>
        private bool AlertsThreadStarted = false;

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

        private MediaOverlayPage OverlayPage;

        /// <summary>
        /// Build and initialize object.
        /// </summary>
        public BotOverlayServer()
        {
            BotClientName = Bots.MediaOverlayServer;

            BotEvent?.Invoke(this, new() { MethodName = BotEvents.HandleBotEventEmpty });
        }

        public void SetOverlayWindowGUI(MediaOverlayPage window)
        {
            OverlayPage = window;
            SendOverlayToServer += OverlayPage.GetOverlayActionReceivedHandler();
            SendTickerToServer += OverlayPage.GetupdatedTickerReceivedHandler();
        }

        public void ManageStreamOnlineOfflineStatus(bool Start)
        {
            if (OptionFlags.MediaOverlayStartWithStream)
            {
                if (Start)
                {
                    StartBot();
                }
                else
                {
                    StopBot();
                }
            }
        }

        /// <summary>
        /// Probably starts the bot.
        /// </summary>
        /// <returns>True, the bot has started.</returns>
        public override void StartBot()
        {
            IsActive = true;

            if (OverlayPage == null)
            {
                SetOverlayWindow?.Invoke(this, new SetOverlayWindowEventArgs() { SetOverlay = SetOverlayWindowGUI });
            }

            if (!AlertsThreadStarted)
            {
                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () => ProcessAlerts(), waitState: ThreadWaitStates.Wait, Priority: ThreadExitPriority.High);
            }

            InvokeBotStarted();
        }

        /// <summary>
        /// Manages when user closes the Overlay Server window, notify & update GUI for the closed window
        /// and turns off the bot.
        /// </summary>
        /// <param name="sender">Invoking object-unused.</param>
        /// <param name="e">Payload data-unused.</param>
        private void OverlayWindow_UserHideWindow(object sender, EventArgs e)
        {
            InvokeBotStopped();
            StopBot();
        }

        /// <summary>
        /// Should stop the bot.
        /// </summary>
        /// <returns>True for bot stopped.</returns>
        public override void StopBot()
        {
            IsActive = false;

            SendOverlayToServer -= OverlayPage?.GetOverlayActionReceivedHandler();
            SendTickerToServer -= OverlayPage?.GetupdatedTickerReceivedHandler();

            OverlayPage?.StopController();
            OverlayPage = null;

            InvokeBotStopped();
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
                SendAlerts.Enqueue(ThreadManager.CreateThread(MethodBase.GetCurrentMethod().Name, () => SendAlert(e.OverlayAction)));
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

            Thread.Sleep((int)overlayActionType.Duration * 1000); // sleep to pause and wait for the alert, to avoid collisions with next alert
        }

        #endregion

        #region Alerts

        /// <summary>
        /// Spins and sends alerts to the Overlay server, waits for alert to finish before sending another
        /// </summary>
        private void ProcessAlerts()
        {
            AlertsThreadStarted = true; // flag, this loop has started
            while (IsActive == true)
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
        public override void Send(string s)
        {
            return;
        }

        public void GetAllFollowers()
        {
        }

        public void GetAllFollowers(bool OverrideFollowers)
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
