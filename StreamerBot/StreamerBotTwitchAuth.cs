
using StreamerBot.Web;

using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamerBot
{
    /// <summary>
    /// Handles functionality for the Twitch Auth Code flow
    /// </summary>
    public partial class StreamerBotWindow
    {
        /// <summary>
        /// Once user sets the Twitch Channel Name and Bot account name, check and get the user Ids for the Streamer/Bot account(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TwitchChannelBotNames_TargetUpdated(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(OptionFlags.TwitchChannelName) && !string.IsNullOrEmpty(OptionFlags.TwitchBotUserName))
            {
                Controller.CheckTwitchChannelBotIds();
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                CheckFocus();
            }
        }
        private void TextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                CheckFocus();
            }
        }

        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                CheckFocus();
            }
        }

        private void Button_TwitchAuthToken_ReAuthorize(object sender, RoutedEventArgs e)
        {
            Controller.ForceTwitchAuthReauthorization();
            CheckFocus();
        }

        private void ToggleButton_ChooseTwitchAuth_Click(object sender, RoutedEventArgs e)
        {
            GUIStopBots_Click(this, new()); // stop the bots to use the new tokens

            if (OptionFlags.TwitchTokenUseAuth)
            {
                StackPanel_TwitchTokenFlow.Visibility = Visibility.Collapsed;
                StackPanel_TwitchAuthCodeFlow.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_TwitchTokenFlow.Visibility = Visibility.Visible;
                StackPanel_TwitchAuthCodeFlow.Visibility = Visibility.Collapsed;
            }

            CheckFocus();
        }

        private void Button_TwitchAuthCode_ApproveBotURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_Button_AuthorizeStreamer.IsEnabled = false;

            StreamerBotLib.BotIOController.BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthClientId, TwitchAuth_PopupURLAuth, AuthenticatedAttempToStartBots);
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_BotAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void Button_TwitchAuthCode_ApproveStreamerURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_Button_AuthorizeBot.IsEnabled = false;

            StreamerBotLib.BotIOController.BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthStreamerClientId, TwitchAuth_PopupURLAuth, AuthenticatedAttempToStartBots);
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_StreamerAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void TwitchAuth_PopupURLAuth(string URL)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (URL == LocalizedMsgSystem.GetVar(Msg.MsgTwitchAuthFailedAuthentication))
                {
                    MessageBox.Show($"Twitch Authentication Code", LocalizedMsgSystem.GetVar(Msg.MsgTwitchAuthFailedAuthentication));
                    CheckFocus();
                }
                else
                {
                    try
                    {
                        // use the user's choice - internal app browser
                        if (OptionFlags.TwitchAuthUseInternalBrowser)
                        {
                            AuthWebBrowser authWebBrowser = new();
                            authWebBrowser.Show();
                            authWebBrowser.NavigateToURL(URL);
                        }
                        else
                        { // use User's default browser
                            Process startBrowser = new();
                            startBrowser.StartInfo.UseShellExecute = true;
                            startBrowser.StartInfo.FileName = $"\"{URL}\"";
                            _ = startBrowser.Start();
                        }
                    }
                    catch (Exception Ex)
                    {
                        LogWriter.LogException(Ex, MethodBase.GetCurrentMethod().Name);
                    }
                }
            });
        }

        /// <summary>
        /// Callback method to call when the authentication is completed, and attempt to start any bots the user 
        /// selected to start when the app starts.
        /// </summary>
        private void AuthenticatedAttempToStartBots()
        {
            Dispatcher.BeginInvoke(() =>
            {
                CheckFocus();
                StartAutoBots();
            });
        }

        /// <summary>
        /// Handles when the authorization access failed and notifies the user to address the access error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Controller_InvalidAuthorizationToken(object sender, InvalidAccessTokenEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                switch (e.Platform)
                {
                    case Platform.Default:
                        break;
                    case Platform.Twitch:
                        switch (e.BotType)
                        {
                            case BotType.BotAccount:
                                StatusBar_TwitchAuth_BotAuthCodeInvalid.Visibility = Visibility.Visible;
                                break;
                            case BotType.StreamerAccount:
                                StatusBar_TwitchAuth_StreamerAuthCodeInvalid.Visibility = Visibility.Visible;
                                break;
                        }
                        break;
                }
            });
        }
    }
}
