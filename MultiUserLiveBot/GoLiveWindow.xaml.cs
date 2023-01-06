using MultiUserLiveBot.Properties;

using StreamerBotLib.BotClients.Twitch;
using StreamerBotLib.MultiLive;
using StreamerBotLib.Static;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiUserLiveBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GoLiveWindow : Window
    {
        private bool IsBotEnabled = false; // prevent bot from starting twice

        private readonly TwitchBotLiveMonitorSvc TwitchLiveBot;

        public GoLiveWindow()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
            }

            InitializeComponent();

            TwitchLiveBot = Resources["TwitchLiveBot"] as TwitchBotLiveMonitorSvc;
        }

        #region GUI events and helpers

        private void TabItem_Twitch_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Parse(Twitch_RefreshDate.Content.ToString()) - DateTime.Now.ToLocalTime()) <= new TimeSpan(14, 0, 0, 0))
            {
                Twitch_RefreshDate.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                Twitch_RefreshDate.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        /// <summary>
        /// Verify certain text fields contain data for the bot login to permit the radio button to be enabled for user to click start
        /// </summary>
        public void CheckFocus()
        {
            if (TB_Twitch_BotUser.Text.Length != 0
                && TB_Twitch_ClientID.Text.Length != 0
                && TB_Twitch_AccessToken.Text.Length != 0)
            {
                Radio_Twitch_StartBot.IsEnabled = true;
            }
        }

        /// <summary>
        /// Disable or Enable UIElements to prevent user changes while bot is active.
        /// Same method takes the opposite value for the start then stop then start, i.e. toggling start/stop bot operations.
        /// </summary>
        private void ToggleInputEnabled()
        {
            TB_Twitch_AccessToken.IsEnabled = !TB_Twitch_AccessToken.IsEnabled;
            TB_Twitch_BotUser.IsEnabled = !TB_Twitch_BotUser.IsEnabled;
            TB_Twitch_ClientID.IsEnabled = !TB_Twitch_ClientID.IsEnabled;
            Btn_Twitch_RefreshDate.IsEnabled = !Btn_Twitch_RefreshDate.IsEnabled;
            Slider_TimeGoLivePollSeconds.IsEnabled = !Slider_TimeGoLivePollSeconds.IsEnabled;
        }

        // event handlers from the GUI/UIElements

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            CheckFocus();
        }

        private void BC_Twitch_StartStopBot(object sender, MouseButtonEventArgs e)
        {
            StartStopBot();
        }

        private void StartStopBot()
        {
            if (!IsBotEnabled)
            {
                if (TwitchLiveBot.StartBot())
                {
                    ToggleInputEnabled();
                    IsBotEnabled = true;
                    Radio_Twitch_StartBot.IsEnabled = false;
                    Radio_Twitch_StartBot.IsChecked = true;
                    Radio_Twitch_StopBot.IsEnabled = true;

                    TwitchLiveBot.StartMultiLive();
                }
            }
            else if (IsBotEnabled)
            {
                TwitchLiveBot.StopBot();
                ToggleInputEnabled();
                IsBotEnabled = false;
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_StopBot.IsChecked = true;
                Radio_Twitch_StopBot.IsEnabled = false;

                TwitchLiveBot.StopMultiLive();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckFocus();

            if (Settings.Default.TwitchLiveStreamSvcAutoStart && Radio_Twitch_StartBot.IsEnabled)
            {
                StartStopBot();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TwitchLiveBot.MultiLiveDataManager.SaveData();
            TwitchLiveBot.MultiDisconnect();
            TwitchLiveBot.StopBot();

            Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Twitch_RefreshDate.Content = DateTime.Now.ToLocalTime().AddDays(60);
        }

        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(((TextBox)sender).SelectAll);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        #endregion GUI events and helpers

        private void MultiLive_Data_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            (MultiLive_Data.Content as MultiLiveDataGrids).SetIsEnabled(true);

            (MultiLive_Data.Content as MultiLiveDataGrids).SetHandlers(Settings_LostFocus, TB_BotActivityLog_TextChanged);
            (MultiLive_Data.Content as MultiLiveDataGrids).SetLiveManagerBot(TwitchLiveBot.MultiLiveDataManager);

            ThreadManager.CreateThreadStart(() =>
            {
                if (!TwitchLiveBot.IsMultiConnected)
                {
                    TwitchLiveBot.MultiConnect();
                }
            });
        }
    }
}
