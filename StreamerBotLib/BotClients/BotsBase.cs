using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBotLib.BotClients
{
    public class BotsBase : IBotTypes
    {
        public event EventHandler<BotEventArgs> BotEvent;

        protected Collection<Thread> MultiThreadOps = [];
        /// <summary>
        /// Utilize the read-only version of the data manager, designed to only read data
        /// </summary>
        public static IDataManagerReadOnly DataManager { get; set; }

        internal Collection<IIOModule> BotsList { get; private set; } = [];

        public void AddBot(IIOModule bot)
        {
            BotsList.Add(bot);
        }

        public void Send(string s, bool Announcement = false)
        {
            foreach (IIOModule a in BotsList)
            {
                a.Send(s, Announcement);
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
                LogWriter.LogException(ex, "StopBots");
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
                LogWriter.LogException(ex, "StopThreads");
            }
        }

        protected void InvokeBotEvent(object sender, BotEvents Botevent, EventArgs eventargs)
        {
            BotEvent?.Invoke(sender, new() { MethodName = Botevent, e = eventargs });
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
