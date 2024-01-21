using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.Api.Auth;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;


namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchTokenBot : TwitchBotsBase
    {
        private readonly ExtAuth AuthBot;

        private bool TokenRenewalStarted;
        private bool AbortRenewToken;

        private readonly string TokenLock = "lock";

        /// <summary>
        /// The upcoming expiration date of the token. This token is for accessing Twitch through the bot account.
        /// </summary>
        private DateTime BotAccessTokenExpireDate;
        /// <summary>
        /// The Upcoming expiration date of the token. When the user uses the bot account for chatting, but needs streamer token access to access streamer level information.
        /// </summary>
        private DateTime StreamerAccessTokenExpireDate;
        /// <summary>
        /// The date & time the token bot checked the bot account access token for validity.
        /// </summary>
        private DateTime BotAccessTokenLastCheckedDate;
        /// <summary>
        /// The date & time the token bot checked the streamer account access token for validity.
        /// </summary>
        private DateTime StreamerAccessTokenLastCheckedDate;

        private const int MaxInterval = 60 * 60;  // 60s/min * 60min/hr
        private const int TokenCheckTimeWindow = 20; // seconds within which code won't check the access token for validity, saves many requests within a very short timeframe

        internal event EventHandler BotAccessTokenChanged;
        internal event EventHandler BotAccessTokenUnChanged;
        internal event EventHandler StreamerAccessTokenChanged;
        internal event EventHandler StreamerAccessTokenUnChanged;

        internal event EventHandler<TwitchAuthCodeExpiredEventArgs> BotAcctAuthCodeExpired;
        internal event EventHandler<TwitchAuthCodeExpiredEventArgs> StreamerAcctAuthCodeExpired;

        public TwitchTokenBot()
        {
            AbortRenewToken = false;

            ApiSettings apiSettings = new();
            AuthBot = new(apiSettings, new BypassLimiter(), new TwitchHttpClient(null));

            Task checkToken = new(() =>
            {
                CheckToken();
            });

            checkToken.Start();
        }

        public override bool StartBot()
        {
            if (!IsStarted || IsStopped)
            {
                IsStarted = true;
                IsStopped = false;
                AbortRenewToken = false;
                StartRenewToken();
            }
            return true;
        }

        public override bool StopBot()
        {
            if (IsStarted)
            {
                IsStarted = false;
                IsStopped = true;
                AbortRenewToken = true;
            }
            return true;
        }

        private void StartRenewToken()
        {
            if (!TokenRenewalStarted)
            {
                TokenRenewalStarted = true;
                ThreadManager.CreateThreadStart(RenewToken);
            }
        }

        /// <summary>
        /// Calls <see cref="CheckToken"/> to validate or refresh access tokens; and picks a random time 
        /// to sleep before calling <see cref="CheckToken"/> again.
        /// </summary>
        private void RenewToken()
        {
            Random IntervalRandom = new();

            while (!AbortRenewToken && OptionFlags.ActiveToken)
            {
                _ = CheckToken();

                DateTime wakeup = DateTime.Now.AddSeconds(IntervalRandom.Next(MaxInterval / 2, MaxInterval));

                while (DateTime.Now < wakeup && OptionFlags.ActiveToken && !AbortRenewToken)
                {
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// Thread blocks to check access tokens are valid, and returns token active status.
        /// Invokes the Changed and Unchanged events for both the Bot token and Streamer token; to help
        /// with notifications and to restart applicable bot activity.
        /// </summary>
        /// <returns>true if either Token changed, false if either Token is unchanged.</returns>
        internal bool CheckToken()
        {
            bool result = false;

            try
            {
                if (OptionFlags.TwitchTokenUseAuth)
                {
                    StartBot(); // ensure bot is started
                }
                else
                {
                    StopBot();
                }

                if (IsStarted) // only calculate if bot is started, meaning the User is using this operation mode.
                {
                    lock (TokenLock)
                    {
                        if ((DateTime.Now - BotAccessTokenLastCheckedDate).TotalSeconds > TokenCheckTimeWindow)
                        { // only check if we haven't checked in the last 1 second - avoid lots of checks in a single second
                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"Now checking Bot token.");
                            // try to refresh Bot token
                            Tuple<string, string, int, bool> Botresponse = ProcessToken(
                                OptionFlags.TwitchAuthBotAuthCode,
                                OptionFlags.TwitchAuthClientId,
                                OptionFlags.TwitchAuthBotClientSecret,
                                OptionFlags.TwitchAuthBotRefreshToken,
                                OptionFlags.TwitchAuthBotAccessToken);

                            result = Botresponse.Item4;
                            BotAccessTokenLastCheckedDate = DateTime.Now;

                            // with a good response, set the token data
                            if (Botresponse.Item1 != "" && Botresponse.Item2 != "" && Botresponse.Item3 != 0)
                            {
                                TwitchAccessToken = Botresponse.Item1;
                                TwitchRefreshToken = Botresponse.Item2;
                                BotAccessTokenExpireDate = DateTime.Now.AddSeconds(Botresponse.Item3);
                                result = true;

                                BotAccessTokenChanged?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                BotAccessTokenUnChanged?.Invoke(this, EventArgs.Empty);
                            }
                        }

                        if (OptionFlags.TwitchStreamerUseToken)
                        {
                            if ((DateTime.Now - StreamerAccessTokenLastCheckedDate).TotalSeconds > TokenCheckTimeWindow)
                            { // only check if we haven't checked in the last 1 second - avoid lots of checks in a single second
                                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"Now checking Streamer token.");
                                // try to refresh streamer token
                                Tuple<string, string, int, bool> Streamerresponse = ProcessToken(
                                    OptionFlags.TwitchAuthStreamerAuthCode,
                                    OptionFlags.TwitchAuthStreamerClientId,
                                    OptionFlags.TwitchAuthStreamerClientSecret,
                                    OptionFlags.TwitchAuthStreamerRefreshToken,
                                    OptionFlags.TwitchAuthStreamerAccessToken);

                                result = Streamerresponse.Item4;
                                StreamerAccessTokenLastCheckedDate = DateTime.Now;

                                // with a good response, set the token data
                                if (Streamerresponse.Item1 != "" && Streamerresponse.Item2 != "" && Streamerresponse.Item3 != 0)
                                {
                                    TwitchStreamerAccessToken = Streamerresponse.Item1;
                                    TwitchStreamerRefreshToken = Streamerresponse.Item2;
                                    StreamerAccessTokenExpireDate = DateTime.Now.AddSeconds(Streamerresponse.Item3);
                                    result = true;

                                    StreamerAccessTokenChanged?.Invoke(this, EventArgs.Empty);
                                }
                                else
                                {
                                    StreamerAccessTokenUnChanged?.Invoke(this, EventArgs.Empty);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
            return result;
        }

        /// <summary>
        /// First determines if we have a client ID and client secret, both necessary to authorize the application
        /// If we don't have the auth code, tell the user to authenticate the application
        /// if we do have the auth code but no access & refresh tokens, attempt to get the access & refresh tokens
        /// if the access token is invalid and we do have a refresh token, attempt to refresh the access token
        /// if refreshing the access token fails, we clear the authentication data and tell the user to reauthenticate
        /// 
        /// Refresh tokens become invalid when the user changes the account password or disconnects the app through the Twitch website user settings
        /// </summary>
        /// <param name="authcode">The recent auth code used to receive a refresh token.</param>
        /// <param name="clientId">The client id for the token.</param>
        /// <param name="clientsecret">The client secret for the client id.</param>
        /// <param name="refreshtoken">The refresh token received in prior auth code flow request.</param>
        /// <param name="accesstoken">The access token received from the prior auth code flow request - validated before proceeding to refresh the token.</param>
        /// <returns>If token is still valid, returns Tuple {"","",0}; otherwise, Tuple {access token, refresh token, expires in}</returns>
        private Tuple<string, string, int, bool> ProcessToken(string authcode, string clientId, string clientsecret, string refreshtoken, string accesstoken)
        {
            string AccessToken = "";
            string RefreshToken = "";
            int ExpiresIn = 0;

            ValidateAccessTokenResponse validToken = accesstoken != null ? AuthBot.ValidateAccessTokenAsync(accesstoken).Result : null;

            // only proceed if the clientId & client secret is valid
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientsecret))
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"clientId and clientsecret are not null.");

                // if we have no auth code, tell the GUI user to authorize the app - since we have client ID & secret
                if (string.IsNullOrEmpty(authcode))
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"auth code is null or empty.");

                    // notify GUI if user needs to reauthenticate
                    if (clientId == OptionFlags.TwitchAuthClientId)
                    {
                        BotAcctAuthCodeExpired?.Invoke(this, new() { BotType = BotType.BotAccount });
                    }
                    else if (OptionFlags.TwitchStreamerUseToken)
                    {
                        StreamerAcctAuthCodeExpired?.Invoke(this, new() { BotType = BotType.StreamerAccount });
                    }
                }
                else // we have an authcode, try to get the first refresh token & access token
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"auth code is available.");

                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"testing access token, it is {(validToken == null ? "invalid" : "valid")}");

                    // clientId_!null, clientsecret_!null, authcode_!null, refreshtoken_null-try to get one
                    if (string.IsNullOrEmpty(refreshtoken))
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"refresh token is null or empty.");

                        try
                        {
                            AuthCodeResponse BotAuthRefresh = AuthBot.GetAccessTokenFromCodeAsync(authcode, clientsecret, OptionFlags.TwitchAuthRedirectURL, clientId).Result;
                            // successful API call, we get a refresh token and access token
                            RefreshToken = BotAuthRefresh.RefreshToken;
                            AccessToken = BotAuthRefresh.AccessToken;
                            ExpiresIn = BotAuthRefresh.ExpiresIn;

                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"Twitch provided updated access & refresh tokens.");

                        }
                        catch (BadRequestException AuthCodEx)
                        {
                            LogWriter.LogException(AuthCodEx, MethodBase.GetCurrentMethod().Name);
                            // notify GUI if user needs to reauthenticate, failed auth code
                            if (clientId == OptionFlags.TwitchAuthClientId)
                            {
                                BotAcctAuthCodeExpired?.Invoke(this, null);
                            }
                            else
                            {
                                StreamerAcctAuthCodeExpired?.Invoke(this, null);
                            }
                        }
                    }
                    else if (validToken == null) // if we already have a refresh token, try to refresh the access token; failure means user needs to reauthorize app
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"Access token is no longer valid, refreshing the access token.");

                        // if we're here, we had a badrequesttokenexception 

                        //if (string.IsNullOrEmpty(accesstoken) || validToken == null)
                        //{ // only try to refresh the access token if it's empty or invalid, otherwise, don't need to do anything
                        try
                        {
                            // get the new access/refresh tokens
                            RefreshResponse BotTokenResponse = AuthBot.RefreshAuthTokenAsync(refreshtoken, clientsecret, clientId).Result;

                            RefreshToken = BotTokenResponse.RefreshToken;
                            AccessToken = BotTokenResponse.AccessToken;
                            ExpiresIn = BotTokenResponse.ExpiresIn;

                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"Twitch gave us a new fresh access token.");
                        }
                        catch (BadRequestException ReqEx)
                        {  // the refresh token has failed, clear it all out and tell the user to reauthorize the app
                            LogWriter.LogException(ReqEx, MethodBase.GetCurrentMethod().Name);

                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, $"Twitch rejected the refresh token. User must reauthorize the application.");

                            // if we're here, the tokens are expired and need to start over with the authorization process
                            if (clientId == OptionFlags.TwitchBotClientId) // determine if the client Id is the bot client
                            {
                                OptionFlags.TwitchAuthBotRefreshToken = "";
                                OptionFlags.TwitchAuthBotAccessToken = "";
                                OptionFlags.TwitchAuthBotAuthCode = "";
                                BotAcctAuthCodeExpired?.Invoke(this, null);
                            }
                            else
                            {
                                OptionFlags.TwitchAuthStreamerAccessToken = "";
                                OptionFlags.TwitchAuthStreamerAuthCode = "";
                                OptionFlags.TwitchAuthStreamerRefreshToken = "";
                                StreamerAcctAuthCodeExpired?.Invoke(this, null);
                            }
                        }
                        //}
                    }
                }
            }

            return new(AccessToken, RefreshToken, ExpiresIn, validToken != null);
        }

        /// <summary>
        /// Generate an approval URL to point the user to a Twitch link to authorize account access
        /// </summary>
        /// <param name="clientId">The client Id registered to the account used in the app (could be bot account or streamer account).</param>
        /// <param name="OpenBrowser">An action code bit to open the browser at the end of this code flow.</param>
        /// <param name="AuthenticationFinished">An action to perform once the authentication is finished-specifically a GUI action.</param>
        internal void GenerateAuthCodeURL(string clientId, Action<string> OpenBrowser = null, Action AuthenticationFinished = null)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Request to generate an auth code URL.");

            string seedvalue = (clientId + DateTime.Now.ToLongDateString()).Replace(" ", "");
            string[] splitseed = seedvalue.Split(['a', 'b', 'e', 'j', 'm', 'w', 'g']);
            Random random = new();

            string buildstring = "";
            // reorder the substring segments
            for (int i = 0; i < splitseed.Length; i++)
            {
                buildstring += splitseed[random.Next(splitseed.Length)];
            }

            string State = buildstring[..Math.Min(buildstring.Length, 30)];

            // invoke event to get the user involved with authorizing the application
            if (clientId == OptionFlags.TwitchAuthClientId) // determine if the client Id is the bot client
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Generating bot account authorization URL.");

                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    OptionFlags.TwitchStreamerUseToken ? // check if the bot account is the streamer account, determines scopes to request
                    Resources.CredentialsTwitchScopesDiffOauthBot.Split(' ') :
                    Resources.CredentialsTwitchScopesOauthSame.Split(' '),
                    state: State,
                    clientId: clientId);

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "URL generated.");

                BotAcctAuthCodeExpired?.Invoke(this, new(buildURL, State, OpenBrowser, AuthenticationFinished));
            }
            else
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Generating streamer account authorization URL.");

                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    Resources.CredentialsTwitchScopesDiffOauthChannel.Split(' '),
                    state: State,
                    clientId: clientId
                    );
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "URL generated.");

                StreamerAcctAuthCodeExpired?.Invoke(this, new(buildURL, State, OpenBrowser, AuthenticationFinished));
            }
        }

        /// <summary>
        /// Clears the authentication codes for the bot & streamer accounts.
        /// </summary>
        internal void ForceReauthorization()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchTokenBot, "Request to reauthorize the application. Clearing the authentication code(s).");

            OptionFlags.TwitchAuthBotAuthCode = "";
            OptionFlags.TwitchAuthStreamerAuthCode = "";
        }
    }
}
