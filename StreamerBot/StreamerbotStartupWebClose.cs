using Microsoft.Web.WebView2.Wpf;

using StreamerBotLib.BotClients;
using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace StreamerBot
{
    public partial class StreamerBotWindow
    {
        private void CheckSettings()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            if (Settings.Default.AppCurrWorkingAppData)
            {
                Directory.CreateDirectory(GetAppDataCWD());
                Directory.SetCurrentDirectory(GetAppDataCWD());
            }
        }

        /// <summary>
        /// Get the application current working directory
        /// </summary>
        /// <returns>The user'AppVersion local application data path to store application save data.</returns>
        private static string GetAppDataCWD()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name, "Data");
        }

        /// <summary>
        /// Called from constructor, sets events 
        /// </summary>
        private void ConstructEvents()
        {
            guiTwitchBot.OnBotStopped += GUI_OnBotStopped;
            guiTwitchBot.OnBotStarted += GUI_OnBotStarted;
            guiTwitchBot.OnBotStarted += GuiTwitchBot_GiveawayEvents;
            guiTwitchBot.OnBotFailedStart += GuiTwitchBot_OnBotFailedStart;
            guiTwitchBot.OnFollowerBotStarted += GuiTwitchBot_OnFollowerBotStarted;
            guiTwitchBot.OnLiveStreamStarted += GuiTwitchBot_OnLiveStreamStarted;
            guiTwitchBot.OnLiveStreamStarted += GuiTwitchBot_OnLiveStreamEvent;
            guiTwitchBot.OnLiveStreamUpdated += GuiTwitchBot_OnLiveStreamEvent;
            guiTwitchBot.OnLiveStreamStopped += GuiTwitchBot_OnLiveStreamStopped;
            guiTwitchBot.RegisterChannelPoints(TwitchBotUserSvc_GetChannelPoints);

            guiAppServices.AppDataDirectory = GetAppDataCWD();
            guiAppServices.OnBotStarted += GUI_OnBotStarted;
            guiAppServices.OnBotStopped += GUI_OnBotStopped;

            Controller.OnStreamCategoryChanged += BotEvents_GetChannelGameName;
            Controller.InvalidAuthorizationToken += Controller_InvalidAuthorizationToken;

            ThreadManager.OnThreadCountUpdate += ThreadManager_OnThreadCountUpdate;

            NotifyExpiredCredentials += BotWindow_NotifyExpiredCredentials;
            VerifyNewVersion += StreamerBotWindow_VerifyNewVersion;
        }


        #region GitHub webpage
        /// <summary>
        /// Handles a WebView GUI control navigation, when it completes. The GitHub link resolves to a 
        /// link to a stable release link. Using this result, we can determine if a new stable version
        /// is available to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView2_GitHub_StableVersion_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            string NewVersionLink = WebView2_GitHub_StableVersion.Source.ToString();

            // "https://github.com/WrithemTwine/TwineChatBot/releases/tag/v.1.2.10.0"
            if (NewVersionLink != OptionFlags.GitHubCheckStable)
            {
                OptionFlags.GitHubCheckStable = NewVersionLink;
            }

            string newversion = (from s in NewVersionLink.Split('/')
                                 select s).Last().Split('_').FirstOrDefault();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string AppVersion = $"v.{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            // check if the saved link is the default, also check if the found link doesn't have the current version
            // true=> link not default and the stable version link doesn't have the current app version in it
            if (OptionFlags.GitHubCheckStable != OptionFlags.GitHubStableLink && AppVersion.CompareTo(newversion) < 0)
            {
                StatusBarItem_NewStableVersion.Visibility = Visibility.Visible;
            }
        }

        private void WebView2Reload_Click(object sender, RoutedEventArgs e)
        {
            (((sender as Button).Parent as StackPanel).Children[1] as WebView2).Reload();
        }

        #endregion
        #region Window Open and Close

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Begin Window Loaded events.");

            ToggleButton_ChooseTwitchAuth_Click(this, null);

            CheckFocus();
            StartAutoBots();

            CheckMessageBoxes();
            CheckBox_ManageData_Click(sender, new());
            CheckBox_TabifySettings_Clicked(this, new());
            CheckDebug(this, new());
            SetVisibility(this, new());
            CheckFocus();

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "End Window Loaded events.");
        }

        /// <summary>
        /// Check each bot if it's enabled-the credentials are proper- and start any bot the user selected to start when the app is loaded.
        /// Also, when the Twitch authentication token method changes, need to restart any bots if the tokens are available.
        /// </summary>
        private void StartAutoBots()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIHelpers, "Attempting to start bots.");

            if ((OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchRefreshDate) >= CheckRefreshDate
                && !OptionFlags.TwitchTokenUseAuth) // when not using the Twitch auth token method
            ||
                (OptionFlags.TwitchTokenUseAuth // when using the Twitch auth token method
                                                // check the bot/streamer tokens are available assigned
                && (!OptionFlags.TwitchStreamerUseToken && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAccessToken)
                 || (OptionFlags.TwitchStreamerUseToken
                     && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAccessToken)
                     && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAccessToken))
                   )
                )
            )
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIHelpers, "The access tokens are available and ready to start bots.");
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIHelpers, "Starting any bots when the user checked 'auto-start bots'.");

                BotController.InitializeHelix();

                foreach (Tuple<bool, RadioButton> tuple in from Tuple<bool, RadioButton> tuple in BotOps
                                                           where tuple.Item1 && tuple.Item2.IsEnabled
                                                           select tuple)
                {
                    Dispatcher.BeginInvoke(new BotOperation(() =>
                    {
                        (tuple.Item2.DataContext as IOModule)?.StartBot();
                    }));
                }

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIHelpers, "Finished starting bots and beginning to update category.");

                BeginUpdateCategory();
            }
        }

        /// <summary>
        /// When the user clicks the close button to the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Closing the application. Setting flags to end threaded procedures.");

            WatchProcessOps = false;
            OptionFlags.IsStreamOnline = false;
            OptionFlags.ActiveToken = false;

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Sending an exit to the bot controller.");
            Controller.ExitBots();

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Exiting the log writers. No more logging available.");
            LogWriter.ExitCloseLogs();
        }

        #endregion


    }
}
