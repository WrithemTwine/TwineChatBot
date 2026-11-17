using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;

namespace StreamerBotLib.GUI.toDelete
{
    public class GUIBotBase
    {
        public static event EventHandler<BotStartStopEventArgs> OnBotStarted;
        public static event EventHandler<BotStartStopEventArgs> OnBotStopped;
        public static event EventHandler<BotStartStopEventArgs> OnBotFailedStart;

        protected void BotStarted(BotStartStopEventArgs e)
        {
            LogWriter.DebugLog("BotStarted", DebugLogTypes.GUIBotComs, $"Bot started, {e.Bots}.");
            OnBotStarted?.Invoke(this, e);
        }

        protected void BotStopped(BotStartStopEventArgs e)
        {
            LogWriter.DebugLog("BotStopped", DebugLogTypes.GUIBotComs, $"Bot stopped, {e.Bots}.");
            OnBotStopped?.Invoke(this, e);
        }

        protected void BotFailedStart(BotStartStopEventArgs e)
        {
            LogWriter.DebugLog("BotFailedStart", DebugLogTypes.GUIBotComs, $"Bot failed to start, {e.Bots}.");
            OnBotFailedStart?.Invoke(this, e);
        }
    }
}
