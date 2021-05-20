using System;

namespace ChatBot_Net5.Interfaces
{
    public interface IIOModule
    {
        public string ChatClientName { get; set; }

        /// <summary>
        /// User name used in the connection
        /// </summary>
        public static string TwitchClientID { get; set; }

        /// <summary>
        /// Name of the Bot Account
        /// </summary>
        public static string TwitchBotUserName { get; set; }

        /// <summary>
        /// Channel name used in the connection
        /// </summary>
        public static string TwitchChannelName { get; set; }

        /// <summary>
        /// Token used for the connection.
        /// </summary>
        public static string TwitchAccessToken { get; set; }

        /// <summary>
        /// Refresh token used to generate a new access token.
        /// </summary>
        public static string TwitchRefreshToken { get; set; }

        /// <summary>
        /// the date by which to generate/refresh a new access token.
        /// </summary>
        public static DateTime TwitchRefreshDate { get; set; }

        /// <summary>
        /// The poll time in seconds to check the channel for new followers
        /// </summary>
        public static double TwitchFrequencyFollowerTime { get; set; }

        /// <summary>
        /// The poll time in seconds to check for channel going live
        /// </summary>
        public static double TwitchFrequencyLiveNotifyTime { get; set; }

        /// <summary>
        /// Whether to display bot connection to channel.
        /// </summary>
        public static bool ShowConnectionMsg { get; set; }

        // Connect to the data provider, must have Stream Key and Token set to connect
        bool Connect();

        // Send data to the provider
        bool Send(string s);

        // Send whisper to the provider
        bool SendWhisper(string user, string s);

        // Receive Whisper data from the provider via the callback method
        bool ReceiveWhisper(Action<string> ReceiveWhisperCallback);

        // Start send receive operations
        bool StartBot();

        // Stop operations
        bool StopBot();

        bool RefreshSettings();
    }
}
