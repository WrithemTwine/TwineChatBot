using StreamerBotLib.BotClients;
using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.Models;
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
using System.Windows.Threading;

namespace StreamerBot
{
    // TODO: add "announcement" option to commands, to use Twitch's 'announcement' chat adornment
    // TODO: add "shoutout" user option to invoke Twitch's chat level shoutout option
    // TODO: look at using "localhost" for the clip's referback URL to grab a clip to send to overlay-reconnect into Overlay

    // TODO: consider a flag in datamanager to more reliably commit when viewers enter and exit channel-to decrease lag when users join and leave & displayed in the GUI 

    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class StreamerBotWindow : Window, INotifyPropertyChanged
    {
        internal static BotController Controller { get; private set; }

        private readonly GUITwitchBots guiTwitchBot;
        private readonly GUIAppStats guiAppStats;
        private readonly GUIAppServices guiAppServices;
        private readonly DateTime StartBotDate;
        private DateTime TwitchFollowRefresh;

        private DateTime ChannelPtRetrievalDate = DateTime.MinValue;
        private readonly TimeSpan ChannelPtRefresh = new(0, 15, 0);

        private short TwitchFollowerCurrRefreshHrs = 0;
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

            CheckSettings();
            SetDatabaseChoice();

            WatchProcessOps = true;

            // TODO: determine database connection strings, start if available, defer until setup parameter(s) are added
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

            ComboBox_TwitchFollower_RefreshHrs.ItemsSource = new List<short>() { 1, 2, 4, 8, 12, 16, 24, 36, 48, 60, 72 };
            SetTwitchFollowerRefreshTime();

            ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, ProcessWatcher);

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            StatusBarItem_BetaLabel.Visibility = version.Revision != 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusBar_Label_Version.Content = $"Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            ConstructEvents();

            DataManagerLoaded();
        }

        #region Bot_Ops
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

                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
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

            Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_RefreshCategory, new Action<string>((s) => GUITwitchBots.GetUserGameCategory(UserName: s)));
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
                    !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAccessToken)
                    && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAccessToken)
                    && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode)
                    && (!OptionFlags.TwitchStreamerUseToken || !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode))
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

            if (OptionFlags.TwitchChannelName != OptionFlags.TwitchBotUserName)
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

        #endregion

        #region Data side

        private void RadioButton_StartBot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as RadioButton).IsEnabled)
            {
                Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    ((sender as RadioButton).DataContext as IOModule)?.StartBot();
                }), null);
            }
        }

        private void RadioButton_StopBot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            GUITwitchBots.Send(TextBox_BotChat.Text);
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

        #endregion

        private void TextBox_TwitchChannelBotNames_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            CheckFocus();
        }

    }
}
