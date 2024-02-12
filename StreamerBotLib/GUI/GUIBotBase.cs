using StreamerBotLib.Events;

namespace StreamerBotLib.GUI
{
    public class GUIBotBase
    {
        public static event EventHandler<BotStartStopEventArgs> OnBotStarted;
        public static event EventHandler<BotStartStopEventArgs> OnBotStopped;
        public static event EventHandler<BotStartStopEventArgs> OnBotFailedStart;

        protected void BotStarted(BotStartStopEventArgs e)
        {
            OnBotStarted?.Invoke(this, e);
        }

        protected void BotStopped(BotStartStopEventArgs e)
        {
            OnBotStopped?.Invoke(this, e);
        }

        protected void BotFailedStart(BotStartStopEventArgs e)
        {
            OnBotFailedStart?.Invoke(this, e);
        }
    }
}
