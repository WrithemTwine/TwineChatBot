using ChatBot_Net5.Enum;
using ChatBot_Net5.Properties;

using System;

namespace ChatBot_Net5.BotClients
{
    /// <summary>
    /// Abstract base class for all Twitch type Bots
    /// </summary>
    public abstract class TwitchBots : IOModule
    {
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
        /// The poll time in seconds to check the channel for new clips
        /// </summary>
        public static double TwitchFrequencyClipTime { get; set; }

        public override bool RefreshSettings()
        {
            SaveParams();
            TwitchAccessToken = Settings.Default.TwitchAccessToken;
            TwitchBotUserName = Settings.Default.TwitchBotUserName;
            TwitchChannelName = Settings.Default.TwitchChannelName;
            TwitchClientID = Settings.Default.TwitchClientID;
            TwitchFrequencyFollowerTime = Settings.Default.TwitchFrequencyFollow;
            TwitchFrequencyLiveNotifyTime = Settings.Default.TwitchGoLiveFrequency;
            TwitchFrequencyClipTime = Settings.Default.TwitchFrequencyClipTime;
            TwitchRefreshToken = Settings.Default.TwitchRefreshToken;
            TwitchRefreshDate = Settings.Default.TwitchRefreshDate;
            ShowConnectionMsg = Settings.Default.MsgBotConnection;
            return true;
        }
    }
}
