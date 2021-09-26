using ChatBot_Net5.BotClients;
using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Events;
using ChatBot_Net5.Models;
using ChatBot_Net5.Properties;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for BotWindow.xaml
    /// </summary>
    public partial class BotWindow : Window, INotifyPropertyChanged
    {
        // TODO: Add color themes
        //private readonly ChatPopup CP;
        private const string MultiLiveName = "MultiUserLiveBot";

        private readonly TimeSpan CheckRefreshDate = new(7, 0, 0, 0);

        private readonly BotController controller;

        private bool IsAddNewRow;

        public BotWindow()
        {
            // move settings to the newest version, if the application version upgrades
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            WatchProcessOps = true;
            IsMultiProcActive = null;
            OptionFlags.SetSettings();


            InitializeComponent();

            ResourceDictionary language = new() { Source = new("/Culture/GUI_Msgs.xaml", UriKind.Relative)};
            Resources.MergedDictionaries.Add(language);

            OptionFlags.SetSettings();
            controller = Resources["ControlBot"] as BotController;

            // TODO: debug ChatPopup
            //CP = new();
            //CP.Closing += CP_Closing;
            //CP.DataContext = this;
            //CP.Page_ChatPopup_FlowDocViewer.SetBinding(System.Windows.Controls.Primitives.DocumentViewerBase.DocumentProperty, new Binding("Document") { Source = FlowDoc_ChatBox, Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //CP.SetBinding(OpacityProperty, new Binding("Opacity") { Source = Slider_PopOut_Opacity, Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            new Thread(new ThreadStart(ProcessWatcher)).Start();
            NotifyExpiredCredentials += BotWindow_NotifyExpiredCredentials;
            controller.OnBotStarted += Controller_OnBotStarted;
            controller.OnBotStopped += Controller_OnBotStopped;
        }

        #region Events
        #region Windows & Tab Ops
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckFocus();
            if (OptionFlags.CurrentToTwitchRefreshDate() >= CheckRefreshDate)
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
            }

            // TODO: add follower service online, offline, and repeat timers to re-run service
            // TODO: *done*turn off bots & prevent starting if the token is expired*done* - research auto-refreshing token
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            WatchProcessOps = false;
            IsAppClosing = true;
            OptionFlags.ProcessOps = false;
            //CP.Close();
            controller.ExitSave();
            OptionFlags.SetSettings();
        }

        private void TabItem_Twitch_GotFocus(object sender, RoutedEventArgs e)
        {
            Twitch_RefreshDate.Foreground = OptionFlags.CurrentToTwitchRefreshDate() <= CheckRefreshDate
                ? new SolidColorBrush(Color.FromRgb(255, 0, 0))
                : new SolidColorBrush(Color.FromRgb(0, 0, 0));

        }

        /// <summary>
        /// Determines whether the streamer channel name and token are entered, to enable the checkbox option.
        /// </summary>
        /// <param name="sender">Object sending the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox FollowbackStreamer = (CheckBox)sender;
            FollowbackStreamer.IsEnabled = false;

            if (OptionFlags.TwitchStreamerToken != null && OptionFlags.CurrentToTwitchRefreshDate(true).TotalSeconds >= 0)
            {
                FollowbackStreamer.IsEnabled = true;
            }
        }

        #endregion

        private void Controller_OnBotStopped(object sender, BotStartStopArgs e)
        {
            Dispatcher.BeginInvoke(new BotOperation(() =>
            {
                ToggleInputEnabled(true);

                RadioButton radio;
                switch (e.BotName)
                {
                    case Enum.Bots.TwitchChatBot:
                        radio = Radio_Twitch_StopBot;
                        break;
                    case Enum.Bots.TwitchClipBot:
                        radio = Radio_Twitch_ClipBotStop;
                        break;
                    case Enum.Bots.TwitchFollowBot:
                        radio = Radio_Twitch_FollowBotStop;
                        break;
                    case Enum.Bots.TwitchLiveBot:
                        radio = Radio_Twitch_LiveBotStop;
                        break;
                    default:
                        radio = Radio_MultiLiveTwitch_StopBot;
                        break;
                }

                HelperStopBot(radio);
            }),null);
        }

        private void Controller_OnBotStarted(object sender, BotStartStopArgs e)
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

                      RadioButton radio;
                      switch (e.BotName)
                      {
                          case Enum.Bots.TwitchChatBot:
                              radio = Radio_Twitch_StartBot;
                              break;
                          case Enum.Bots.TwitchClipBot:
                              radio = Radio_Twitch_ClipBotStart;
                              break;
                          case Enum.Bots.TwitchFollowBot:
                              radio = Radio_Twitch_FollowBotStart;
                              break;
                          case Enum.Bots.TwitchLiveBot:
                              radio = Radio_Twitch_LiveBotStart;
                              break;
                          default:
                              radio = Radio_MultiLiveTwitch_StartBot;
                              break;
                      }

                      HelperStartBot(radio);
                  }
              }), null);
        }

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

        #region PopOut Chat Window
        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
        //    CP.Show();
        //    CP.Height = 500;
        //    CP.Width = 300;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new(propname));
        }

        private void Slider_PopOut_Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OnPropertyChanged("Opacity");
        }

        private bool IsAppClosing;
        private void CP_Closing(object sender, CancelEventArgs e)
        {
        //    if (!IsAppClosing) // flag to really close the window
        //    {
        //        e.Cancel = true;
        //        CP.Hide();
        //    }
        }

        #endregion

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFocus();
            OptionFlags.SetSettings();
        }

        private void CheckBox_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();
        }

        private void CheckBox_ManageData_Click(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();

            static Visibility SetVisibility(bool Check) { return Check ? Visibility.Visible : Visibility.Collapsed; }

            TabItem_Users.Visibility = SetVisibility(OptionFlags.ManageUsers);
            TabItem_StreamStats.Visibility = SetVisibility(OptionFlags.ManageStreamStats);

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
                Dispatcher.BeginInvoke(new BotOperation(() =>
                {
                    (Radio_Twitch_FollowBotStart.DataContext as IOModule).StopBot();
                }), null);
            }

            controller.ManageDatabase();
        }

        private void JoinCollectionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ((BotController)LV_JoinList.DataContext).JoinCollection.Remove((sender as CheckBox).DataContext as UserJoin);
        }

        private void TextBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            if ((sender as TextBox).Name == "TB_Twitch_AccessToken")
            {
                RefreshButton_Click(this, null); // invoke date refresh when the access token is changed
            }

            CheckFocus();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Twitch_RefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(60);
            TextBlock_ExpiredCredentialsMsg.Visibility = Visibility.Collapsed;
            CheckFocus();
        }

        private void RefreshStreamButton_Click(object sender, RoutedEventArgs e)
        {
            Twitch_StreamerRefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(60);
            TextBlock_ExpiredCredentialsMsg.Visibility = Visibility.Collapsed;
            CheckFocus();
        }

        private void TextBox_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private delegate void BotOperation();

        private void RadioButton_StartBot_PreviewMoustLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as RadioButton).IsEnabled)
            {
                Dispatcher.BeginInvoke(new BotOperation( () =>
                {
                    ((sender as RadioButton).DataContext as IOModule)?.StartBot();
                }),null);
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

        private void DG_CommonMsgs_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGrid dg = (sender as DataGrid);

            // find the new item, hide columns other than the primary data columns, i.e. relational columns
            switch (dg.Name)
            {
                case "DG_Users":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Id" and not "UserName" and not "FirstDateSeen" and not "CurrLoginDate" and not "LastDateSeen" and not "WatchTime")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case "DG_Followers":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Id" and not "UserName" and not "IsFollower" and not "FollowedDate")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case "DG_Currency":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "CurrencyName" and not "AccrueAmt" and not "Seconds")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case "DG_CurrencyAccrual":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "UserName" and not "CurrencyName" and not "Value")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;

                case "DG_BuiltInCommands":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() == "CmdName")
                        {
                            dc.IsReadOnly = true;
                        }
                    }
                    break;

                case "DG_CategoryList" or "DG_CategoryList_Clips":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() is not "Category" and not "CategoryId")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }


        /// <summary>
        /// Sets a DataGrid to accept a new row.
        /// </summary>
        /// <param name="sender">Object sending event.</param>
        /// <param name="e">Mouse arguments related to sending object.</param>
        private void DataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGrid curr = sender as DataGrid;

            if (curr.IsMouseOver)
            {
                curr.CanUserAddRows = true;
            }
        }

        /// <summary>
        /// Sets a DataGrid to no longer accept a new row.
        /// </summary>
        /// <param name="sender">Object sending event.</param>
        /// <param name="e">Mouse arguments related to sending object.</param>
        private void DataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            DataGrid curr = sender as DataGrid;

            if (!(curr.IsMouseOver || IsAddNewRow)) // check for mouse over object and check if adding new row
            {
                curr.CanUserAddRows = false;    // this fails if mouse leaves while user hasn't finished adding a new row - the if flag prevents this
            }
        }

        /// <summary>
        /// Need to call this event to manage the mouse over "Adding New Rows" event change. 
        /// Leaving a DataGrid and trying to change "CanUserAddRows" to false while still editing a new row will throw an exception.
        /// Sets a flag used for the "MouseLeave" to prevent DataGrid from entering an error state.
        /// </summary>
        /// <param name="sender">Object sending the event.</param>
        /// <param name="e">Params from the object.</param>
        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            IsAddNewRow = true;
        }

        /// <summary>
        /// Need to call this event to manage the mouse over "Adding New Rows" event change. 
        /// Leaving a DataGrid and trying to change "CanUserAddRows" to false while still editing a new row will throw an exception.
        /// Sets a flag used for the "MouseLeave" to prevent DataGrid from entering an error state.
        /// </summary>
        /// <param name="sender">Object sending the event.</param>
        /// <param name="e">Params from the object.</param>
        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (IsAddNewRow)
            {
                IsAddNewRow = false;
                (sender as DataGrid).CommitEdit();
            }
            
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

        private void Button_ClearWatchTime_Click(object sender, RoutedEventArgs e)
        {
            controller.ClearWatchTime();
        }

        private void Button_ClearCurrencyAccrlValues_Click(object sender, RoutedEventArgs e)
        {
            controller.ClearAllCurrenciesValues();
        }

        #endregion

        #region Helpers

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

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void CheckFocus()
        {
            if (TB_Twitch_Channel.Text.Length != 0 && TB_Twitch_BotUser.Text.Length != 0 && TB_Twitch_ClientID.Text.Length != 0 && TB_Twitch_AccessToken.Text.Length != 0 && OptionFlags.CurrentToTwitchRefreshDate() >= new TimeSpan(0,0,0))
            {
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_FollowBotStart.IsEnabled = true;
                Radio_Twitch_LiveBotStart.IsEnabled = true;
                Radio_Twitch_ClipBotStart.IsEnabled = true;
            } else
            {
                Radio_Twitch_StartBot.IsEnabled = false;
                Radio_Twitch_FollowBotStart.IsEnabled = false;
                Radio_Twitch_LiveBotStart.IsEnabled = false;
                Radio_Twitch_ClipBotStart.IsEnabled = false;

            }
        }

        private void SetMultiLiveActive(bool ProcessFound = false)
        {
            Label_LiveStream_MultiLiveActiveMsg.Visibility = ProcessFound ? Visibility.Visible : Visibility.Collapsed;
            SetMultiLiveButtons();
        }

        private void SetMultiLiveButtons()
        {
            if (IsMultiProcActive == false)
            {
                controller.MultiConnect();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = Radio_Twitch_LiveBotStart.IsChecked ?? false;
                Radio_Twitch_LiveBotStop.IsEnabled = false; // can't stop the live bot service while monitoring multiple channels
            }
            else if (IsMultiProcActive == true)
            {
                MultiBotRadio();
                controller.MultiDisconnect();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
            }
        }

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
            ProcWatch watch = SetMultiLiveActive;
            _ = Application.Current.Dispatcher.BeginInvoke(watch, IsActive);
        }

        private void ProcessWatcher()
        {
            const int sleep = 5000;

            while (WatchProcessOps)
            {
                Process[] processes = Process.GetProcessesByName(MultiLiveName);
                if ((processes.Length > 0) != IsMultiProcActive) // only change IsMultiProcActive when the process activity changes
                {
                    UpdateProc(processes.Length > 0);
                    IsMultiProcActive = processes.Length > 0;
                }

                if (OptionFlags.CurrentToTwitchRefreshDate() <= new TimeSpan(0, 5, sleep / 1000))
                {
                    NotifyExpiredCredentials?.Invoke(this, new());
                }

                Thread.Sleep(sleep);
            }
        }

        #endregion

        #region MultiLive
        private void Radio_Twitch_LiveBotStart_Checked(object sender, RoutedEventArgs e)
        {
            Radio_MultiLiveTwitch_StartBot.IsEnabled = IsMultiProcActive == false && ((sender as RadioButton).IsChecked ?? false);
        }

        private void Radio_Twitch_LiveBotStop_Checked(object sender, RoutedEventArgs e)
        {
            MultiBotRadio();
        }

        private void BC_MultiLiveTwitch_StartBot(object sender, MouseButtonEventArgs e)
        {
            MultiBotRadio(true);
        }

        private void BC_MultiLiveTwitch_StopBot(object sender, MouseButtonEventArgs e)
        {
            MultiBotRadio();
        }

        private void MultiBotRadio(bool Start = false)
        {
            if (controller != null && controller.TwitchLiveMonitor.IsMultiConnected)
            {
                if (Start && Radio_MultiLiveTwitch_StartBot.IsEnabled && Radio_MultiLiveTwitch_StartBot.IsChecked != true)
                {
                    controller.StartMultiLive();
                    Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
                    Radio_MultiLiveTwitch_StartBot.IsChecked = true;
                    Radio_MultiLiveTwitch_StopBot.IsEnabled = true;

                    DG_Multi_LiveStreamStats.ItemsSource = null;
                    DG_Multi_LiveStreamStats.Visibility = Visibility.Collapsed;

                    Panel_BotActivity.Visibility = Visibility.Visible;
                }
                else
                {
                    controller.StopMultiLive();
                    Radio_MultiLiveTwitch_StartBot.IsEnabled = true;
                    Radio_MultiLiveTwitch_StopBot.IsEnabled = false;
                    Radio_MultiLiveTwitch_StopBot.IsChecked = true;

                    if (IsMultiProcActive == true)
                    {
                        DG_Multi_LiveStreamStats.ItemsSource = controller.MultiLiveDataManager.LiveStream;
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

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void DG_ChannelNames_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            controller.UpdateChannels();
            IsAddNewRow = false;
        }





        #endregion

        #region Debug Empty Stream

        private DateTime DebugStreamStarted = DateTime.MinValue;

        private void StartDebugStream_Click(object sender, RoutedEventArgs e)
        {
            if (DebugStreamStarted == DateTime.MinValue)
            {
                DebugStreamStarted = DateTime.Now.ToLocalTime();

                string User = TwitchBots.TwitchChannelName;
                string Category = "All";
                string Title = "Testing a debug stream";

                controller.HandleOnStreamOnline(User, Title, DebugStreamStarted, Category, true);
            }

        }

        private void EndDebugStream_Click(object sender, RoutedEventArgs e)
        {
            if(DebugStreamStarted != DateTime.MinValue)
            {
                controller.HandleOnStreamOffline();

                DebugStreamStarted = DateTime.MinValue;
            }

        }

        #endregion




    }
}
