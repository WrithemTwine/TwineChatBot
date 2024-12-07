using Microsoft.Extensions.Logging;

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

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotEventSubChatClient : TwitchBotsBase
    {
        private IEventSubMessageIdsLogger eventSubMessageIdsLogger;
        private readonly TwitchTokenBot tokenBot;

        public event EventHandler<ChannelChatMessageEventArgs> OnChannelChatMessageReceived;

        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        internal TwitchBotEventSubChatClient(ILoggerFactory loggerFactory,
            IEventSubMessageIdsLogger messageIdsLogger, TwitchTokenBot TokenBot)
        {
            BotClientName = Enums.Bots.TwitchBotEventSub;
            tokenBot = TokenBot;
            eventSubMessageIdsLogger = messageIdsLogger;

            _eventSubWebsocketClient = new(loggerFactory); // add logger factory

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessage;
        }

        public override Task StartBot()
        {
            return Task.Run(async () =>
            {
                if (OptionFlags.TwitchStreamerUseToken)
                {
                    try
                    {
                        try
                        {
                            if (IsActive == null || IsActive == false)
                            {
                                await StartAsync(new());
                            }
                        }
                        catch (TokenExpiredException ex)
                        {
                            LogWriter.LogException(ex, "StartBot");
                            tokenBot.CheckToken();

                            await StartAsync(new());

                            IsActive = true;
                            eventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                            InvokeBotStarted();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, "StartBot");
                        IsActive = false;
                        InvokeBotFailedStart();
                        LogWriter.DebugLog("StartBot", Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket failed to start.");
                    }
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
                }
            });
        }

        private Task OnChannelChatMessage(object sender, ChannelChatMessageArgs args)
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
                    ChannelChatMessage msg = args.Notification.Payload.Event;
                    OnChannelChatMessageReceived?.Invoke(this, new(msg));
                }
            });
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnErrorOccurred", Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} encountered an error:\r\n" +
                 $"Exception: {args.Exception}\r\n" +
                 $"Message: {args.Message}");
            });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnWebsocketReconnected", Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} reconnected!");
            });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("OnWebsocketDisconnected", Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} disconnected.");

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
            return Task.Run(async () =>
            {
                if (!args.IsRequestedReconnect)
                {
                    var conditions = new Dictionary<string, string>
                {
                    {"broadcaster_user_id", OptionFlags.TwitchStreamerUserId },
                    {"user_id", OptionFlags.TwitchBotUserId }
                };

                    if (IsActive != true)
                    {
                        IsActive = true;
                        InvokeBotStarted();
                        eventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                        eventSubMessageIdsLogger.MsgLogCleanup();
                    }

                    await tokenBot.BotHelixApi.Helix.EventSub.CreateEventSubSubscriptionAsync(
                        new ChatMessageHandler().SubscriptionType,
                        "1", conditions,
                        EventSubTransportMethod.Websocket,
                        _eventSubWebsocketClient.SessionId);
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
    }
}
