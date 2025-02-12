using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets.Handler.Channel;

namespace StreamerBotLib.BotClients.Twitch.EventSubSubscriptionManagers
{
    internal class TwitchBotEventSubChatClient : ITwitchBotEventSubSubscriptions
    {
        public BotType CurrBot => BotType.BotAccount;

        private IEventSubMessageIdsLogger EventSubMessageIdsLogger;
        private EventSubWebsocketClient _eventSubWebsocketClient;
        private static TwitchTokenBot tokenBot;

        /// <summary>
        /// maintains the ID list for the current subscriptions, removed when deleting the subscription
        /// Key: subscription type name, e.g. channel.chat.message
        /// Value: the subscription ID from creating the subscription
        /// </summary>
        private readonly Dictionary<string, string> SubscriptionIdKeys = [];

        internal event EventHandler<ChannelChatMessageEventArgs> OnChannelChatMessageReceived;

        private Task OnChannelChatMessage(object sender, ChannelChatMessageArgs args)
        {
            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                  {
                      return
                      m.MessageId == args.Notification.Metadata.MessageId &&
                      m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                  }))
                {
                    ChannelChatMessage msg = args.Notification.Payload.Event;
                    OnChannelChatMessageReceived?.Invoke(this, new(msg));
                }
            });
        }

        public ITwitchBotEventSubSubscriptions ConfigureMessageLogger(IEventSubMessageIdsLogger eventSubMessageIdsLogger)
        {
            EventSubMessageIdsLogger = eventSubMessageIdsLogger;
            return this;
        }

        public ITwitchBotEventSubSubscriptions AddEventHandlers(EventSubWebsocketClient EventSubClient)
        {
            _eventSubWebsocketClient = EventSubClient;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessage;

            return this;
        }

        ITwitchBotEventSubSubscriptions ITwitchBotEventSubSubscriptions.ConfigureTokenBot(TwitchTokenBot TokenBot)
        {
            tokenBot = TokenBot;
            return this;
        }

        public void AddSubscriptions()
        {
        }

        public void AddConnectionSubscriptions()
        {
            void CreateSubAction(string SubscriptionType)
            {
                LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    var conditions = new Dictionary<string, string>
                                {
                                    {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                                    {"user_id", OptionFlags.TwitchBotUserId }
                                };

                    var SubResponse = tokenBot.BotHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                                SubscriptionType,
                                "1", conditions,
                                EventSubTransportMethod.Websocket,
                                _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];

                    SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
                }
            }

            try
            {
                CreateSubAction(new ChatMessageHandler().SubscriptionType);
            }
            catch (BadTokenException ex)
            {
                LogWriter.LogException(ex, "CreateEventSubSubscription");
                tokenBot.CheckToken();
                CreateSubAction(new ChatMessageHandler().SubscriptionType);
            }
        }

        public void RemoveSubscriptions()
        {
            try
            {
                foreach (string key in SubscriptionIdKeys.Keys)
                {
                    if (tokenBot.BotHelixApi.Helix.EventSub.DeleteEventSubSubscriptionAsync(SubscriptionIdKeys[key]).Result)
                    {
                        SubscriptionIdKeys.Remove(key);
                    }
                    LogWriter.DebugLog("DeleteEventSubSubscription",
                       DebugLogTypes.TwitchStreamerEventSubBot,
                       $"Deleted the {key} subscription.");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "DeleteEventSubSubscription");
            }
        }

        public void ClearSubscriptions()
        {
            SubscriptionIdKeys.Clear();
        }
    }
}
