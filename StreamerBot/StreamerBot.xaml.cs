using StreamerBotLib.BotClients;
using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StreamerBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StreamerBotWindow : Window, INotifyPropertyChanged
    {
        internal static BotController Controller { get; private set; }
        private ManageWindows PopupWindows { get; set; } = new();

        private readonly GUITwitchBots guiTwitchBot;
        private readonly GUIAppStats guiAppStats;
        private readonly DateTime StartBotDate;
        private DateTime TwitchFollowRefresh;
        private int TwitchFollowerCurrRefreshHrs = 0;
        private readonly TimeSpan CheckRefreshDate = new(7, 0, 0, 0);
        private const string MultiLiveName = "MultiUserLiveBot";

        internal Dispatcher AppDispatcher { get; private set; } = Dispatcher.CurrentDispatcher;

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
            WatchProcessOps = true;
            IsMultiProcActive = null;
            OptionFlags.SetSettings();

            Controller = new();
            Controller.SetDispatcher(AppDispatcher);

            InitializeComponent();

            guiTwitchBot = Resources["TwitchBot"] as GUITwitchBots;
            guiAppStats = Resources["AppStats"] as GUIAppStats;

            guiTwitchBot.OnBotStopped += GUI_OnBotStopped;
            guiTwitchBot.OnBotStarted += GUI_OnBotStarted;
            guiTwitchBot.OnBotStarted += GuiTwitchBot_GiveawayEvents;
            guiTwitchBot.OnLiveStreamStarted += GuiTwitchBot_OnLiveStreamEvent;
            guiTwitchBot.OnFollowerBotStarted += GuiTwitchBot_OnFollowerBotStarted;
            guiTwitchBot.OnLiveStreamUpdated += GuiTwitchBot_OnLiveStreamEvent;
            guiTwitchBot.RegisterChannelPoints(TwitchBotUserSvc_GetChannelPoints);
            Controller.OnStreamCategoryChanged += BotEvents_GetChannelGameName;
            ThreadManager.OnThreadCountUpdate += ThreadManager_OnThreadCountUpdate;

            List<int> hrslist = new() { 1, 2, 4, 8, 12, 16, 24, 36, 48, 60, 72 };
            ComboBox_TwitchFollower_RefreshHrs.ItemsSource = hrslist;
            SetTwitchFollowerRefreshTime();

            ThreadManager.CreateThreadStart(ProcessWatcher);
            NotifyExpiredCredentials += BotWindow_NotifyExpiredCredentials;

#if !DEBUG
            TabItem_Data_MultiLive.Visibility = Visibility.Collapsed;
            TabItem_Data_Separator.Visibility = Visibility.Collapsed;
            GroupBox_Bots_Starts_MultiLive.Visibility = Visibility.Collapsed;
#endif
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
                        Bots.Default => throw new NotImplementedException(),
                        Bots.TwitchUserBot => throw new NotImplementedException(),
                        _ => throw new NotImplementedException()
                    };
                    HelperStartBot(radio);
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
                      Bots.Default => throw new NotImplementedException(),
                      Bots.TwitchUserBot => throw new NotImplementedException(),
                      _ => throw new NotImplementedException()
                  };
                  HelperStopBot(radio);
              }), null);
        }

        #endregion

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
                    CommandText = $"!{DefaultCommand.soactive}",
                    UserType = ViewerTypes.Broadcaster,
                    IsBroadcaster = true,
                    DisplayName = OptionFlags.TwitchChannelName,
                    Channel = OptionFlags.TwitchChannelName,
                    Message = $"!{DefaultCommand.soactive}"
                },
                Bots.TwitchChatBot);
        }

        #region Refresh data from bot

        /// <summary>
        /// The GUI provides buttons to click and refresh data to the interface. Still must handle response events from the bot.
        /// </summary>
        /// <param name="targetclick">The button to disable while the operation begins./param>
        /// <param name="InvokeMethod">The bot method to invoke for the refresh operation.</param>
        private void UpdateData(Button targetclick, Action<string> InvokeMethod)
        {
            if(!OptionFlags.CheckSettingIsDefault("TwitchChannelName", OptionFlags.TwitchChannelName)) // prevent operation if default value
            {
                targetclick.IsEnabled = false;

                new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        InvokeMethod.Invoke(Settings.Default.TwitchChannelName);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    }
                })).Start();
            }
        }

        private void Button_RefreshCategory_Click(object sender, RoutedEventArgs e)
        {
            BeginUpdateCategory();
        }

        private void GuiTwitchBot_OnLiveStreamEvent(object sender, EventArgs e)
        {
            BeginUpdateCategory();
        }

        private void BeginUpdateCategory()
        {
            Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_RefreshCategory, new Action<string>((s) => guiTwitchBot.GetUserGameCategory(UserName: s)));
        }

        private void BotEvents_GetChannelGameName(object sender, OnGetChannelGameNameEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TextBlock_CurrentCategory.Text = e.GameName;
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
            List<RadioButton> BotOps = new()
            {
                Radio_MultiLiveTwitch_StopBot,
                Radio_Twitch_FollowBotStop,
                Radio_Twitch_LiveBotStop,
                Radio_Twitch_StopBot,
                Radio_Twitch_ClipBotStop
            };

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

        #region MultiLive

        private void SetMultiLiveActive(bool ProcessFound = false)
        {
            Label_LiveStream_MultiLiveActiveMsg.Visibility = ProcessFound ? Visibility.Visible : Visibility.Collapsed;
            SetMultiLiveButtons();
        }

        private void SetMultiLiveButtons()
        {
            if (IsMultiProcActive == false)
            {
                SetMultiLiveTabItems(true);

                BotController.ConnectTwitchMultiLive();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = !Radio_Twitch_LiveBotStart.IsChecked ?? false;
                Radio_Twitch_LiveBotStop.IsEnabled = false; // can't stop the live bot service while monitoring multiple channels
                NotifyPropertyChanged(nameof(guiTwitchBot));
            }
            else if (IsMultiProcActive == true)
            {
                SetMultiLiveTabItems();
                MultiBotRadio();
                BotController.DisconnectTwitchMultiLive();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
            }
        }

        private void SetMultiLiveTabItems(bool Visible = false)
        {
            TabItem_Data_MultiLive.Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;
            TabItem_Data_Separator.Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;
            GroupBox_Bots_Starts_MultiLive.Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Radio_Twitch_LiveBotStart_Checked(object sender, RoutedEventArgs e)
        {
            Radio_MultiLiveTwitch_StartBot.IsEnabled = IsMultiProcActive == false && ((sender as RadioButton).IsChecked ?? false);
        }

        private void Radio_Twitch_LiveBotStop_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                MultiBotRadio();
            }
        }

        private void BC_MultiLiveTwitch_BotOp(object sender, MouseButtonEventArgs e)
        {
            if (sender == Radio_MultiLiveTwitch_StartBot)
            {
                MultiBotRadio(true);
            }
            else if (sender == Radio_MultiLiveTwitch_StopBot)
            {
                MultiBotRadio();
            }
        }

        private void MultiBotRadio(bool Start = false)
        {
            if (Controller != null && guiTwitchBot != null && guiTwitchBot.TwitchLiveMonitor.IsMultiConnected)
            {
                if (Start && Radio_MultiLiveTwitch_StartBot.IsEnabled && Radio_MultiLiveTwitch_StartBot.IsChecked != true)
                {
                    BotController.StartTwitchMultiLive();
                    Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
                    Radio_MultiLiveTwitch_StartBot.IsChecked = true;
                    Radio_MultiLiveTwitch_StopBot.IsEnabled = true;

                    DG_Multi_LiveStreamStats.ItemsSource = null;
                    DG_Multi_LiveStreamStats.Visibility = Visibility.Collapsed;

                    Panel_BotActivity.Visibility = Visibility.Visible;
                }
                else
                {
                    BotController.StopTwitchMultiLive();
                    Radio_MultiLiveTwitch_StartBot.IsEnabled = true;
                    Radio_MultiLiveTwitch_StopBot.IsEnabled = false;
                    Radio_MultiLiveTwitch_StopBot.IsChecked = true;

                    if (IsMultiProcActive == true)
                    {
                        DG_Multi_LiveStreamStats.ItemsSource = TwitchBotLiveMonitorSvc.MultiLiveDataManager.LiveStream;
                    }
                    DG_Multi_LiveStreamStats.Visibility = Visibility.Visible;

                    Panel_BotActivity.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            int start = TB_LiveMsg.SelectionStart;

            if (TB_LiveMsg.SelectionLength > 0)
            {
                TB_LiveMsg.Text = TB_LiveMsg.Text.Remove(start, TB_LiveMsg.SelectionLength);
            }

            TB_LiveMsg.Text = TB_LiveMsg.Text.Insert(start, (sender as MenuItem).Header.ToString());
            TB_LiveMsg.SelectionStart = start;
        }

        private void DG_ChannelNames_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            BotController.UpdateTwitchMultiLiveChannels();
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
            CheckFocus();
            if (OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchRefreshDate) >= CheckRefreshDate)
            {
                List<Tuple<bool, RadioButton>> BotOps = new()
                {
                    new(Settings.Default.TwitchChatBotAutoStart, Radio_Twitch_StartBot),
                    new(Settings.Default.TwitchFollowerSvcAutoStart, Radio_Twitch_FollowBotStart),
                    new(Settings.Default.TwitchLiveStreamSvcAutoStart, Radio_Twitch_LiveBotStart),
                    new(Settings.Default.TwitchMultiLiveAutoStart, Radio_MultiLiveTwitch_StartBot),
                    new(Settings.Default.TwitchClipAutoStart, Radio_Twitch_ClipBotStart)
                };
                foreach (Tuple<bool, RadioButton> tuple in from Tuple<bool, RadioButton> tuple in BotOps
                                                           where tuple.Item1 && tuple.Item2.IsEnabled
                                                           select tuple)
                {
                    if (tuple.Item2 != Radio_MultiLiveTwitch_StartBot)
                    {
                        Dispatcher.BeginInvoke(new BotOperation(() =>
                        {
                            (tuple.Item2.DataContext as IOModule)?.StartBot();
                        }), null);
                    }
                    else
                    {
                        SetMultiLiveButtons();
                        MultiBotRadio(true);
                    }
                }
                BeginUpdateCategory();
            }

            CheckMessageBoxes();

            CheckBox_ManageData_Click(sender, new());

            // TODO: research auto-refreshing token
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            WatchProcessOps = false;
            OptionFlags.IsStreamOnline = false;
            OptionFlags.ActiveToken = false;
            //CP.Close();
            Controller.ExitBots();
            OptionFlags.SetSettings();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
        }

        private void CheckMessageBoxes()
        {

//#if DEBUG
//            OptionFlags.ManageDataArchiveMsg = true;
//#endif

            if (OptionFlags.ManageDataArchiveMsg)
            {
                MessageBox.Show(LocalizedMsgSystem.GetVar(MsgBox.MsgBoxManageDataArchiveMsg), LocalizedMsgSystem.GetVar(MsgBox.MsgBoxManageDataArchiveTitle));

                Settings.Default.ManageDataArchiveMsg = false;
                OptionFlags.SetSettings();
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
            OptionFlags.SetSettings();
        }

        private void CheckBox_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();

            CheckDebug();
        }

        private void CheckDebug()
        {
            StackPanel_DebugLivestream.Visibility = Settings.Default.DebugLiveStream ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckBox_ManageData_Click(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();

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
                if (Settings.Default.TwitchFollowerSvcAutoStart)
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

            Controller.ManageDatabase();
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void CheckFocus()
        {
            if (TB_Twitch_Channel.Text.Length != 0 && TB_Twitch_BotUser.Text.Length != 0 && TB_Twitch_ClientID.Text.Length != 0 && TB_Twitch_AccessToken.Text.Length != 0 && OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchRefreshDate) >= new TimeSpan(0, 0, 0))
            {
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_FollowBotStart.IsEnabled = true;
                Radio_Twitch_LiveBotStart.IsEnabled = true;
                Radio_Twitch_ClipBotStart.IsEnabled = true;
                Radio_Twitch_PubSubBotStart.IsEnabled = true;
            }
            else
            {
                Radio_Twitch_StartBot.IsEnabled = false;
                Radio_Twitch_FollowBotStart.IsEnabled = false;
                Radio_Twitch_LiveBotStart.IsEnabled = false;
                Radio_Twitch_ClipBotStart.IsEnabled = false;
                Radio_Twitch_PubSubBotStart.IsEnabled = false;
            }

            // Twitch

            if(TB_Twitch_Channel.Text != TB_Twitch_BotUser.Text)
            {
                GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Visible;
                TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Visible;
                TextBox_TwitchScopesOauthSame.Visibility = Visibility.Collapsed;
                Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Visible;
                Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Visible;
                Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Collapsed;
            }
            else
            {
                GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Collapsed;
                TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Collapsed;
                TextBox_TwitchScopesOauthSame.Visibility = Visibility.Visible;
                Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Collapsed;
                Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Collapsed;
                Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Visible;
            }

            // set earliest token expiration date

            List<DateTime> RefreshTokenDateExpiry = new() { OptionFlags.TwitchRefreshDate, OptionFlags.TwitchStreamerTokenDate };
            RefreshTokenDateExpiry.RemoveAll((d) => d < DateTime.Now);
            StatusBarItem_TokenDate.Content = RefreshTokenDateExpiry.Count != 0 ? RefreshTokenDateExpiry?.Min().ToShortDateString() : "None Valid";

            MultiBotRadio();
            CheckDebug();
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
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock_TwitchBotLog.ScrollToEnd();
        }

        // TODO: fix scrolling in Sliders but not scroll the whole panel

        private bool SliderMouseCaptured;

        private void Slider_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider curr = (Slider)sender;
            curr.Value += (e.Delta > 0 ? 1 : -1) * curr.SmallChange;
        }

        private void Slider_MouseEnter(object sender, MouseEventArgs e)
        {
            SliderMouseCaptured = true;
        }

        private void Slider_MouseLeave(object sender, MouseEventArgs e)
        {
            SliderMouseCaptured = false;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0 && SliderMouseCaptured)
            {
                e.Handled = true;
                return;
            }
        }

        private void CheckBox_Checked_PanelVisibility(object sender, RoutedEventArgs e)
        {
            CheckBox CBSource = null;
            StackPanel SPSource = null;
            if (sender.GetType() == typeof(CheckBox))
            {
                CBSource = (CheckBox)sender;
            }
            else if (sender.GetType() == typeof(StackPanel))
            {
                SPSource = (StackPanel)sender;
            }

            void SetVisibility(CheckBox box, StackPanel panel)
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

            if (CBSource?.Name == CheckBox_ModFollower_BanEnable.Name || SPSource?.Name == StackPanel_ModerateFollowers_Count.Name)
            {
                SetVisibility(CheckBox_ModFollower_BanEnable, StackPanel_ModerateFollowers_Count);
            }
            else if (CBSource?.Name == CheckBox_TwitchFollower_LimitMsgs.Name || SPSource?.Name == StackPanel_TwitchFollower_LimitMsgs_Count.Name)
            {
                SetVisibility(CheckBox_TwitchFollower_LimitMsgs, StackPanel_TwitchFollower_LimitMsgs_Count);
            }
            else if (CBSource?.Name == CheckBox_TwitchFollower_AutoRefresh.Name || SPSource?.Name == StackPanel_TwitchFollows_RefreshHrs.Name)
            {
                SetVisibility(CheckBox_TwitchFollower_AutoRefresh, StackPanel_TwitchFollows_RefreshHrs);
            }
        }

        private void TextBox_Follower_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox src = (TextBox)sender;

            if(int.TryParse(src.Text, out int result))
            {
                if(result is <1 or > 100)
                {
                    src.Text = (result < 1 ? 1 : result > 100 ? 100 : result).ToString();
                }
            }
        }

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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Label_Twitch_RefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(60);
            TextBlock_ExpiredCredentialsMsg.Visibility = Visibility.Collapsed;
            CheckFocus();
        }

        private void RefreshStreamButton_Click(object sender, RoutedEventArgs e)
        {
            Label_Twitch_StreamerRefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(60);
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

            void SetWidth(DataGridColumn dgc, int Width = DGColWidth)
            {
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
                        if (dc.Header.ToString() is not "Id" and not "UserName" and not "FirstDateSeen" and not "LastDateSeen" and not "WatchTime")
                        {
                            Collapse(dc);
                        }
                    }
                    break;
                case "DG_Followers":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Id" and not "UserName" and not "IsFollower" and not "FollowedDate")
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
                case "DG_Webhooks":
                    foreach(DataGridColumn dc in dg.Columns)
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
            // TODO: Setup MultiLiveBot Context Menu Add/Edit records
            if (sender.GetType() == typeof(DataGrid))
            {
                if (((DataGrid)sender).Name is "DG_BuiltInCommands" or "DG_CommonMsgs")
                {
                    ((MenuItem)Resources["DataGridContextMenu_AddItem"]).IsEnabled = false;
                    ((MenuItem)Resources["DataGridContextMenu_DeleteItems"]).IsEnabled = false;
                }
                else
                {
                    ((MenuItem)Resources["DataGridContextMenu_AddItem"]).IsEnabled = true;
                    ((MenuItem)Resources["DataGridContextMenu_DeleteItems"]).IsEnabled = true;
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
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as DataGrid;

            Popup_DataEdit(item);
        }

        private void Popup_DataEdit(DataGrid sourceDataGrid, bool AddNew = true)
        {
            if (AddNew)
            {
                DataView CurrdataView = (DataView)sourceDataGrid.ItemsSource;
                if (CurrdataView != null)
                {
                    PopupWindows.DataGridAddNewItem(CurrdataView.Table);
                }
            }
            else
            {
                DataRowView dataView = (DataRowView)sourceDataGrid.SelectedItem;
                if (dataView != null)
                {
                    PopupWindows.DataGridEditItem(dataView.Row.Table, dataView.Row);
                }
            }
        }

        private void MenuItem_EditClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as DataGrid;

            Popup_DataEdit(item, false);
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            DataGrid item = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as DataGrid;

            SystemsController.DeleteRows(new List<DataRow>(item.SelectedItems.Cast<DataRowView>().Select(DRV => DRV.Row)));
        }

        #endregion

        #region Debug Empty Stream

        private DateTime DebugStreamStarted = DateTime.MinValue;

        private void StartDebugStream_Click(object sender, RoutedEventArgs e)
        {
            if (DebugStreamStarted == DateTime.MinValue)
            {
                DebugStreamStarted = DateTime.Now.ToLocalTime();

                string User = "";
                string Category = "Microsoft Flight Simulator";
                string ID = "7193";
                string Title = "Testing a debug stream";

                Controller.HandleOnStreamOnline(User, Title, DebugStreamStarted, ID, Category, true);

                List<Tuple<string, string>> output = SystemsController.DataManage.GetGameCategories();
                Random random = new();
                Tuple<string, string> itemfound = output[random.Next(output.Count)];
                Controller.HandleOnStreamUpdate(itemfound.Item1, itemfound.Item2);
            }

        }

        private void EndDebugStream_Click(object sender, RoutedEventArgs e)
        {
            if (DebugStreamStarted != DateTime.MinValue)
            {
                BotController.HandleOnStreamOffline();

                DebugStreamStarted = DateTime.MinValue;
            }

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

        private void Button_Giveaway_RefreshChannelPoints_Click(object sender, RoutedEventArgs e)
        {
            BeginGiveawayChannelPtsUpdate();
        }

        private void BeginGiveawayChannelPtsUpdate()
        {
            _ = Dispatcher.BeginInvoke(new RefreshBotOp(UpdateData), Button_Giveaway_RefreshChannelPoints, new Action<string>((s) => guiTwitchBot.GetChannelPoints(UserName: s)));
        }

        private void TwitchBotUserSvc_GetChannelPoints(object sender, OnGetChannelPointsEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ComboBox_Giveaway_ChanPts.ItemsSource = e.ChannelPointNames;
                Button_Giveaway_RefreshChannelPoints.IsEnabled = true;
            });
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
                   ItemName = (string) ComboBox_Giveaway_Coms.SelectedValue;
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

        #region WatcherTools

        private bool WatchProcessOps;

        /// <summary>
        /// Handler to stop the bots when the credentials are expired. The thread acting on the bots must be the GUI thread, hence this notification.
        /// </summary>
        public event EventHandler NotifyExpiredCredentials;

        /// <summary>
        /// True - "MultiUserLiveBot.exe" is active, False - "MultiUserLiveBot.exe" is not active
        /// </summary>
        private bool? IsMultiProcActive;

        private delegate void ProcWatch(bool IsActive);

        private void UpdateProc(bool IsActive)
        {
            _ = Application.Current.Dispatcher.BeginInvoke(new ProcWatch(SetMultiLiveActive), IsActive);
        }

        private void ProcessWatcher()
        {
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

                    if( OptionFlags.TwitchFollowerAutoRefresh && DateTime.Now >= TwitchFollowRefresh)
                    {
                        Controller.TwitchStartUpdateAllFollowers();
                        TwitchFollowRefresh = DateTime.Now.AddHours(TwitchFollowerCurrRefreshHrs);
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

            OptionFlags.SetSettings();
            CheckDebug();

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
