using Microsoft.Extensions.Logging;

using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets.Handler.Channel;
using TwitchLib.EventSub.Websockets.Handler.Channel.ChannelPoints.Redemptions;
using TwitchLib.EventSub.Websockets.Handler.Channel.Cheers;
using TwitchLib.EventSub.Websockets.Handler.Channel.Follows;
using TwitchLib.EventSub.Websockets.Handler.Channel.Subscription;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchStreamerEventSubBotScopes : TwitchBotsBase
    {
        private IEventSubMessageIdsLogger eventSubMessageIdsLogger;
        private TwitchTokenBot tokenBot;

        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

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

        internal TwitchStreamerEventSubBotScopes(ILoggerFactory loggerFactory,
            IEventSubMessageIdsLogger messageIdsLogger, TwitchTokenBot TokenBot)
        {
            LogWriter.DebugLog(".ctor_TwitchStreamerEventSubBotScopes", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Building the TwitchStreamerEventSubBotScopes object.");

            BotClientName = Enums.Bots.TwitchStreamerEventSubScopes;
            tokenBot = TokenBot;
            eventSubMessageIdsLogger = messageIdsLogger;

            LogWriter.DebugLog(".ctor_TwitchStreamerEventSubBotScopes", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Attaching events to handle bot actions.");

            _eventSubWebsocketClient = new(loggerFactory);

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelFollow += ChannelFollow;
            _eventSubWebsocketClient.ChannelCheer += ChannelCheer;
            _eventSubWebsocketClient.ChannelSubscriptionGift += ChannelSubscriptionGift;
            _eventSubWebsocketClient.ChannelSubscribe += ChannelSubscribe;
            _eventSubWebsocketClient.ChannelSubscriptionMessage += ChannelSubscriptionMessage;
            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += ChannelPointsCustomRewardRedemptionAdd;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessage;
        }

        #region Subscription Events
        private Task ChannelPointsCustomRewardRedemptionAdd(object sender, ChannelPointsCustomRewardRedemptionArgs args)
        {
            LogWriter.DebugLog("ChannelPointsCustomRewardRedemptionAdd", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelPointsCustomRewardRedemption event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                {
                    return
                    m.MessageId == args.Notification.Metadata.MessageId &&
                    m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                }))
                {
                    NewChannelCustomRewardRedemption?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }

        private Task ChannelSubscriptionMessage(object sender, ChannelSubscriptionMessageArgs args)
        {
            LogWriter.DebugLog("ChannelSubscriptionMessage", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelSubscriptionMessage event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                {
                    return
                    m.MessageId == args.Notification.Metadata.MessageId &&
                    m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                }))
                {
                    NewChannelSubscriptionMessage?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }

        private Task ChannelSubscribe(object sender, ChannelSubscribeArgs args)
        {
            LogWriter.DebugLog("ChannelSubscribe", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelSubscribe event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
                {
                    NewChannelSubscribe?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }

        private Task ChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            LogWriter.DebugLog("ChannelSubscriptionGift", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelSubscriptionGift event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
             {
                 return
                 m.MessageId == args.Notification.Metadata.MessageId &&
                 m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
             }))
                {
                    NewChannelSubscriptionGift?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }

        private Task ChannelCheer(object sender, ChannelCheerArgs args)
        {
            LogWriter.DebugLog("ChannelCheer", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelCheer event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
                {
                    NewChannelCheer?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }

        private Task ChannelFollow(object sender, ChannelFollowArgs args)
        {
            LogWriter.DebugLog("ChannelFollow", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing ChannelFollow event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
             {
                 return
                 m.MessageId == args.Notification.Metadata.MessageId &&
                 m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
             }))
                {
                    NewChannelFollow?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }

        private Task OnChannelChatMessage(object sender, ChannelChatMessageArgs args)
        {
            LogWriter.DebugLog("OnChannelChatMessage", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Processing OnChannelChatMessage event.");

            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
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

        #region Edit Subscriptions
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions)
        {
            /*
            //void CreateSubAction()
            //{
            //    LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
            //    if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
            //    {
            //        LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
            //        var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
            //        SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
            //        LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

            //        SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
            //    }
            //}
            */

            try
            {
                //CreateSubAction();

                LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
                    LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

                    SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
                }
            }
            catch (BadTokenException ex)
            {
                LogWriter.LogException(ex, "CreateEventSubSubscription");
                tokenBot.CheckToken();
                //CreateSubAction();

                LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
                    LogWriter.DebugLog("CreateEventSubSubscription", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

                    SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
                }
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
                       Enums.DebugLogTypes.TwitchStreamerEventSubBot,
                       $"Deleted the {key} subscription.");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "DeleteEventSubSubscription");
            }
        }

        #endregion
        #endregion

        public override Task StartBot()
        {
            return Task.Run(async () =>
            {
                try
                {
                    LogWriter.DebugLog("StartBot", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Attempting to start bot.");

                    if (IsActive == null || IsActive == false)
                    {
                        LogWriter.DebugLog("StartBot", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Bot inactive, starting bot.");

                        await StartAsync(new());
                    }
                }
                catch (BadScopeException ex)
                {
                    LogWriter.LogException(ex, "StartBot");
                    tokenBot.CheckToken();
                    InvokeBotFailedStart();
                    LogWriter.DebugLog("StartBot", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket failed to start.");
                }
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            LogWriter.DebugLog("StartAsync", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Connecting EventSub websocket.");
            await _eventSubWebsocketClient.ConnectAsync();
        }

        private void StartServices()
        {
            ThreadManager.CreateThreadStart("StartServices", () =>
            {
                LogWriter.DebugLog("StartServices", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Starting EventSub subscription services.");

                CreateEventSubSubscription(new ChannelPointsCustomRewardRedemptionAddHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelSubscriptionMessageHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelSubscribeHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelSubscriptionGiftHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelCheerHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelFollowHandler().SubscriptionType, "2", new Dictionary<string, string> { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId }, { "moderator_user_id", OptionFlags.TwitchStreamerUserId } });

                if (!OptionFlags.TwitchStreamerUseToken)
                {
                    CreateEventSubSubscription(new ChatMessageHandler().SubscriptionType, "1", new Dictionary<string, string> { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId }, { "user_id", OptionFlags.TwitchStreamerUserId } });
                    OnChannelChatMessageStarted?.Invoke(this, new());
                }
            });
        }

        public override Task StopBot()
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("StopBot", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Attempting to stop bot.");

                try
                {
                    if (IsActive == true)
                    {
                        LogWriter.DebugLog("StopBot", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Bot active, now stopping bot.");

                        await StopAsync(new());
                        if (!OptionFlags.TwitchStreamerUseToken)
                        {
                            OnChannelChatMessageStopping?.Invoke(this, new());
                        }

                        DeleteEventSubSubscription(new ChannelPointsCustomRewardRedemptionAddHandler().SubscriptionType);
                        DeleteEventSubSubscription(new ChannelSubscriptionMessageHandler().SubscriptionType);
                        DeleteEventSubSubscription(new ChannelSubscribeHandler().SubscriptionType);
                        DeleteEventSubSubscription(new ChannelSubscriptionGiftHandler().SubscriptionType);
                        DeleteEventSubSubscription(new ChannelCheerHandler().SubscriptionType);
                        DeleteEventSubSubscription(new ChannelFollowHandler().SubscriptionType);

                        if (!OptionFlags.TwitchStreamerUseToken)
                        {
                            OnChannelChatMessageStopped?.Invoke(this, new());
                        }

                        IsActive = false;
                        InvokeBotStopped();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "StopBot");
                    LogWriter.DebugLog("StopBot", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket failed to stop.");

                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            LogWriter.DebugLog("StopAsync", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Disconnecting websocket.");
            await _eventSubWebsocketClient.DisconnectAsync();
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnErrorOccurred", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} encountered an error:\r\n" +
             $"Exception: {args.Exception}\r\n" +
             $"Message: {args.Message}");
            });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnWebsocketReconnected", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} reconnected!");
            });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("OnWebsocketDisconnected", Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} disconnected.");

                if (IsActive == true)
                {
                    Thread.Sleep(1000);
                    await StartAsync(new());
                }
                else
                {
                    await StopBot();
                }

            });
        }

        private Task OnWebsocketConnected(object sender, WebsocketConnectedArgs args)
        {
            LogWriter.DebugLog("OnWebsocketConnected", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Websocket connected.");
            return Task.Run(() =>
            {
                if (!args.IsRequestedReconnect)
                {
                    LogWriter.DebugLog("OnWebsocketConnected", Enums.DebugLogTypes.TwitchStreamerEventSubBot, "Initiating services.");
                    StartServices();
                }

                if (IsActive != true)
                {
                    IsActive = true;
                    InvokeBotStarted();
                    eventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                    eventSubMessageIdsLogger.MsgLogCleanup();
                }
            });
        }

    }
}
