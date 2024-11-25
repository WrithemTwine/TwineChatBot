using Microsoft.Extensions.Logging;

using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Stream;
using TwitchLib.EventSub.Websockets.Handler.Channel;
using TwitchLib.EventSub.Websockets.Handler.Channel.Raids;
using TwitchLib.EventSub.Websockets.Handler.Stream;

namespace StreamerBotLib.BotClients.Twitch
{
    /// <summary>
    /// Event Sub bot using the Twitch Streamer client Id but requires no access scopes: stream online, stream offline, raid
    /// </summary>
    public class TwitchStreamerEventSubBotNoScopes : TwitchBotsBase
    {
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        /// <summary>
        /// maintains the ID list for the current subscriptions, removed when deleting the subscription
        /// Key: subscription type name, e.g. channel.chat.message
        /// Value: the subscription ID from creating the subscription
        /// </summary>
        private readonly Dictionary<string, string> SubscriptionIdKeys = [];

        public event EventHandler<NewChannelRaidEventArgs> NewChannelRaid;
        public event EventHandler<NewStreamOfflineEventArgs> NewStreamOffline;
        public event EventHandler<NewStreamOnlineEventArgs> NewStreamOnline;
        public event EventHandler<NewChannelUpdateEventArgs> NewChannelUpdate;

        public event EventHandler OnNewLiveStreamStarted;

        public TwitchStreamerEventSubBotNoScopes(ILoggerFactory loggerFactory)
        {
            BotClientName = Enums.Bots.TwitchStreamerEventSubNoScopes;

            _eventSubWebsocketClient = new(loggerFactory);

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.StreamOnline += StreamOnline;
            _eventSubWebsocketClient.StreamOffline += StreamOffline;
            _eventSubWebsocketClient.ChannelRaid += ChannelRaid;
            _eventSubWebsocketClient.ChannelUpdate += ChannelUpdate;

        }

        #region Subscription Events

        private Task ChannelRaid(object sender, ChannelRaidArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelRaid?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }
        private Task StreamOffline(object sender, StreamOfflineArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewStreamOffline?.Invoke(this, new(args.Notification.Payload.Event));

                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                { // stop the offline subscriptions that won't happen while stream is offline
                    DeleteEventSubSubscription(new ChannelRaidHandler().SubscriptionType);
                    DeleteEventSubSubscription(new StreamOfflineHandler().SubscriptionType);
                });
            }

            return new Task(() => { });
        }
        private Task ChannelUpdate(object sender, ChannelUpdateArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewChannelUpdate?.Invoke(this, new(args.Notification.Payload.Event));
            }
            return new Task(() => { });
        }
        private Task StreamOnline(object sender, StreamOnlineArgs args)
        {
            if (MessageIdLog.UniqueAdd(args.Notification.Metadata, (m) =>
            {
                return
                m.MessageId == args.Notification.Metadata.MessageId &&
                m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
            }))
            {
                NewStreamOnline?.Invoke(this, new(args.Notification.Payload.Event));

                StartMoreServices();
            }
            return new Task(() => { });
        }

        #endregion

        /// <summary>
        /// Bot only needs to listen when stream goes online, then once online, 
        /// turn on the raid & stream offline subscriptions.
        /// </summary>
        private void StartMoreServices()
        {
            ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
            {
                CreateEventSubSubscription(new ChannelUpdateHandler().SubscriptionType, "2", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelRaidHandler().SubscriptionType, "1", new() { { "to_broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new StreamOfflineHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            });
        }

        public void StreamAlreadyOnlineStartServices()
        {
            StartMoreServices();
        }

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

        public override void StopBot()
        {
            try
            {
                if (IsActive == true)
                {
                    ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                    {
                        StopAsync(new());
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
                CreateEventSubSubscription(new StreamOnlineHandler().SubscriptionType, "1", new Dictionary<string, string>
                {
                    {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                    {"user_id", OptionFlags.TwitchBotUserId }
                });
            }
            return new Task(() => { });
        }

        public void StartAsync(CancellationToken cancellationToken)
        {
            _eventSubWebsocketClient.ConnectAsync();
        }

        public void StopAsync(CancellationToken cancellationToken)
        {
            _eventSubWebsocketClient.DisconnectAsync();
        }

        #region Edit Subscriptions
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions)
        {
            Action CreateSubAction = () =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Requesting new subscription for {SubscriptionType}.");
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType) && _eventSubWebsocketClient.SessionId != null)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Adding new subscription for {SubscriptionType}.");
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"New {SubscriptionType} subscription added. Current EventSub cost is {SubResponse.Cost} with a {SubResponse.Status} status.");

                    SubscriptionIdKeys.Add(SubResponse.Type, SubResponse.Id);
                }
            };

            try
            {
                CreateSubAction.Invoke();
            }
            catch (BadTokenException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                tokenBot.CheckToken();
                CreateSubAction.Invoke();
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
    }
}
