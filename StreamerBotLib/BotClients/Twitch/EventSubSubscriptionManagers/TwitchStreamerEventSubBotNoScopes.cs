using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Stream;
using TwitchLib.EventSub.Websockets.Handler.Channel;
using TwitchLib.EventSub.Websockets.Handler.Channel.Raids;
using TwitchLib.EventSub.Websockets.Handler.Stream;

namespace StreamerBotLib.BotClients.Twitch.EventSubSubscriptionManagers
{
    /// <summary>
    /// Event Sub bot using the Twitch Streamer client Id but requires no access scopes: stream online, stream offline, raid
    /// </summary>
    public class TwitchStreamerEventSubBotNoScopes : ITwitchBotEventSubSubscriptions
    {
        public BotType CurrBot => BotType.StreamerNoScopes;

        private IEventSubMessageIdsLogger EventSubMessageIdsLogger;
        private EventSubWebsocketClient _eventSubWebsocketClient;
        private static TwitchTokenBot tokenBot;

        /// <summary>
        /// maintains the ID list for the current subscriptions, removed when deleting the subscription
        /// Key: subscription type name, e.g. channel.chat.message
        /// Value: the subscription ID from creating the subscription
        /// </summary>
        private readonly Dictionary<string, string> SubscriptionIdKeys = [];

        public event EventHandler<NewChannelRaidEventArgs> OutChannelRaid;
        public event EventHandler<NewChannelRaidEventArgs> NewChannelRaid;
        public event EventHandler<NewStreamOfflineEventArgs> NewStreamOffline;
        public event EventHandler<NewStreamOnlineEventArgs> NewStreamOnline;
        public event EventHandler<NewChannelUpdateEventArgs> NewChannelUpdate;

        public event EventHandler OnNewLiveStreamStarted;

        public ITwitchBotEventSubSubscriptions ConfigureMessageLogger(IEventSubMessageIdsLogger eventSubMessageIdsLogger)
        {
            EventSubMessageIdsLogger = eventSubMessageIdsLogger;
            return this;
        }

        public ITwitchBotEventSubSubscriptions AddEventHandlers(EventSubWebsocketClient EventSubClient)
        {
            _eventSubWebsocketClient = EventSubClient;

            _eventSubWebsocketClient.StreamOnline += StreamOnline;
            _eventSubWebsocketClient.StreamOffline += StreamOffline;
            _eventSubWebsocketClient.ChannelRaid += ChannelRaid;
            _eventSubWebsocketClient.ChannelUpdate += ChannelUpdate;

            return this;
        }

        ITwitchBotEventSubSubscriptions ITwitchBotEventSubSubscriptions.ConfigureTokenBot(TwitchTokenBot TokenBot)
        {
            tokenBot = TokenBot;
            return this;
        }

        public void AddSubscriptions()
        {
            LogWriter.DebugLog("AddSubscriptions", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Adding subscriptions.");

            CreateEventSubSubscription(new StreamOnlineHandler().SubscriptionType, "1",
                new Dictionary<string, string> { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId }, { "user_id", OptionFlags.TwitchBotUserId } });

            CreateEventSubSubscription(new ChannelUpdateHandler().SubscriptionType, "2", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            CreateEventSubSubscription(new ChannelRaidHandler().SubscriptionType, "1", new() { { "to_broadcaster_user_id", OptionFlags.TwitchStreamerUserId } }, "ChannelRaidTo");
            CreateEventSubSubscription(new ChannelRaidHandler().SubscriptionType, "1", new() { { "from_broadcaster_user_id", OptionFlags.TwitchStreamerUserId } }, "ChannelRaidFrom");
            CreateEventSubSubscription(new StreamOfflineHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
        }
        public void AddConnectionSubscriptions()
        {
            LogWriter.DebugLog("AddConnectionSubscriptions", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Adding connection subscriptions.");

            CreateEventSubSubscription(
                new StreamOnlineHandler().SubscriptionType,
                "1",
                new Dictionary<string, string>
                {
                        {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                        {"user_id", OptionFlags.TwitchBotUserId }
                }
             );
        }
        public void RemoveSubscriptions()
        {
            LogWriter.DebugLog("RemoveSubscriptions", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Removing subscriptions.");

            foreach (string Key in SubscriptionIdKeys.Keys)
            {
                DeleteEventSubSubscription(Key);
            }
        }
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions, string KeyOverride = null)
        {
            void CreateSubAction()
            {
                LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(KeyOverride ?? SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, $"Adding new subscription for {SubscriptionType}.");

                    var SubResponse = tokenBot.StreamerNoScopesHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
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
        private void DeleteEventSubSubscription(string key)
        {
            try
            {
                if (SubscriptionIdKeys.ContainsKey(key))
                {
                    if (tokenBot.StreamerNoScopesHelixApi.Helix.EventSub.DeleteEventSubSubscriptionAsync(SubscriptionIdKeys[key]).Result)
                    {
                        SubscriptionIdKeys.Remove(key);
                    }
                    LogWriter.DebugLog("DeleteEventSubSubscription",
                       DebugLogTypes.TwitchStreamerNoScopesEventSubBot,
                       $"Deleted the {key} subscription.");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "DeleteEventSubSubscription");
            }
        }

        #region Subscription Events

        private Task ChannelRaid(object sender, ChannelRaidArgs args)
        {
            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                {
                    return
                    m.MessageId == args.Notification.Metadata.MessageId &&
                    m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                })
                   )
                {
                    if (args.Notification.Payload.Event.FromBroadcasterUserId == OptionFlags.TwitchStreamerUserId)
                    {
                        LogWriter.DebugLog("ChannelRaid", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Channel raid is outgoing.");
                        OutChannelRaid?.Invoke(this, new(args.Notification.Payload.Event, args.Notification.Metadata.MessageTimestamp.ToLocalTime()));
                    }
                    else
                    {
                        LogWriter.DebugLog("ChannelRaid", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Channel raid is incoming.");
                        NewChannelRaid?.Invoke(this, new(args.Notification.Payload.Event, args.Notification.Metadata.MessageTimestamp.ToLocalTime()));
                    }
                }
            });
        }
        private Task StreamOffline(object sender, StreamOfflineArgs args)
        {
            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                {
                    return
                    m.MessageId == args.Notification.Metadata.MessageId &&
                    m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                })
                )
                {
                    LogWriter.DebugLog("StreamOffline", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Stream is offline.");

                    NewStreamOffline?.Invoke(this, new(args.Notification.Payload.Event));

                    // stop the offline subscriptions that won't happen while stream is offline
                    DeleteEventSubSubscription(new ChannelRaidHandler().SubscriptionType);
                    DeleteEventSubSubscription(new StreamOfflineHandler().SubscriptionType);

                    CreateEventSubSubscription(new StreamOnlineHandler().SubscriptionType, "1", new Dictionary<string, string>
                    {
                         {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                         {"user_id", OptionFlags.TwitchBotUserId }
                    });
                }

            });
        }
        private Task ChannelUpdate(object sender, ChannelUpdateArgs args)
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
                    LogWriter.DebugLog("ChannelUpdate", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Channel has been updated.");

                    NewChannelUpdate?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }
        private Task StreamOnline(object sender, StreamOnlineArgs args)
        {
            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                {
                    return
                    m.MessageId == args.Notification.Metadata.MessageId &&
                    m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                })
                )
                {
                    LogWriter.DebugLog("StreamOnline", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Stream is online.");

                    NewStreamOnline?.Invoke(this, new(args.Notification.Payload.Event));
                    AddSubscriptions();
                    DeleteEventSubSubscription(new StreamOnlineHandler().SubscriptionType);
                }
            });
        }
        public void ClearSubscriptions()
        {
            LogWriter.DebugLog("ClearSubscriptions", DebugLogTypes.TwitchStreamerNoScopesEventSubBot, "Clearing subscriptions.");

            SubscriptionIdKeys.Clear();
        }

        #endregion

    }
}
