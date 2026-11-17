using StreamerBotLib.BotIOController;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace StreamerBot
{
    // TODO: review expired token handling for null calls

    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class StreamerBotWindow : Window
    {
        public StreamerBotWindow()
        {
            OptionFlags.ActiveToken = true;
            StartBotDate = DateTime.Now;
            ThreadManager.SetGUIDispatcher(Dispatcher.CurrentDispatcher);

            LogWriter.WriteLog(StartBotDate, $"Now starting active bot session status log. Bot version {App.Version}.");

            WatchProcessOps = true;

            OptionFlags.CheckSettings();
            SetDatabaseChoice();

            Controller = new BotController(DataManage_OnLoadCompleted);

            InitializeComponent();
            SetTheme(); // adjust the theme, if user selected a different theme.
            DataManagerViewLoaded();

            guiAppStats = Resources["AppStats"] as GUIAppStats;

            BotStatusData = [];
            TabItem_Logs_BotStatus.DataContext = BotStatusData;

            SetTwitchFollowerRefreshTime();

            ThreadManager.CreateThreadStart(".ctor_StreamerBotWindow", ProcessWatcher);

            StatusBarItem_BetaLabel.Visibility = App.Version.Revision != 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusBar_Label_Version.Content = $"Version: {App.Version}";

            ConstructEvents();
            ComboBox_TwitchFollower_RefreshHrs.ItemsSource = new List<int>() { 1, 2, 4, 8, 12, 16, 24, 36, 48, 60, 72 };
        }

        #region Initializers

        /// <summary>
        /// detect if user has setup a database, show them the database configuration; otherwise skip the configuration
        /// </summary>
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

        /// <summary>
        /// Called from constructor, sets events 
        /// </summary>
        private void ConstructEvents()
        {
            ThreadManager.OnThreadCountUpdate += ThreadManager_OnThreadCountUpdate;

            Controller.OnStreamOnline += BotController_OnLiveStreamStarted;
            Controller.OnStreamOffline += BotController_OnLiveStreamStopped;
            Controller.OnStreamCategoryChanged += BotEvents_GetChannelGameName;
            Controller.InvalidAuthorizationToken += Controller_InvalidAuthorizationToken;
            Controller.TokensInitialized += Controller_TokensInitializedAsync;


            NotifyExpiredCredentials += BotWindow_NotifyExpiredCredentials;
            VerifyNewVersion += StreamerBotWindow_VerifyNewVersion;
        }

        #endregion


        #region GUI support methods
        #region Window Open and Close
        private bool DataManagerLoaded = false;
        private bool WindowLoaded = false;

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
                    BotBridge = new(Controller, botControlElements);

                    ToggleButton_ChooseTwitchAuth_Click(this, null);

                    CheckMessageBoxes();
                    CheckBox_ManageData_Click(this, new());
                    CheckBox_TabifySettings_Clicked(this, new());
                    CheckDebug(this, new());
                    SetVisibility(this, new());
                });
            });
        }

        /// <summary>
        /// Perform showing MessageBoxes based on certain settings flags. Gives users details about certain settings.
        /// </summary>
        private void CheckMessageBoxes()
        {
            if (OptionFlags.ManageDataArchiveMsg)
            {
                MessageBox.Show(LocalizedMsgSystem.GetVar(MsgBox.MsgBoxManageDataArchiveMsg), LocalizedMsgSystem.GetVar(MsgBox.MsgBoxManageDataArchiveTitle));

                Settings.Default.ManageDataArchiveMsg = false;
            }
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

        private void ChooseDatabase_ExitApp(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Close();
            });
        }


        #endregion

        #region Bot support methods
        private void BotEvents_GetChannelGameName(object sender, FindChannelCategoryEventArgs e)
        {
            LogWriter.DebugLog("BotEvents_GetChannelGameName", DebugLogTypes.GUIEvents, "Received update to the channel game category.");

            Dispatcher.Invoke(() =>
            {
                TextBlock_CurrentCategory.Content = e.GameName;
                Button_RefreshCategory.IsEnabled = true;
            });
        }


        #region LiveStatus Online Indicator
        private void BotController_OnLiveStreamStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("BotController_OnLiveStreamStarted", DebugLogTypes.GUIEvents, "Received an livestream started event.");

            SetLiveStreamActive(true);
        }
        private void BotController_OnLiveStreamStopped(object sender, EventArgs e)
        {
            LogWriter.DebugLog("BotController_OnLiveStreamStopped", DebugLogTypes.GUIEvents, "Received livestream stopped. Notify GUI to update appearance for offline.");

            SetLiveStreamActive(false);
        }
        private void SetLiveStreamActive(bool online = true)
        {
            Dispatcher.Invoke(
                () =>
                {
                    if (online)
                    {
                        Label_StreamStatusOff.Visibility = Visibility.Collapsed;
                        Label_StreamStatusOn.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Label_StreamStatusOff.Visibility = Visibility.Visible;
                        Label_StreamStatusOn.Visibility = Visibility.Collapsed;
                    }
                });
        }

        #endregion

        private void BeginUpdateCategory()
        {
            LogWriter.DebugLog("BeginUpdateCategory", DebugLogTypes.GUIBotComs, "Received request to begin updating the channel category.");

            Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_RefreshCategory, new Action<string>((s) => BotController.GetUserCategory()));
        }


        #endregion

        private void TextBlock_EventSubNotify_AccessToken_Scopes_Valid_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
