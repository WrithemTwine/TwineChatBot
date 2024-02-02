using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System.Reflection;
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
                    if (!OptionFlags.TwitchTokenUseAuth && OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchRefreshDate) <= new TimeSpan(0, 5, sleep / 1000))
                    {
                        NotifyExpiredCredentials?.Invoke(this, new());
                    }

                    if (OptionFlags.TwitchFollowerAutoRefresh && DateTime.Now >= TwitchFollowRefresh)
                    {
                        Controller.TwitchStartUpdateAllFollowers();
                        TwitchFollowRefresh = DateTime.Now.AddHours(TwitchFollowerCurrRefreshHrs);
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
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        #region Refresh Followers

        private void GuiTwitchBot_OnFollowerBotStarted(object sender, EventArgs e)
        {
            SetTwitchFollowerRefreshTime();
        }

        /// <summary>
        /// Initialize the DateTime used to refresh Twitch Followers in the Follower Refresh process after user specified hours.
        /// Activates with constructor and when "follow bot" is started - to prevent null/non-sensible values - and spec is 'refresh every N hours after follow bot starts".
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
            TwitchFollowerCurrRefreshHrs = hrs;
        }

        private void StatusBar_Button_UpdateFollows_Click(object sender, RoutedEventArgs e)
        {
            Controller.TwitchStartUpdateAllFollowers(true);
        }

        #endregion
        #endregion

        #region GUIAppStats
        private void ThreadManager_OnThreadCountUpdate(object sender, ThreadManagerCountArg e)
        {
            AppDispatcher.BeginInvoke(new BotOperation(() =>
            {
                guiAppStats.Threads.UpdateValue(e.AllThreadCount);
                guiAppStats.ClosedThreads.UpdateValue(e.ClosedThreadCount);
            }));
        }

        private void UpdateAppTime()
        {
            AppDispatcher.BeginInvoke(new BotOperation(() => { guiAppStats.Uptime.UpdateValue(DateTime.Now - StartBotDate); }));
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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Notification the bot tokens are now expired.");

            List<RadioButton> BotOps =
            [
                Radio_MultiLiveTwitch_StopBot,
                Radio_Twitch_FollowBotStop,
                Radio_Twitch_LiveBotStop,
                Radio_Twitch_StopBot,
                Radio_Twitch_ClipBotStop
            ];

            Dispatcher.Invoke(() =>
            {
                foreach (RadioButton button in BotOps)
                {
                    HelperStopBot(button);
                }

                CheckFocus();
            });
        }

        #endregion
    }
}
