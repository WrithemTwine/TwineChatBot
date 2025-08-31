using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
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
    public class TwitchStreamerEventSubBotScopes : ITwitchBotEventSubSubscriptions
    {
        public BotType CurrBot => BotType.StreamerAccount;

        private IEventSubMessageIdsLogger EventSubMessageIdsLogger;
        private EventSubWebsocketClient _eventSubWebsocketClient;
        private static TwitchTokenBot tokenBot;

        /// <summary>
        /// maintains the ID list for the current subscriptions, removed when deleting the subscription
        /// Key: subscription type name, e.g. channel.chat.message
        /// Value: the subscription ID from creating the subscription
        /// </summary>
        private readonly Dictionary<string, string> SubscriptionIdKeys = [];

        public event EventHandler<NewChannelCustomRewardRedemptionEventArgs> NewChannelCustomRewardRedemption;
        public event EventHandler<NewChannelSubscriptionMessageEventArgs> NewChannelSubscriptionMessage;
        public event EventHandler<NewChannelSubscribeEventArgs> NewChannelSubscribe;
        public event EventHandler<NewChannelSubscriptionGiftEventArgs> NewChannelSubscriptionGift;
        public event EventHandler<NewChannelCheerEventArgs> NewChannelCheer;
        public event EventHandler<NewChannelFollowEventArgs> NewChannelFollow;

        public event EventHandler<ChannelChatMessageEventArgs> OnChannelChatMessageReceived;

        public event EventHandler OnNewLiveStreamStarted;

        // handles the case when the bot account and streamer account are the same and this bot 
        // sets up the channelchatmessage subscription
        public event EventHandler OnChannelChatMessageStarted;
        public event EventHandler OnChannelChatMessageStopping;
        public event EventHandler OnChannelChatMessageStopped;

        // /////////////////////////////////////////////////////////////////////////////////////
        ITwitchBotEventSubSubscriptions ITwitchBotEventSubSubscriptions.ConfigureMessageLogger(IEventSubMessageIdsLogger eventSubMessageIdsLogger)
        {
            EventSubMessageIdsLogger = eventSubMessageIdsLogger;
            return this;
        }

        ITwitchBotEventSubSubscriptions ITwitchBotEventSubSubscriptions.AddEventHandlers(EventSubWebsocketClient EventSubClient)
        {
            _eventSubWebsocketClient = EventSubClient;

            _eventSubWebsocketClient.ChannelFollow += ChannelFollow;
            _eventSubWebsocketClient.ChannelCheer += ChannelCheer;
            _eventSubWebsocketClient.ChannelSubscriptionGift += ChannelSubscriptionGift;
            _eventSubWebsocketClient.ChannelSubscribe += ChannelSubscribe;
            _eventSubWebsocketClient.ChannelSubscriptionMessage += ChannelSubscriptionMessage;
            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += ChannelPointsCustomRewardRedemptionAdd;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessage;

            return this;
        }

        ITwitchBotEventSubSubscriptions ITwitchBotEventSubSubscriptions.ConfigureTokenBot(TwitchTokenBot TokenBot)
        {
            tokenBot = TokenBot;
            return this;
        }

        void ITwitchBotEventSubSubscriptions.AddSubscriptions()
        {
            LogWriter.DebugLog("AddSubscriptions", DebugLogTypes.TwitchStreamerEventSubBot, "Adding all subscriptions.");

            CreateEventSubSubscription("channel.channel_points_custom_reward_redemption.add", "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            CreateEventSubSubscription("channel.subscription.message", "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            CreateEventSubSubscription("channel.subscribe", "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            CreateEventSubSubscription("channel.subscription.gift", "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            CreateEventSubSubscription("channel.cheer", "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            CreateEventSubSubscription("channel.follow", "2", new Dictionary<string, string> { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId }, { "moderator_user_id", OptionFlags.TwitchStreamerUserId } });

            if (!OptionFlags.TwitchStreamerUseToken)
            {
                CreateEventSubSubscription("channel.chat.message", "1", new Dictionary<string, string> { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId }, { "user_id", OptionFlags.TwitchStreamerUserId } });
                OnChannelChatMessageStarted?.Invoke(this, new());
            }
        }

        public void AddConnectionSubscriptions()
        {
        }

        void ITwitchBotEventSubSubscriptions.RemoveSubscriptions()
        {
            LogWriter.DebugLog("RemoveSubscriptions", DebugLogTypes.TwitchStreamerEventSubBot, "Deleting all subscriptions.");

            DeleteEventSubSubscription("channel.channel_points_custom_reward_redemption.add");
            DeleteEventSubSubscription("channel.subscription.message");
            DeleteEventSubSubscription("channel.subscribe");
            DeleteEventSubSubscription("channel.subscription.gift");
            DeleteEventSubSubscription("channel.cheer");
            DeleteEventSubSubscription("channel.follow");

            if (!OptionFlags.TwitchStreamerUseToken)
            {
                DeleteEventSubSubscription("channel.chat.message");
            }
        }

        #region Subscription Events
        private Task ChannelPointsCustomRewardRedemptionAdd(object sender, ChannelPointsCustomRewardRedemptionArgs args)
        {
            LogWriter.DebugLog("ChannelPointsCustomRewardRedemptionAdd", DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelPointsCustomRewardRedemption event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    NewChannelCustomRewardRedemption?.Invoke(this, new(args.Payload.Event));
                }
            });
        }

        private Task ChannelSubscriptionMessage(object sender, ChannelSubscriptionMessageArgs args)
        {
            LogWriter.DebugLog("ChannelSubscriptionMessage", DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelSubscriptionMessage event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    NewChannelSubscriptionMessage?.Invoke(this, new(args.Payload.Event));
                }
            });
        }

        private Task ChannelSubscribe(object sender, ChannelSubscribeArgs args)
        {
            LogWriter.DebugLog("ChannelSubscribe", DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelSubscribe event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    NewChannelSubscribe?.Invoke(this, new(args.Payload.Event));
                }
            });
        }

        private Task ChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            LogWriter.DebugLog("ChannelSubscriptionGift", DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelSubscriptionGift event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    NewChannelSubscriptionGift?.Invoke(this, new(args.Payload.Event));
                }
            });
        }

        private Task ChannelCheer(object sender, ChannelCheerArgs args)
        {
            LogWriter.DebugLog("ChannelCheer", DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelCheer event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    NewChannelCheer?.Invoke(this, new(args.Payload.Event));
                }
            });
        }

        private Task ChannelFollow(object sender, ChannelFollowArgs args)
        {
            LogWriter.DebugLog("ChannelFollow", DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelFollow event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    NewChannelFollow?.Invoke(this, new(args.Payload.Event));
                }
            });
        }

        private Task OnChannelChatMessage(object sender, ChannelChatMessageArgs args)
        {
            LogWriter.DebugLog("OnChannelChatMessage", DebugLogTypes.TwitchStreamerEventSubBot, "Processing OnChannelChatMessage event.");

            return Task.Run(() =>
            {
                if (EventSubMessageIdsLogger.AddMessageId(((WebsocketEventSubMetadata)args.Metadata), (m) =>
                {
                    return
                    m.MessageId == ((WebsocketEventSubMetadata)args.Metadata).MessageId &&
                    m.SubscriptionType == ((WebsocketEventSubMetadata)args.Metadata).SubscriptionType;
                }))
                {
                    ChannelChatMessage msg = args.Payload.Event;
                    OnChannelChatMessageReceived?.Invoke(this, new(msg));
                }
            });
        }

        #region Edit Subscriptions
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions)
        {
            void CreateSubAction()
            {
                LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
                    LogWriter.DebugLog("CreateEventSubSubscription", DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

                    SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
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
                    if (tokenBot.StreamerHelixApi.Helix.EventSub.DeleteEventSubSubscriptionAsync(SubscriptionIdKeys[key]).Result)
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
            LogWriter.DebugLog("ClearSubscriptions", DebugLogTypes.TwitchStreamerEventSubBot, "Clearing all subscriptions.");

            SubscriptionIdKeys.Clear();
        }


        #endregion
        #endregion

    }
}
