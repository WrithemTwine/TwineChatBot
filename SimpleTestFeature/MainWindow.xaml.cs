using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows;

using TwitchLib.Api.Auth;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Enums;

namespace SimpleTestFeature
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<AuthScopes> scopes = new();
        private Auth AuthBot { get; set; }
        private string RedirectURL = "http://localhost:3000";

        public MainWindow()
        {
            InitializeComponent();

            scopes.Add(AuthScopes.Helix_Moderator_Read_Chatters);

            TBaccessscope.Text = $"{scopes[0]}";
        }

        private void StartListener()
        {
            // start an http listener to receive auth code
            Thread listener = new(new ThreadStart(() =>
            {
                HttpListener httpListener = new();
                httpListener.Prefixes.Add(RedirectURL.EndsWith('/') ? RedirectURL : $"{RedirectURL}/");

                httpListener.Start();
                HttpListenerRequest request = httpListener.GetContext().Request;

                Uri uridata = request.Url;
                httpListener.Close();

                /*
                 * from: https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#authorization-code-grant-flow
                 * expected return, affirm or deny, when attempting to authorize the application
                 * 
                 If the user authorized your app by clicking Authorize, the server sends the authorization code to your redirect URI (see the code query parameter):

http://localhost:3000/
    ?code=gulfwdmys5lsm6qyz4xiz9q32l10
    &scope=channel%3Amanage%3Apolls+channel%3Aread%3Apolls
    &state=c3ab8aa609ea11e793ae92361f002671

If the user didn’t authorize your app, the server sends the error code and description to your redirect URI (see the error and error_description query parameters):

http://localhost:3000/
    ?error=access_denied
    &error_description=The+user+denied+you+access
    &state=c3ab8aa609ea11e793ae92361f002671

                 */

                var QueryValues = HttpUtility.ParseQueryString(uridata.Query);

                if (!QueryValues.AllKeys.Contains("error"))
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        TBauthcode.Text = QueryValues["code"];
                    });
                }
            }));

            listener.Start();
        }

        private void Button_Authorize_Click(object sender, RoutedEventArgs e)
        {
            string clientId = TBclientId.Text;
            string clientSecret = TBclientsecret.Text;

            AuthBot = new(new ApiSettings()
            {
                ClientId = clientId,
                Secret = clientSecret,
                Scopes = scopes
            }, null, null);

            string Url = AuthBot.GetAuthorizationCodeUrl(RedirectURL, scopes);

            StartListener();

            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"\"{Url}\"";
            process.Start();
        }

        private void Button_GetTokens_Click(object sender, RoutedEventArgs e)
        {
            string clientId = TBclientId.Text;
            string clientSecret = TBclientsecret.Text;

            AuthCodeResponse authCodeResponse = AuthBot.GetAccessTokenFromCodeAsync(TBauthcode.Text, clientSecret, RedirectURL, clientId).Result;

            TBaccesstoken.Text = authCodeResponse.AccessToken;
            TBrefreshtoken.Text = authCodeResponse.RefreshToken;
        }
    }
}
