using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;

using TwitchLib.EventSub.Websockets.Core.Models;

namespace StreamerBotLib.BotClients.Twitch
{
    public class EventSubMessageIdsLogger : IEventSubMessageIdsLogger
    {
        public bool MsgLogging { get; set; }
        public List<EventSubMetadata> MessageIdLog = [];

        public void MsgLogCleanup()
        {
            if (!MsgLogging)
            {
                ThreadManager.CreateThreadStart(Task.Run(async () =>
                {
                    MessageIdLog.Clear();

                    TimeSpan interval = new(0, 10, 0);
                    while (OptionFlags.ActiveToken && MsgLogging)
                    {
                        MessageIdLog.RemoveAll((M) => DateTime.Now - M.MessageTimestamp > interval);

                        await Task.Delay(500);
                    }
                }));
            }
        }

        public bool AddMessageId(EventSubMetadata args, Predicate<EventSubMetadata> action)
        {
            return MessageIdLog.UniqueAdd(args, action);
        }
    }
}
