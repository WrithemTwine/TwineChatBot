using StreamerBot.Web;

using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Diagnostics;
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
        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }
        private async void TextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }
        private async void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }

        private async void Button_TwitchAuthToken_ReAuthorize(object sender, RoutedEventArgs e)
        {
            BotController.ForceTwitchAuthReauthorization();
            GUIStopBots_Click(this, new());
            await TwitchCheckFocusAsync();
        }

        private async void ToggleButton_ChooseTwitchAuth_Click(object sender, RoutedEventArgs e)
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

            await TwitchCheckFocusAsync();
        }

        private void Button_TwitchAuthCode_ApproveBotURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_Button_AuthorizeStreamer.IsEnabled = false;

            BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthBotClientId, false, TwitchAuth_PopupURLAuth,
                async () => { await AuthenticatedAttemptToStartBotsAsync(); });
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_BotAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void Button_TwitchAuthCode_NoScopes_ApproveURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.IsEnabled = false;

            BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthBotClientId, true, TwitchAuth_PopupURLAuth,
                async () => { await AuthenticatedAttemptToStartBotsAsync(); });
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_NoScopesAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void Button_TwitchAuthCode_ApproveStreamerURL(object sender, RoutedEventArgs e)
        {
            Twitch_AuthCode_Button_AuthorizeBot.IsEnabled = false;

            BotController.TwitchTokenAuthCodeAuthorize(OptionFlags.TwitchAuthStreamerClientId, false, TwitchAuth_PopupURLAuth,
                async () => { await AuthenticatedAttemptToStartBotsAsync(); });
            Dispatcher.BeginInvoke(() =>
            {
                StatusBar_TwitchAuth_StreamerAuthCodeInvalid.Visibility = Visibility.Collapsed;
            });
        }

        private void TwitchAuth_PopupURLAuth(string URL)
        {
            Task.Run(async () =>
            {
                if (URL == LocalizedMsgSystem.GetVar(Msg.MsgTwitchAuthFailedAuthentication))
                {
                    MessageBox.Show($"Twitch Authentication Code", LocalizedMsgSystem.GetVar(Msg.MsgTwitchAuthFailedAuthentication));
                    await TwitchCheckFocusAsync();
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
                        LogWriter.LogException(Ex, "TwitchAuth_PopupURLAuth");
                    }
                }
            });
        }

        /// <summary>
        /// Callback method to call when the authentication is completed, and attempt to start any bots the user 
        /// selected to start when the app starts.
        /// </summary>
        private async Task AuthenticatedAttemptToStartBotsAsync()
        {
            await TwitchCheckFocusAsync();
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

                TwitchCheckFocusAsync();
            });
        }

        private async Task TwitchCheckFocusAsync()
        {
            await Task.Run(TwitchCheckFocus);
        }

        /// <summary>
        /// Check the conditions for starting the bot, where the data fields require data before the bot can be successfully started.
        /// </summary>
        private void TwitchCheckFocus()
        {
            _ = Dispatcher.BeginInvoke(async () =>
            {
                /*
                evaluate Twitch credentials based on string groups
                user access tokens or auth code tokens

                generate collections of credentials per choice area, apply !string.IsNullOrEmpty to each string, and test if all are true
                */

                // User token data

                // bot account data

                bool UserBotTokenData = (from BUT in (ICollection<string>)[
                                                        OptionFlags.TwitchBotUserName,
                                                    OptionFlags.TwitchBotClientId,
                                                    OptionFlags.TwitchBotAccessToken,
                                                    OptionFlags.TwitchChannelName
                                                      ]
                                         select !string.IsNullOrEmpty(BUT)).All((bt) => bt == true);

                bool UserStreamerTokenData = (!OptionFlags.TwitchStreamerUseToken && UserBotTokenData
                                             && !string.IsNullOrEmpty(OptionFlags.TwitchStreamerNoScopesAccessToken)) ||
                                             (from SUT in (ICollection<string>)[
                                                 OptionFlags.TwitchChannelName,
                                             OptionFlags.TwitchStreamerClientId,
                                             OptionFlags.TwitchStreamerAccessToken,
                                             OptionFlags.TwitchStreamerNoScopesAccessToken
                                                 ]
                                              select !string.IsNullOrEmpty(SUT)).All((st) => st == true);
                // Auth token data

                bool AuthBotTokenData = (from BAT in (ICollection<string>)[
                                                        OptionFlags.TwitchBotUserName,
                                                    OptionFlags.TwitchAuthBotClientId,
                                                    OptionFlags.TwitchAuthBotClientSecret,
                                                    OptionFlags.TwitchChannelName
                                                      ]
                                         select !string.IsNullOrEmpty(BAT)).All((bt) => bt == true);

                bool AuthStreamerTokenData = (!OptionFlags.TwitchStreamerUseToken && UserBotTokenData) ||
                                            (from SUT in (ICollection<string>)[
                                            OptionFlags.TwitchChannelName,
                                        OptionFlags.TwitchAuthStreamerClientId,
                                        OptionFlags.TwitchAuthStreamerClientSecret
                                            ]
                                             select !string.IsNullOrEmpty(SUT)).All((st) => st == true);

                // Check if credentials are empty and we still need to allow the user to authenticate the application, but block it when successfully authenticated
                // The authentication code bot checking clears out the auth code when there's a failure, so this checks it's enabled when
                // both the user adds client Id & secret are available and auth code is not available (not authenticated)
                Twitch_AuthCode_Button_AuthorizeBot.IsEnabled = AuthBotTokenData && OptionFlags.TwitchAuthBotAuthCode == "";
                Twitch_AuthCode_Button_AuthorizeStreamer.IsEnabled = OptionFlags.TwitchStreamerUseToken && AuthStreamerTokenData && OptionFlags.TwitchAuthStreamerAuthCode == "";
                Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.IsEnabled = OptionFlags.TwitchAuthStreamerNoScopesAuthCode == "";

                // Twitch

                if (OptionFlags.TwitchStreamerUseToken)
                {
                    GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Visible;
                    TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Visible;
                    TextBox_TwitchScopesOauthSame.Visibility = Visibility.Collapsed;
                    Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Visible;
                    Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Visible;
                    Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Collapsed;

                    Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Visible;

                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.Visibility = Visibility.Visible;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeBot.Visibility = Visibility.Collapsed;

                    SP_Twitch_UserToken_NoScopes_Bot.Visibility = Visibility.Collapsed;
                    SP_Twitch_UserToken_NoScopes_Streamer.Visibility = Visibility.Visible;

                    GroupBox_Twitch_StartBots_BotEventSub.Visibility = Visibility.Visible;
                }
                else
                {
                    GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Collapsed;
                    TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Collapsed;
                    TextBox_TwitchScopesOauthSame.Visibility = Visibility.Visible;
                    Help_TwitchBot_DiffAuthScopes_Bot.Visibility = Visibility.Collapsed;
                    Help_TwitchBot_DiffAuthScopes_Streamer.Visibility = Visibility.Collapsed;
                    Help_TwitchBot_SameAuthScopes.Visibility = Visibility.Visible;

                    Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Collapsed;

                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeBot.Visibility = Visibility.Visible;

                    SP_Twitch_UserToken_NoScopes_Bot.Visibility = Visibility.Visible;
                    SP_Twitch_UserToken_NoScopes_Streamer.Visibility = Visibility.Collapsed;

                    GroupBox_Twitch_StartBots_BotEventSub.Visibility = Visibility.Collapsed;
                }

                // set earliest token expiration date

                List<DateTime> RefreshTokenDateExpiry = (from R in (ICollection<DateTime>)[OptionFlags.TwitchBotTokenDate, OptionFlags.TwitchStreamerTokenDate]
                                                         where OptionFlags.CurrentToTwitchRefreshDate(R) > new TimeSpan(0, 5, 2)
                                                         select R).ToList();
                StatusBarItem_TokenDate.Content = OptionFlags.TwitchTokenUseAuth ?
                    "Auth Code Refresh" :
                    RefreshTokenDateExpiry.Count != 0 ?
                    RefreshTokenDateExpiry?.Min().ToShortDateString() :
                    "None Valid";

                if (

                    // OptionFlags.CurrentToTwitchRefreshDate(OptionFlags.TwitchBotTokenDate) <= new TimeSpan(0, 5, sleep / 1000)

                    // use User set tokens, check on using the streamer token
                    (!OptionFlags.TwitchTokenUseAuth && UserBotTokenData && UserStreamerTokenData && RefreshTokenDateExpiry.Count == (OptionFlags.TwitchStreamerUseToken ? 2 : 1))
                    ||
                    // use Auth code tokens, check on using the streamer token
                    (OptionFlags.TwitchTokenUseAuth &&
                        (AuthBotTokenData && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode)) &&
                        (AuthStreamerTokenData && (!OptionFlags.TwitchStreamerUseToken || !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAuthCode)))
                    )

                    )
                {
                    await BotController.TwitchInitializeHelix();
                }
                else
                {
                    SetBotRadioButtons(false, Platform.Twitch);
                    BotController.NotifyInvalidTwitchTokens();
                }
            });
        }

        private void SetBotRadioButtons(bool value, Platform platform)
        {
            foreach (RadioButton rb in
                                        from A in BotOps
                                        where A.Item2 == Platform.Service || A.Item2 == platform
                                        select A.Item3
                                        )
            {
                rb.IsEnabled = value;
            }
        }

        private void Controller_TokensInitializedAsync(object sender, EventArgs e)
        {
            ThreadManager.AddTaskToGUIDispatcher(new Task(async () =>
            {
                SetBotRadioButtons(true, Platform.Twitch); // event handler from Twitch bots, at this point the tokens are authorized
                await StartAutoBots();
            }));
        }
    }
}
