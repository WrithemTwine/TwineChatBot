using StreamerBot.BotClients.Twitch;
using StreamerBot.Events;

using System;

namespace StreamerBot.GUI
{
    public class GUIBotBase
    {
        public event EventHandler<BotStartStopEventArgs> OnBotStarted;
        public event EventHandler<BotStartStopEventArgs> OnBotStopped;

        protected void BotStarted(BotStartStopEventArgs e)
        {
            OnBotStarted?.Invoke(this, e);
        }

        protected void BotStopped(BotStartStopEventArgs e)
        {
            OnBotStopped?.Invoke(this, e);
        }
    }
}
