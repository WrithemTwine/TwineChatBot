using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;

namespace StreamerBotLib.BotClients.Twitch
{
    /// <summary>
    /// Designed to handle UserProvided-Mode and AuthCode Mode tokens.
    /// UserProvided will expire in the future and the user must update the access tokens to use in the ApiSettings.
    /// AuthCode tokens will continue to refresh when access tokens expire, so long as the token scopes remain unchnaged.
    /// 
    /// Twitch automatically expires access tokens when the user name or password changes.
    /// </summary>
    internal class TwitchTokenBot : IOModule
    {
        private readonly ExtAuth AuthBot = new(
                        settings: new ApiSettings(),
                        rateLimiter: new BypassLimiter(),
                        http: new TwitchHttpClient(null));

        internal ApiSettings BotApiSettings { get; private set; }
        internal TwitchAPI BotHelixApi { get; private set; }
        internal ApiSettings StreamerApiSettings { get; private set; }
        internal TwitchAPI StreamerHelixApi { get; private set; }
        internal ApiSettings StreamerNoScopesApiSettings { get; private set; }
        internal TwitchAPI StreamerNoScopesHelixApi { get; private set; }

        /// <summary>
        /// Manages per bot is active to determine whether to update that token
        /// </summary>
        private Dictionary<BotType, bool> ActiveBotTokens { get; set; }


        private bool TokenRenewalStarted; // flag to use a single thread for checking AuthCode access tokens
        private bool InitializeTokens;

        private const string TokenLock = "lock";
        private const int MaxInterval = 60 * 60;  // 60s/min * 60min/hr
        private const int TokenCheckTimeWindow = 20; // seconds within which code won't check the access token for validity, saves many requests within a very short timeframe

        internal event EventHandler BotAccessTokenChanged;
        internal event EventHandler BotAccessTokenUnChanged;
        internal event EventHandler StreamerAccessTokenChanged;
        internal event EventHandler StreamerAccessTokenUnChanged;
        internal event EventHandler StreamerNoScopesAccessTokenChanged;
        internal event EventHandler StreamerNoScopesAccessTokenUnChanged;

        public event EventHandler AccessTokensInitialized;
        internal event EventHandler<TwitchAuthCodeExpiredEventArgs> BotAcctAuthCodeExpired;
        internal event EventHandler<TwitchAuthCodeExpiredEventArgs> StreamerAcctAuthCodeExpired;
        internal event EventHandler<TwitchAuthCodeExpiredEventArgs> StreamerNoScopesAuthCodeExpired;

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

        /// <summary>
        /// The Upcoming expiration date of the token. When the user uses the bot account for chatting, but needs streamer token access to access streamer level information.
        /// </summary>
        private DateTime StreamerNoScopesAccessTokenExpireDate;

        /// <summary>
        /// The date & time the token bot checked the streamer account access token for validity.
        /// </summary>
        private DateTime StreamerNoScopesAccessTokenLastCheckedDate;

        private Func<LiveUser, string> GetUserId;

        public TwitchTokenBot()
        {
            BotClientName = Bots.TwitchTokenBot;
            InitializeTokens = false;

            ActiveBotTokens = [];

            foreach (BotType b in Enum.GetValues<BotType>())
            {
                ActiveBotTokens.Add(b, false);
            }
        }

        public void InitializeGetUserId(Func<LiveUser, string> GetUserIdFunc)
        {
            GetUserId = GetUserIdFunc;
        }

        /// <summary>
        /// Starting the bot will build the ApiSettings with the user provided credentials as specified in the GUI.
        /// Supports "UserProvided-Mode" and "AuthCode-Mode" access tokens. The 'UserProvided' requires user updated
        /// access tokens, while the "AuthCode" mode uses the user authorized auth codes to then generate access tokens.
        /// </summary>
        /// <returns><code>true</code>-when token building is finished</returns>
        public override Task StartBot()
        {
            return Task.Run(() =>
            {
                if (IsActive is null or false)
                {
                    IsActive = true;

                    if (OptionFlags.TwitchTokenUseAuth)
                    {
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchTokenBot, "Building AuthCode mode tokens.");
                        BuildAuthTokens(); // the user activated authcode mode
                    }
                    else
                    {
                        LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchTokenBot, "Building UserProvided-Token mode tokens.");
                        BuildUserTokens(); // the user specifies the access tokens
                    }
                }
            });
        }

        /// <summary>
        /// Pause any token checking to permit changing token method: user-set vs auth code tokens
        /// </summary>
        /// <returns></returns>
        public override Task StopBot()
        {
            return Task.Run(() =>
            {
                if (IsActive == true)
                {
                    IsActive = false;
                    // at this point, clear the helix api data and 'start bot' will reset it, even if token mode changed
                    StreamerHelixApi = null;
                    BotHelixApi = null;
                    StreamerNoScopesHelixApi = null;
                }
            });
        }

        public void UpdateActiveTokens(BotType botType, bool active)
        {
            ActiveBotTokens[botType] = active;
        }

        private void SetTwitchApis()
        {
            if (BotHelixApi == null && StreamerHelixApi == null && BotApiSettings != null && StreamerApiSettings != null && StreamerNoScopesApiSettings != null)
            {
                BotHelixApi = new(settings: BotApiSettings);
                StreamerHelixApi = new(settings: StreamerApiSettings);
                StreamerNoScopesHelixApi = new(settings: StreamerNoScopesApiSettings);

                if (!InitializeTokens)
                {
                    SetIds();

                    InitializeTokens = true;
                    AccessTokensInitialized?.Invoke(this, new());
                }
            }
        }

        private void SetIds()
        {
            if (string.IsNullOrEmpty(OptionFlags.TwitchBotUserId))
            {
                LogWriter.DebugLog("SetIds", DebugLogTypes.TwitchTokenBot, "Setting bot user ID.");
                string botuserId = GetUserId(new(OptionFlags.TwitchBotUserName, Platform.Twitch));
                OptionFlags.TwitchBotUserId = botuserId != default ? botuserId : StreamerHelixApi.Helix.Users.GetUsersAsync(logins: [OptionFlags.TwitchBotUserName]).Result.Users[0].Id;
            }

            if (string.IsNullOrEmpty(OptionFlags.TwitchStreamerUserId))
            {
                LogWriter.DebugLog("SetIds", DebugLogTypes.TwitchTokenBot, "Setting streamer user ID.");
                string streameruserId = GetUserId(new(OptionFlags.TwitchChannelName, Platform.Twitch));
                OptionFlags.TwitchStreamerUserId =
                    OptionFlags.TwitchChannelName == OptionFlags.TwitchBotUserName ?
                        OptionFlags.TwitchBotUserId
                        : streameruserId != default ? streameruserId : StreamerHelixApi.Helix.Users.GetUsersAsync(logins: [OptionFlags.TwitchChannelName]).Result.Users[0].Id;
            }
        }

        /// <summary>
        /// UserProvided-Mode tokens, builds the ApiSettings using the clientId and access tokens depending on 
        /// if the bot account and streamer account are the same or separate.
        /// </summary>
        private void BuildUserTokens()
        {
            BotApiSettings = new()
            {
                ClientId = OptionFlags.TwitchBotClientId,
                AccessToken = OptionFlags.TwitchBotAccessToken
            };

            StreamerApiSettings = OptionFlags.TwitchStreamerUseToken
                ? new()
                {
                    ClientId = OptionFlags.TwitchStreamerClientId,
                    AccessToken = OptionFlags.TwitchStreamerAccessToken
                } : BotApiSettings;

            StreamerNoScopesApiSettings = new()
            {
                ClientId = OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchStreamerClientId : OptionFlags.TwitchBotClientId,
                AccessToken = OptionFlags.TwitchStreamerNoScopesAccessToken
            };

            SetTwitchApis();

            SetIds();

            BotAccessTokenChanged?.Invoke(this, new());
            StreamerAccessTokenChanged?.Invoke(this, new());
            StreamerNoScopesAccessTokenChanged?.Invoke(this, new());

            LogWriter.DebugLog("BuildUserTokens", DebugLogTypes.TwitchTokenBot, "Finished building UserProvided-Token mode api settings.");
        }

        /// <summary>
        /// Builds "AuthCode-Mode" tokens. Only succeeds when user provided client Ids, client secrets, and 
        /// auth codes are available. This method chain uses the auth codes to obtain access & refresh tokens or 
        /// use existing refresh tokens to generate access tokens; for the bot & streamer accounts (the user may
        /// specify the bot account to be same as the streamer account, or both accounts are different).
        /// </summary>
        private void BuildAuthTokens()
        {
            CheckToken(true); // force check all tokens, this will build the ApiSettings if successful
            StartRenewToken();
        }

        private void StartRenewToken()
        {
            if (IsActive == true && !TokenRenewalStarted)
            {
                TokenRenewalStarted = true;
                ThreadManager.CreateThreadStart("StartRenewToken", RenewToken);
            }
        }

        /// <summary>
        /// Calls <see cref="CheckToken"/> to validate or refresh access tokens; and picks a random time 
        /// to sleep before calling <see cref="CheckToken"/> again.
        /// </summary>
        private void RenewToken()
        {
            Random IntervalRandom = new();

            while (IsActive == true && OptionFlags.ActiveToken)
            // continue renewing tokens while token bot is started and app is active
            {
                LogWriter.DebugLog("RenewToken", DebugLogTypes.TwitchTokenBot, "Checking tokens.");
                CheckToken();

                DateTime wakeup = DateTime.Now.AddSeconds(IntervalRandom.Next(MaxInterval / 2, MaxInterval));

                DateTime Current = DateTime.Now;

                // check every couple seconds until it's time to check a token sometime every hour or a token is expiring
                while ((Current < wakeup
                        || BotAccessTokenExpireDate > Current
                        || StreamerAccessTokenExpireDate > Current
                        || StreamerNoScopesAccessTokenExpireDate > Current)
                        && OptionFlags.ActiveToken
                        && IsActive == true)
                {
                    Thread.Sleep(2000);
                    Current = DateTime.Now;  // refresh current time for loop check
                }
            }
        }

        /// <summary>
        /// check access tokens are valid.
        /// Invokes the Changed and Unchanged events for both the Bot token and Streamer token; to help
        /// with notifications and to restart applicable bot activity.
        /// </summary>
        /// <param name="Override">Set to true to check all tokens</param>
        /// <returns>true if either Token changed, false if either Token is unchanged.</returns>
        internal void CheckToken(bool Override = false)
        {
            try
            {
                foreach (var a in ActiveBotTokens)
                {
                    LogWriter.DebugLog("CheckToken", DebugLogTypes.TwitchTokenBot, $"The {a.Key} token is active (data connection): {a.Value}");
                }

                LogWriter.DebugLog("CheckToken", DebugLogTypes.TwitchTokenBot, $"Checking if all tokens, active/inactive, are valid: {Override}");

                if (IsActive == true) // only calculate if bot is started, meaning the User is using this operation mode.
                {
                    lock (TokenLock)
                    {
                        if (Override || (ActiveBotTokens[BotType.BotAccount] && (DateTime.Now - BotAccessTokenLastCheckedDate).TotalSeconds > TokenCheckTimeWindow))
                        { // only check if we haven't checked in the last 1 second - avoid lots of checks in a single second
                            LogWriter.DebugLog("CheckToken", DebugLogTypes.TwitchTokenBot, $"Now checking Bot token.");
                            // try to refresh Bot token
                            Tuple<string, string, int> Botresponse = ProcessToken(
                                OptionFlags.TwitchAuthBotAuthCode,
                                OptionFlags.TwitchAuthBotClientId,
                                OptionFlags.TwitchAuthBotClientSecret,
                                OptionFlags.TwitchAuthBotRefreshToken,
                                OptionFlags.TwitchAuthBotAccessToken);

                            BotAccessTokenLastCheckedDate = DateTime.Now;

                            // with a good response, set the token data
                            if (Botresponse.Item1 != "" && Botresponse.Item2 != "")
                            {
                                OptionFlags.TwitchAuthBotAccessToken = Botresponse.Item1;
                                OptionFlags.TwitchAuthBotRefreshToken = Botresponse.Item2;
                                BotAccessTokenExpireDate = DateTime.Now.AddSeconds(Botresponse.Item3);

                                if (BotApiSettings != null)
                                {
                                    BotApiSettings.AccessToken = OptionFlags.TwitchAuthBotAccessToken;
                                    BotAccessTokenChanged?.Invoke(this, EventArgs.Empty);
                                }
                            }
                            else
                            {
                                BotAccessTokenUnChanged?.Invoke(this, EventArgs.Empty);
                            }
                        }

                        if (Override || (ActiveBotTokens[BotType.StreamerNoScopes] && (DateTime.Now - StreamerNoScopesAccessTokenLastCheckedDate).TotalSeconds > TokenCheckTimeWindow))
                        { // only check if we haven't checked in the last 1 second - avoid lots of checks in a single second
                            LogWriter.DebugLog("CheckToken", DebugLogTypes.TwitchTokenBot, $"Now checking NoScopes token.");
                            // try to refresh Bot token
                            Tuple<string, string, int> StreamerNoScopesresponse = ProcessToken(
                                OptionFlags.TwitchAuthStreamerNoScopesAuthCode,
                                OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchAuthStreamerClientId : OptionFlags.TwitchAuthBotClientId,
                                OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchAuthStreamerClientSecret : OptionFlags.TwitchAuthBotClientSecret,
                                OptionFlags.TwitchAuthStreamerNoScopesRefreshToken,
                                OptionFlags.TwitchAuthStreamerNoScopesAccessToken, true);

                            StreamerNoScopesAccessTokenLastCheckedDate = DateTime.Now;

                            // with a good response, set the token data
                            if (StreamerNoScopesresponse.Item1 != "" && StreamerNoScopesresponse.Item2 != "")
                            {
                                OptionFlags.TwitchAuthStreamerNoScopesAccessToken = StreamerNoScopesresponse.Item1;
                                OptionFlags.TwitchAuthStreamerNoScopesRefreshToken = StreamerNoScopesresponse.Item2;
                                StreamerNoScopesAccessTokenExpireDate = DateTime.Now.AddSeconds(StreamerNoScopesresponse.Item3);

                                if (StreamerNoScopesApiSettings != null)
                                {
                                    StreamerNoScopesApiSettings.AccessToken = OptionFlags.TwitchAuthStreamerNoScopesAccessToken;
                                    StreamerNoScopesAccessTokenChanged?.Invoke(this, EventArgs.Empty);
                                }
                            }
                            else
                            {
                                StreamerNoScopesAccessTokenUnChanged?.Invoke(this, EventArgs.Empty);
                            }
                        }

                        if (OptionFlags.TwitchStreamerUseToken)
                        {
                            if (Override || (ActiveBotTokens[BotType.StreamerAccount] && (DateTime.Now - StreamerAccessTokenLastCheckedDate).TotalSeconds > TokenCheckTimeWindow))
                            { // only check if we haven't checked in the last 1 second - avoid lots of checks in a single second
                                LogWriter.DebugLog("CheckToken", DebugLogTypes.TwitchTokenBot, $"Now checking Streamer token.");
                                // try to refresh streamer token
                                Tuple<string, string, int> Streamerresponse = ProcessToken(
                                    OptionFlags.TwitchAuthStreamerAuthCode,
                                    OptionFlags.TwitchAuthStreamerClientId,
                                    OptionFlags.TwitchAuthStreamerClientSecret,
                                    OptionFlags.TwitchAuthStreamerRefreshToken,
                                    OptionFlags.TwitchAuthStreamerAccessToken);

                                StreamerAccessTokenLastCheckedDate = DateTime.Now;

                                // with a good response, set the token data
                                if (Streamerresponse.Item1 != "" && Streamerresponse.Item2 != "")
                                {
                                    OptionFlags.TwitchAuthStreamerAccessToken = Streamerresponse.Item1;
                                    OptionFlags.TwitchAuthStreamerRefreshToken = Streamerresponse.Item2;
                                    StreamerAccessTokenExpireDate = DateTime.Now.AddSeconds(Streamerresponse.Item3);

                                    if (StreamerApiSettings != null)
                                    {
                                        StreamerApiSettings.AccessToken = OptionFlags.TwitchAuthStreamerAccessToken;
                                        StreamerAccessTokenChanged?.Invoke(this, EventArgs.Empty);
                                    }
                                }
                                else
                                {
                                    StreamerAccessTokenUnChanged?.Invoke(this, EventArgs.Empty);
                                }
                            }
                        }

                        BotApiSettings ??= new()
                        {
                            ClientId = OptionFlags.TwitchAuthBotClientId,
                            AccessToken = OptionFlags.TwitchAuthBotAccessToken,
                        };

                        StreamerApiSettings ??= OptionFlags.TwitchStreamerUseToken
                                ? new ApiSettings()
                                {
                                    ClientId = OptionFlags.TwitchAuthStreamerClientId,
                                    AccessToken = OptionFlags.TwitchAuthStreamerAccessToken
                                }
                                : BotApiSettings;

                        StreamerNoScopesApiSettings ??= new ApiSettings()
                        {
                            ClientId = OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchAuthStreamerClientId : OptionFlags.TwitchAuthBotClientId,
                            AccessToken = OptionFlags.TwitchAuthStreamerNoScopesAccessToken
                        };

                        SetTwitchApis();
                        //SetIds();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "CheckToken");
            }
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
        private Tuple<string, string, int> ProcessToken(string authcode, string clientId, string clientsecret, string refreshtoken, string accesstoken, bool NoScopes = false)
        {
            string AccessToken = "";
            string RefreshToken = "";
            int ExpiresIn = 0;

            ValidateAccessTokenResponse validToken = accesstoken != null ? AuthBot.ValidateAccessTokenAsync(accesstoken).Result : null;

            // only proceed if the clientId & client secret is valid
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientsecret))
            {
                LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"clientId and clientsecret are not null.");

                // if we have no auth code, tell the GUI user to authorize the app - since we have client ID & secret
                if (string.IsNullOrEmpty(authcode))
                {
                    LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"auth code is null or empty.");

                    // notify GUI if user needs to reauthenticate
                    if (NoScopes)
                    {
                        StreamerNoScopesAuthCodeExpired?.Invoke(this, new() { BotType = BotType.StreamerNoScopes });
                    }
                    else if (clientId == OptionFlags.TwitchAuthBotClientId)
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
                    LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"auth code is available.");
                    LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"testing access token, it is {(validToken == null ? "invalid" : "valid")}");

                    // clientId_!null, clientsecret_!null, authcode_!null, refreshtoken_null-try to get one
                    if (string.IsNullOrEmpty(refreshtoken))
                    {
                        LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"refresh token is null or empty.");

                        try
                        {
                            AuthCodeResponse BotAuthRefresh = AuthBot.GetAccessTokenFromCodeAsync(authcode, clientsecret, OptionFlags.TwitchAuthRedirectURL, clientId).Result;
                            // successful API call, we get a refresh token and access token
                            RefreshToken = BotAuthRefresh.RefreshToken;
                            AccessToken = BotAuthRefresh.AccessToken;
                            ExpiresIn = BotAuthRefresh.ExpiresIn;

                            LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"Twitch provided updated access & refresh tokens.");
                        }
                        catch (BadRequestException AuthCodEx)
                        {
                            LogWriter.LogException(AuthCodEx, "ProcessToken");
                            // notify GUI if user needs to reauthenticate, failed auth code
                            if (NoScopes)
                            {
                                StreamerNoScopesAuthCodeExpired?.Invoke(this, null);
                            }
                            else if (clientId == OptionFlags.TwitchAuthBotClientId)
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
                        LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"Access token is no longer valid, refreshing the access token.");

                        // if we're here, we had a badrequesttokenexception 

                        try
                        {
                            // get the new access/refresh tokens
                            RefreshResponse BotTokenResponse = AuthBot.RefreshAuthTokenAsync(refreshtoken, clientsecret, clientId).Result;

                            RefreshToken = BotTokenResponse.RefreshToken;
                            AccessToken = BotTokenResponse.AccessToken;
                            ExpiresIn = BotTokenResponse.ExpiresIn;

                            LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"Twitch gave us a new fresh access token.");
                        }
                        catch (BadRequestException ReqEx)
                        {  // the refresh token has failed, clear it all out and tell the user to reauthorize the app
                            LogWriter.LogException(ReqEx, "ProcessToken");

                            LogWriter.DebugLog("ProcessToken", DebugLogTypes.TwitchTokenBot, $"Twitch rejected the refresh token. User must reauthorize the application.");

                            // if we're here, the tokens are expired and need to start over with the authorization process
                            if (NoScopes)
                            {
                                OptionFlags.TwitchAuthStreamerNoScopesAccessToken = "";
                                OptionFlags.TwitchAuthStreamerNoScopesAuthCode = "";
                                OptionFlags.TwitchAuthStreamerNoScopesRefreshToken = "";
                                StreamerNoScopesAuthCodeExpired?.Invoke(this, null);
                            }
                            else if (clientId == OptionFlags.TwitchBotClientId) // determine if the client Id is the bot client
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
                            StopBot();
                        }
                    }
                }
            }

            return new(AccessToken, RefreshToken, ExpiresIn);
        }

        /// <summary>
        /// Generate an approval URL to point the user to a Twitch link to authorize account access
        /// </summary>
        /// <param name="clientId">The client Id registered to the account used in the app (could be bot account or streamer account).</param>
        /// <param name="OpenBrowser">An action code bit to open the browser at the end of this code flow.</param>
        /// <param name="AuthenticationFinished">An action to perform once the authentication is finished-specifically a GUI action.</param>
        internal void GenerateAuthCodeURL(string clientId, bool NoScopes, Action<string> OpenBrowser = null, Action AuthenticationFinished = null)
        {
            LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "Request to generate an auth code URL.");

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

            if (NoScopes)
            {
                LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "Generating no scopes authorization URL.");

                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    [""],
                    state: State,
                    clientId: OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchAuthStreamerClientId : OptionFlags.TwitchAuthBotClientId);

                LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "URL generated.");

                StreamerNoScopesAuthCodeExpired?.Invoke(this, new(buildURL, State, OpenBrowser, AuthenticationFinished));

            }
            else
            {
                // invoke event to get the user involved with authorizing the application
                if (clientId == OptionFlags.TwitchAuthBotClientId) // determine if the client Id is the bot client
                {
                    LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "Generating bot account authorization URL.");

                    string buildURL = AuthBot.GetAuthorizationCodeUrl(
                        OptionFlags.TwitchAuthRedirectURL,
                        OptionFlags.TwitchStreamerUseToken ? // check if the bot account is the streamer account, determines scopes to request
                        Resources.CredentialsTwitchScopesDiffOauthBot.Split(' ') :
                        Resources.CredentialsTwitchScopesOauthSame.Split(' '),
                        state: State,
                        clientId: clientId);

                    LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "URL generated.");

                    BotAcctAuthCodeExpired?.Invoke(this, new(buildURL, State, OpenBrowser, AuthenticationFinished));
                }
                else
                {
                    LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "Generating streamer account authorization URL.");

                    string buildURL = AuthBot.GetAuthorizationCodeUrl(
                        OptionFlags.TwitchAuthRedirectURL,
                        Resources.CredentialsTwitchScopesDiffOauthChannel.Split(' '),
                        state: State,
                        clientId: clientId
                        );
                    LogWriter.DebugLog("GenerateAuthCodeURL", DebugLogTypes.TwitchTokenBot, "URL generated.");

                    StreamerAcctAuthCodeExpired?.Invoke(this, new(buildURL, State, OpenBrowser, AuthenticationFinished));
                }
            }
        }

        /// <summary>
        /// Clears the authentication codes, access & refresh tokens for the bot & streamer accounts.
        /// Required when access scopes change.
        /// </summary>
        internal static void ForceReauthorization()
        {
            LogWriter.DebugLog("ForceReauthorization", DebugLogTypes.TwitchTokenBot, "Request to reauthorize the application. Clearing the authentication code(s).");

            OptionFlags.TwitchAuthBotAuthCode = "";
            OptionFlags.TwitchAuthBotAccessToken = "";
            OptionFlags.TwitchAuthBotRefreshToken = "";

            OptionFlags.TwitchAuthStreamerAuthCode = "";
            OptionFlags.TwitchAuthStreamerAccessToken = "";
            OptionFlags.TwitchAuthStreamerRefreshToken = "";

            OptionFlags.TwitchAuthStreamerNoScopesAuthCode = "";
            OptionFlags.TwitchAuthStreamerNoScopesAccessToken = "";
            OptionFlags.TwitchAuthStreamerNoScopesRefreshToken = "";
        }
    }
}
