using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Models;
using ChatBot_Net5.Properties;

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ChatBot_Net5
{
    /// <summary>
    /// Interaction logic for BotWindow.xaml
    /// </summary>
    public partial class BotWindow : Window
    {
        private int SelectedDataTabIndex;

        private readonly ChatPopup CP;

        public BotWindow()
        {
            // move settings to the newest version, if the application version upgrades
            if (Settings.Default.UpgradeRequired )
            { 
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            InitializeComponent();

            CP = new ChatPopup
            {
                Page_ChatPopup_RichText = RichTextBox_ChatBox
            };
            CP.Page_ChatPopup_RichText.Opacity = Slider_PopOut_Opacity.Value;            
        }

        /// <summary>
        /// Handles add a new item to a Datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DG_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void Button_PreviewMouseLeftButtonDown_DGJoinList(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (Resources["ControlBot"] as BotController).ExitSave();
            Settings.Default.Save();
        }

        private void BC_Twitch_StartBot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // ignore the following block if using debug build
#if !DEBUG
            TabItem_Users.Visibility = Visibility.Collapsed;
            TabItem_Followers.Visibility = Visibility.Collapsed;
            SelectedDataTabIndex = TabControl_DataTabs.SelectedIndex;
            if (SelectedDataTabIndex < 2) TabControl_DataTabs.SelectedIndex = 2;
#endif

            BotController io = (sender as RadioButton).DataContext as BotController;
            io.StartBot();
            ToggleInputEnabled();
        }

        /// <summary>
        /// Disable or Enable UIElements to prevent user changes while bot is active.
        /// Same method takes the opposite value for the start then stop then start, i.e. toggling start/stop bot operations.
        /// </summary>
        private void ToggleInputEnabled()
        {
            TB_Twitch_AccessToken.IsEnabled = !TB_Twitch_AccessToken.IsEnabled;
            TB_Twitch_BotUser.IsEnabled = !TB_Twitch_BotUser.IsEnabled;
            TB_Twitch_Channel.IsEnabled = !TB_Twitch_Channel.IsEnabled;
            TB_Twitch_ClientID.IsEnabled = !TB_Twitch_ClientID.IsEnabled;
            Btn_Twitch_RefreshDate.IsEnabled = !Btn_Twitch_RefreshDate.IsEnabled;
            Slider_TimePollSeconds.IsEnabled = !Slider_TimePollSeconds.IsEnabled;
        }

        private void BC_Twitch_StopBot(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BotController io = (sender as RadioButton).DataContext as BotController;
            io.StopBot();
            ToggleInputEnabled();

            // ignore the following block if using debug build
#if !DEBUG
            TabItem_Users.Visibility = Visibility.Visible;
            TabItem_Followers.Visibility = Visibility.Visible;
            TabControl_DataTabs.SelectedIndex = SelectedDataTabIndex;
#endif
        }

        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            CP.Visibility = Visibility.Visible;
            CP.Height = 500;
            CP.Width = 300;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Twitch_RefreshDate.Content = DateTime.Now.AddDays(60);

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFocus();
            Settings.Default.Save();
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        internal void CheckFocus()
        {
            if (TB_Twitch_Channel.Text.Length != 0 && TB_Twitch_BotUser.Text.Length != 0 && TB_Twitch_ClientID.Text.Length != 0 && TB_Twitch_AccessToken.Text.Length != 0)
            {
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_StopBot.IsEnabled = true;
            }
        }

        private void TextBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) => CheckFocus();

        private void DG_CommonMsgs_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGrid dg = (sender as DataGrid);

            switch (dg.Name)
            {
                case "DG_Users":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() != "Id" && dc.Header.ToString() != "UserName" && dc.Header.ToString() != "FirstDateSeen" && dc.Header.ToString() != "CurrLoginDate" && dc.Header.ToString() != "LastDateSeen" && dc.Header.ToString() != "WatchTime")
                        {
                            dc.Visibility = Visibility.Collapsed ;
                        }
                    }
                    break;
                case "DG_Followers":
                    foreach (DataGridColumn dc in dg.Columns)
                    {
                        if (dc.Header.ToString() != "Id" && dc.Header.ToString() != "UserName" && dc.Header.ToString() != "IsFollower" && dc.Header.ToString() != "FollowedDate" )
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
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => CheckFocus();

        private void RichTextBox_ChatBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox r = (sender as RichTextBox);
            r.Document = (r.DataContext as FlowDocument);
        }

        private void TextBlock_TwitchBotLog_TextChanged(object sender, TextChangedEventArgs e) => (sender as TextBox).ScrollToEnd();

        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }
    }
}
