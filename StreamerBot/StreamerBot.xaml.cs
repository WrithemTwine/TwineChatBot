using Microsoft.Web.WebView2.Wpf;

using StreamerBotLib.BotClients;
using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.MultiLive;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StreamerBot
{
    // TODO: add "announcement" option to commands, to use Twitch's 'announcement' chat adornment
    // TODO: add "shoutout" user option to invoke Twitch's chat level shoutout option
    // TODO: look at using "localhost" for the clip's referback URL to grab a clip to send to overlay-reconnect into Overlay

    // TODO: consider a flag in datamanager to more reliably commit when viewers enter and exit channel-to decrease lag when users join and leave & displayed in the GUI 


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StreamerBotWindow : Window, INotifyPropertyChanged
    {
        internal static BotController Controller { get; private set; }
        private ManageWindows PopupWindows { get; set; } = new();

        private readonly GUITwitchBots guiTwitchBot;
        private readonly GUIAppStats guiAppStats;
        private readonly GUIAppServices guiAppServices;
        private readonly DateTime StartBotDate;
        private DateTime TwitchFollowRefresh;

        private DateTime ChannelPtRetrievalDate = DateTime.MinValue;
        private TimeSpan ChannelPtRefresh = new(0, 15, 0);

        private int TwitchFollowerCurrRefreshHrs = 0;
        private readonly TimeSpan CheckRefreshDate = new(7, 0, 0, 0);

        internal Dispatcher AppDispatcher { get; private set; } = Dispatcher.CurrentDispatcher;

        /// <summary>
        /// A collection of options and RadioButton pairs for each bot, to start and stop.
        /// </summary>
        private List<Tuple<bool, RadioButton>> BotOps { get; }

        #region delegates
        private delegate void RefreshBotOp(Button targetclick, Action<string> InvokeMethod);
        private delegate void BotOperation();

        #endregion

        public StreamerBotWindow()
        {
            StartBotDate = DateTime.Now;

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

            WatchProcessOps = true;
            IsMultiProcActive = null;

            Controller = new();
            Controller.SetDispatcher(AppDispatcher);

            InitializeComponent();

            BotOps =
            [
                new(Settings.Default.TwitchChatBotAutoStart, Radio_Twitch_StartBot),
                new(Settings.Default.TwitchFollowerSvcAutoStart, Radio_Twitch_FollowBotStart),
                new(Settings.Default.TwitchLiveStreamSvcAutoStart, Radio_Twitch_LiveBotStart),
                new(Settings.Default.TwitchClipAutoStart, Radio_Twitch_ClipBotStart),
                new(OptionFlags.MediaOverlayAutoStart, Radio_Services_OverlayBotStart),
                new(false, Radio_Twitch_PubSubBotStart)
            ];

            SetTheme(); // adjust the theme, if user selected a different theme.

            guiTwitchBot = Resources["TwitchBot"] as GUITwitchBots;
            guiAppStats = Resources["AppStats"] as GUIAppStats;
            guiAppServices = Resources["AppServices"] as GUIAppServices;

            ComboBox_TwitchFollower_RefreshHrs.ItemsSource = new List<int>() { 1, 2, 4, 8, 12, 16, 24, 36, 48, 60, 72 };
            SetTwitchFollowerRefreshTime();

            ThreadManager.CreateThreadStart(ProcessWatcher);

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            StatusBarItem_BetaLabel.Visibility = version.Revision != 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusBar_Label_Version.Content = $"Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            ConstructEvents();
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
                                 select s).Last();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string AppVersion = $"v.{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            // check if the saved link is the default, also check if the found link doesn't have the current version
            // true=> link not default and the stable version link doesn't have the current app version in it
            if (OptionFlags.GitHubCheckStable != OptionFlags.GitHubStableLink && AppVersion.CompareTo(newversion) < 0)
            {
                StatusBarItem_NewStableVersion.Visibility = Visibility.Visible;
            }
        }

        #region Bot_Ops
        #region Controller Events

        private void GUI_OnBotStarted(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                if (!e.Started)
                {
                    ToggleInputEnabled(true);
                }
                else
                {
                    ToggleInputEnabled(false);
                    RadioButton radio = e.BotName switch
                    {
                        Bots.TwitchChatBot => Radio_Twitch_StartBot,
                        Bots.TwitchClipBot => Radio_Twitch_ClipBotStart,
                        Bots.TwitchFollowBot => Radio_Twitch_FollowBotStart,
                        Bots.TwitchLiveBot => Radio_Twitch_LiveBotStart,
                        Bots.TwitchMultiBot => Radio_MultiLiveTwitch_StartBot,
                        Bots.TwitchPubSub => Radio_Twitch_PubSubBotStart,
                        Bots.MediaOverlayServer => Radio_Services_OverlayBotStart,
                        Bots.Default => throw new NotImplementedException(),
                        Bots.TwitchUserBot => throw new NotImplementedException(),
                        _ => throw new NotImplementedException()
                    };
                    HelperStartBot(radio);

                    if (e.BotName == Bots.MediaOverlayServer)
                    {
                        Controller.Systems.SendInitialTickerItems();
                    }
                }
            }), null);
        }

        private void GUI_OnBotStopped(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
              {
                  ToggleInputEnabled(true);
                  RadioButton radio = e.BotName switch
                  {
                      Bots.TwitchChatBot => Radio_Twitch_StopBot,
                      Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                      Bots.TwitchFollowBot => Radio_Twitch_FollowBotStop,
                      Bots.TwitchLiveBot => Radio_Twitch_LiveBotStop,
                      Bots.TwitchMultiBot => Radio_MultiLiveTwitch_StopBot,
                      Bots.TwitchPubSub => Radio_Twitch_PubSubBotStop,
                      Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                      Bots.Default => throw new NotImplementedException(),
                      Bots.TwitchUserBot => throw new NotImplementedException(),
                      _ => throw new NotImplementedException()
                  };
                  HelperStopBot(radio);
              }), null);
        }

        #endregion

        /// <summary>
        /// Will force start all bots currently not started, regardless of "start if Live" setting is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUIStartBots_Click(object sender, RoutedEventArgs e)
        {
            foreach (var B in BotOps)
            {
                _ = Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    if (B.Item2.IsEnabled) // is enabled is a check the credentials are added to the bot
                    {
                        (B.Item2.DataContext as IOModule)?.StartBot();
                    }
                }), null);
            }
        }

        /// <summary>
        /// Will stop all bots currently not stopped, regardless of "stop when offline" setting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUIStopBots_Click(object sender, RoutedEventArgs e)
        {
            foreach (var B in BotOps)
            {
                _ = Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    (B.Item2.DataContext as IOModule)?.StopBot();
                }), null);
            }
        }

        /// <summary>
        /// Manages the GUI updates for when bots are started. Hides and enables buttons 
        /// to set the GUI for bots started and allows the user to Stop Bots.
        /// </summary>
        /// <param name="rb">The radio button of the started bot.</param>
        private static void HelperStartBot(RadioButton rb)
        {
            rb.IsChecked = true;

            foreach (UIElement child in (VisualTreeHelper.GetParent(rb) as WrapPanel).Children)
            {
                if (child.GetType() == typeof(RadioButton))
                {
                    (child as RadioButton).IsEnabled = (child as RadioButton).IsChecked != true;
                }
                else if (child.GetType() == typeof(Label))
                {
                    Label currLabel = (Label)child;
                    if (currLabel.Name.Contains("Start"))
                    {
                        currLabel.Visibility = Visibility.Visible;
                    }
                    else if (currLabel.Name.Contains("Stop"))
                    {
                        currLabel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Manages the GUI updates for when bots are stopped. Hides and enables buttons
        /// to set the GUI for bots stopped and allows the user to Start Bots.
        /// </summary>
        /// <param name="rb">The radio button of the stopped bot.</param>
        private static void HelperStopBot(RadioButton rb)
        {
            rb.IsChecked = true;

            foreach (UIElement child in (VisualTreeHelper.GetParent(rb) as WrapPanel).Children)
            {
                if (child.GetType() == typeof(RadioButton))
                {
                    (child as RadioButton).IsEnabled = (child as RadioButton).IsChecked != true;
                }
                else if (child.GetType() == typeof(Label))
                {
                    Label currLabel = (Label)child;
                    if (currLabel.Name.Contains("Start"))
                    {
                        currLabel.Visibility = Visibility.Collapsed;
                    }
                    else if (currLabel.Name.Contains("Stop"))
                    {
                        currLabel.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void Button_SystemEvents_Click(object sender, RoutedEventArgs e)
        {
            BotController.SetSystemEventsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_BuiltInCommands_Click(object sender, RoutedEventArgs e)
        {
            BotController.SetBuiltInCommandsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_UserDefinedCommands_Click(object sender, RoutedEventArgs e)
        {
            BotController.SetUserDefinedCommandsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_DiscordWebhooks_Click(object sender, RoutedEventArgs e)
        {
            BotController.SetDiscordWebhooksEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void ShoutUsers_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleChatCommandReceived(
                new()
                {
                    CommandText = $"{DefaultCommand.soactive}",
                    CommandArguments = [""],
                    UserType = ViewerTypes.Broadcaster,
                    IsBroadcaster = true,
                    DisplayName = OptionFlags.TwitchChannelName,
                    Channel = OptionFlags.TwitchChannelName,
                    Message = $"{DefaultCommand.soactive}"
                },
                Platform.Twitch);
        }

        #region Refresh data from bot

        /// <summary>
        /// The GUI provides buttons to click and refresh data to the interface. Still must handle response events from the bot.
        /// </summary>
        /// <param name="targetclick">The button to disable while the operation begins./param>
        /// <param name="InvokeMethod">The bot method to invoke for the refresh operation.</param>
        private void UpdateData(Button targetclick, Action<string> InvokeMethod)
        {
            if (!OptionFlags.CheckSettingIsDefault("TwitchChannelName", OptionFlags.TwitchChannelName)) // prevent operation if default value
            {
                targetclick.IsEnabled = false;

                ThreadManager.CreateThreadStart(() =>
                {
                    try
                    {
                        InvokeMethod.Invoke(Settings.Default.TwitchChannelName);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    }
                });
            }
        }
        private void Button_RefreshCategory_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "User pushed the category button.");

            BeginUpdateCategory();
        }
        private void GuiTwitchBot_OnLiveStreamStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Received an livestream started event.");

            SetLiveStreamActive(true);
        }
        private void GuiTwitchBot_OnLiveStreamStopped(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Received livestream stopped. Notify GUI to update appearance for offline.");

            SetLiveStreamActive(false);
        }
        private void GuiTwitchBot_OnLiveStreamEvent(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Received a livestream event.");

            BeginUpdateCategory();
        }
        private void BeginUpdateCategory()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIBotComs, "Received request to begin updating the channel category.");

            Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_RefreshCategory, new Action<string>((s) => guiTwitchBot.GetUserGameCategory(UserName: s)));
        }
        private void BotEvents_GetChannelGameName(object sender, OnGetChannelGameNameEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.GUIEvents, "Received update to the channel game category.");

            Dispatcher.Invoke(() =>
            {
                TextBlock_CurrentCategory.Content = e.GameName;
                Button_RefreshCategory.IsEnabled = true;
            });
        }

        #endregion

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

        #endregion

        #region PopOut Chat Window
        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            //    CP.Show();
            //    CP.Height = 500;
            //    CP.Width = 300;
        }


        private void Slider_PopOut_Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            NotifyPropertyChanged("Opacity");
        }

        private readonly bool IsAppClosing = true;
        private void CP_Closing(object sender, CancelEventArgs e)
        {
            if (!IsAppClosing) // flag to really close the window
            {
                e.Cancel = true;
                //CP.Hide();
            }
        }

        #endregion

        #region Helpers

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

                foreach (Tuple<bool, RadioButton> tuple in from Tuple<bool, RadioButton> tuple in BotOps
                                                           where tuple.Item1 && tuple.Item2.IsEnabled
                                                           select tuple)
                {
                    Dispatcher.BeginInvoke(new BotOperation(() =>
                    {
                        (tuple.Item2.DataContext as IOModule)?.StartBot();
                    }));
                }
                Controller.InitializeHelix();

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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
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


            if (!OptionFlags.DataLoaded)
            {
                MessageBox.Show(LocalizedMsgSystem.GetVar(MsgBox.MsgBoxDataLoadedMsg), LocalizedMsgSystem.GetVar(MsgBox.MsgBoxDataLoadedTitle));
            }
        }

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFocus();
        }

        /// <summary>
        /// Manage GUI element visibility based on user selection. The intent is to add friendly protection
        /// to prevent accidentally clicking buttons and deleting, often unrecoverable, data-or the latest backup at the least.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            if (Button_ClearCurrencyAccrlValues != null && Button_ClearCurrencyAccrlValues != null && Button_ClearWatchTime != null && GroupBox_ManageDataOptions != null)
            {
                Button_ClearCurrencyAccrlValues.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                Button_ClearNonFollowers.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                Button_ClearWatchTime.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                GroupBox_ManageDataOptions.Visibility = OptionFlags.EnableManageDataOptions ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CheckBox_ManageData_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_ManageUsers.IsChecked == true)
            {
                CheckBox_ManageFollowers.IsEnabled = true;
            }
            else
            {
                CheckBox_ManageFollowers.IsEnabled = false;
                CheckBox_ManageFollowers.IsChecked = false; // requires the Manage Users to be enabled
            }

            if (CheckBox_ManageFollowers.IsChecked == true)
            {
                CheckBox_ManageUsers.IsEnabled = false;
                if (Settings.Default.TwitchFollowerSvcAutoStart && Radio_Twitch_FollowBotStart.IsChecked == true)
                {
                    Dispatcher.BeginInvoke(new BotOperation(() =>
                    {
                        (Radio_Twitch_FollowBotStart.DataContext as IOModule).StartBot();
                    }), null);
                }
            }
            else
            {
                CheckBox_ManageUsers.IsEnabled = true;
                Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    (Radio_Twitch_FollowBotStart.DataContext as IOModule).StopBot();
                }), null);
            }

            BotController.ManageDatabase();
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void CheckFocus()
        {
            if (!OptionFlags.TwitchTokenUseAuth)
            {
                SetBotRadioButtons(
                        TB_Twitch_Channel.Text.Length != 0
                    && TB_Twitch_BotUser.Text.Length != 0
                    && TB_Twitch_ClientID.Text.Length != 0
                    && TB_Twitch_AccessToken.Text.Length != 0
                    && OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchRefreshDate) >= new TimeSpan(0, 0, 0)
                    , Platform.Twitch);
            }
            else
            {
                SetBotRadioButtons(
                    OptionFlags.TwitchAuthBotRefreshToken != ""
                    && OptionFlags.TwitchAuthBotAuthCode != ""
                    && OptionFlags.TwitchAuthBotAuthCode != ""
                    , Platform.Twitch);
            }

            // Check if credentials are empty and we still need to allow the user to authenticate the application, but block it when successfully authenticated
            // The authentication code bot checking clears out the auth code when there's a failure, so this checks it's enabled when
            // both the user adds client Id & secret are available and auth code is not available (not authenticated)
            Twitch_AuthCode_Button_AuthorizeBot.IsEnabled = OptionFlags.TwitchAuthClientId != ""
                                                            && OptionFlags.TwitchChannelName != ""
                                                            && OptionFlags.TwitchAuthBotClientSecret != ""
                                                            && OptionFlags.TwitchBotUserName != ""
                                                            && OptionFlags.TwitchAuthBotAuthCode == "";

            Twitch_AuthCode_Button_AuthorizeStreamer.IsEnabled = OptionFlags.TwitchAuthStreamerClientId != ""
                                                                    && OptionFlags.TwitchChannelName != ""
                                                                    && OptionFlags.TwitchAuthStreamerClientSecret != ""
                                                                    && OptionFlags.TwitchAuthStreamerAuthCode == "";

            Radio_Twitch_PubSubBotStart.IsEnabled = OptionFlags.TwitchTokenUseAuth ?
                (OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchAuthStreamerAccessToken != "" : OptionFlags.TwitchAuthBotAccessToken != "") :
                OptionFlags.TwitchStreamerUseToken ?
                                                    (OptionFlags.TwitchStreamOauthToken != "" && OptionFlags.TwitchStreamerValidToken) : OptionFlags.TwitchBotAccessToken != "";

            // Twitch

            if (TB_Twitch_Channel.Text != TB_Twitch_BotUser.Text)
            {
                GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Visible;
                TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Visible;
                TextBox_TwitchScopesOauthSame.Visibility = Visibility.Collapsed;
                Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Visible;
                Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Visible;
                Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Collapsed;

                Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Visible;
            }
            else
            {
                GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Collapsed;
                TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Collapsed;
                TextBox_TwitchScopesOauthSame.Visibility = Visibility.Visible;
                Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Collapsed;
                Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Collapsed;
                Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Visible;

                Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Collapsed;
            }

            // set earliest token expiration date

            List<DateTime> RefreshTokenDateExpiry = [OptionFlags.TwitchRefreshDate, OptionFlags.TwitchStreamerTokenDate];
            RefreshTokenDateExpiry.RemoveAll((d) => d < DateTime.Now);
            StatusBarItem_TokenDate.Content = OptionFlags.TwitchTokenUseAuth ? "Auth Code Refresh" : RefreshTokenDateExpiry.Count != 0 ? RefreshTokenDateExpiry?.Min().ToShortDateString() : "None Valid";
        }

        private void SetBotRadioButtons(bool value, Platform platform)
        {
            foreach (RadioButton rb in
                                        from A in BotOps
                                        where A.Item2.Name.Contains(platform.ToString())
                                        select A.Item2
                                        )
            {
                rb.IsEnabled = value;
            }
        }

        private async void PreviewMouseLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        /// <summary>
        /// Disable or Enable UIElements to prevent user changes while bot is active.
        /// Same method takes the opposite value for the start then stop then start, i.e. toggling start/stop bot operations.
        /// </summary>
        private void ToggleInputEnabled(bool setvalue = true)
        {
            TB_Twitch_AccessToken.IsEnabled = setvalue;
            TB_Twitch_BotUser.IsEnabled = setvalue;
            TB_Twitch_Channel.IsEnabled = setvalue;

            TB_Twitch_ClientID.IsEnabled = setvalue;
            Btn_Twitch_RefreshDate.IsEnabled = setvalue;
            Slider_TimeFollowerPollSeconds.IsEnabled = setvalue;
            Slider_TimeGoLivePollSeconds.IsEnabled = setvalue;
            Slider_TimeClipPollSeconds.IsEnabled = setvalue;

            //TODO: add more textboxes to block when the bots are enabled - to prevent changes
            //ToggleButton_TwitchToken.IsEnabled = setvalue;
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock_TwitchBotLog.ScrollToEnd();
        }

        private void CheckBox_Checked_PanelVisibility(object sender, RoutedEventArgs e)
        {
            ToggleButton TBSource = null;
            StackPanel SPSource = null;
            GroupBox GBSource = null;
            if (sender.GetType() == typeof(CheckBox) || sender.GetType() == typeof(RadioButton))
            {
                TBSource = (ToggleButton)sender;
            }
            else if (sender.GetType() == typeof(StackPanel))
            {
                SPSource = (StackPanel)sender;
            }
            else if (sender.GetType() == typeof(GroupBox))
            {
                GBSource = (GroupBox)sender;
            }

            static void SetVisibility(ToggleButton box, UIElement panel)
            {
                if (panel != null)
                {
                    panel.Visibility = (box.IsChecked == true) switch
                    {
                        true => Visibility.Visible,
                        false => Visibility.Collapsed
                    };
                }
            }

            // be sure this list is in XAML object order
            if (TBSource?.Name == CheckBox_RepeatCommands_Enable?.Name || SPSource?.Name == StackPanel_RepeatCommands_RepeatOptions?.Name)
            {
                SetVisibility(CheckBox_RepeatCommands_Enable, StackPanel_RepeatCommands_RepeatOptions);
                Controller.ActivateRepeatTimers();
            }
            else if (TBSource?.Name == RadioButton_RepeatTimer_NoAdjustment.Name || TBSource?.Name == RadioButton_RepeatTimer_SlowDownOption.Name || TBSource?.Name == RadioButton_RepeatTimer_ThresholdOption.Name || GBSource?.Name == GroupBox_RepeatTimer_ThresholdOptions.Name)
            {
                if (TBSource?.Name == RadioButton_RepeatTimer_ThresholdOption?.Name || TBSource?.Name == RadioButton_RepeatTimer_ThresholdOption?.Name)
                {
                    SetVisibility(RadioButton_RepeatTimer_ThresholdOption, StackPanel_Repeat_ThresholdsOptions);
                }

                if (RadioButton_RepeatTimer_NoAdjustment != null && RadioButton_RepeatTimer_SlowDownOption != null && GroupBox_RepeatTimer_ThresholdOptions != null)
                {
                    SetVisibility(RadioButton_RepeatTimer_SlowDownOption.IsChecked == true ? RadioButton_RepeatTimer_SlowDownOption : RadioButton_RepeatTimer_ThresholdOption, GroupBox_RepeatTimer_ThresholdOptions);
                }
            }
            else if (TBSource?.Name == CheckBox_MediaOverlay_Enable.Name || SPSource?.Name == StackPanel_MediaOverlay_MediaOptions.Name)
            {
                SetVisibility(CheckBox_MediaOverlay_Enable, StackPanel_MediaOverlay_MediaOptions);

                if (TabItem_Overlays != null)
                {
                    TabItem_Overlays.Visibility = CheckBox_MediaOverlay_Enable.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else if (TBSource?.Name == CheckBox_ModFollower_BanEnable.Name || SPSource?.Name == StackPanel_ModerateFollowers_Count.Name)
            {
                SetVisibility(CheckBox_ModFollower_BanEnable, StackPanel_ModerateFollowers_Count);
            }
            else if (TBSource?.Name == CheckBox_TwitchFollower_LimitMsgs.Name || SPSource?.Name == StackPanel_TwitchFollower_LimitMsgs_Count.Name)
            {
                SetVisibility(CheckBox_TwitchFollower_LimitMsgs, StackPanel_TwitchFollower_LimitMsgs_Count);
            }
            else if (TBSource?.Name == CheckBox_TwitchFollower_AutoRefresh.Name || SPSource?.Name == StackPanel_TwitchFollows_RefreshHrs.Name)
            {
                SetVisibility(CheckBox_TwitchFollower_AutoRefresh, StackPanel_TwitchFollows_RefreshHrs);
            }
        }

        private void TextBox_Follower_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox src = (TextBox)sender;

            if (int.TryParse(src.Text, out int result))
            {
                if (result is < 1 or > 100)
                {
                    src.Text = (result < 1 ? 1 : result > 100 ? 100 : result).ToString();
                }
            }
        }

        private void TextBlock_MouseEnter_Visible(object sender, MouseEventArgs e)
        {
            TextBlock_AppDataDir.Visibility = Visibility.Visible;
        }

        private void TextBlock_MouseEnter_Hidden(object sender, MouseEventArgs e)
        {
            TextBlock_AppDataDir.Visibility = Visibility.Hidden;
        }


        #region GitHub webpage

        private void WebView2Reload_Click(object sender, RoutedEventArgs e)
        {
            (((sender as Button).Parent as StackPanel).Children[1] as WebView2).Reload();
        }

        #endregion

        #region LiveStatus Online Indicator

        private void SetLiveStreamActive(bool Online = true)
        {
            Dispatcher.Invoke(
                () =>
                {
                    if (Online)
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

        #region Moderate Followers

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

        #region Overlay Service

        private void TabItem_Overlays_GotFocus(object sender, RoutedEventArgs e)
        {
            BeginGiveawayChannelPtsUpdate();
        }
        private void TabItem_ModApprove_GotFocus(object sender, RoutedEventArgs e)
        {
            BeginGiveawayChannelPtsUpdate();
        }

        private void Button_Overlay_PauseAlerts_Click(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).DataContext as BotOverlayServer).SetPauseAlert((sender as CheckBox).IsChecked == true);
        }

        private void Button_Overlay_ClearAlerts_Click(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as BotOverlayServer).SetClearAlerts();
        }

        private void UpdateOverlayChannelPointList(List<string> channelPointNames)
        {
            Controller.Systems.SetChannelRewardList(channelPointNames);
        }

        #endregion

        #endregion

        #region Data side

        private void RadioButton_StartBot_PreviewMoustLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as RadioButton).IsEnabled)
            {
                Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    ((sender as RadioButton).DataContext as IOModule)?.StartBot();
                }), null);
            }
        }

        private void RadioButton_StopBot_PreviewMoustLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as RadioButton).IsEnabled)
            {
                Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    ((sender as RadioButton).DataContext as IOModule)?.StopBot();
                }), null);
            }
        }

        private void TextBox_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        /// <summary>
        /// Every time a text box changes, relative to access credentials, call the CheckFocus method, which checks the data entry for whether buttons can be enabled so the user can click them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            CheckFocus();
        }

        private void JoinCollectionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UserJoin curr = (sender as CheckBox).DataContext as UserJoin;

            //guiDataManagerViews.JoinCollection.Remove(curr);

            ((ObservableCollection<UserJoin>)LV_JoinList.ItemsSource).Remove(curr);
        }

        private void BotChat_SendButton_Click(object sender, RoutedEventArgs e)
        {
            guiTwitchBot.Send(TextBox_BotChat.Text);
            TextBox_BotChat.Text = "";
        }

        private const int TwitchTokenRefreshDays = 53;

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Label_Twitch_RefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(TwitchTokenRefreshDays);
            TextBlock_ExpiredCredentialsMsg.Visibility = Visibility.Collapsed;
            CheckFocus();
        }

        private void RefreshStreamButton_Click(object sender, RoutedEventArgs e)
        {
            Label_Twitch_StreamerRefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(TwitchTokenRefreshDays);
            TextBlock_ExpiredStreamerCredentialsMsg.Visibility = Visibility.Collapsed;
            CheckFocus();
        }

        #region DataGrid Columns and Editing
        private void DG_AutoGeneratedColumns(object sender, EventArgs e)
        {
            const int DGColWidth = 250;

            void Collapse(DataGridColumn dgc)
            {
                dgc.Visibility = Visibility.Collapsed;
            }

            void ReadOnly(DataGridColumn dgc)
            {
                dgc.IsReadOnly = true;
            }

            void SetWidth(DataGridColumn dgc, int Width = -1)
            {
                Width = Width < 0 ? DGColWidth : Width;

                // TODO: Research and update setting column width based on actual value, currently doesn't appear available when "autogeneratedcolumns" occurs

                // change the column width only if the specified value is less than current width
                if (dgc.Width.DisplayValue > Width || (dgc.Header.ToString() is "Message" or "WebHook" or "Webhook" or "TeachingMsg"))
                {
                    dgc.Width = Width;
                }
            }

            DataGrid dg = (DataGrid)sender;

            // find the new item, hide columns other than the primary data columns, i.e. relational columns
            switch (dg.Name)
            {
                case "DG_Users":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Id" and not "UserName" and not "FirstDateSeen" and not "LastDateSeen" and not "WatchTime" and not "UserId" and not "Platform")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
                case "DG_Followers":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Id" and not "UserName" and not "IsFollower" and not "FollowedDate" and not "UserId" and not "Platform" and not "StatusChangeDate" and not "Category")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
                case "DG_Currency":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "CurrencyName" and not "AccrueAmt" and not "Seconds" and not "MaxValue")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
                case "DG_CurrencyAccrual":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "UserName" and not "CurrencyName" and not "Value")
                        {
                            Collapse(dc);
                        }
                    }
                    break;

                case "DG_BuiltInCommands":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() == "CmdName")
                        {
                            ReadOnly(dc);
                        }
                        SetWidth(dc);
                    }
                    break;
                case "DG_UserDefinedCommands" or "DG_CommonMsgs" or "DG_CustomWelcome":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        SetWidth(dc);
                    }
                    break;
                case "DG_CategoryList" or "DG_CategoryList_Clips":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Category" and not "CategoryId" and not "StreamCount")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
                case "DG_DeathCounter":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Category" and not "Counter")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
                case "DG_Webhooks":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        SetWidth(dc);
                    }
                    break;
                case "BanRules" or "LearnMsgs":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is "LearnMsgsBanReasons" or "BanRulesLearnMsgs")
                        {
                            Collapse(dc);
                        }
                        else
                        {
                            SetWidth(dc);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Button_ClearWatchTime_Click(object sender, RoutedEventArgs e)
        {
            BotController.ClearWatchTime();
        }

        private void Button_ClearCurrencyAccrlValues_Click(object sender, RoutedEventArgs e)
        {
            BotController.ClearAllCurrenciesValues();
        }

        private void Button_ClearNonFollowers_Click(object sender, RoutedEventArgs e)
        {
            BotController.ClearUsersNonFollowers();
        }

        private void DG_Edit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                bool FoundAddEdit = ((DataGrid)sender).Name is "DG_BuiltInCommands" or "DG_CommonMsgs";
                bool FoundAddShout = ((DataGrid)sender).Name is "DG_Users" or "DG_Followers";
                bool FoundIsEnabled = SystemsController.CheckField(((DataView)((DataGrid)sender).ItemsSource).Table.TableName, "IsEnabled");

                foreach (var M in ((ContextMenu)Resources["DataGrid_ContextMenu"]).Items)
                {
                    if (M.GetType() == typeof(MenuItem))
                    {
                        if (((MenuItem)M).Name is "DataGridContextMenu_AddItem" or "DataGridContextMenu_DeleteItems")
                        {
                            ((MenuItem)M).IsEnabled = !FoundAddEdit;
                        }
                        else if (((MenuItem)M).Name is "DataGridContextMenu_AutoShout" or "DataGridContextMenu_LiveMonitor")
                        {
                            // TODO: limit 'live monitor' menu access to only when the multi-live datamanager is active
                            ((MenuItem)M).Visibility = FoundAddShout ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else if (((MenuItem)M).Name is "DataGridContextMenu_EnableItems" or "DataGridContextMenu_DisableItems")
                        {
                            ((MenuItem)M).IsEnabled = FoundIsEnabled;
                        }
                    }
                    else if (M.GetType() == typeof(Separator))
                    {
                        if (((Separator)M).Name == "DataGridContextMenu_Separator")
                        {
                            ((Separator)M).Visibility = FoundAddShout ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid item = sender as DataGrid;

            Popup_DataEdit(item, false);
        }

        private void MenuItem_AddClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            Popup_DataEdit(item);
        }

        private void Popup_DataEdit(DataGrid sourceDataGrid, bool AddNew = true)
        {
            if (sourceDataGrid.Name is "DataGrid_OverlayService_Actions" or "DG_ModApprove")
            {
                PopupWindows.SetTableData(Controller.Systems.GetOverlayActions());
            }

            if (AddNew)
            {
                DataView CurrdataView = (DataView)sourceDataGrid.ItemsSource;
                if (CurrdataView != null)
                {
                    PopupWindows.DataGridAddNewItem(SystemsController.DataManage, CurrdataView.Table);
                }
            }
            else
            {
                DataRowView dataView = (DataRowView)sourceDataGrid.SelectedItem;
                if (dataView != null)
                {
                    PopupWindows.DataGridEditItem(SystemsController.DataManage, dataView.Row.Table, dataView.Row);
                }
            }
        }

        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            Popup_DataEdit(item, false);
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            SystemsController.DeleteRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)));
        }

        private void MenuItem_AutoShoutClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (DataRow dr in new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)))
            {
                BotController.AddNewAutoShoutUser(dr["UserName"].ToString(), dr["UserId"].ToString(), dr["Platform"].ToString());
            }
        }

        private void MenuItem_LiveMonitorClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            foreach (DataRow dr in new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)))
            {
                (MultiLive_Data.Content as MultiLiveDataGrids).AddNewMonitorChannel(dr["UserName"].ToString(), dr["UserId"].ToString());
            }
        }
        private void DataGridContextMenu_EnableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            SystemsController.UpdateIsEnabledRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)), true);
        }

        private void DataGridContextMenu_DisableItems_Click(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as Popup).PlacementTarget as DataGrid;

            SystemsController.UpdateIsEnabledRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)), false);
        }

        #endregion


        #region Giveaway

        private delegate void RefreshChannelPoints();

        private void TabItem_Giveaways_Loaded(object sender, RoutedEventArgs e)
        {
            //BeginGiveawayChannelPtsUpdate();
            CheckGiveawayFocusStatus();
        }

        private void GuiTwitchBot_GiveawayEvents(object sender, BotStartStopEventArgs e)
        {
            if (e.BotName == Bots.TwitchChatBot)
            {
                BeginGiveawayChannelPtsUpdate();
            }
        }

        private void Button_RefreshChannelPoints_Click(object sender, RoutedEventArgs e)
        {
            Button_Giveaway_RefreshChannelPoints.IsEnabled = false;
            Button_ChannelPts_Refresh.IsEnabled = false;
            ChannelPtRetrievalDate = DateTime.MinValue; // reset the retrieve date to force retrieval
            BeginGiveawayChannelPtsUpdate();
        }

        private void BeginGiveawayChannelPtsUpdate()
        {
            if (DateTime.Now >= ChannelPtRetrievalDate + ChannelPtRefresh)
            {
                _ = Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_Giveaway_RefreshChannelPoints, new Action<string>((s) => guiTwitchBot.GetChannelPoints(UserName: s)));
                ChannelPtRetrievalDate = DateTime.Now;
            }
        }

        private void TwitchBotUserSvc_GetChannelPoints(object sender, OnGetChannelPointsEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateGiveawayList(e.ChannelPointNames);
                UpdateOverlayChannelPointList(e.ChannelPointNames);
            });
        }

        private void UpdateGiveawayList(List<string> ChannelPointNames)
        {
            ComboBox_Giveaway_ChanPts.ItemsSource = ChannelPointNames;
            ComboBox_ChannelPoints.ItemsSource = ChannelPointNames;
            Button_Giveaway_RefreshChannelPoints.IsEnabled = true;
            Button_ChannelPts_Refresh.IsEnabled = true;
        }

        private void Button_GiveawayBegin_Click(object sender, RoutedEventArgs e)
        {
            GiveawayTypes givetype = GiveawayTypes.None;
            if (RadioButton_GiveawayCommand.IsChecked == true)
            {
                givetype = GiveawayTypes.Command;
            }
            else if (RadioButton_GiveawayCustomRewards.IsChecked == true)
            {
                givetype = GiveawayTypes.CustomRewards;
            }

            string ItemName = "";

            switch (givetype)
            {
                case GiveawayTypes.Command:
                    ItemName = (string)ComboBox_Giveaway_Coms.SelectedValue;
                    break;
                case GiveawayTypes.CustomRewards:
                    ItemName = (string)ComboBox_Giveaway_ChanPts.SelectedValue;
                    break;
            }

            Controller.HandleGiveawayBegin(givetype, ItemName);
            Giveaway_Toggle(false);

            Button_GiveawayBegin.IsEnabled = false;
            Button_GiveawayEnd.IsEnabled = true;
        }

        private void Giveaway_Toggle(bool Enabled = true)
        {
            RadioButton_GiveawayCustomRewards.IsEnabled = Enabled;
            RadioButton_GiveawayCommand.IsEnabled = Enabled;
            ComboBox_Giveaway_ChanPts.IsEnabled = Enabled;
            ComboBox_Giveaway_Coms.IsEnabled = Enabled;
        }

        private void Button_GiveawayEnd_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleGiveawayEnd();
            Giveaway_Toggle();
            Button_GiveawayBegin.IsEnabled = true;
            Button_GiveawayEnd.IsEnabled = false;
            Button_GiveawayPickWinner.IsEnabled = true;
        }

        private void Button_GiveawayPickWinner_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleGiveawayWinner();
            Giveaway_Toggle();
            Button_GiveawayBegin.IsEnabled = true;
        }

        private void ComboBox_Giveaway_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (((ComboBox)sender).Name)
            {
                case "ComboBox_Giveaway_ChanPts":
                    RadioButton_GiveawayCustomRewards.IsChecked = true;
                    break;
                case "ComboBox_Giveaway_Coms":
                    RadioButton_GiveawayCommand.IsChecked = true;
                    break;
            }
        }

        private void ComboBox_Giveaway_DropDownClosed(object sender, EventArgs e)
        {
            CheckGiveawayFocusStatus();
        }

        private void CheckGiveawayFocusStatus()
        {
            if (Radio_Twitch_StartBot.IsChecked == true &&
                ((RadioButton_GiveawayCustomRewards.IsChecked == true && (string)ComboBox_Giveaway_ChanPts.SelectedValue != "" && Radio_Twitch_PubSubBotStart.IsChecked == true)
                || (RadioButton_GiveawayCommand.IsChecked == true && (string)ComboBox_Giveaway_Coms.SelectedValue != "")))
            {
                Button_GiveawayBegin.IsEnabled = true;
            }
        }

        #endregion

        #endregion

        private void TextBox_TwitchChannelBotNames_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            CheckFocus();
        }
    }
}
