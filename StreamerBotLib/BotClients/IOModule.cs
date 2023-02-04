using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;

using System;

namespace StreamerBotLib.BotClients
{
    /// <summary>
    /// Abstract base class for any bot attached to this application
    /// </summary>
    public abstract class IOModule : IIOModule
    {
        public Bots BotClientName { get; set; }

        /// <summary>
        /// Whether to display bot connection to channel.
        /// </summary>
        public static bool ShowConnectionMsg { get; set; }

        public bool IsStarted { get; set; }
        public bool HandlersAdded { get; set; }
        public bool IsStopped { get; set; } = true;

        public event EventHandler OnBotStarted;
        public event EventHandler OnBotStopped;
        public event EventHandler OnBotStopping;

        protected void InvokeBotStarted()
        {
            OnBotStarted?.Invoke(this, new EventArgs());
        }

        protected void InvokeBotStopped()
        {
            OnBotStopped?.Invoke(this, new EventArgs());
        }

        protected void InvokeBotStopping()
        {
            OnBotStopping?.Invoke(this, new EventArgs());
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
            throw new NotImplementedException();
        }

        public virtual bool Send(string s)
        {
            return false;
        }

        public virtual bool SendWhisper(string user, string s)
        {
            throw new NotImplementedException();
        }

        public virtual bool StartBot()
        {
            return true;
        }

        public virtual bool StopBot()
        {
            return true;
        }

        public virtual bool ExitBot()
        {
            return true;
        }
        #endregion
    }
}
