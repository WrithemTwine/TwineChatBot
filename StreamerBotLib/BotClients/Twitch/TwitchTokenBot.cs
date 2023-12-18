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
        private readonly ExtAuth AuthBot;

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

       /*     if (IsStarted) // only calculate if bot is started, meaning the User is using this operation mode.
            {
                // try to refresh Bot token
                Tuple<string, string, int> Botresponse = ProcessToken(
                    OptionFlags.TwitchAuthBotAuthCode,
                    TwitchClientID,
                    OptionFlags.TwitchAuthBotClientSecret,
                    TwitchRefreshToken,
                    TwitchAccessToken);

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
                    // try to refresh streamer token
                    Tuple<string, string, int> Streamerresponse = ProcessToken(
                        OptionFlags.TwitchAuthStreamerAuthCode,
                        TwitchStreamClientId,
                        OptionFlags.TwitchAuthStreamerClientSecret,
                        TwitchStreamerRefreshToken,
                        TwitchStreamerAccessToken);

                    if (Streamerresponse.Item1 != "" && Streamerresponse.Item2 != "" && Streamerresponse.Item3 != 0)
                    {
                        TwitchStreamerAccessToken = Streamerresponse.Item1;
                        TwitchStreamerRefreshToken = Streamerresponse.Item2;
                        StreamerAccessTokenExpireDate = DateTime.Now.AddSeconds(Streamerresponse.Item3);
                        StreamerAccessTokenLastCheckedDate = DateTime.Now;
                        result = true;
                    }
                }
            }*/

            return result;
        }

        /// <summary>
        /// Tests the access token if still valid. If valid returns empty tokens.
        /// If invalid, attempts to use the refresh token to receive another access token. Returns if successful.
        /// If the refresh token is also invalid, attempts to use the auth code to receive another access & refresh token.
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

            if (AuthBot.ValidateAccessTokenAsync(accesstoken).Result == null)
            {
                RefreshResponse BotTokenResponse = null;
                try
                {
                    BotTokenResponse = AuthBot.RefreshAuthTokenAsync(refreshtoken, clientsecret, clientId).Result;
                }
                catch (BadRequestException ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                    try
                    {
                        AuthCodeResponse BotAuthRefresh = AuthBot.GetAccessTokenFromCodeAsync(authcode, clientsecret, OptionFlags.TwitchAuthRedirectURL, clientId).Result;
                        RefreshToken = BotAuthRefresh.RefreshToken;
                        AccessToken = BotAuthRefresh.AccessToken;
                    }
                    catch (BadRequestException AuthCodEx)
                    {
                        LogWriter.LogException(AuthCodEx, MethodBase.GetCurrentMethod().Name);
                        GenerateAuthCodeURL(clientId);
                    }
                }

                RefreshToken = BotTokenResponse.RefreshToken;
                AccessToken = BotTokenResponse.AccessToken;
                ExpiresIn = BotTokenResponse.ExpiresIn;
            }

            return new(AccessToken, RefreshToken, ExpiresIn);
        }

        private void GenerateAuthCodeURL(string clientId)
        {
            string State = Convert.ToBase64String(Encoding.UTF8.GetBytes((clientId + DateTime.Now.ToLongDateString())))[30..];

            // invoke event to get the user involved with authorizing the application
            if (clientId == OptionFlags.TwitchBotClientId) // determine if the client Id is the bot client
            {

                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    OptionFlags.TwitchStreamerUseToken ? // check if the bot account is the streamer account, determines scopes to request
                    Resources.CredentialsTwitchScopesDiffOauthBot.Split(' ') :
                    Resources.CredentialsTwitchScopesOauthSame.Split(' '),
                    state: State,
                    clientId: clientId);

                BotAcctAuthCodeExpired?.Invoke(this, new(buildURL, State));
            }
            else
            {
                string buildURL = AuthBot.GetAuthorizationCodeUrl(
                    OptionFlags.TwitchAuthRedirectURL,
                    Resources.CredentialsTwitchScopesDiffOauthChannel.Split(' '),
                    state: State,
                    clientId: clientId
                    );
                StreamerAcctAuthCodeExpired?.Invoke(this, new(buildURL, State));
            }
        }
    }
}
