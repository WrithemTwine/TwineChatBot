using Microsoft.Extensions.Logging;

using StreamerBotLib.BotClients.Twitch.TwitchLib.Events.EventSub;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Static;

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
        private IEventSubMessageIdsLogger eventSubMessageIdsLogger;
        private readonly TwitchTokenBot tokenBot;

        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

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

        internal TwitchStreamerEventSubBotNoScopes(ILoggerFactory loggerFactory,
            IEventSubMessageIdsLogger messageIdsLogger,
            TwitchTokenBot TokenBot)
        {
            BotClientName = Enums.Bots.TwitchStreamerEventSubNoScopes;
            tokenBot = TokenBot;
            eventSubMessageIdsLogger = messageIdsLogger;

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
            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                     {
                         return
                         m.MessageId == args.Notification.Metadata.MessageId &&
                         m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                     })
                   )
                {
                    if (args.Notification.Payload.Event.FromBroadcasterUserId == OptionFlags.TwitchStreamerUserId)
                    {
                        OutChannelRaid?.Invoke(this, new(args.Notification.Payload.Event, args.Notification.Metadata.MessageTimestamp.ToLocalTime()));
                    }
                    else
                    {
                        NewChannelRaid?.Invoke(this, new(args.Notification.Payload.Event, args.Notification.Metadata.MessageTimestamp.ToLocalTime()));
                    }
                }
            });
        }
        private Task StreamOffline(object sender, StreamOfflineArgs args)
        {
            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                      {
                          return
                          m.MessageId == args.Notification.Metadata.MessageId &&
                          m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                      })
                )
                {
                    NewStreamOffline?.Invoke(this, new(args.Notification.Payload.Event));

                    ThreadManager.CreateThreadStart("StreamOffline", () =>
                    { // stop the offline subscriptions that won't happen while stream is offline
                        DeleteEventSubSubscription(new ChannelRaidHandler().SubscriptionType);
                        DeleteEventSubSubscription(new StreamOfflineHandler().SubscriptionType);
                    });
                }

            });
        }
        private Task ChannelUpdate(object sender, ChannelUpdateArgs args)
        {
            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
             {
                 return
                 m.MessageId == args.Notification.Metadata.MessageId &&
                 m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
             }))
                {
                    NewChannelUpdate?.Invoke(this, new(args.Notification.Payload.Event));
                }
            });
        }
        private Task StreamOnline(object sender, StreamOnlineArgs args)
        {
            return Task.Run(() =>
            {
                if (eventSubMessageIdsLogger.AddMessageId(args.Notification.Metadata, (m) =>
                    {
                        return
                        m.MessageId == args.Notification.Metadata.MessageId &&
                        m.SubscriptionType == args.Notification.Metadata.SubscriptionType;
                    })
                )
                {
                    NewStreamOnline?.Invoke(this, new(args.Notification.Payload.Event));

                    StartMoreServices();
                }
            });
        }

        #endregion

        /// <summary>
        /// Bot only needs to listen when stream goes online, then once online, 
        /// turn on the raid & stream offline subscriptions.
        /// </summary>
        private void StartMoreServices()
        {
            ThreadManager.CreateThreadStart("StartMoreServices", () =>
            {
                CreateEventSubSubscription(new ChannelUpdateHandler().SubscriptionType, "2", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelRaidHandler().SubscriptionType, "1", new() { { "to_broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelRaidHandler().SubscriptionType, "1", new() { { "from_broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new StreamOfflineHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
            });
        }

        public void StreamAlreadyOnlineStartServices()
        {
            StartMoreServices();
        }

        public override Task StartBot()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (IsActive is null or false)
                    {
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

        public override Task StopBot()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (IsActive == true)
                    {
                        await StopAsync(new());
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
            return Task.Run(() =>
            {
                if (!args.IsRequestedReconnect)
                {
                    ThreadManager.CreateThreadStart("OnWebsocketConnected", () =>
                    {
                        CreateEventSubSubscription(new StreamOnlineHandler().SubscriptionType, "1", new Dictionary<string, string>
                        {
                        {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                        {"user_id", OptionFlags.TwitchBotUserId }
                        });
                    });

                    if (IsActive != true)
                    {
                        IsActive = true;
                        InvokeBotStarted();
                        eventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                        eventSubMessageIdsLogger.MsgLogCleanup();
                    }
                }
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.DisconnectAsync();
        }

        #region Edit Subscriptions
        private void CreateEventSubSubscription(string SubscriptionType, string Version, Dictionary<string, string> conditions)
        {
            void CreateSubAction()
            {
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
    }
}
