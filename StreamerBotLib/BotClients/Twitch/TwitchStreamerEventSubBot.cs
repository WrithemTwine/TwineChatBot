using Microsoft.Extensions.Hosting;
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
using TwitchLib.EventSub.Websockets.Handler.Channel.ChannelPoints.Redemptions;
using TwitchLib.EventSub.Websockets.Handler.Channel.Cheers;
using TwitchLib.EventSub.Websockets.Handler.Channel.Follows;
using TwitchLib.EventSub.Websockets.Handler.Channel.Raids;
using TwitchLib.EventSub.Websockets.Handler.Channel.Subscription;
using TwitchLib.EventSub.Websockets.Handler.Stream;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchStreamerEventSubBot : TwitchBotsBase
    {
        private readonly ILogger<TwitchBotEventSubChatClient> _logger;
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
        public event EventHandler<NewChannelRaidEventArgs> NewChannelRaid;
        public event EventHandler<NewChannelCheerEventArgs> NewChannelCheer;
        public event EventHandler<NewChannelFollowEventArgs> NewChannelFollow;
        public event EventHandler<NewChannelUpdateEventArgs> NewChannelUpdate;
        public event EventHandler<NewStreamOfflineEventArgs> NewStreamOffline;
        public event EventHandler<NewStreamOnlineEventArgs> NewStreamOnline;

        public TwitchStreamerEventSubBot()
        {
            BotClientName = Enums.Bots.TwitchStreamerEventSub;

            _eventSubWebsocketClient = new(); // TODO: add logger factory

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.StreamOnline += StreamOnline;
            _eventSubWebsocketClient.StreamOffline += StreamOffline;
            _eventSubWebsocketClient.ChannelUpdate += ChannelUpdate;
            _eventSubWebsocketClient.ChannelFollow += ChannelFollow;
            _eventSubWebsocketClient.ChannelCheer += ChannelCheer;
            _eventSubWebsocketClient.ChannelRaid += ChannelRaid;
            _eventSubWebsocketClient.ChannelSubscriptionGift += ChannelSubscriptionGift;
            _eventSubWebsocketClient.ChannelSubscribe += ChannelSubscribe;
            _eventSubWebsocketClient.ChannelSubscriptionMessage += ChannelSubscriptionMessage;
            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += ChannelPointsCustomRewardRedemptionAdd;
        }

        private Task ChannelPointsCustomRewardRedemptionAdd(object sender, ChannelPointsCustomRewardRedemptionArgs args)
        {
            return new Task(() =>
            {
                NewChannelCustomRewardRedemption?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelSubscriptionMessage(object sender, ChannelSubscriptionMessageArgs args)
        {
            return new Task(() =>
            {
                NewChannelSubscriptionMessage?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelSubscribe(object sender, ChannelSubscribeArgs args)
        {
            return new Task(() =>
            {
                NewChannelSubscribe?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelSubscriptionGift(object sender, ChannelSubscriptionGiftArgs args)
        {
            return new Task(() =>
            {
                NewChannelSubscriptionGift?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelRaid(object sender, ChannelRaidArgs args)
        {
            return new Task(() =>
            {
                NewChannelRaid?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelCheer(object sender, ChannelCheerArgs args)
        {
            return new Task(() =>
            {
                NewChannelCheer?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelFollow(object sender, ChannelFollowArgs args)
        {
            return new Task(() =>
            {
                NewChannelFollow?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task ChannelUpdate(object sender, ChannelUpdateArgs args)
        {
            return new Task(() =>
            {
                NewChannelUpdate?.Invoke(this, new(args.Notification.Payload.Event));
            });
        }

        private Task StreamOffline(object sender, StreamOfflineArgs args)
        {
            return new Task(() =>
            {
                NewStreamOffline?.Invoke(this, new(args.Notification.Payload.Event));

                ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                {
                    DeleteEventSubSubscription(new ChannelPointsCustomRewardRedemptionAddHandler().SubscriptionType);
                    DeleteEventSubSubscription(new ChannelSubscriptionMessageHandler().SubscriptionType);
                    DeleteEventSubSubscription(new ChannelSubscribeHandler().SubscriptionType);
                    DeleteEventSubSubscription(new ChannelSubscriptionGiftHandler().SubscriptionType);
                    DeleteEventSubSubscription(new ChannelRaidHandler().SubscriptionType);
                    DeleteEventSubSubscription(new ChannelCheerHandler().SubscriptionType);
                    DeleteEventSubSubscription(new ChannelUpdateHandler().SubscriptionType);
                    DeleteEventSubSubscription(new StreamOfflineHandler().SubscriptionType);
                });

            });
        }

        private Task StreamOnline(object sender, StreamOnlineArgs args)
        {
            return new Task(() =>
            {
                NewStreamOnline?.Invoke(this, new(args.Notification.Payload.Event));

                StartMoreServices();
            });
        }

        private void StartMoreServices()
        {
            ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
            {
                CreateEventSubSubscription(new ChannelPointsCustomRewardRedemptionAddHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelSubscriptionMessageHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelSubscribeHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelSubscriptionGiftHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelRaidHandler().SubscriptionType, "1", new() { { "to_broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelCheerHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
                CreateEventSubSubscription(new ChannelUpdateHandler().SubscriptionType, "1", new() { { "broadcaster_user_id", OptionFlags.TwitchStreamerUserId } });
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
                    });
                    IsActive = true;
                    InvokeBotStarted();
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
            return new Task(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} encountered an error:\r\n" +
                $"Exception: {args.Exception}\r\n" +
                $"Message: {args.Message}");
            });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            return new Task(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} reconnected!");
            });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            return new Task(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchStreamerEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} disconnected.");

            });
        }

        private Task OnWebsocketConnected(object sender, WebsocketConnectedArgs args)
        {
            if (!args.IsRequestedReconnect)
            {
                return new Task(() =>
                {
                    CreateEventSubSubscription(new StreamOnlineHandler().SubscriptionType, "1", new Dictionary<string, string>
                    {
                        {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                        {"user_id", OptionFlags.TwitchBotUserId }
                    });

                    CreateEventSubSubscription(new ChannelFollowHandler().SubscriptionType, "1", new Dictionary<string, string>
                    {
                        {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                        {"moderator_id", OptionFlags.TwitchBotUserId }
                    });
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
                if (!SubscriptionIdKeys.ContainsKey(SubscriptionType))
                {
                    var SubResponse = tokenBot.StreamerHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    SubscriptionType, Version, conditions, EventSubTransportMethod.Websocket, _eventSubWebsocketClient.SessionId).Result.Subscriptions[0];

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
