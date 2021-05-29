
using ChatBot_Net5.Interfaces;
using ChatBot_Net5.Properties;

using System;

namespace ChatBot_Net5.Clients
{
    public abstract class IOModule : IIOModule 
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

        public bool IsStarted { get; set; } = false;
        public bool HandlersAdded { get; set; } = false;
        public bool IsStopped { get; set; } = false;

        public event EventHandler OnBotStarted;
        public event EventHandler OnBotStopped;

        protected void InvokeBotStarted()
        {
            OnBotStarted?.Invoke(this, new EventArgs());
        }

        protected void InvokeBotStopped()
        {
            OnBotStopped?.Invoke(this, new EventArgs());
        }

        public IOModule()
        {

        }

        #region interface

        public virtual bool Connect()
        {
            return false;
        }

        public virtual bool Disconnect()
        {
            return false;
        }

        public virtual bool ReceiveWhisper(Action<string> ReceiveWhisperCallback)
        {
            throw new();
        }

        public virtual bool Send(string s)
        {
            return false;
        }

        public virtual bool SendWhisper(string user, string s)
        {
            throw new();
        }

        public virtual bool StartBot()
        {
            return true;
        }

        public virtual bool StopBot()
        {
            return true;
        }

        public virtual bool RefreshSettings()
        {
            SaveParams();
            TwitchAccessToken = Settings.Default.TwitchAccessToken;
            TwitchBotUserName = Settings.Default.TwitchBotUserName;
            TwitchChannelName = Settings.Default.TwitchChannelName;
            TwitchClientID = Settings.Default.TwitchClientID;
            TwitchFrequencyFollowerTime = Settings.Default.TwitchFrequency;
            TwitchFrequencyLiveNotifyTime = Settings.Default.TwitchGoLiveFrequency;
            TwitchRefreshToken = Settings.Default.TwitchRefreshToken;
            TwitchRefreshDate = Settings.Default.TwitchRefreshDate;
            ShowConnectionMsg = Settings.Default.BotConnectionMsg;
            return true;
        }

        public virtual bool SaveParams() { Settings.Default.Save(); return true; }
        #endregion

    }

   
}
