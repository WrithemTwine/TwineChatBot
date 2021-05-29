using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Data;
using ChatBot_Net5.Models;
using ChatBot_Net5.Properties;

using System;
using System.Collections.Generic;
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
    public partial class BotWindow : Window
    {
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

            CP = new();
            CP.Page_ChatPopup_FlowDocViewer.Document = FlowDoc_ChatBox.Document;
            CP.Page_ChatPopup_FlowDocViewer.Opacity = Slider_PopOut_Opacity.Value;


            new Thread(new ThreadStart(ProcessWatcher)).Start();

            controller = Resources["ControlBot"] as BotController;
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
                if (tuple.Item1 && tuple.Item2.IsEnabled && tuple.Item2 != Radio_MultiLiveTwitch_StartBot)
                {
                    HelperStartBot(tuple.Item2);
                }
                else if (tuple.Item1 && tuple.Item2.IsEnabled)
                {
                    SetMultiLiveButtons();
                    MultiBotRadio(true);
                }
            }
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WatchProcessOps = false;
            controller.ExitSave();
            Settings.Default.Save();
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

        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            CP.Visibility = Visibility.Visible;
            CP.Height = 500;
            CP.Width = 300;
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => Twitch_RefreshDate.Content = DateTime.Now.AddDays(60);

        private void TextBox_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e) => (sender as TextBox).ScrollToEnd();

        private void RadioButton_StartBot_PreviewMoustLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RadioButton rb = (sender as RadioButton);

            HelperStartBot(rb);
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
            RadioButton rb = (sender as RadioButton);

            if (rb.IsEnabled)
            {
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
            static Visibility SetVisibility(bool Check) { return (Check ? Visibility.Visible : Visibility.Collapsed); }

            TabItem_Users.Visibility = SetVisibility(OptionFlags.ManageUsers);
            TabItem_Followers.Visibility = SetVisibility(OptionFlags.ManageFollowers);
            TabItem_StreamStats.Visibility = SetVisibility(OptionFlags.ManageStreamStats);

            controller.ManageDatabase();
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Disable or Enable UIElements to prevent user changes while bot is active.
        /// Same method takes the opposite value for the start then stop then start, i.e. toggling start/stop bot operations.
        /// </summary>
        private void ToggleInputEnabled(bool setvalue=true)
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
                Radio_Twitch_FollowBotStart.IsEnabled = true;
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
            if (IsMultiProcActive==false)
            {
                controller.MultiConnect();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = Radio_Twitch_LiveBotStart.IsChecked ?? false;
            }
            else if(IsMultiProcActive==true)
            {
                MultiBotRadio();
                controller.MultiDisconnect();
                Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
                Radio_Twitch_LiveBotStop.IsEnabled = false;
            }
        }

        #endregion

        #region WatcherTools
        
        private bool WatchProcessOps;
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
                if ((processes.Length > 0) != IsMultiProcActive)
                {
                    UpdateProc(processes.Length > 0);
                    IsMultiProcActive = (processes.Length > 0);
                }

                Thread.Sleep(5000);
            }
        }

        #endregion

        #region MultiLive
        private void Radio_Twitch_LiveBotStart_Checked(object sender, RoutedEventArgs e) => Radio_MultiLiveTwitch_StartBot.IsEnabled = IsMultiProcActive == false && ((sender as RadioButton).IsChecked ?? false);
        private void Radio_Twitch_LiveBotStop_Checked(object sender, RoutedEventArgs e) => MultiBotRadio();
        private void BC_MultiLiveTwitch_StartBot(object sender, MouseButtonEventArgs e) => MultiBotRadio(true);
        private void BC_MultiLiveTwitch_StopBot(object sender, MouseButtonEventArgs e) => MultiBotRadio();

        private void MultiBotRadio(bool Start = false)
        {
            if (controller != null && controller.TwitchLiveMonitor.IsMultiConnected)
            {
                if (Start && Radio_MultiLiveTwitch_StartBot.IsEnabled == true && Radio_MultiLiveTwitch_StartBot.IsChecked != true)
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

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e) => (sender as TextBox).ScrollToEnd();
        private void DG_ChannelNames_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) => controller.UpdateChannels();

        #endregion

    }
}
