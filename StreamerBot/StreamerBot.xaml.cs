using StreamerBot.BotClients;
using StreamerBot.Static;
using StreamerBot.BotIOController;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using StreamerBot.Properties;
using StreamerBot.Events;
using System.Windows.Media;
using StreamerBot.GUI;

namespace StreamerBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StreamerBotWindow : Window
    {
        public static BotController Controller { get; private set; } = new();

        private GUIBotBase guiTwitchBot;

        public StreamerBotWindow()
        {
            // move settings to the newest version, if the application version upgrades
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            InitializeComponent();

            guiTwitchBot = Resources["TwitchBot"] as GUIBotBase;

            guiTwitchBot.OnBotStopped += GUI_OnBotStopped;
            guiTwitchBot.OnBotStarted += GUI_OnBotStarted;
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

        #region Bot_Ops
        #region Controller Events

        private void GUI_OnBotStopped(object sender, BotStartStopEventArgs e)
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
            }), null);
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



   
    }
}
