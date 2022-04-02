using StreamerBotLib.Events;

using System;

namespace StreamerBotLib.Interfaces
{
    public interface IBotTypes
    {
        public event EventHandler<BotEventArgs> BotEvent;

        public void Send(string s);

        public void StopBots();

        public void GetAllFollowers();

        public void SetIds();
    }
}
