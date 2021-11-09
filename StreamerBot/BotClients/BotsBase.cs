using StreamerBot.Events;
using StreamerBot.Enum;
using StreamerBot.Interfaces;

using System;
using System.Collections.ObjectModel;

namespace StreamerBot.BotClients
{
    public class BotsBase : IBotTypes
    {
        public event EventHandler<BotEventArgs> BotEvent;


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
            foreach (IIOModule a in BotsList)
            {
                a.ExitBot();
            }
        }

        protected void InvokeBotEvent(object sender, BotEvents Botevent, EventArgs eventargs)
        {
            BotEvent?.Invoke(sender, new() { MethodName = Botevent.ToString(), e = eventargs });
        }


    }
}
