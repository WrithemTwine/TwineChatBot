using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;

namespace StreamerBotLib.BotClients
{
    public class BotsBase : IBotTypes
    {
        public event EventHandler<BotEventArgs> BotEvent;

        protected Collection<Thread> MultiThreadOps = new();
        /// <summary>
        /// Utilize the read-only version of the data manager, designed to only read data
        /// </summary>
        public static IDataManageReadOnly DataManager { get; set; }

        internal Collection<IIOModule> BotsList { get; private set; } = new();

        public void AddBot(IIOModule bot)
        {
            BotsList.Add(bot);
        }

        public void Send(string s)
        {
            foreach (IIOModule a in BotsList)
            {
                a.Send(s);
            }
        }

        public void StopBots()
        {
            try
            {
                StopThreads();
                foreach (IIOModule a in BotsList)
                {
                    a.ExitBot();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void StopThreads()
        {
            try
            {
                foreach (Thread t in MultiThreadOps)
                {
                    if (t?.IsAlive == true)
                    {
                        t.Join();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected void InvokeBotEvent(object sender, BotEvents Botevent, EventArgs eventargs)
        {
            BotEvent?.Invoke(sender, new() { MethodName = Botevent.ToString(), e = eventargs });
        }

        public virtual void GetAllFollowers()
        {
            throw new NotImplementedException();
        }

        public virtual void SetIds()
        {
            throw new NotImplementedException();
        }

        public virtual void ManageStreamOnlineOfflineStatus(bool Start)
        {
            throw new NotImplementedException();
        }
    }
}
