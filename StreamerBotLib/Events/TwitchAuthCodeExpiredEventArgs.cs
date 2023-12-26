using System;

namespace StreamerBotLib.Events
{
    /// <summary>
    /// Event response when the Twitch Authentication Code expired or is unavailable 
    /// and the user needs to authenticate the application.
    /// </summary>
    internal class TwitchAuthCodeExpiredEventArgs : EventArgs
    {
        public TwitchAuthCodeExpiredEventArgs() { }

        /// <summary>
        /// Construct a TwitchAuthCodeExpiredEventArgs object.
        /// </summary>
        /// <param name="authURL">The URL to have the user authenticate the application.</param>
        /// <param name="state">The state-a string added to the authenticate URL to validate the Twitch server return data.</param>
        public TwitchAuthCodeExpiredEventArgs(string authURL, string state, Action<string> callAction, Action authenticationFinished)
        {
            AuthURL = authURL;
            State = state;
            CallAction = callAction;
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

        public string BotType { get; set; }

        public Action<string> CallAction { get; set; }

        public Action AuthenticationFinished { get; set; }
    }
}
