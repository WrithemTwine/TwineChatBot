using StreamerBot.Web;

using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace StreamerBot
{
    /// <summary>
    /// Handles functionality for the Twitch Auth Code flow
    /// </summary>
    public partial class StreamerBotWindow
    {
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
            StartAutoBots(); // start chosen auto-start bots if credentials are proper
        }

        private void Button_TwitchAuthCode_ApproveBotURL(object sender, RoutedEventArgs e)
        {
            Controller.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthClientId, TwitchAuth_PopupURLAuth, AuthenticatedAttempToStartBots);
        }

        private void Button_TwitchAuthCode_ApproveStreamerURL(object sender, RoutedEventArgs e)
        {
            Controller.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthStreamerClientId, TwitchAuth_PopupURLAuth, AuthenticatedAttempToStartBots);
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
                            startBrowser.Start();
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
                MessageBox.Show($"Invalid Token - {e.BotType}", $"The authorization for {e.Platform} has failed. The bot(s) have stopped due to no access. Please re-authorize the application access.");
            });
        }
    }
}
