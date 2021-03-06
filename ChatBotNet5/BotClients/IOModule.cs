﻿using ChatBot_Net5.Interfaces;
using ChatBot_Net5.Properties;

using System;

namespace ChatBot_Net5.BotClients
{
    /// <summary>
    /// Abstract base class for any bot attached to this application
    /// </summary>
    public abstract class IOModule : IIOModule
    {
        public string ChatClientName { get; set; }

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

        public virtual bool RefreshSettings()
        {
            return true;
        }

        public virtual bool SaveParams()
        {
            Settings.Default.Save();
            return true;
        }

        public virtual bool ExitBot()
        {
            return true;
        }
        #endregion
    }
}
