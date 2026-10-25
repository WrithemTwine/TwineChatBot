using StreamerBot.Twitch;
using StreamerBot.Web;

using StreamerBotLib.BotClients;
using StreamerBotLib.BotIOController;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StreamerBot
{
    internal class oldGUIstartupbot_source
    {
        internal static GUIOverlayTypeAlerts GUIOverlayTypeAlerts { get; set; }

        internal static BotController Controller { get; private set; }

        private readonly GUITwitchBots guiTwitchBot;
        private readonly GUIAppStats guiAppStats;
        private readonly GUIAppServices guiAppServices;
        private readonly DateTime StartBotDate;
        private DateTime TwitchFollowRefresh;

        private DateTime ChannelPtRetrievalDate = DateTime.MinValue;
        private readonly TimeSpan ChannelPtRefresh = new(0, 15, 0);

        private int TwitchFollowerCurrRefreshHrs = 0;
        private readonly TimeSpan CheckRefreshDate = new(7, 0, 0, 0);

        /// <summary>
        /// A collection of options and RadioButton pairs for each bot, to start and stop.
        /// </summary>
        private List<Tuple<bool, Platform, RadioButton>> BotOps { get; }

        public StreamerBotWindow()
        {
            OptionFlags.ActiveToken = true;
            StartBotDate = DateTime.Now;
            ThreadManager.SetGUIDispatcher(Dispatcher.CurrentDispatcher);

            LogWriter.WriteLog(StartBotDate, $"Now starting active bot session status log. Bot version {App.Version}.");

            CheckSettings();
            SetDatabaseChoice();

            WatchProcessOps = true;

            Controller = new BotController(DataManage_OnLoadCompleted);
            GUIOverlayTypeAlerts = ActionSystem.GUIOverlayTypeAlerts;

            InitializeComponent();

            BotOps =
            [
                new(Settings.Default.TwitchBotEventSubAutoStart, Platform.Twitch, Radio_Twitch_EventSubBotStart),
                new(Settings.Default.TwitchStreamEventSubAutoStart, Platform.Twitch, Radio_Twitch_EventSubStreamerStart),
                new(Settings.Default.TwitchLiveStreamSvcAutoStart, Platform.Twitch, Radio_Twitch_LiveBotStart),
                new(Settings.Default.TwitchClipAutoStart, Platform.Twitch, Radio_Twitch_ClipBotStart),
                new(Settings.Default.MediaOverlayAutoStart, Platform.Service, Radio_Services_OverlayBotStart)
            ];

            SetTheme(); // adjust the theme, if user selected a different theme.

            guiTwitchBot = Resources["TwitchBot"] as GUITwitchBots;
            guiAppStats = Resources["AppStats"] as GUIAppStats;
            guiAppServices = Resources["AppServices"] as GUIAppServices;
            GUIDataManagerViews = TryFindResource("DataViews") as GUIDataManagerViews;

            DataManagerViewLoaded();

            ComboBox_TwitchFollower_RefreshHrs.ItemsSource = new List<int>() { 1, 2, 4, 8, 12, 16, 24, 36, 48, 60, 72 };
            SetTwitchFollowerRefreshTime();

            ThreadManager.CreateThreadStart(".ctor_StreamerBotWindow", ProcessWatcher);

            StatusBarItem_BetaLabel.Visibility = App.Version.Revision != 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusBar_Label_Version.Content = $"Version: {App.Version}";

            ConstructEvents();

            PopupWindows = new(BotController.DataBot);
        }

        #region Bot_Ops
        private void Button_SystemEvents_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetSystemEventsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_BuiltInCommands_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetBuiltInCommandsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_UserDefinedCommands_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetUserDefinedCommandsEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void Button_DiscordWebhooks_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetDiscordWebhooksEnabled(((Button)sender).Name.Contains("Enabled"));
        }

        private void ShoutUsers_Click(object sender, RoutedEventArgs e)
        {
            Controller.HandleChatCommandReceived(
                new(commandText: $"{DefaultCommand.soactive}",
                     commandArguments: [""],
                     userType: ViewerTypes.Broadcaster,
                     isBroadcaster: true,
                     displayName: OptionFlags.TwitchChannelName,
                     channel: OptionFlags.TwitchChannelName,
                     message: $"{DefaultCommand.soactive}"));
        }

        #region Refresh data from bot

        /// <summary>
        /// The GUI provides buttons to click and refresh data to the interface. Still must handle response events from the bot.
        /// </summary>
        /// <param name="targetclick">The button to disable while the operation begins./param>
        /// <param name="invokeMethod">The bot method to invoke for the refresh operation.</param>
        private void UpdateData(Button targetclick, Action<string> invokeMethod)
        {
            if (!OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchChannelName))) // prevent operation if default value
            {
                targetclick.IsEnabled = false;

                ThreadManager.CreateThreadStart("UpdateData", () =>
                {
                    try
                    {
                        invokeMethod.Invoke(OptionFlags.TwitchChannelName);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, "UpdateData");
                    }
                });
            }
        }
        private void GuiTwitchBot_OnLiveStreamStarted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("GuiTwitchBot_OnLiveStreamStarted", DebugLogTypes.GUIEvents, "Received an livestream started event.");

            SetLiveStreamActive(true);
        }
        private void GuiTwitchBot_OnLiveStreamStopped(object sender, EventArgs e)
        {
            LogWriter.DebugLog("GuiTwitchBot_OnLiveStreamStopped", DebugLogTypes.GUIEvents, "Received livestream stopped. Notify GUI to update appearance for offline.");

            SetLiveStreamActive(false);
        }
        private void Button_RefreshCategory_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("Button_RefreshCategory_Click", DebugLogTypes.GUIEvents, "User pushed the category button.");

            BeginUpdateCategory();
        }

        private void BeginUpdateCategory()
        {
            LogWriter.DebugLog("BeginUpdateCategory", DebugLogTypes.GUIBotComs, "Received request to begin updating the channel category.");

            Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_RefreshCategory, new Action<string>((s) => BotController.GetUserCategory()));
        }

        private void BotEvents_GetChannelGameName(object sender, FindChannelCategoryEventArgs e)
        {
            LogWriter.DebugLog("BotEvents_GetChannelGameName", DebugLogTypes.GUIEvents, "Received update to the channel game category.");

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
        }

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("Settings_LostFocus", DebugLogTypes.GUIEvents, "Settings lost focus, check Twitch button enable status.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Manage GUI element visibility based on user selection. The intent is to add friendly protection
        /// to prevent accidentally clicking buttons and deleting, often unrecoverable, data-or the latest backup at the least.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            if (Button_ClearCurrencyAccrlValues != null
                && Button_ClearCurrencyAccrlValues != null
                && Button_ClearWatchTime != null
                && GroupBox_ManageDataOptions != null)
            {
                Button_ClearCurrencyAccrlValues.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                Button_ClearNonFollowers.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                Button_ClearWatchTime.IsEnabled = OptionFlags.ManageClearButtonEnabled;
                GroupBox_ManageDataOptions.Visibility = OptionFlags.EnableManageDataOptions ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CheckBox_ManageData_Click(object sender, RoutedEventArgs e)
        {
            // to enable Followers, the Manage Users and Manage Categories (Stream Data) must be enabled first

            if (CheckBox_ManageUsers.IsChecked == true && CheckBox_ManageStreamStat.IsChecked == true)
            {
                CheckBox_ManageFollowers.IsEnabled = true;
            }
            else if (CheckBox_ManageStreamStat.IsChecked == false)
            {
                CheckBox_ManageFollowers.IsEnabled = false;
                CheckBox_ManageFollowers.IsChecked = false; // requires the Manage Users to be enabled
            }

            if (CheckBox_ManageFollowers.IsChecked == true)
            {
                CheckBox_ManageUsers.IsEnabled = false;
                CheckBox_ManageStreamStat.IsEnabled = false;
            }
            else
            {
                CheckBox_ManageUsers.IsEnabled = true;
                CheckBox_ManageStreamStat.IsEnabled = true;
            }

            //if (CheckBox_ManageFollowers.IsChecked == true)
            //{
            //    CheckBox_ManageUsers.IsEnabled = false;
            //    if (Settings.Default.TwitchStreamEventSubAutoStart && Radio_Twitch_StreamerEventSubStart.IsChecked == true)
            //    {
            //        Dispatcher.BeginInvoke(new BotOperation(() =>
            //        {
            //            (Radio_Twitch_StreamerEventSubStart.DataContext as IOModule).StartBot();
            //        }), null);
            //    }
            //}
            //else
            //{
            //    CheckBox_ManageUsers.IsEnabled = true;
            //    if (Radio_Twitch_StreamerEventSubStart.IsChecked == true)
            //    {
            //        Dispatcher.BeginInvoke(new BotOperation(() =>
            //        {
            //            (Radio_Twitch_StreamerEventSubStart.DataContext as IOModule).StopBot();
            //        }), null);
            //    }
            //}

            Controller.ManageDatabase();
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
            //Slider_TimeFollowerPollSeconds.IsEnabled = setvalue;
            Slider_TimeGoLivePollSeconds.IsEnabled = setvalue;
            Slider_TimeClipPollSeconds.IsEnabled = setvalue;

            //TODO: add more textboxes to block when the bots are enabled - to prevent changes
            //ToggleButton_TwitchToken.IsEnabled = setvalue;
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            //TextBlock_TwitchBotLog.ScrollToEnd();
        }

        private void CheckBox_TwitchAdsNotify_Checked(object sender, RoutedEventArgs e)
        {
            Controller.StartTwitchAdNotifications();
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
                if (GroupBox_Options_RepeatCommands != null)
                {
                    GroupBox_Options_RepeatCommands.Width = CheckBox_RepeatCommands_Enable.IsChecked == true ? 460 : 230;
                }
                Controller.ActivateRepeatTimers();
            }
            else if (TBSource?.Name == RadioButton_RepeatTimer_NoAdjustment.Name || TBSource?.Name == RadioButton_RepeatTimer_SlowDownOption.Name || TBSource?.Name == RadioButton_RepeatTimer_ThresholdOption.Name || GBSource?.Name == GroupBox_RepeatTimer_ThresholdOptions.Name)
            {
                if (RadioButton_RepeatTimer_NoAdjustment != null && RadioButton_RepeatTimer_SlowDownOption != null && GroupBox_RepeatTimer_ThresholdOptions != null)
                {
                    SetVisibility(RadioButton_RepeatTimer_SlowDownOption.IsChecked == true ? RadioButton_RepeatTimer_SlowDownOption : RadioButton_RepeatTimer_ThresholdOption, GroupBox_RepeatTimer_ThresholdOptions);
                }
            }
            else if (TBSource?.Name == CheckBox_MediaOverlay_Enable.Name || SPSource?.Name == StackPanel_MediaOverlay_MediaOptions.Name)
            {
                SetVisibility(CheckBox_MediaOverlay_Enable, StackPanel_MediaOverlay_MediaOptions);

                TabItem_Overlays?.Visibility = CheckBox_MediaOverlay_Enable.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (TBSource?.Name == Options_MediaOverlay_CheckBox_UseMultipleEvents.Name || SPSource?.Name == Options_MediaOverlay_StackPanel_MultiEventOptions.Name)
            {
                SetVisibility(Options_MediaOverlay_CheckBox_UseMultipleEvents, Options_MediaOverlay_StackPanel_MultiEventOptions);
            }
            else if (TBSource?.Name == Options_CheckBox_UseSchedule.Name)
            {
                SetVisibility(Options_CheckBox_UseSchedule, TabItem_Schedule);

                TabItem_Schedule?.Visibility = Options_CheckBox_UseSchedule.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
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

        private void CheckBox_RepeatWhenLive_UnChecked(object sender, RoutedEventArgs e)
        {
            // setup the repeat timer; setting is unchecked so the repeat timer should be started for the user
            Controller.ActivateRepeatTimers();
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

        private void Button_ResetStreamCount_Click(object sender, RoutedEventArgs e)
        {
            Controller.ResetCategoryStreamCount();
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

        private void BeginUpdateOverlayTypes()
        { // needs to start only when the Twitch-Helix is ready to accept an API call
            ThreadManager.CreateThreadStart("BeginUpdateOverlayTypes", () =>
            {
                GUIOverlayTypeAlerts.AddData(OverlayTypes.Giveaway, [OverlayTypes.Giveaway.ToString()]);
                GUIOverlayTypeAlerts.AddData(OverlayTypes.ChannelEvents, Enum.GetNames<ChannelEventActions>());

                BotController.DataBot.GetCommandList(false, (data) =>
                {
                    GUIOverlayTypeAlerts.AddData(OverlayTypes.Commands, data);
                });
                RefreshUpdateOverlayTypeChannelPoints();
            });
        }

        private void RefreshUpdateOverlayTypeChannelPoints()
        {
            ThreadManager.CreateThreadStart("RefreshUpdateOverlayTypeChannelPoints", () =>
            {
                GUITwitchBots.GetChannelPoints(OptionFlags.TwitchChannelName);
            });
        }

        private void TwitchBotUserSvc_GetChannelPoints(object sender, OnGetChannelPointsEventArgs e)
        {
            GUIOverlayTypeAlerts.AddData(OverlayTypes.ChannelPoints,
                e.ChannelPointNames);

            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                UpdateGiveawayList(e.ChannelPointNames);

                OverlayService_Bulk_ComboBoxType.ItemsSource = GUIOverlayTypeAlerts.TypeNames;
            });
        }

        private void RadioButton_StartBot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as RadioButton).IsEnabled)
            {
                (sender as RadioButton).IsEnabled = false;
                DispatchStartBotAsync((sender as RadioButton).DataContext as IOModule);
            }
        }

        private void RadioButton_StopBot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as RadioButton).IsEnabled)
            {
                DispatchStopBot((sender as RadioButton).DataContext as IOModule);
            }
        }

        private void TextBox_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        /// <summary>
        /// Every time a text box changes, relative to access credentials, call the TwitchCheckFocusAsync method, which checks the data entry for whether buttons can be enabled so the user can click them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            LogWriter.DebugLog("TextBox_SourceUpdated", DebugLogTypes.GUIEvents, "Text box source updated, check Twitch button enable status.");

            if (sender == TB_Twitch_AccessToken)
            {
                if (!string.IsNullOrEmpty(TB_Twitch_AccessToken.Text))
                {
                    OptionFlags.TwitchBotApproveScopes = OptionFlags.TwitchStreamerUseToken ?
                        OptionFlags.CredentialsTwitchScopesDiffOauthBot : OptionFlags.CredentialsTwitchScopesOauthSame;
                }
            }
            else if (sender == TB_TwitchStreamerAccessToken)
            {
                if (!string.IsNullOrEmpty(TB_TwitchStreamerAccessToken.Text))
                {
                    OptionFlags.TwitchStreamerScopesApproveScopes = OptionFlags.CredentialsTwitchScopesDiffOauthChannel;
                }
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
            LogWriter.DebugLog("RefreshButton_Click", DebugLogTypes.GUIEvents, "User clicked the refresh button, update the label and hide the expired message.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void RefreshButtonNoScopes_Click(object sender, RoutedEventArgs e)
        {
            Label_Twitch_RefreshDate_NoScopes.Content = DateTime.Now.ToLocalTime().AddDays(TwitchTokenRefreshDays);
            TextBlock_ExpiredCredentialsMsg.Visibility = Visibility.Collapsed;
            LogWriter.DebugLog("RefreshButtonNoScopes_Click", DebugLogTypes.GUIEvents, "User clicked the refresh button, update the label and hide the expired message.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void RefreshStreamButton_Click(object sender, RoutedEventArgs e)
        {
            Label_Twitch_StreamerRefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(TwitchTokenRefreshDays);
            TextBlock_ExpiredStreamerCredentialsMsg.Visibility = Visibility.Collapsed;
            LogWriter.DebugLog("RefreshStreamButton_Click", DebugLogTypes.GUIEvents, "User clicked the refresh button, update the label and hide the expired message.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        #endregion

        private void TextBox_TwitchChannelBotNames_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (((TextBox)sender) == TextBox_TwitchBotUserName)
            {
                OptionFlags.TwitchPriorBotName = TextBox_TwitchBotUserName.Text;
            }
            else
            {
                OptionFlags.TwitchPriorChannelName = TextBox_TwitchChannelUserName.Text;
            }

            LogWriter.DebugLog("TextBox_TwitchChannelBotNames_TargetUpdated", DebugLogTypes.GUIEvents, "Text box source updated, check Twitch button enable status.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TwitchCheckFocusAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        #region delegates
        internal delegate void RefreshBotOp(Button targetclick, Action<string> InvokeMethod);
        internal delegate void BotOperation();
        #endregion

        internal async void DispatchStartBotAsync(IIOModule sender)
        {
            await sender?.StartBot();
        }

        internal async void DispatchStopBot(IIOModule sender)
        {
            await sender?.StopBot();
        }

        private void MediaOverlayServer_SetOverlayWindow(object sender, SetOverlayWindowEventArgs e)
        {
            ThreadManager.CreateThreadStart("MediaOverlayServer_SetOverlayWindow", () =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.SetOverlay(((MediaOverlayPage)((Frame)TabItem_Bots_OverlayTab.Content).Content));
                }), null);
            });
        }

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
                        Bots.TwitchEventSubBot => Radio_Twitch_EventSubBotStart,
                        Bots.TwitchEventSubStreamer => Radio_Twitch_EventSubStreamerStart,

                        Bots.TwitchClipBot => Radio_Twitch_ClipBotStart,
                        Bots.TwitchMultiBot => Radio_Twitch_LiveBotStart,
                        Bots.MediaOverlayServer => Radio_Services_OverlayBotStart,
                        Bots.TwitchHelixBot => throw new NotImplementedException(),
                        Bots.DiscordWebhooks => throw new NotImplementedException(),
                        Bots.TwitchBotSendChatClient => throw new NotImplementedException(),
                        Bots.TwitchTokenBot => throw new NotImplementedException(),
                        Bots.Default => throw new NotImplementedException(),
                        _ => throw new NotImplementedException()
                    };

                    HelperStartBot(radio);

                    if (e.BotName == Bots.MediaOverlayServer)
                    {
                        Controller.SendInitialTickerItems();
                    }
                }
            }), null);
        }

        private void GuiTwitchBot_OnBotFailedStart(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                ToggleInputEnabled(true);
                RadioButton radio = e.BotName switch
                {
                    Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                    Bots.TwitchMultiBot => Radio_Twitch_LiveBotStop,

                    Bots.TwitchEventSubBot => Radio_Twitch_EventSubBotStop,
                    Bots.TwitchEventSubStreamer => Radio_Twitch_EventSubStreamerStop,

                    Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                    Bots.TwitchHelixBot => throw new NotImplementedException(),
                    Bots.DiscordWebhooks => throw new NotImplementedException(),
                    Bots.TwitchBotSendChatClient => throw new NotImplementedException(),
                    Bots.TwitchTokenBot => throw new NotImplementedException(),
                    Bots.Default => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };
                HelperStopBot(radio);
            }), null);
        }

        private void GUI_OnBotStopped(object sender, BotStartStopEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                ToggleInputEnabled(true);
                RadioButton radio = e.BotName switch
                {
                    Bots.TwitchClipBot => Radio_Twitch_ClipBotStop,
                    Bots.TwitchMultiBot => Radio_Twitch_LiveBotStop,

                    Bots.TwitchEventSubBot => Radio_Twitch_EventSubBotStop,
                    Bots.TwitchEventSubStreamer => Radio_Twitch_EventSubStreamerStop,

                    Bots.MediaOverlayServer => Radio_Services_OverlayBotStop,
                    Bots.TwitchHelixBot => throw new NotImplementedException(),
                    Bots.DiscordWebhooks => throw new NotImplementedException(),
                    Bots.TwitchTokenBot => throw new NotImplementedException(),
                    Bots.Default => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };
                HelperStopBot(radio);
            }), null);
        }

        /// <summary>
        /// Will force start all bots currently not started, regardless of "start if Live" setting is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUIStartBots_Click(object sender, RoutedEventArgs e)
        {
            foreach (var B in BotOps)
            {
                if (B.Item3.IsEnabled) // isenabled is a check the credentials are added to the bot
                {
                    DispatchStartBotAsync(B.Item3.DataContext as IOModule);
                }
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
                if ((B.Item3.DataContext as IOModule).IsActive == true)
                {
                    DispatchStopBot(B.Item3.DataContext as IOModule);
                }
            }
        }

        /// <summary>
        /// Manages the GUI updates for when bots are started. Hides and enables buttons 
        /// to set the GUI for bots started and allows the user to Stop Bots.
        /// </summary>
        /// <param name="rb">The radio button of the started bot.</param>
        private static void HelperStartBot(RadioButton rb)
        {
            LogWriter.DebugLog("HelperStartBot", DebugLogTypes.GUIEvents, $"GUI updates for {rb.Name} - StartBot buttons.");

            rb.IsChecked = true;

            foreach (UIElement child in (VisualTreeHelper.GetParent(rb) as WrapPanel).Children)
            {
                if (child.GetType() == typeof(RadioButton))
                {
                    (child as RadioButton).IsEnabled = (child as RadioButton).IsChecked != true;
                }
                else if (child.GetType() == typeof(StackPanel))
                {
                    foreach (UIElement SPchild in ((StackPanel)child).Children)
                    {
                        Label currLabel = (Label)SPchild;
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
        }

        /// <summary>
        /// Manages the GUI updates for when bots are stopped. Hides and enables buttons
        /// to set the GUI for bots stopped and allows the user to Start Bots.
        /// </summary>
        /// <param name="rb">The radio button of the stopped bot.</param>
        private static void HelperStopBot(RadioButton rb)
        {
            LogWriter.DebugLog("HelperStopBot", DebugLogTypes.GUIEvents, $"GUI updates for {rb.Name} - StopBot buttons.");

            rb.IsChecked = true;

            foreach (UIElement child in (VisualTreeHelper.GetParent(rb) as WrapPanel).Children)
            {
                if (child.GetType() == typeof(RadioButton))
                {
                    (child as RadioButton).IsEnabled = (child as RadioButton).IsChecked != true;
                }
                else if (child.GetType() == typeof(StackPanel))
                {
                    foreach (UIElement SPchild in ((StackPanel)child).Children)
                    {
                        Label currLabel = (Label)SPchild;
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
        }


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

            string AppVersion = $"v.{App.Version.Major}.{App.Version.Minor}.{App.Version.Build}.{App.Version.Revision}";

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

                BeginUpdateOverlayTypes();
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

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogWriter.DebugLog("TextBox_TextChanged", DebugLogTypes.GUIBotComs, "Twitch Auth TextBox changed.");
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }
        private async void TextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            LogWriter.DebugLog("TextBox_TextChanged", DebugLogTypes.GUIBotComs, "Twitch Auth TextBox changed.");
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }
        private async void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("TextBox_TextChanged", DebugLogTypes.GUIBotComs, "Twitch Auth TextBox changed.");
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }

        private async void Button_TwitchAuthToken_ReAuthorize(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("Button_TwitchAuthToken_ReAuthorize", DebugLogTypes.GUIBotComs, "Twitch Auth Re-Authorization requested.");
            GUIStopBots_Click(this, new());
            if (((Button)sender) == Button_TwitchAuthCode_All_ForceReAuthorization)
            {
                BotController.ForceTwitchAuthReauthorization();
            }
            else if ((Button)sender == Twitch_AuthCode_Button_AuthorizeBot_Reauthorize)
            {
                BotController.ForceTwitchAuthReauthorization(Bots.TwitchEventSubBot);
            }
            else if ((Button)sender == Twitch_AuthCode_Button_AuthorizeStreamer_Reauthorize)
            {
                BotController.ForceTwitchAuthReauthorization(Bots.TwitchStreamerEventSubScopes);
            }
            else if ((Button)sender == Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer_NoScopes_Reauthorize || ((Button)sender) == Twitch_AuthCode_Button_NoScopesBot_Reauthorize)
            {
                BotController.ForceTwitchAuthReauthorization(Bots.TwitchStreamerEventSubNoScopes);
            }

            await TwitchCheckFocusAsync();
        }

        private async void ToggleButton_ChooseTwitchAuth_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("ToggleButton_ChooseTwitchAuth_Click", DebugLogTypes.GUIBotComs, $"Twitch Auth Code flow chosen {OptionFlags.TwitchTokenUseAuth}.");
            GUIStopBots_Click(this, new()); // stop the bots to use the new tokens

            if (OptionFlags.TwitchTokenUseAuth)
            {
                StackPanel_TwitchTokenFlow.Visibility = Visibility.Collapsed;
                StackPanel_TwitchAuthCodeFlow.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_TwitchTokenFlow.Visibility = Visibility.Visible;
                StackPanel_TwitchAuthCodeFlow.Visibility = Visibility.Collapsed;
            }

            await TwitchCheckFocusAsync();
        }

        private void Button_TwitchAuthCode_ApproveBotURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_Button_AuthorizeStreamer.IsEnabled = false;

            BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthBotClientId, false, TwitchAuth_PopupURLAuth,
                async () => { await AuthenticatedAttemptToStartBotsAsync(); });
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_BotAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void Button_TwitchAuthCode_NoScopes_ApproveURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.IsEnabled = false;

            BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthBotClientId, true, TwitchAuth_PopupURLAuth,
                async () => { await AuthenticatedAttemptToStartBotsAsync(); });
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_NoScopesAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void Button_TwitchAuthCode_ApproveStreamerURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_Button_AuthorizeBot.IsEnabled = false;

            BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthStreamerClientId, false, TwitchAuth_PopupURLAuth,
                async () => { await AuthenticatedAttemptToStartBotsAsync(); });
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_StreamerAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void TwitchAuth_PopupURLAuth(string URL)
        {
            Task.Run(async () =>
            {
                LogWriter.DebugLog("TwitchAuth_PopupURLAuth", DebugLogTypes.GUIBotComs, $"Twitch Auth URL: {URL}");

                if (URL == LocalizedMsgSystem.GetVar(Msg.MsgTwitchAuthFailedAuthentication))
                {
                    MessageBox.Show($"Twitch Authentication Code", LocalizedMsgSystem.GetVar(Msg.MsgTwitchAuthFailedAuthentication));
                    await TwitchCheckFocusAsync();
                }
                else
                {
                    try
                    {
                        // use the user's choice - internal app browser
                        if (OptionFlags.TwitchAuthUseInternalBrowser)
                        {
                            AuthWebBrowser authWebBrowser = new();
                            authWebBrowser.Show();
                            authWebBrowser.NavigateToURL(URL);
                        }
                        else
                        { // use User's default browser
                            Process startBrowser = new();
                            startBrowser.StartInfo.UseShellExecute = true;
                            startBrowser.StartInfo.FileName = $"\"{URL}\"";
                            _ = startBrowser.Start();
                        }
                    }
                    catch (Exception Ex)
                    {
                        LogWriter.LogException(Ex, "TwitchAuth_PopupURLAuth");
                    }
                }
            });
        }

        /// <summary>
        /// Callback method to call when the authentication is completed, and attempt to start any bots the user 
        /// selected to start when the app starts.
        /// </summary>
        private async Task AuthenticatedAttemptToStartBotsAsync()
        {
            LogWriter.DebugLog("AuthenticatedAttemptToStartBotsAsync", DebugLogTypes.GUIBotComs, "Twitch Auth completed, attempting to start bots.");
            await TwitchCheckFocusAsync();
        }

        /// <summary>
        /// Handles when the authorization access failed and notifies the user to address the access error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Controller_InvalidAuthorizationToken(object sender, InvalidAccessTokenEventArgs e)
        {
            Dispatcher.BeginInvoke(async () =>
            {
                LogWriter.DebugLog("Controller_InvalidAuthorizationToken", DebugLogTypes.GUIBotComs, $"Twitch Auth invalid token for {e.BotType}, notifying user.");
                switch (e.Platform)
                {
                    case Platform.Default:
                        break;
                    case Platform.Twitch:
                        switch (e.BotType)
                        {
                            case BotType.BotAccount:
                                StatusBar_TwitchAuth_BotAuthCodeInvalid.Visibility = Visibility.Visible;
                                break;
                            case BotType.StreamerAccount:
                                StatusBar_TwitchAuth_StreamerAuthCodeInvalid.Visibility = Visibility.Visible;
                                break;
                        }
                        break;
                }

                await TwitchCheckFocusAsync();
            });
        }

        private async Task TwitchCheckFocusAsync()
        {
            await Task.Run(TwitchCheckFocus);
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void TwitchCheckFocus()
        {
            _ = Dispatcher.BeginInvoke(async () =>
            {
                /*
                evaluate Twitch credentials based on string groups
                user access tokens or auth code tokens

                generate collections of credentials per choice area, apply !string.IsNullOrEmpty to each string, and test if all are true
                */

                // User token data

                // bot account data

                bool UserBotTokenData = (from BUT in (ICollection<string>)[
                                                    OptionFlags.TwitchBotUserName,
                                                    OptionFlags.TwitchBotClientId,
                                                    OptionFlags.TwitchBotAccessToken,
                                                    OptionFlags.TwitchChannelName
                                                      ]
                                         select !string.IsNullOrEmpty(BUT)).All((bt) => bt == true);

                bool UserStreamerTokenData = (!OptionFlags.TwitchStreamerUseToken && UserBotTokenData
                                             && !string.IsNullOrEmpty(OptionFlags.TwitchStreamerNoScopesAccessToken)) ||
                                             (from SUT in (ICollection<string>)[
                                                 OptionFlags.TwitchChannelName,
                                                 OptionFlags.TwitchStreamerClientId,
                                                 OptionFlags.TwitchStreamerAccessToken,
                                                 OptionFlags.TwitchStreamerNoScopesAccessToken
                                                 ]
                                              select !string.IsNullOrEmpty(SUT)).All((st) => st == true);
                // Auth token data

                bool AuthBotTokenData = (from BAT in (ICollection<string>)[
                                                        OptionFlags.TwitchBotUserName,
                                                        OptionFlags.TwitchAuthBotClientId,
                                                        OptionFlags.TwitchAuthBotClientSecret,
                                                        OptionFlags.TwitchChannelName
                                                      ]
                                         select !string.IsNullOrEmpty(BAT)).All((bt) => bt == true);

                bool AuthStreamerTokenData = (!OptionFlags.TwitchStreamerUseToken && UserBotTokenData) ||
                                            (from SUT in (ICollection<string>)[
                                            OptionFlags.TwitchChannelName,
                                            OptionFlags.TwitchAuthStreamerClientId,
                                            OptionFlags.TwitchAuthStreamerClientSecret
                                            ]
                                             select !string.IsNullOrEmpty(SUT)).All((st) => st == true);

                // Check if credentials are empty and we still need to allow the user to authenticate the application, but block it when successfully authenticated
                // The authentication code bot checking clears out the auth code when there's a failure, so this checks it's enabled when
                // both the user adds client Id & secret are available and auth code is not available (not authenticated)
                Twitch_AuthCode_Button_AuthorizeBot.IsEnabled = AuthBotTokenData && OptionFlags.TwitchAuthBotAuthCode == "";
                Twitch_AuthCode_Button_AuthorizeStreamer.IsEnabled = OptionFlags.TwitchStreamerUseToken && AuthStreamerTokenData && OptionFlags.TwitchAuthStreamerAuthCode == "";
                Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.IsEnabled = OptionFlags.TwitchAuthStreamerNoScopesAuthCode == "";

                // Twitch

                SetBotValidAccessLabels();

                (Frame_Bots_Twitch_ManualTokenHelp?.Content as ManualTokenHelp)?.SetScopes();

                if (OptionFlags.TwitchStreamerUseToken)
                {
                    GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Visible;
                    TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Visible;
                    TextBox_TwitchScopesOauthSame.Visibility = Visibility.Collapsed;

                    Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Visible;

                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.Visibility = Visibility.Visible;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer_NoScopes_Reauthorize.Visibility = Visibility.Visible;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeBot.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_Button_NoScopesBot_Reauthorize.Visibility = Visibility.Collapsed;

                    SP_Twitch_UserToken_NoScopes_Bot.Visibility = Visibility.Collapsed;
                    SP_Twitch_UserToken_NoScopes_Streamer.Visibility = Visibility.Visible;

                    GroupBox_Twitch_StartBots_EventSubStreamer.Visibility = Visibility.Visible;
                }
                else
                {
                    GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Collapsed;
                    TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Collapsed;
                    TextBox_TwitchScopesOauthSame.Visibility = Visibility.Visible;

                    Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Collapsed;

                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer_NoScopes_Reauthorize.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeBot.Visibility = Visibility.Visible;
                    Twitch_AuthCode_Button_NoScopesBot_Reauthorize.Visibility = Visibility.Visible;

                    SP_Twitch_UserToken_NoScopes_Bot.Visibility = Visibility.Visible;
                    SP_Twitch_UserToken_NoScopes_Streamer.Visibility = Visibility.Collapsed;

                    GroupBox_Twitch_StartBots_EventSubStreamer.Visibility = Visibility.Collapsed;
                }
                // set earliest token expiration date

                List<DateTime> RefreshTokenDateExpiry = [.. (from R in (ICollection<DateTime>)[OptionFlags.TwitchBotTokenDate, OptionFlags.TwitchStreamerTokenDate]
                                                         where OptionFlags.CurrentToTwitchRefreshDate(R) > new TimeSpan(0, 5, 2)
                                                         select R)];
                StatusBarItem_TokenDate.Content = OptionFlags.TwitchTokenUseAuth ?
                    "Auth Code Refresh" :
                    RefreshTokenDateExpiry.Count != 0 ?
                    RefreshTokenDateExpiry?.Min().ToShortDateString() :
                    "None Valid";

                if (
                    !OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchChannelName))
                    && !OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchBotUserName))
                    && (
                            // use User set tokens, check on using the streamer token
                            (!OptionFlags.TwitchTokenUseAuth && UserBotTokenData && UserStreamerTokenData && RefreshTokenDateExpiry.Count == (OptionFlags.TwitchStreamerUseToken ? 2 : 1))
                        ||
                            // use Auth code tokens, check on using the streamer token
                            (OptionFlags.TwitchTokenUseAuth &&
                                (AuthBotTokenData && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode)) &&
                                (AuthStreamerTokenData && (!OptionFlags.TwitchStreamerUseToken
                                                            || (!string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAuthCode)
                                                                && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerNoScopesAuthCode)
                                                           )))
                            )
                        )
                    )
                {
                    await BotController.TwitchInitializeHelix();
                    SetBotRadioButtons(true, Platform.Twitch); // event handler from Twitch bots, at this point the tokens are authorized
                }
                else
                {
                    SetBotRadioButtons(false, Platform.Twitch);
                    BotController.NotifyInvalidTwitchTokens();
                }
            });
        }

        private void SetBotValidAccessLabels()
        {
            if (OptionFlags.TwitchStreamerUseToken)
            {
                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchBotAccessToken)))
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Collapsed;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Visible;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Visible;
                    }
                }

                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchStreamerAccessToken)))
                {
                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Visible;
                }

                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerNoScopesAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchStreamerNoScopesAccessToken)))
                {
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Invalid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Invalid.Visibility = Visibility.Visible;
                }


                if (TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility == Visibility.Visible
                    && TextBlock_EventSubNotify_AccessToken_NoScopes_Valid.Visibility == Visibility.Visible)
                { // if both accesses are valid, then hide the invalid info
                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode) && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerNoScopesAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchBotAccessToken)))
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Collapsed;

                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Collapsed;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Hidden;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Hidden;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Hidden;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Visible;

                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Visible;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SetBotRadioButtons(bool value, Platform platform)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (RadioButton rb in
                                            from A in BotOps
                                            where A.Item2 == Platform.Service || A.Item2 == platform
                                            select A.Item3
                                            )
                {
                    rb.IsEnabled = value;
                }
            });
        }

        private void Controller_TokensInitializedAsync(object sender, EventArgs e)
        {
            ThreadManager.AddTaskToGUIDispatcher(() => Task.Run(() =>
            {
                SetBotRadioButtons(true, Platform.Twitch); // event handler from Twitch bots, at this point the tokens are authorized
                StartAutoBots();

                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    Bots_StartBots_ButtonStartAllBots.IsEnabled = true;
                });
            }));
        }

        #region MultiLive

        private void MultiLive_Data_Loaded(object sender, RoutedEventArgs e)
        {
            Controller.HandleOnDataCollectionUpdated((MultiLive_Data.Content as MultiLiveDataGrids).DataManager_OnDataCollectionUpdated);

            // allow edits while bot is active
            (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(true);
            (MultiLive_Data.Content as MultiLiveDataGrids).SetHandlers(Settings_LostFocus, TB_BotActivityLog_TextChanged);
            (MultiLive_Data.Content as MultiLiveDataGrids).SummarizeChannels += StreamerBotWindow_SummarizeChannels;
            (MultiLive_Data.Content as MultiLiveDataGrids).FindMultiChannelUserId += StreamerBotWindow_FindMultiChannelUserId;
            (MultiLive_Data.Content as MultiLiveDataGrids).AddNewMultiChannelUser += StreamerBotWindow_AddNewMultiChannelUser;
            (MultiLive_Data.Content as MultiLiveDataGrids).GUISaveEdits = Controller.GUISaveDataGridEdits;
            (MultiLive_Data.Content as MultiLiveDataGrids).PreviewKeyDownDeleteRows += MultiLive_DG_PreviewKeyDown_Click;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemAddClick += MenuItem_AddClick;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemEditClick += MenuItem_EditClick;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemDeleteClick += MenuItem_DeleteClick;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemEnabledClick += DataGridContextMenu_EnableItems_Click;
            (MultiLive_Data.Content as MultiLiveDataGrids).MenuItemDisabledClick += DataGridContextMenu_DisableItems_Click;
            AddMultiLiveFoundUserId += (MultiLive_Data.Content as MultiLiveDataGrids).UpdateMultiChannelUserId;

#if DEBUG
            (MultiLive_Data.Content as MultiLiveDataGrids).DebugAddNewMultiLiveData += StreamerBotWindow_DebugAddNewMultiLiveData;
#endif

            ((Frame)sender).Loaded -= MultiLive_Data_Loaded;
        }

#if DEBUG
        private void StreamerBotWindow_DebugAddNewMultiLiveData(object sender, EventArgs e)
        {
            Controller.DebugAddNewMultiLiveData();
        }
#endif

        private const string MultiLiveName = "MultiUserLiveBot";

        private event EventHandler<AddNewMultiChannelUserEventArgs> AddMultiLiveFoundUserId;

        private void StreamerBotWindow_AddNewMultiChannelUser(object sender, AddNewMultiChannelUserEventArgs e)
        {
            Controller.AddNewMonitorChannel([e.LiveUser]);
        }

        private void StreamerBotWindow_FindMultiChannelUserId(object sender, AddNewMultiChannelUserEventArgs e)
        {
            e.LiveUser.UserId = BotController.GetMultiChannelUserId(e.LiveUser.UserName);
            AddMultiLiveFoundUserId?.Invoke(this, new(e.LiveUser));
        }

        private bool SummarizeChannels = false;

        private void StreamerBotWindow_SummarizeChannels(object sender, MultiLiveSummarizeEventArgs e)
        {
            if (!SummarizeChannels) // some reason, this event is being called three times, prevent extra calls
            {
                SummarizeChannels = true;
                Controller.MultiChannelSummarize(e);
                SummarizeChannels = false;
            }
        }

        #endregion


    }
}
