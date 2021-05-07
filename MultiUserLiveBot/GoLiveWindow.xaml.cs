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

namespace MultiUserLiveBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GoLiveWindow : Window
    {
        private bool IsBotEnabled; // prevent bot from starting twice

        public GoLiveWindow()
        {
            InitializeComponent();

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
            }
        }

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


        internal void CheckFocus()
        {
            if (TB_Twitch_BotUser.Text.Length != 0
                && TB_Twitch_ClientID.Text.Length != 0
                && TB_Twitch_AccessToken.Text.Length != 0)
            {
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_StopBot.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Twitch_RefreshDate.Content = DateTime.Now.AddDays(60);
        private void TextBox_SourceUpdated(object sender, DataTransferEventArgs e) => CheckFocus();
        private void Window_Loaded(object sender, RoutedEventArgs e) => CheckFocus();

        private void Settings_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
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

        private void BC_Twitch_StartBot(object sender, MouseButtonEventArgs e)
        {
            if (!IsBotEnabled)
            {
                TwitchLiveBot io = (sender as RadioButton).DataContext as TwitchLiveBot;
                io.StartBot();
                ToggleInputEnabled();
                IsBotEnabled = !IsBotEnabled;
                Radio_Twitch_StartBot.IsEnabled = false;
                Radio_Twitch_StopBot.IsEnabled = true;
            }
        }

        private async void PreviewMoustLeftButton_SelectAll(object sender, MouseButtonEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void BC_Twitch_StopBot(object sender, MouseButtonEventArgs e)
        {
            if (IsBotEnabled)
            {
                TwitchLiveBot io = (sender as RadioButton).DataContext as TwitchLiveBot;
                io.StopBot();
                ToggleInputEnabled();
                IsBotEnabled = !IsBotEnabled;
                Radio_Twitch_StartBot.IsEnabled = true;
                Radio_Twitch_StopBot.IsEnabled = false;
            }
        }

        private void FollowBot_OnLiveNotification(object sender, LiveAlertArgs e)
        {


            //string msg = (string)DataManage.GetRowData(DataRetrieve.EventMessage, ChannelEventActions.Live);
            string msg = "@everyone, #user is now live streaming #category - #title! &lt;br/&gt; Come join and say hi at: #url";

            Dictionary<string, string> dictionary = new()
            {
                { "#user", e.ChannelStream.UserName },
                { "#category", e.ChannelStream.GameName },
                { "#title", e.ChannelStream.Title },
                { "#url", "https://www.twitch.tv/" + e.ChannelStream.UserName }
            };

            foreach (Uri u in (Resources["DataManager"] as DataManager).Discord)
            {
                DiscordWebhook.SendLiveMessage(u, ParseReplace(msg, dictionary)).Wait();
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataManager data = Resources["DataManager"] as DataManager;
            data.SaveData();
        }
    }
}
