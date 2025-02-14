using Microsoft.Web.WebView2.Wpf;

using StreamerBotLib.BotClients;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
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
                Version thisversion = this.GetType().Assembly.GetName().Version;

                if (thisversion.Major == 1 && thisversion.MajorRevision == 3 && thisversion.Minor == 1 && thisversion.MinorRevision == 3)
                { // reset the credentials, new access scopes for each token
                    Settings.Default.TwitchAuthBotClientId = (string)Settings.Default.GetPreviousVersion("TwitchAuthClientId"); ;

                    Settings.Default.TwitchAuthBotAccessToken = null;
                    Settings.Default.TwitchAuthBotAuthCode = null;
                    Settings.Default.TwitchAuthBotRefreshToken = null;
                    Settings.Default.TwitchAuthStreamerAccessToken = null;
                    Settings.Default.TwitchAuthStreamerAuthCode = null;
                    Settings.Default.TwitchAuthStreamerRefreshToken = null;

                    Settings.Default.TwitchBotAccessToken = null;
                    Settings.Default.TwitchStreamerAccessToken = null;
                }

                Settings.Default.Save();
            }

            if (Settings.Default.AppCurrWorkingAppData)
            {
                Directory.CreateDirectory(GetAppDataCWD());
                Directory.SetCurrentDirectory(GetAppDataCWD());
            }
        }

        private void SetDatabaseChoice()
        {
            if (!
#if DEBUG || DEBUG_VIEWXAML || RELEASE_SQLITE
            OptionFlags.EFCDatabaseProviderSqlite
#elif RELEASE_POSTGRE
            OptionFlags.EFCDatabaseProviderPostgreSQL
#elif RELEASE_SQLSERVER
            OptionFlags.EFCDatabaseProviderSqlServer
#elif RELEASE_KNET
            OptionFlags.EFCDatabaseProviderKNet
#elif RELEASE_COSMOS
            OptionFlags.EFCDatabaseProviderCosmos
#elif RELEASE_MYSQL || RELEASE_POMELOMYSQL
            OptionFlags.EFCDatabaseProviderMySql
#endif
            )
            {
                ChooseDatabase chooseDatabase = new();
                chooseDatabase.ExitApp += ChooseDatabase_ExitApp;
                chooseDatabase.ShowDialog();
            }
        }

        private void ChooseDatabase_ExitApp(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Close();
            });
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
            GUITwitchBots.OnBotStopped += GUI_OnBotStopped;
            GUITwitchBots.OnBotStarted += GUI_OnBotStarted;
            GUITwitchBots.OnBotStarted += GuiTwitchBot_GiveawayEvents;
            GUITwitchBots.OnBotFailedStart += GuiTwitchBot_OnBotFailedStart;
            GUITwitchBots.OnFollowerBotStarted += GuiTwitchBot_OnFollowerBotStarted;

            Controller.OnStreamOnline += GuiTwitchBot_OnLiveStreamStarted;
            Controller.OnStreamOffline += GuiTwitchBot_OnLiveStreamStopped;
            Controller.OnStreamCategoryChanged += BotEvents_GetChannelGameName;
            Controller.InvalidAuthorizationToken += Controller_InvalidAuthorizationToken;
            Controller.TokensInitialized += Controller_TokensInitializedAsync;


            GUITwitchBots.RegisterChannelPoints(TwitchBotUserSvc_GetChannelPoints);

            guiAppServices.AppDataDirectory = GetAppDataCWD();
            GUIAppServices.OnBotStarted += GUI_OnBotStarted;
            GUIAppServices.OnBotStopped += GUI_OnBotStopped;
            guiAppServices.MediaOverlayServer.SetOverlayWindow += MediaOverlayServer_SetOverlayWindow;

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
            LogWriter.DebugLog("Window_Loaded", DebugLogTypes.GUIEvents, "Begin Window Loaded events.");


            ThreadManager.CreateThreadStart("Window_Loaded", () =>
            {
                _ = Dispatcher.BeginInvoke(() =>
                {
                    ToggleButton_ChooseTwitchAuth_Click(this, null);

                    CheckMessageBoxes();
                    CheckBox_ManageData_Click(sender, new());
                    CheckBox_TabifySettings_Clicked(this, new());
                    CheckDebug(this, new());
                    SetVisibility(this, new());
                });
            });

            LogWriter.DebugLog("Window_Loaded", DebugLogTypes.GUIEvents, "End Window Loaded events.");
        }

        /// <summary>
        /// Check each bot if it's enabled-the credentials are proper- and start any bot the user selected to start when the app is loaded.
        /// Also, when the Twitch authentication token method changes, need to restart any bots if the tokens are available.
        /// </summary>
        private Task StartAutoBots()
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("StartAutoBots", DebugLogTypes.GUIHelpers, "Attempting to start bots.");

                if ((OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchBotTokenDate) >= CheckRefreshDate
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
                    LogWriter.DebugLog("StartAutoBots", DebugLogTypes.GUIHelpers, "The access tokens are available and ready to start bots.");
                    LogWriter.DebugLog("StartAutoBots", DebugLogTypes.GUIHelpers, "Starting any bots when the user checked 'auto-start bots'.");

                    Dispatcher.BeginInvoke(() =>
                    {
                        foreach (Tuple<bool, Platform, RadioButton> tuple in from Tuple<bool, Platform, RadioButton> tuple in BotOps
                                                                             where tuple.Item1 && tuple.Item3.IsEnabled
                                                                             select tuple)
                        {
                            DispatchStartBotAsync(tuple.Item3.DataContext as IOModule);
                            LogWriter.DebugLog("StartAutoBots", DebugLogTypes.GUIHelpers, $"Starting {(tuple.Item3.DataContext as IOModule).BotClientName}.");
                        }
                        LogWriter.DebugLog("StartAutoBots", DebugLogTypes.GUIHelpers, "Finished starting bots and beginning to update category.");
                    });
                }
            });
        }

        /// <summary>
        /// When the user clicks the close button to the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.GUIEvents, "Closing the application. Setting flags to end threaded procedures.");

            WatchProcessOps = false;
            OptionFlags.IsStreamOnline = false;
            OptionFlags.ActiveToken = false;

#if DEBUG
            if (TestingWindow != null && TestingWindow.IsActive)
            {
                TestingWindow?.Close();
            }
#endif

            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.GUIEvents, "Sending an exit to the bot controller.");
            Controller.ExitBots();

            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.BotController, "Closing/Exiting all of the output logs, including me!");
            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.GUIEvents, "Exiting the log writers. No more logging available.");
            LogWriter.ExitCloseLogs();
        }

        #endregion

    }
}
