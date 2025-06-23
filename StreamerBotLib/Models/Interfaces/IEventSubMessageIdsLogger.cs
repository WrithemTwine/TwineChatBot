namespace StreamerBotLib.Models.Interfaces
{
    using TwitchLib.EventSub.Websockets.Core.Models;

    public interface IEventSubMessageIdsLogger
    {
        public static List<EventSubMetadata> MessageIdLog = [];

        bool MsgLogging { get; set; }

        public void MsgLogCleanup();

        public bool AddMessageId(EventSubMetadata args, Predicate<EventSubMetadata> action);
    }
}
