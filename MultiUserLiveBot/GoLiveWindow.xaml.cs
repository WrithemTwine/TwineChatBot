using MultiUserLiveBot.Clients;
using MultiUserLiveBot.Data;
using MultiUserLiveBot.Properties;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private readonly TwitchLiveBot TwitchLiveBot;

        public GoLiveWindow()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
            }

            InitializeComponent();

            TwitchLiveBot = Resources["TwitchLiveBot"] as TwitchLiveBot;
        }

        #region static string helpers
        private const string codekey = "#";

        /// <summary>
        /// Replace in a message the keys from a dictionary for the matching values, must begin with the key token
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        internal static string ParseReplace(string message, Dictionary<string, string> dictionary)
        {
            string temp = ""; // build the message to return

            string[] words = message.Split(' ');    // tokenize the message by ' ' delimiters

            // submethod to replace the found key with paired value
            string Rep(string key)
            {
                string hit = "";

                foreach (string s in dictionary.Keys)
                {
                    if (key.Contains(s))
                    {
                        hit = s;
                        break;
                    }
                }

                dictionary.TryGetValue(hit, out string value);
                return key.Replace(hit, (hit == codekey + "user" ? "@" : "") + value) ?? "";
            }

            // review the incoming string message for all of the keys in the dictionary, replace with paired value
            for (int x = 0; x < words.Length; x++)
            {
                temp += (words[x].StartsWith(codekey, StringComparison.CurrentCulture) ? Rep(words[x]) : words[x]) + " ";
            }

            return temp.Trim();
        }

        /// <summary>
        /// Takes the incoming string integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic], 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        internal static string Plurality(string src, string single, string plural)
        {
            return Plurality(Convert.ToInt32(src, CultureInfo.CurrentCulture), single, plural);
        }

        /// <summary>
        /// Takes the incoming integer and determines plurality >1 and returns the appropriate word, e.g. 1 viewers [sic], 1 viewer.
        /// </summary>
        /// <param name="src">Representative number</param>
        /// <param name="single">Singular version of the word to return</param>
        /// <param name="plural">Plural version of the word to return</param>
        /// <returns>The source number and the version of word to match the plurality of src.</returns>
        internal static string Plurality(int src, string single, string plural)
        {
            return src.ToString(CultureInfo.CurrentCulture) + " " + (src != 1 ? plural : single);
        }
        #endregion static string helpers

        #region GUI events and helpers

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

        /// <summary>
        /// Verify certain text fields contain data for the bot login to permit the radio button to be enabled for user to click start
        /// </summary>
        internal void CheckFocus()
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

        private void BC_Twitch_StartStopBot(object sender, MouseButtonEventArgs e) => StartStopBot();

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

                    DG_LiveStreamStats.ItemsSource = null;
                    DG_LiveStreamStats.Visibility = Visibility.Collapsed;

                    Panel_BotActivity.Visibility = Visibility.Visible;
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

                DG_LiveStreamStats.ItemsSource = (DG_LiveStreamStats.DataContext as DataManager).LiveStream;
                DG_LiveStreamStats.Visibility = Visibility.Visible;

                Panel_BotActivity.Visibility = Visibility.Collapsed;
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
            TwitchLiveBot.DataManage.SaveData();
            TwitchLiveBot.StopBot();

            Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Twitch_RefreshDate.Content = DateTime.Now.AddDays(60);
        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e) => await Application.Current.Dispatcher.InvokeAsync(((TextBox)sender).SelectAll);
        private void CheckBox_Click(object sender, RoutedEventArgs e) => Settings.Default.Save();
        private void DG_ChannelNames_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit) { TwitchLiveBot.UpdateChannelList(); }
        }
        private void TB_BotActivityLog_TextChanged(object sender, TextChangedEventArgs e) => (sender as TextBox).ScrollToEnd();
        #endregion GUI events and helpers

        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            ((DataGrid)sender).CanUserAddRows = true;
        }

        private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            ((DataGrid)sender).CanUserAddRows = false;
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            ((DataGrid)sender).CanUserAddRows = true;
        }
    }
}
