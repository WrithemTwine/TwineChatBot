using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        #region WatcherTools

        private bool WatchProcessOps;

        /// <summary>
        /// Handler to stop the bots when the credentials are expired. The thread acting on the bots must be the GUI thread, hence this notification.
        /// </summary>
        public event EventHandler NotifyExpiredCredentials;

        public event EventHandler VerifyNewVersion;

        private delegate void ProcWatch(bool IsActive);

        private void ProcessWatcher()
        {
            const int NewVersionIntervalHours = 18;
            DateTime VersionCheckDate = DateTime.Now.AddHours(NewVersionIntervalHours);

            const int sleep = 2000;

            try
            {
                while (WatchProcessOps)
                {
                    if (!OptionFlags.TwitchTokenUseAuth && OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchBotTokenDate) <= new TimeSpan(0, 5, sleep / 1000))
                    {
#if DEBUG
#else
                        NotifyExpiredCredentials?.Invoke(this, new());
#endif
                    }

                    if (OptionFlags.TwitchFollowerAutoRefresh && DateTime.Now >= TwitchFollowRefresh)
                    {
                        Controller.TwitchStartUpdateAllFollowers();
                        TwitchFollowRefresh = DateTime.Now.AddHours(OptionFlags.TwitchFollowerRefreshHrs);
                    }

                    if (DateTime.Now >= VersionCheckDate)
                    {
                        VersionCheckDate.AddHours(NewVersionIntervalHours);

                        VerifyNewVersion?.Invoke(this, new());
                    }

                    UpdateAppTime();

                    Thread.Sleep(sleep);
                }
            }
            catch (ThreadInterruptedException ex) // will always throw exception when exiting during a Sleep
            {
                LogWriter.LogException(ex, "ProcessWatcher");
            }
        }

        #region Refresh Followers

        private void GuiTwitchBot_OnBulkFollowerStopped(object sender, EventArgs e)
        {
            SetTwitchFollowerRefreshTime();
        }

        /// <summary>
        /// Initialize the DateTime used to refresh Twitch Followers in the Follower Refresh process after user specified hours.
        /// Refreshes after the bulk follow process completes, either after EventSub starts and performs the bulk follow 
        /// loading or the user clicks the "Follower" button on the GUI to initiate a bulk follower load.
        /// </summary>
        private void SetTwitchFollowerRefreshTime()
        {
            TwitchFollowRefresh = DateTime.Now.AddHours(OptionFlags.TwitchFollowerRefreshHrs);
        }

        private void ComboBox_TwitchFollower_RefreshHrs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox srchrs = sender as ComboBox;
            int hrs = (int)srchrs.SelectedValue;

            // changes the refresh time - which is already set at this point
            TwitchFollowRefresh = TwitchFollowRefresh.AddHours(hrs - TwitchFollowerCurrRefreshHrs);
            TwitchFollowerCurrRefreshHrs = OptionFlags.TwitchFollowerRefreshHrs;
        }

        private void StatusBar_Button_UpdateFollows_Click(object sender, RoutedEventArgs e)
        {
            Controller.TwitchStartUpdateAllFollowers();
        }

        #endregion
        #endregion

        #region GUIAppStats
        private void ThreadManager_OnThreadCountUpdate(object sender, ThreadManagerCountArg e)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                _ = new BotOperation(() =>
                {
                    guiAppStats.Threads.UpdateValue(e.AllThreadCount);
                    guiAppStats.ClosedThreads.UpdateValue(e.ClosedThreadCount);
                });
            }
            );
        }

        private void UpdateAppTime()
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                _ = new BotOperation(() => { guiAppStats.Uptime.UpdateValue(DateTime.Now - StartBotDate); });
            }
            );
        }

        #endregion

        /// <summary>
        /// A running watcher thread checks for elapsed time, and raises an event when current time exceeds the 
        /// time to check for another version; and not just when the application starts - the user can have the 
        /// application open for weeks and would know of a new version without restarting it.
        /// Handles the event when it's time to check for a new version.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StreamerBotWindow_VerifyNewVersion(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                // navigate to the predefined stable link.
                WebView2_GitHub_StableVersion.NavigateToString(OptionFlags.GitHubStableLink);
            });
        }


        #region BotOps-changes in token expiration

        /// <summary>
        /// Event to handle when the Bot Credentials expire. The expiration date 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BotWindow_NotifyExpiredCredentials(object sender, EventArgs e)
        {
            LogWriter.DebugLog("BotWindow_NotifyExpiredCredentials", DebugLogTypes.GUIEvents, "Notification the bot tokens are now expired.");

            List<RadioButton> BotOps =
            [
                Radio_Twitch_ClipBotStop,
                Radio_Twitch_LiveBotStop,
            ];

            Dispatcher.Invoke(() =>
            {
                foreach (RadioButton button in BotOps)
                {
                    HelperStopBot(button);
                }

                LogWriter.DebugLog("BotWindow_NotifyExpiredCredentials", DebugLogTypes.GUIEvents, "Stopped all bots due to expired tokens.");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            });
        }

        #endregion
    }
}
