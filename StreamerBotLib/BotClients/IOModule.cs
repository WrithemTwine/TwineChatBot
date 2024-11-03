using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;

namespace StreamerBotLib.BotClients
{
    /// <summary>
    /// Abstract base class for any bot attached to this application
    /// </summary>
    public abstract class IOModule : IIOModule
    {
        public Bots BotClientName { get; set; }

        /// <summary>
        /// Flag for bot activity:
        /// <code>null - first time bot active</code>
        /// <code>true - bot is started</code>
        /// <code>false - bot is stopped</code>
        /// </summary>
        public bool? IsActive { get; set; }
        public bool HandlersAdded { get; set; }

        public event EventHandler OnBotStarted;
        public event EventHandler OnBotStopped;
        public event EventHandler OnBotStopping;
        public event EventHandler OnBotFailedStart;

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

        protected void InvokeBotFailedStart()
        {
            OnBotFailedStart?.Invoke(this, new EventArgs());
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

        public virtual void Send(string s)
        {
            return;
        }

        public virtual bool SendWhisper(string user, string s)
        {
            throw new NotImplementedException();
        }

        public virtual void StartBot()
        {
        }

        public virtual void StopBot()
        {
        }

        public virtual bool ExitBot()
        {
            return true;
        }
        #endregion
    }
}
