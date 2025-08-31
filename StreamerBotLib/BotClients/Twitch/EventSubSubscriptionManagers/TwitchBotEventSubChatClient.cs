using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Core.EventArgs.Channel;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.Models;

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
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                  {
                      return
                      m.MessageId == ((WebsocketEventSubMetadata)((WebsocketEventSubMetadata)args.Metadata)).MessageId &&
                      m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                  }))
                {
                    LogWriter.DebugLog("OnChannelChatMessage", DebugLogTypes.TwitchBotEventSubBot, "Received a new chat message event.");

                    ChannelChatMessage msg = args.Payload.Event;
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

        /// <summary>
        /// The EventSub subscriptions to add when EventSub is already connected. Especially for stream online versus offline; requiring different subscriptions.
        /// </summary>
        public void AddSubscriptions()
        {
            CreateEventSubSubscription("channel.chat.message", "1", new Dictionary<string, string>
            {
                {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                {"user_id", OptionFlags.TwitchBotUserId }
            });
        }

        /// <summary>
        /// The EventSub subscriptions to add when EventSub establishes a connection.
        /// </summary>
        public void AddConnectionSubscriptions()
        {
            CreateEventSubSubscription("channel.chat.message", "1", new Dictionary<string, string>
            {
                {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                {"user_id", OptionFlags.TwitchBotUserId }
            });
        }

        /// <summary>
        /// Creates a new EventSub subscription for the given type, version, and conditions.
        /// </summary>
        /// <param name="SubscriptionType">Type of the Subscription, also used as a description key in tracking subscriptions.</param>
        /// <param name="Version">The Twitch EventSub API version number.</param>
        /// <param name="conditions">The JSON entry parameters for the subscription, per Twitch EventSub API.</param>
        /// <param name="KeyOverride">Alternate key for tracking subscriptions, utilized for duplicate subscription types (with different conditions) otherwise prevented.</param>
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions, string KeyOverride = null)
        {
            void CreateSubAction()
            {
                LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchBotEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, $"Adding new subscription for {SubscriptionType}.");

                    var SubResponse = tokenBot.BotHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];

                    LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a(n) {SubResponse.Status} status.");

                    SubscriptionIdKeys.Add(KeyOverride ?? SubResponse.Type, SubResponse.Id);
                }
            }

            try
            {
                CreateSubAction();
            }
            catch (BadTokenException ex)
            {
                LogWriter.LogException(ex, "CreateEventSubSubscription");
                tokenBot.CheckToken();
                CreateSubAction();
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
            LogWriter.DebugLog("ClearEventSubSubscriptions", DebugLogTypes.TwitchStreamerEventSubBot, "Clearing all subscriptions.");

            SubscriptionIdKeys.Clear();
        }
    }
}
