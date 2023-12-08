using StreamerBotLib.Static;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
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

        /// <summary>
        /// True - "MultiUserLiveBot.exe" is active, False - "MultiUserLiveBot.exe" is not active
        /// </summary>
        private bool? IsMultiProcActive { get; set; }

        private delegate void ProcWatch(bool IsActive);

        private void UpdateProc(bool IsActive)
        {
            _ = AppDispatcher.BeginInvoke(new ProcWatch(SetMultiLiveActive), IsActive);
        }

        private void ProcessWatcher()
        {
            const int NewVersionIntervalHours = 18;
            DateTime VersionCheckDate = DateTime.Now.AddHours(NewVersionIntervalHours);

            const int sleep = 2000;

            try
            {
                while (WatchProcessOps)
                {
                    Process[] processes = Process.GetProcessesByName(MultiLiveName);
                    if ((processes.Length > 0) != IsMultiProcActive) // only change IsMultiProcActive when the process activity changes
                    {
                        UpdateProc(processes.Length > 0);
                        IsMultiProcActive = processes.Length > 0;
                    }

                    if (OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchRefreshDate) <= new TimeSpan(0, 5, sleep / 1000))
                    {
                        NotifyExpiredCredentials?.Invoke(this, new());
                    }

                    if (OptionFlags.TwitchFollowerAutoRefresh && DateTime.Now >= TwitchFollowRefresh)
                    {
                        Controller.TwitchStartUpdateAllFollowers();
                        TwitchFollowRefresh = DateTime.Now.AddHours(TwitchFollowerCurrRefreshHrs);
                    }

                    if(DateTime.Now >= VersionCheckDate)
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
            Controller.TwitchStartUpdateAllFollowers();
        }

        #endregion 
        #endregion


    }
}
