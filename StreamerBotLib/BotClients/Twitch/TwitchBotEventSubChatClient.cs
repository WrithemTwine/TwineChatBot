using Microsoft.Extensions.Hosting;
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

        private readonly ILogger<TwitchBotEventSubChatClient> _logger;
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        public TwitchBotEventSubChatClient()
        {
            BotClientName = Enums.Bots.TwitchBotEventSub;

            _eventSubWebsocketClient = new(); // add logger factory

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelChatMessage += OnChannelChatMessage;
        }

        public override void StartBot()
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
            return new Task(() =>
            {
                ChannelChatMessage msg = args.Notification.Payload.Event;
                OnChannelChatMessageReceived?.Invoke(this, new(msg));
            });
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            return new Task(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} encountered an error:\r\n" +
                $"Exception: {args.Exception}\r\n" +
                $"Message: {args.Message}");
            });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            return new Task(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} reconnected!");
            });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            return new Task(() =>
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, Enums.DebugLogTypes.TwitchBotEventSubBot, $"Websocket session {_eventSubWebsocketClient.SessionId} disconnected.");

            });
        }

        private Task OnWebsocketConnected(object sender, WebsocketConnectedArgs args)
        {
            if(!args.IsRequestedReconnect)
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
