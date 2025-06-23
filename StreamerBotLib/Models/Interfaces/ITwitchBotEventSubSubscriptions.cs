
namespace StreamerBotLib.Models.Interfaces
{
    using StreamerBotLib.BotClients.Twitch;
    using StreamerBotLib.Models.Enums;

    using TwitchLib.EventSub.Websockets;

    public interface ITwitchBotEventSubSubscriptions
    {
        public BotType CurrBot { get; }

        public ITwitchBotEventSubSubscriptions ConfigureMessageLogger(IEventSubMessageIdsLogger eventSubMessageIdsLogger);
        public ITwitchBotEventSubSubscriptions AddEventHandlers(EventSubWebsocketClient EventSubClient);
        internal ITwitchBotEventSubSubscriptions ConfigureTokenBot(TwitchTokenBot TokenBot);
        public void AddSubscriptions();
        public void AddConnectionSubscriptions();
        public void RemoveSubscriptions();
        public void ClearSubscriptions();
    }
}
