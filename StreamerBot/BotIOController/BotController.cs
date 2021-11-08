using StreamerBot.BotClients;
using StreamerBot.Interfaces;
using StreamerBot.Systems;

using System.Collections.ObjectModel;

namespace StreamerBot.BotIOController
{
    public class BotController
    {
        public SystemsController Systems { get; private set; }
        internal Collection<IBotTypes> BotsList { get; private set; } = new();

        public BotController()
        {
            Systems.PostChannelMessage += Systems_PostChannelMessage;


            IBotTypes Twitch = new BotsTwitch();

            

            BotsList.Add(Twitch);

        }

        private void Systems_PostChannelMessage(object sender, Events.PostChannelMessageEventArgs e)
        {
            Send(e.Msg);
        }

        public void Send(string s)
        {
            foreach (IBotTypes bot in BotsList)
            {
                bot.Send(s);
            }
        }

        public void ExitBots()
        {
            foreach (IBotTypes bot in BotsList)
            {
                bot.StopBots();
            }
        }
    }
}
