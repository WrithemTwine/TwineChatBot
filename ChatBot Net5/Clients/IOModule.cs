
using ChatBot_Net5.Interfaces;

using System;

namespace ChatBot_Net5.Clients
{
    public abstract class IOModule : IIOModule 
    {
        public string ChatClientName { get; set; }

        /// <summary>
        /// User name used in the connection
        /// </summary>
        public string ClientID { get; set; }

        /// <summary>
        /// Name of the Bot Account
        /// </summary>
        public string BotUserName { get; set; }

        /// <summary>
        /// Channel name used in the connection
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Token used for the connection.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token used to generate a new access token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// the date by which to generate/refresh a new access token.
        /// </summary>
        public DateTime RefreshDate { get; set; }

        /// <summary>
        /// The poll time in seconds to check the data source
        /// </summary>
        public double FrequencyTime { get; set; }

        /// <summary>
        /// Whether to display bot connection to channel.
        /// </summary>
        public bool ShowConnectionMsg { get; set; }

        internal IOModule()
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
            throw new();
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

        internal virtual bool SaveParams() { return true; }
        #endregion

    }

   
}
