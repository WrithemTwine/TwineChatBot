using StreamerBot.BotClients;
using StreamerBot.Static;
using StreamerBot.BotIOController;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StreamerBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StreamerBotWindow : Window
    {
        public static BotController Controller { get; private set; } = new();
        

        public StreamerBotWindow()
        {
            InitializeComponent();
        }

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFocus();
            OptionFlags.SetSettings();
        }

        private void CheckBox_Click_SaveSettings(object sender, RoutedEventArgs e)
        {
            OptionFlags.SetSettings();
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void CheckFocus()
        {
            if (TB_Twitch_Channel.Text.Length != 0 && TB_Twitch_BotUser.Text.Length != 0 && TB_Twitch_ClientID.Text.Length != 0 && TB_Twitch_AccessToken.Text.Length != 0 && OptionFlags.CurrentToTwitchRefreshDate() >= new TimeSpan(0, 0, 0))
            {
                //Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_FollowBotStart.IsEnabled = true;
                //Radio_Twitch_LiveBotStart.IsEnabled = true;
                //Radio_Twitch_ClipBotStart.IsEnabled = true;
            }
            else
            {
                //Radio_Twitch_StartBot.IsEnabled = false;
                Radio_Twitch_FollowBotStart.IsEnabled = false;
                //Radio_Twitch_LiveBotStart.IsEnabled = false;
                //Radio_Twitch_ClipBotStart.IsEnabled = false;

            }
        }

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

        private delegate void BotOperation();

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

        private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e)
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

        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0 && SliderMouseCaptured)
            {
                e.Handled = true;
                return;
            }
        }

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

        #region MultiLive
        //private void Radio_Twitch_LiveBotStart_Checked(object sender, RoutedEventArgs e)
        //{
        //    Radio_MultiLiveTwitch_StartBot.IsEnabled = IsMultiProcActive == false && ((sender as RadioButton).IsChecked ?? false);
        //}

        //private void Radio_Twitch_LiveBotStop_Checked(object sender, RoutedEventArgs e)
        //{
        //    MultiBotRadio();
        //}

        //private void BC_MultiLiveTwitch_StartBot(object sender, MouseButtonEventArgs e)
        //{
        //    MultiBotRadio(true);
        //}

        //private void BC_MultiLiveTwitch_StopBot(object sender, MouseButtonEventArgs e)
        //{
        //    MultiBotRadio();
        //}

        //private void MultiBotRadio(bool Start = false)
        //{
        //    if (controller != null && controller.TwitchLiveMonitor.IsMultiConnected)
        //    {
        //        if (Start && Radio_MultiLiveTwitch_StartBot.IsEnabled && Radio_MultiLiveTwitch_StartBot.IsChecked != true)
        //        {
        //            controller.StartMultiLive();
        //            Radio_MultiLiveTwitch_StartBot.IsEnabled = false;
        //            Radio_MultiLiveTwitch_StartBot.IsChecked = true;
        //            Radio_MultiLiveTwitch_StopBot.IsEnabled = true;

        //            DG_Multi_LiveStreamStats.ItemsSource = null;
        //            DG_Multi_LiveStreamStats.Visibility = Visibility.Collapsed;

        //            Panel_BotActivity.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            controller.StopMultiLive();
        //            Radio_MultiLiveTwitch_StartBot.IsEnabled = true;
        //            Radio_MultiLiveTwitch_StopBot.IsEnabled = false;
        //            Radio_MultiLiveTwitch_StopBot.IsChecked = true;

        //            if (IsMultiProcActive == true)
        //            {
        //                DG_Multi_LiveStreamStats.ItemsSource = controller.MultiLiveDataManager.LiveStream;
        //            }
        //            DG_Multi_LiveStreamStats.Visibility = Visibility.Visible;

        //            Panel_BotActivity.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //}

        //private void MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    int start = TB_LiveMsg.SelectionStart;

        //    if (TB_LiveMsg.SelectionLength > 0)
        //    {
        //        TB_LiveMsg.Text = TB_LiveMsg.Text.Remove(start, TB_LiveMsg.SelectionLength);
        //    }

        //    TB_LiveMsg.Text = TB_LiveMsg.Text.Insert(start, (sender as MenuItem).Header.ToString());
        //    TB_LiveMsg.SelectionStart = start;
        //}

        //private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    (sender as TextBox).ScrollToEnd();
        //}

        //private void DG_ChannelNames_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        //{
        //    controller.UpdateChannels();
        //    IsAddNewRow = false;
        //}

        #endregion
    }
}
