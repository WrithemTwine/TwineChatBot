using StreamerBot.Twitch;
using StreamerBot.Web;

using StreamerBotLib.BotIOController;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
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
            LogWriter.DebugLog("TextBox_TextChanged", DebugLogTypes.GUIBotComs, "Twitch Auth TextBox changed.");
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }
        private async void TextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            LogWriter.DebugLog("TextBox_TextChanged", DebugLogTypes.GUIBotComs, "Twitch Auth TextBox changed.");
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }
        private async void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("TextBox_TextChanged", DebugLogTypes.GUIBotComs, "Twitch Auth TextBox changed.");
            if (Twitch_AuthCode_Button_AuthorizeBot != null)
            {
                await TwitchCheckFocusAsync();
            }
        }

        private async void Button_TwitchAuthToken_ReAuthorize(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("Button_TwitchAuthToken_ReAuthorize", DebugLogTypes.GUIBotComs, "Twitch Auth Re-Authorization requested.");
            GUIStopBots_Click(this, new());
            if (((Button)sender) == Button_TwitchAuthCode_All_ForceReAuthorization)
            {
                BotController.ForceTwitchAuthReauthorization();
            }
            else if ((Button)sender == Twitch_AuthCode_Button_AuthorizeBot_Reauthorize)
            {
                BotController.ForceTwitchAuthReauthorization(Bots.TwitchEventSubBot);
            }
            else if ((Button)sender == Twitch_AuthCode_Button_AuthorizeStreamer_Reauthorize)
            {
                BotController.ForceTwitchAuthReauthorization(Bots.TwitchStreamerEventSubScopes);
            }
            else if ((Button)sender == Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer_NoScopes_Reauthorize || ((Button)sender) == Twitch_AuthCode_Button_NoScopesBot_Reauthorize)
            {
                BotController.ForceTwitchAuthReauthorization(Bots.TwitchStreamerEventSubNoScopes);
            }

            await TwitchCheckFocusAsync();
        }

        private async void ToggleButton_ChooseTwitchAuth_Click(object sender, RoutedEventArgs e)
        {
            LogWriter.DebugLog("ToggleButton_ChooseTwitchAuth_Click", DebugLogTypes.GUIBotComs, $"Twitch Auth Code flow chosen {OptionFlags.TwitchTokenUseAuth}.");
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
                LogWriter.DebugLog("TwitchAuth_PopupURLAuth", DebugLogTypes.GUIBotComs, $"Twitch Auth URL: {URL}");

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
            LogWriter.DebugLog("AuthenticatedAttemptToStartBotsAsync", DebugLogTypes.GUIBotComs, "Twitch Auth completed, attempting to start bots.");
            await TwitchCheckFocusAsync();
        }

        /// <summary>
        /// Handles when the authorization access failed and notifies the user to address the access error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Controller_InvalidAuthorizationToken(object sender, InvalidAccessTokenEventArgs e)
        {
            Dispatcher.BeginInvoke(async () =>
            {
                LogWriter.DebugLog("Controller_InvalidAuthorizationToken", DebugLogTypes.GUIBotComs, $"Twitch Auth invalid token for {e.BotType}, notifying user.");
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

                await TwitchCheckFocusAsync();
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

                SetBotValidAccessLabels();

                (Frame_Bots_Twitch_ManualTokenHelp?.Content as ManualTokenHelp)?.SetScopes();

                if (OptionFlags.TwitchStreamerUseToken)
                {
                    GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Visible;
                    TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Visible;
                    TextBox_TwitchScopesOauthSame.Visibility = Visibility.Collapsed;

                    Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Visible;

                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.Visibility = Visibility.Visible;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer_NoScopes_Reauthorize.Visibility = Visibility.Visible;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeBot.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_Button_NoScopesBot_Reauthorize.Visibility = Visibility.Collapsed;

                    SP_Twitch_UserToken_NoScopes_Bot.Visibility = Visibility.Collapsed;
                    SP_Twitch_UserToken_NoScopes_Streamer.Visibility = Visibility.Visible;

                    GroupBox_Twitch_StartBots_EventSubStreamer.Visibility = Visibility.Visible;
                }
                else
                {
                    GroupBox_Twitch_AdditionalStreamerCredentials.Visibility = Visibility.Collapsed;
                    TextBox_TwitchScopesDiffOauthBot.Visibility = Visibility.Collapsed;
                    TextBox_TwitchScopesOauthSame.Visibility = Visibility.Visible;

                    Twitch_AuthCode_GroupBox_StreamerInfo.Visibility = Visibility.Collapsed;

                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeStreamer_NoScopes_Reauthorize.Visibility = Visibility.Collapsed;
                    Twitch_AuthCode_NoScopes_Button_AuthorizeBot.Visibility = Visibility.Visible;
                    Twitch_AuthCode_Button_NoScopesBot_Reauthorize.Visibility = Visibility.Visible;

                    SP_Twitch_UserToken_NoScopes_Bot.Visibility = Visibility.Visible;
                    SP_Twitch_UserToken_NoScopes_Streamer.Visibility = Visibility.Collapsed;

                    GroupBox_Twitch_StartBots_EventSubStreamer.Visibility = Visibility.Collapsed;
                }
                // set earliest token expiration date

                List<DateTime> RefreshTokenDateExpiry = [.. (from R in (ICollection<DateTime>)[OptionFlags.TwitchBotTokenDate, OptionFlags.TwitchStreamerTokenDate]
                                                         where OptionFlags.CurrentToTwitchRefreshDate(R) > new TimeSpan(0, 5, 2)
                                                         select R)];
                StatusBarItem_TokenDate.Content = OptionFlags.TwitchTokenUseAuth ?
                    "Auth Code Refresh" :
                    RefreshTokenDateExpiry.Count != 0 ?
                    RefreshTokenDateExpiry?.Min().ToShortDateString() :
                    "None Valid";

                if (
                    !OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchChannelName))
                    && !OptionFlags.CheckSettingIsDefault(nameof(OptionFlags.TwitchBotUserName))
                    && (
                            // use User set tokens, check on using the streamer token
                            (!OptionFlags.TwitchTokenUseAuth && UserBotTokenData && UserStreamerTokenData && RefreshTokenDateExpiry.Count == (OptionFlags.TwitchStreamerUseToken ? 2 : 1))
                        ||
                            // use Auth code tokens, check on using the streamer token
                            (OptionFlags.TwitchTokenUseAuth &&
                                (AuthBotTokenData && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode)) &&
                                (AuthStreamerTokenData && (!OptionFlags.TwitchStreamerUseToken
                                                            || (!string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAuthCode)
                                                                && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerNoScopesAuthCode)
                                                           )))
                            )
                        )
                    )
                {
                    await BotController.TwitchInitializeHelix();
                    SetBotRadioButtons(true, Platform.Twitch); // event handler from Twitch bots, at this point the tokens are authorized
                }
                else
                {
                    SetBotRadioButtons(false, Platform.Twitch);
                    BotController.NotifyInvalidTwitchTokens();
                }
            });
        }

        private void SetBotValidAccessLabels()
        {
            if (OptionFlags.TwitchStreamerUseToken)
            {
                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchBotAccessToken)))
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Collapsed;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Visible;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Visible;
                    }
                }

                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchStreamerAccessToken)))
                {
                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Visible;
                }

                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerNoScopesAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchStreamerNoScopesAccessToken)))
                {
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Invalid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubNotify_AccessToken_NoScopes_Invalid.Visibility = Visibility.Visible;
                }


                if (TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility == Visibility.Visible
                    && TextBlock_EventSubNotify_AccessToken_NoScopes_Valid.Visibility == Visibility.Visible)
                { // if both accesses are valid, then hide the invalid info
                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                if ((OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchAuthBotAuthCode) && !string.IsNullOrEmpty(OptionFlags.TwitchAuthStreamerNoScopesAuthCode))
                    || (!OptionFlags.TwitchTokenUseAuth && !string.IsNullOrEmpty(OptionFlags.TwitchBotAccessToken)))
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Collapsed;

                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Visible;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Collapsed;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Hidden;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Hidden;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Hidden;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    TextBlock_EventSubChat_AccessToken_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubChat_AccessToken_Invalid.Visibility = Visibility.Visible;

                    TextBlock_EventSubNotify_AccessToken_Scopes_Valid.Visibility = Visibility.Collapsed;
                    TextBlock_EventSubNotify_AccessToken_Scopes_Invalid.Visibility = Visibility.Visible;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlock_EventSubChat_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubChat_ManualToken_Info.Visibility = Visibility.Visible;
                        TextBlock_EventSubNotify_AuthCode_Info.Visibility = Visibility.Collapsed;
                        TextBlock_EventSubNotify_ManualToken_Info.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SetBotRadioButtons(bool value, Platform platform)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (RadioButton rb in
                                            from A in BotOps
                                            where A.Item2 == Platform.Service || A.Item2 == platform
                                            select A.Item3
                                            )
                {
                    rb.IsEnabled = value;
                }
            });
        }

        private void Controller_TokensInitializedAsync(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => Task.Run(() =>
            {
                SetBotRadioButtons(true, Platform.Twitch); // event handler from Twitch bots, at this point the tokens are authorized
                StartAutoBots();
            }));
        }
    }
}
