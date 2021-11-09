using StreamerBot.Events;
using System;

namespace StreamerBot.Interfaces
{
    public interface IBotTypes
    {
        public event EventHandler<BotEventArgs> BotEvent;

        public void Send(string s);

        public void StopBots();
    }
}
