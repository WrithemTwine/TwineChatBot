using StreamerBotLib.Enums;

namespace StreamerBotLib.Events
{
    /// <summary>
    /// Event response when the Twitch Authentication Code expired or is unavailable 
    /// and the user needs to authenticate the application.
    /// </summary>
    internal class TwitchAuthCodeExpiredEventArgs : EventArgs
    {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public TwitchAuthCodeExpiredEventArgs() { }
        /// <summary>
        /// Construct a TwitchAuthCodeExpiredEventArgs object.
        /// </summary>
        /// <param name="authURL">The URL to have the user authenticate the application.</param>
        /// <param name="state">The state-a string added to the authenticate URL to validate the Twitch server return data.</param>
        /// <param name="openbrowser">An action to open a web browser according to the user's preference-their default browser or this application's WebView2 (Edge) browser.</param>
        /// <param name="authenticationFinished">Action to perform when the authentication is finished.</param>
        public TwitchAuthCodeExpiredEventArgs(string authURL, string state, Action<string> openbrowser, Action authenticationFinished)
        {
            AuthURL = authURL;
            State = state;
            OpenBrowser = openbrowser;
            AuthenticationFinished = authenticationFinished;
        }
        /// <summary>
        /// The URL to present the user to authenticate the application.
        /// </summary>
        internal string AuthURL { get; set; }
        /// <summary>
        /// The 'state' random generated string included in the URL for return, to authenticate the returned data from Twitch.
        /// </summary>
        internal string State { get; set; }
        /// <summary>
        /// Specifies the bot used in this event chain.
        /// </summary>
        public BotType BotType { get; set; }
        /// <summary>
        /// Action for opening a web browser per user preferences.
        /// </summary>
        public Action<string> OpenBrowser { get; set; }
        /// <summary>
        /// Action to perform when the authentication is finished.
        /// </summary>
        public Action AuthenticationFinished { get; set; }
    }
}
