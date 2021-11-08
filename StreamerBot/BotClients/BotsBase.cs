using StreamerBot.Interfaces;

using System.Collections.ObjectModel;

namespace StreamerBot.BotClients
{
    public class BotsBase : IBotTypes
    {
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


    }
}
