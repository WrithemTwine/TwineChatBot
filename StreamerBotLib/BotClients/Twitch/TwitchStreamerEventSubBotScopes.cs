using Microsoft.Extensions.Logging;

using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Events;
using StreamerBotLib.Static;

using System.Reflection;

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

        public TwitchStreamerEventSubBotScopes(ILoggerFactory loggerFactory)
        {
            BotClientName = Enums.Bots.TwitchStreamerEventSubScopes;

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
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelCustomRewardRedemption?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }

        private Task ChannelSubscriptionMessage(object sender, ChannelSubscriptionMessageArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelSubscriptionMessage?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }

        private Task ChannelSubscribe(object sender, ChannelSubscribeArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelSubscribe?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }

        private Task ChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelSubscriptionGift?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }

        private Task ChannelCheer(object sender, ChannelCheerArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelCheer?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }

        private Task ChannelFollow(object sender, ChannelFollowArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelFollow?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }

        private Task OnChannelChatMessage(object sender, ChannelChatMessageArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                ChannelChatMessage msg = args.Notification.Payload.Event;
                OnChannelChatMessageReceived?.Invoke(this, new(msg));
            }
            return new Task(() => { });
        }

        #region Edit Subscriptions
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions)
        {
            /*
            //void CreateSubAction()
            //{
            //    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
            //    if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
            //    {
            //        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
            //        var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
            //        SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
            //        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

            //        SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
            //    }
            //}
            */

            try
            {
                //CreateSubAction();

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

                    SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
                }
            }
            catch (BadTokenException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                tokenBot.CheckToken();
                //CreateSubAction();

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

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
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name,
                       Enums.DebugLogTypes.TwitchStreamerEventSubBot,
                       $"Deleted the {key} subscription.");
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        #endregion
        #endregion

        public override void StartBot()
        {
            try
            {
                if (IsActive == null || IsActive == false)
                {
                    ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                    {
                        StartAsync(new());
                        InvokeBotStarted();
                    });
                    IsActive = true;
                    MsgLogging |= IsActive == true;
                    MsgLogCleanup();
                }
            }
            catch (BadScopeException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                tokenBot.CheckToken();
                InvokeBotFailedStart();
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket failed to start.");
            }
        }

        public void StartAsync(CancellationToken cancellationToken)
        {
            _eventSubWebsocketClient.ConnectAsync();
        }

        private void StartServices()
        {
            ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
            {
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

        public override void StopBot()
        {
            try
            {
                if (IsActive == true)
                {
                    ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                    {
                        StopAsync(new());
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

                    });
                    IsActive = false;
                    InvokeBotStopped();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket failed to stop.");

            }
        }

        public void StopAsync(CancellationToken cancellationToken)
        {
            _eventSubWebsocketClient.DisconnectAsync();
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} encountered an error:\r\n" +
            $"Exception: {args.Exception}\r\n" +
            $"Message: {args.Message}");
            return new Task(() => { });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} reconnected!");
            return new Task(() => { });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} disconnected.");

            if (IsActive == true)
            {
                Thread.Sleep(1000);
                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                {
                    StartAsync(new());
                });
            }
            else
            {
                StopBot();
            }

            return new Task(() => { });
        }

        private Task OnWebsocketConnected(object sender, WebsocketConnectedArgs args)
        {
            if (!args.IsRequestedReconnect)
            {
                StartServices();
            }
            return new Task(() => { });
        }

    }
}
