using StreamerBot.Events;
using StreamerBot.Enum;
using StreamerBot.Interfaces;

using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace StreamerBot.BotClients
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
            StopThreads();
            foreach (IIOModule a in BotsList)
            {
                a.ExitBot();
            }
        }

        private void StopThreads()
        {
            foreach(Thread t in MultiThreadOps)
            {
                if (t?.IsAlive == true)
                {
                    t.Join();
                }
            }
        }

        protected void InvokeBotEvent(object sender, BotEvents Botevent, EventArgs eventargs)
        {
            BotEvent?.Invoke(sender, new() { MethodName = Botevent.ToString(), e = eventargs });
        }


    }
}
