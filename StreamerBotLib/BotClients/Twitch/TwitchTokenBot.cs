using StreamerBotLib.BotClients.Twitch.TwitchLib;
using StreamerBotLib.Events;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System;
using System.Reflection;
using System.Text;
using System.Threading;

using TwitchLib.Api.Auth;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;

namespace StreamerBotLib.BotClients.Twitch
{
    internal class TwitchTokenBot : TwitchBotsBase
    {
        private ExtAuth AuthBot;

        private bool TokenRenewalStarted;
        private bool AbortRenewToken;

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

        private const int MaxInterval = 30 * 60 * 1000;  // 30 mins * 60 s/min * 1000 ms/sec

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
            AuthBot = new(apiSettings, null, null);

            CheckToken();
        }

        public override bool StartBot()
        {
            if (!IsStarted || IsStopped)
            {
                IsStarted = true;
                IsStopped = false;
                AbortRenewToken = false;
                //StartRenewToken();
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

                Thread.Sleep(IntervalRandom.Next(MaxInterval / 4, MaxInterval));
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
                ApiSettings apiBotSettings = new() { ClientId=TwitchClientID, Secret=OptionFlags.TwitchAuthBotClientSecret };
                AuthBot = new(apiBotSettings, null, null);

                // try to refresh Bot token
                Tuple<string, string, int> Botresponse = ProcessToken(
                    OptionFlags.TwitchAuthBotAuthCode,
                    TwitchClientID,
                    OptionFlags.TwitchAuthBotClientSecret,
                    TwitchRefreshToken,
                    TwitchAccessToken);

                // with a good response, set the token data
                if (Botresponse.Item1 != "" && Botresponse.Item2 != "" && Botresponse.Item3 != 0)
                {
                    TwitchAccessToken = Botresponse.Item1;
                    TwitchRefreshToken = Botresponse.Item2;
                    BotAccessTokenExpireDate = DateTime.Now.AddSeconds(Botresponse.Item3);
                    BotAccessTokenLastCheckedDate = DateTime.Now;
                    result = true;
                }

                if (OptionFlags.TwitchStreamerUseToken)
                {
                    ApiSettings apiStreamerSettings = new() { ClientId = TwitchStreamClientId, Secret = OptionFlags.TwitchAuthStreamerClientSecret };
                    AuthBot = new(apiStreamerSettings, null, null);

                    // try to refresh streamer token
                    Tuple<string, string, int> Streamerresponse = ProcessToken(
                        OptionFlags.TwitchAuthStreamerAuthCode,
                        TwitchStreamClientId,
                        OptionFlags.TwitchAuthStreamerClientSecret,
                        TwitchStreamerRefreshToken,
                        TwitchStreamerAccessToken);

                    // with a good response, set the token data
                    if (Streamerresponse.Item1 != "" && Streamerresponse.Item2 != "" && Streamerresponse.Item3 != 0)
                    {
                        TwitchStreamerAccessToken = Streamerresponse.Item1;
                        TwitchStreamerRefreshToken = Streamerresponse.Item2;
                        StreamerAccessTokenExpireDate = DateTime.Now.AddSeconds(Streamerresponse.Item3);
                        StreamerAccessTokenLastCheckedDate = DateTime.Now;
                        result = true;
                    }
                }
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
        private Tuple<string, string, int> ProcessToken(string authcode, string clientId, string clientsecret, string refreshtoken, string accesstoken)
        {
            string AccessToken = "";
            string RefreshToken = "";
            int ExpiresIn = 0;

            // only proceed if the clientId & client secret is valid
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientsecret))
            {
                // when trying to start the app but need to refresh the token due to a change in scopes, clear the auth codes to force a reset
                if (OptionFlags.TwitchAuthNewScopeRefresh)
                {
                    // clear out Auth code
                    OptionFlags.TwitchAuthBotAuthCode = "";
                    OptionFlags.TwitchAuthStreamerAuthCode = "";

                    // clear the scope refresh flag, since we caused the user to re-authorize the app for both
                    // auth codes-depending if user chooses different accounts
                    OptionFlags.TwitchAuthNewScopeRefresh = false;
                }

                // if we have no auth code, tell the GUI user to authorize the app - since we have client ID & secret
                if (string.IsNullOrEmpty(authcode))
                {
                    // notify GUI if user needs to reauthenticate
                    if (clientId == OptionFlags.TwitchAuthClientId)
                    {
                        BotAcctAuthCodeExpired?.Invoke(this, new() { BotType = "Bot Account"});
                    }
                    else if(OptionFlags.TwitchStreamerUseToken)
                    {
                        StreamerAcctAuthCodeExpired?.Invoke(this, new() { BotType = "Streamer Account"});
                    }
                }
                else // we have an authcode, try to get the first refresh token & access token
                {
                    // clientId_!null, clientsecret_!null, authcode_!null, refreshtoken_null-try to get one
                    if (string.IsNullOrEmpty(refreshtoken))
                    {
                        try
                        {
                            AuthCodeResponse BotAuthRefresh = AuthBot.GetAccessTokenFromCodeAsync(authcode, clientsecret, OptionFlags.TwitchAuthRedirectURL, clientId).Result;
                            // successful API call, we get a refresh token and access token
                            RefreshToken = BotAuthRefresh.RefreshToken;
                            AccessToken = BotAuthRefresh.AccessToken;
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
                    else // if we already have a refresh token, try to refresh the access token; failure means user needs to reauthorize app
                    {
                        if (string.IsNullOrEmpty(accesstoken) || AuthBot.ValidateAccessTokenAsync(accesstoken).Result == null)
                        { // only try to refresh the access token if it's empty or invalid, otherwise, don't need to do anything
                            try
                            {
                                // get the new access/refresh tokens
                                RefreshResponse BotTokenResponse = AuthBot.RefreshAuthTokenAsync(refreshtoken, clientsecret, clientId).Result;

                                RefreshToken = BotTokenResponse.RefreshToken;
                                AccessToken = BotTokenResponse.AccessToken;
                                ExpiresIn = BotTokenResponse.ExpiresIn;
                            }
                            catch (BadRequestException ReqEx)
                            {  // the refresh token has failed, clear it all out and tell the user to reauthorize the app
                                LogWriter.LogException(ReqEx, MethodBase.GetCurrentMethod().Name);

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
                        }
                    }
                }
            }

            return new(AccessToken, RefreshToken, ExpiresIn);
        }

        internal void GenerateAuthCodeURL(string clientId, Action<string> action = null, Action AuthenticationFinished = null)
        {
            string seedvalue = (clientId + DateTime.Now.ToLongDateString()).Replace(" ","");
            string[] splitseed = seedvalue.Split(new char[] { 'a', 'b', 'e', 'j', 'm', 'w', 'g' });
            Random random = new();

            string buildstring = "";
            // reorder the substring segments
            for(int i = 0; i < splitseed.Length; i++ )
            {
                buildstring += splitseed[random.Next(splitseed.Length)];
            }

            string State = buildstring.Substring(0, Math.Min(buildstring.Length, 30));

            // invoke event to get the user involved with authorizing the application
            if (clientId == OptionFlags.TwitchAuthClientId) // determine if the client Id is the bot client
            {
                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    OptionFlags.TwitchStreamerUseToken ? // check if the bot account is the streamer account, determines scopes to request
                    Resources.CredentialsTwitchScopesDiffOauthBot.Split(' ') :
                    Resources.CredentialsTwitchScopesOauthSame.Split(' '),
                    state: State,
                    clientId: clientId);

                BotAcctAuthCodeExpired?.Invoke(this, new(buildURL, State, action, AuthenticationFinished));
            }
            else
            {
                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    Resources.CredentialsTwitchScopesDiffOauthChannel.Split(' '),
                    state: State,
                    clientId: clientId
                    );
                StreamerAcctAuthCodeExpired?.Invoke(this, new(buildURL, State, action, AuthenticationFinished));
            }
        }
    }
}
