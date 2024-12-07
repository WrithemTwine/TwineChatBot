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

        public virtual async Task<bool> Connect()
        {
            return await new Task<bool>(() =>
            {
                return false;
            });
        }

        public virtual async Task<bool> Disconnect()
        {
            return await new Task<bool>(() =>
            {
                return false;
            });
        }

        public virtual bool ReceiveWhisper(Action<string> ReceiveWhisperCallback)
        {
            throw new NotImplementedException();
        }

        public virtual async Task Send(string s)
        {
            await new Task(() => { });
        }

        public virtual bool SendWhisper(string user, string s)
        {
            throw new NotImplementedException();
        }

        public virtual Task StartBot()
        {
            return new Task(() => { });
        }

        public virtual Task StopBot()
        {
            return new Task(() => { });
        }

        public virtual async Task<bool> ExitBot()
        {
            return await new Task<bool>(() =>
            {
                return true;
            });
        }
        #endregion
    }
}
