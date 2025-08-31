using TwitchLib.EventSub.Websockets.Core.Models;

namespace StreamerBotLib.Models.Interfaces
{
    public interface IEventSubMessageIdsLogger
    {
        public static List<WebsocketEventSubMetadata> MessageIdLog = [];

        bool MsgLogging { get; set; }

        public void MsgLogCleanup();

        public bool AddMessageId(WebsocketEventSubMetadata args, Predicate<WebsocketEventSubMetadata> action);
    }
}
