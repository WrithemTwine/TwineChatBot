using Microsoft.Web.WebView2.Wpf;

using StreamerBotLib.BotClients;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models.Enums;
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

                #region Manage app authorization for access scope changes
                // reserved for clearing tokens when there is an access scope change
                // have a trailing access scope saved for each token method, if the saved scope doesn't match the current scopes, then clear the tokens and saved scopes to force a new authentication with the new scopes. This is needed because Twitch requires a new access token with the required access scopes when accessing newly implemented API calls within the app.

                // TODO: update the tokenbot or GUI to save the scopes when the manual tokens are entered in the Twitch token credentials
                // TODO: update the authcode flow, tokenbot?, to save the scopes when authorizing the app

                if (Settings.Default.TwitchTokenUseAuth)
                {
                    if (OptionFlags.TwitchStreamerUseToken)
                    {
                        if (Settings.Default.TwitchAuthBotApproveScopes != StreamerBotLib.Properties.Resources.CredentialsTwitchScopesDiffOauthBot)
                        {
                            Settings.Default.TwitchAuthBotAccessToken = string.Empty;
                            Settings.Default.TwitchAuthBotRefreshToken = string.Empty;
                            Settings.Default.TwitchAuthBotAuthCode = string.Empty;
                        }
                        if (Settings.Default.TwitchAuthStreamerScopeApproveScopes != StreamerBotLib.Properties.Resources.CredentialsTwitchScopesDiffOauthChannel)
                        {
                            Settings.Default.TwitchAuthStreamerAccessToken = string.Empty;
                            Settings.Default.TwitchAuthStreamerRefreshToken = string.Empty;
                            Settings.Default.TwitchAuthStreamerAuthCode = string.Empty;
                        }
                    }
                    else
                    {
                        if (Settings.Default.TwitchAuthBotApproveScopes != StreamerBotLib.Properties.Resources.CredentialsTwitchScopesOauthSame)
                        {
                            Settings.Default.TwitchAuthBotAccessToken = string.Empty;
                            Settings.Default.TwitchAuthBotRefreshToken = string.Empty;
                            Settings.Default.TwitchAuthBotAuthCode = string.Empty;
                        }
                    }
                }
                else
                {
                    if (OptionFlags.TwitchStreamerUseToken)
                    {
                        if (Settings.Default.TwitchBotApproveScopes != StreamerBotLib.Properties.Resources.CredentialsTwitchScopesDiffOauthBot)
                        {
                            Settings.Default.TwitchBotAccessToken = string.Empty;
                            Settings.Default.TwitchBotRefreshToken = string.Empty;
                        }
                        if (Settings.Default.TwitchStreamerScopesApproveScopes != StreamerBotLib.Properties.Resources.CredentialsTwitchScopesDiffOauthChannel)
                        {
                            Settings.Default.TwitchStreamerAccessToken = string.Empty;
                            Settings.Default.TwitchStreamerRefreshToken = string.Empty;
                        }
                    }
                    else
                    {
                        if (Settings.Default.TwitchBotApproveScopes != StreamerBotLib.Properties.Resources.CredentialsTwitchScopesOauthSame)
                        {
                            Settings.Default.TwitchBotAccessToken = string.Empty;
                            Settings.Default.TwitchBotRefreshToken = string.Empty;
                        }
                    }
                }

                // end of the reserved section for clearing tokens when there is an access scope change
                #endregion

                Settings.Default.Save();
            }

            if (Settings.Default.AppCurrWorkingPopup)
            {
                Settings.Default.AppCurrWorkingPopup = false;
                string SaveCWDPath = GetAppDataCWD();

                MessageBoxResult boxResult = MessageBox.Show($"This application supports saving all data files at:\r\n{SaveCWDPath}\r\n\tor at the application'AppVersion current location:\r\n{Directory.GetCurrentDirectory()}\r\n\r\nPlease select 'Yes' to enable the APPData save location and restart the app.\r\n\r\nPlease see 'Data/Options/Any - Data Management' to change this option.\r\n\r\nThis dialog will not re-appear unless the settings are reset.", "Decide File Save Location", MessageBoxButton.YesNo);

                if (boxResult == MessageBoxResult.Yes)
                {
                    Settings.Default.AppCurrWorkingAppData = true;
                }
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
#if DEBUG || DEBUG_VIEWXAML || RELEASE_SQLITE || UPDATE_NUGET_ONLY
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
            // Twitch Bots focus
            GUITwitchBots.OnBotStopped += GUI_OnBotStopped;
            GUITwitchBots.OnBotStarted += GUI_OnBotStarted;
            GUITwitchBots.OnBotStarted += GuiTwitchBot_GiveawayEvents;
            GUITwitchBots.OnBotFailedStart += GuiTwitchBot_OnBotFailedStart;
            GUITwitchBots.OnBulkFollowerStopped += GuiTwitchBot_OnBulkFollowerStopped;

            Controller.OnStreamOnline += GuiTwitchBot_OnLiveStreamStarted;
            Controller.OnStreamOffline += GuiTwitchBot_OnLiveStreamStopped;
            Controller.OnStreamCategoryChanged += BotEvents_GetChannelGameName;
            Controller.InvalidAuthorizationToken += Controller_InvalidAuthorizationToken;
            Controller.TokensInitialized += Controller_TokensInitializedAsync;

            GUITwitchBots.RegisterChannelPoints(TwitchBotUserSvc_GetChannelPoints);

            // Service bots focus - such as Media Overlay Server
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

        private bool DataManagerLoaded = false;
        private bool WindowLoaded = false;

        private void DataManage_OnLoadCompleted(object sender, EventArgs e)
        {
            DataManagerLoaded = true;

            if (WindowLoaded)
            {
                FinalizeLoading();
            }
            else
            {
                while (!WindowLoaded)
                {
                    Thread.Sleep(100);
                }
                FinalizeLoading();
            }
        }

        private void FinalizeLoading()
        {
            ThreadManager.CreateThreadStart("Window_Loaded", () =>
            {
                _ = Dispatcher.BeginInvoke(() =>
                {
                    ToggleButton_ChooseTwitchAuth_Click(this, null);

                    CheckMessageBoxes();
                    CheckBox_ManageData_Click(this, new());
                    CheckBox_TabifySettings_Clicked(this, new());
                    CheckDebug(this, new());
                    SetVisibility(this, new());
                });
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowLoaded = true;
            LogWriter.DebugLog("Window_Loaded", DebugLogTypes.GUIEvents, "Begin Window Loaded events.");

            if (DataManagerLoaded)
            {
                FinalizeLoading();
                LogWriter.DebugLog("Window_Loaded", DebugLogTypes.GUIEvents, "Started Window Loaded events.");
            }
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
            if (TestingWindow != null && TestingWindow.ShowActivated)
            {
                TestingWindow?.Close();
            }
#endif

            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.GUIEvents, "Sending an exit to the bot controller.");
            Controller.ExitBots();
            GUIDataGridUpdates?.Join();

            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.BotController, "Closing/Exiting all of the output logs, including me!");
            LogWriter.DebugLog("OnWindowClosing", DebugLogTypes.GUIEvents, "Exiting the log writers. No more logging available.");
            LogWriter.ExitCloseLogs();
        }

        #endregion

    }
}
