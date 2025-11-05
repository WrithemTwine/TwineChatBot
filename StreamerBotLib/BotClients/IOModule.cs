using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;

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
        /// <code>null - first time bot has never started</code>
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
            return await Task.Run(() =>
            {
                return false;
            });
        }

        public virtual async Task<bool> Disconnect()
        {
            return await Task.Run(() =>
            {
                return false;
            });
        }

        public virtual bool ReceiveWhisper(Action<string> ReceiveWhisperCallback)
        {
            throw new NotImplementedException();
        }

        public virtual async Task Send(string s, bool Announcement = false)
        {
            await Task.Run(() => { });
        }

        public virtual bool SendWhisper(string user, string s)
        {
            throw new NotImplementedException();
        }

        public virtual Task StartBot()
        {
            return Task.Run(() => { });
        }

        public virtual Task StopBot()
        {
            return Task.Run(() => { });
        }

        public virtual async Task<bool> ExitBot()
        {
            return await Task.Run(() =>
            {
                return true;
            });
        }
        #endregion
    }
}
