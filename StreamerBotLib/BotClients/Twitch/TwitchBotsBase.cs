using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.EventSub.Websockets.Core.Models;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotsBase : IOModule
    {
        internal static TwitchTokenBot tokenBot = new();

        protected static List<EventSubMetadata> MessageIdLog = [];
        internal static bool MsgLogging;

        protected static void MsgLogCleanup()
        {
            if (!MsgLogging)
            {
                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                {
                    MessageIdLog.Clear();

                    TimeSpan interval = new(0, 10, 0);
                    while (OptionFlags.ActiveToken && MsgLogging)
                    {
                        MessageIdLog.RemoveAll((M) => (DateTime.Now - M.MessageTimestamp) > interval);

                        Thread.Sleep(500);
                    }
                });
            }
        }
    }
}
