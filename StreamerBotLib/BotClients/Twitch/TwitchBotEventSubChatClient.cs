using Microsoft.Extensions.Logging;

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

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotEventSubChatClient : TwitchBotsBase
    {
        public event EventHandler<ChannelChatMessageEventArgs> OnChannelChatMessageReceived;

        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        public TwitchBotEventSubChatClient(ILoggerFactory loggerFactory)
        {
            BotClientName = Enums.Bots.TwitchBotEventSub;

            _eventSubWebsocketClient = new(loggerFactory); // add logger factory

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessage;
        }

        public override void StartBot()
        {
            if (OptionFlags.TwitchStreamerUseToken)
            {
                try
                {
                    try
                    {
                        if (IsActive == null || IsActive == false)
                        {
                            ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () => { StartAsync(new()); });
                            IsActive = true;
                            InvokeBotStarted();
                            MsgLogging |= IsActive == true;
                            MsgLogCleanup();
                        }
                    }
                    catch (TokenExpiredException ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                        tokenBot.CheckToken();

                        ThreadManager.CreateThreadStart(MethodBase.GetCurrentMethod().Name, () =>
                        {
                            StartAsync(new());
                        });

                        IsActive = true;
                        MsgLogging |= IsActive == true;
                        InvokeBotStarted();
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    IsActive = false;
                    InvokeBotFailedStart();
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket failed to start.");
                }
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
            }
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

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} encountered an error:\r\n" +
            $"Exception: {args.Exception}\r\n" +
            $"Message: {args.Message}");
            return new Task(() => { });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} reconnected!");
            return new Task(() => { });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} disconnected.");

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
                var conditions = new Dictionary<string, string>
                {
                    {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                    {"user_id", OptionFlags.TwitchBotUserId }
                };

                return tokenBot.BotHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    new ChatMessageHandler().SubscriptionType,
                    "1", conditions,
                    EventSubTransportMethod.Websocket,
                    _eventSubWebsocketClient.SessionId);
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
    }
}
