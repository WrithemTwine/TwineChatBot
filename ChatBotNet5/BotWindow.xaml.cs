using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Data;
using ChatBot_Net5.Models;
using ChatBot_Net5.Properties;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for BotWindow.xaml
    /// </summary>
    public partial class BotWindow : Window, INotifyPropertyChanged
    {
        // TODO: Add color themes
        private readonly ChatPopup CP;
        private const string MultiLiveName = "MultiUserLiveBot";

        private readonly BotController controller;

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

            InitializeComponent();

            OptionFlags.SetSettings();
            controller = Resources["ControlBot"] as BotController;

            // TODO: debug ChatPopup
            //CP = new();
            //CP.Closing += CP_Closing;
            //CP.DataContext = this;
            //CP.Page_ChatPopup_FlowDocViewer.SetBinding(System.Windows.Controls.Primitives.DocumentViewerBase.DocumentProperty, new Binding("Document") { Source = FlowDoc_ChatBox, Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //CP.SetBinding(OpacityProperty, new Binding("Opacity") { Source = Slider_PopOut_Opacity, Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            new Thread(new ThreadStart(ProcessWatcher)).Start();
        }



        #region Events
        #region Windows & Tab Ops
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckFocus();

            List<Tuple<bool, RadioButton>> BotOps = new()
            {
                new(Settings.Default.TwitchChatBotAutoStart, Radio_Twitch_StartBot),
                new(Settings.Default.TwitchFollowerSvcAutoStart, Radio_Twitch_FollowBotStart),
                new(Settings.Default.TwitchLiveStreamSvcAutoStart, Radio_Twitch_LiveBotStart),
                new(Settings.Default.TwitchMultiLiveAutoStart, Radio_MultiLiveTwitch_StartBot)
            };

            foreach (Tuple<bool, RadioButton> tuple in BotOps)
            {
                if (tuple.Item1 && tuple.Item2.IsEnabled)
                {
                    if (tuple.Item2 != Radio_MultiLiveTwitch_StartBot)
                    {
                        HelperStartBot(tuple.Item2);
                    }
                    else
                    {
                        SetMultiLiveButtons();
                        MultiBotRadio(true);
                    }
                }
            }

            // TODO: add follower service online, offline, and repeat timers to re-run service
            // TODO: turn off bots & prevent starting if the token is expired - research auto-refreshing token
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
            if ((DateTime.Parse(Twitch_RefreshDate.Content.ToString()) - DateTime.Now) <= new TimeSpan(14, 0, 0, 0))
            {
                Twitch_RefreshDate.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                Twitch_RefreshDate.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        #endregion

        #region PopOut Chat Window
        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            CP.Show();
            CP.Height = 500;
            CP.Width = 300;
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
            if (!IsAppClosing) // flag to really close the window
            {
                e.Cancel = true;
                CP.Hide();
            }
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

        private void JoinCollectionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ((BotController)LV_JoinList.DataContext).JoinCollection.Remove((sender as CheckBox).DataContext as UserJoin);
        }

        private void TextBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) => CheckFocus();

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => Twitch_RefreshDate.Content = DateTime.Now.AddDays(50);

        private void TextBox_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e) => (sender as TextBox).ScrollToEnd();

        private void RadioButton_StartBot_PreviewMoustLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HelperStartBot(sender as RadioButton);

            if (Radio_Twitch_StartBot.IsChecked == true && Radio_Twitch_FollowBotStart.IsEnabled == false)
            {
                Radio_Twitch_FollowBotStart.IsEnabled = true;
            }
        }

        private void HelperStartBot(RadioButton rb)
        {
            if (rb.IsEnabled)
            {
                rb.IsChecked = true;
                (rb.DataContext as Clients.IOModule)?.StartBot();
                ToggleInputEnabled(false);

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
        }

        private void RadioButton_StopBot_PreviewMoustLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.IsEnabled)
            {
                if (Radio_Twitch_StopBot == sender && Radio_Twitch_FollowBotStart.IsChecked == true)
                {
                    Radio_Twitch_FollowBotStart.IsEnabled = true;
                    RadioButton_StopBot_PreviewMoustLeftButtonDown(Radio_Twitch_FollowBotStop, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
                }

                rb.IsChecked = true;
                (rb.DataContext as Clients.IOModule)?.StopBot();
                ToggleInputEnabled(true);

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
                        if (dc.Header.ToString() != "Id" && dc.Header.ToString() != "UserName" && dc.Header.ToString() != "FirstDateSeen" && dc.Header.ToString() != "CurrLoginDate" && dc.Header.ToString() != "LastDateSeen" && dc.Header.ToString() != "WatchTime")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case "DG_Followers":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() != "Id" && dc.Header.ToString() != "UserName" && dc.Header.ToString() != "IsFollower" && dc.Header.ToString() != "FollowedDate")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case "DG_Currency":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() != "Id" && dc.Header.ToString() != "CurrencyName" && dc.Header.ToString() != "AccrueRate")
                        {
                            dc.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case "DG_CurrencyAccrual":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() != "Id" && dc.Header.ToString() != "User Name" && dc.Header.ToString() != "Currency Name" && dc.Header.ToString() != "Value")
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
            }
        }

        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void CheckBox_ManageData_Click(object sender, RoutedEventArgs e)
        {
            static Visibility SetVisibility(bool Check) { return Check ? Visibility.Visible : Visibility.Collapsed; }

            TabItem_Users.Visibility = SetVisibility(OptionFlags.ManageUsers);
            TabItem_Followers.Visibility = SetVisibility(OptionFlags.ManageFollowers);
            TabItem_StreamStats.Visibility = SetVisibility(OptionFlags.ManageStreamStats);

            if (CheckBox_ManageUsers.IsChecked == true)
            {
                CheckBox_ManageFollowers.IsEnabled = true;
            } else
            {
                CheckBox_ManageFollowers.IsEnabled = false;
                CheckBox_ManageFollowers.IsChecked = false; // requires the Manage Users to be enabled
            }

            controller.ManageDatabase();
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
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void CheckFocus()
        {
            if (TB_Twitch_Channel.Text.Length != 0 && TB_Twitch_BotUser.Text.Length != 0 && TB_Twitch_ClientID.Text.Length != 0 && TB_Twitch_AccessToken.Text.Length != 0)
            {
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_FollowBotStart.IsEnabled = Radio_Twitch_StartBot.IsChecked == true;
                Radio_Twitch_LiveBotStart.IsEnabled = true;
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
        /// True - "MultiUserLiveBot.exe" is active, False - "MultiUserLiveBot.exe" is not active
        /// </summary>
        private bool? IsMultiProcActive;

        private delegate void ProcWatch(bool IsActive);

        private void UpdateProc(bool IsActive)
        {
            ProcWatch watch = SetMultiLiveActive;
            Application.Current.Dispatcher.BeginInvoke(watch, IsActive);
        }

        private void ProcessWatcher()
        {
            while (WatchProcessOps)
            {
                Process[] processes = Process.GetProcessesByName(MultiLiveName);
                if ((processes.Length > 0) != IsMultiProcActive) // only change IsMultiProcActive when the process activity changes
                {
                    UpdateProc(processes.Length > 0);
                    IsMultiProcActive = processes.Length > 0;
                }

                Thread.Sleep(5000);
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
        }

        #endregion

    }
}
