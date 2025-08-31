#define BROKEN_EVENTSUB

using Microsoft.Extensions.Logging;

using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Static.Logger;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchEventSub : TwitchBotsBase
    {
        private IEventSubMessageIdsLogger _EventSubMessageIdsLogger;
        private EventSubWebsocketClient _EventSubWebsocketClient;
        private readonly TwitchTokenBot tokenBot;

        private List<ITwitchBotEventSubSubscriptions> SubscriptionHandlers = [];

        internal event EventHandler OnInitialBotStartupSubHandlers;

        private bool ErrorFound { get; set; } = false;

#if BROKEN_EVENTSUB
        internal TwitchEventSub(TwitchTokenBot TokenBot, Bots ClientName)
        {
            BotClientName = ClientName;
            tokenBot = TokenBot;

            if (ClientName == Bots.TwitchEventSubBot)
            {
                tokenBot.BotAccessTokenChanged += TokenBot_BotAccessTokenChanged;
            }

            if (ClientName == Bots.TwitchEventSubStreamer)
            {
                tokenBot.StreamerAccessTokenChanged += TokenBot_StreamerAccessTokenChanged;
                tokenBot.StreamerNoScopesAccessTokenChanged += TokenBot_StreamerNoScopesAccessTokenChanged;
            }
        }

        private void BuildClient()
        {
            ILoggerFactory StreamLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddStreamLogger();
            });

            _EventSubMessageIdsLogger = new EventSubMessageIdsLogger();
            _EventSubWebsocketClient = new(StreamLoggerFactory);

            _EventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _EventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _EventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _EventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;
        }

        private void DisposeClient()
        {
            if (_EventSubWebsocketClient != null)
            {
                _EventSubWebsocketClient.WebsocketConnected -= OnWebsocketConnected;
                _EventSubWebsocketClient.WebsocketDisconnected -= OnWebsocketDisconnected;
                _EventSubWebsocketClient.WebsocketReconnected -= OnWebsocketReconnected;
                _EventSubWebsocketClient.ErrorOccurred -= OnErrorOccurred;
                _EventSubWebsocketClient = null;
            }

            foreach (var sub in SubscriptionHandlers)
            {
                sub.RemoveSubscriptions();
            }

            SubscriptionHandlers.Clear();
        }
#else
        internal TwitchEventSub(TwitchTokenBot TokenBot, Bots ClientName)
        {
            BotClientName = ClientName;
            tokenBot = TokenBot;

            ILoggerFactory StreamLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddStreamLogger();
            });

            _EventSubMessageIdsLogger = new EventSubMessageIdsLogger();
            _EventSubWebsocketClient = new(StreamLoggerFactory);

            _EventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _EventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _EventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _EventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            if (ClientName == Bots.TwitchEventSubBot)
            {
                tokenBot.BotAccessTokenChanged += TokenBot_BotAccessTokenChanged;
            }

            if (ClientName == Bots.TwitchEventSubStreamer)
            {
                tokenBot.StreamerAccessTokenChanged += TokenBot_StreamerAccessTokenChanged;
                tokenBot.StreamerNoScopesAccessTokenChanged += TokenBot_StreamerNoScopesAccessTokenChanged;
            }
        }        
#endif

        private void TokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TokenBot_StreamerAccessTokenChanged", DebugLogTypes.TwitchEventSub, "Refreshing streamer scopes access token.");
            ITwitchBotEventSubSubscriptions subscription = SubscriptionHandlers.Find((s) => s.CurrBot == BotType.StreamerAccount);
            RefreshSubscriptions(subscription);
        }

        private void TokenBot_StreamerNoScopesAccessTokenChanged(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TokenBot_StreamerNoScopesAccessTokenChanged", DebugLogTypes.TwitchEventSub, "Refreshing streamer scopes access token.");
            ITwitchBotEventSubSubscriptions subscription = SubscriptionHandlers.Find((s) => s.CurrBot == BotType.StreamerNoScopes);
            RefreshSubscriptions(subscription);
        }

        private void TokenBot_BotAccessTokenChanged(object sender, EventArgs e)
        {
            LogWriter.DebugLog("TokenBot_BotAccessTokenChanged", DebugLogTypes.TwitchEventSub, "Refreshing streamer scopes access token.");
            ITwitchBotEventSubSubscriptions subscription = SubscriptionHandlers.Find((s) => s.CurrBot == BotType.BotAccount);
            RefreshSubscriptions(subscription);
        }

        private void RefreshSubscriptions(ITwitchBotEventSubSubscriptions subscriptions)
        {
            if (subscriptions != null && IsActive == true)
            {
                LogWriter.DebugLog("RefreshSubscriptions", DebugLogTypes.TwitchEventSub, "Refreshing EventSub subscriptions due to access token change.");
                subscriptions.RemoveSubscriptions();
                subscriptions.ClearSubscriptions();


                if (OptionFlags.IsStreamOnline) // restore subscriptions based on if stream is online
                {
                    subscriptions.AddSubscriptions();
                }
                else
                { // restore stream offline, EventSub connected, subscriptions
                    subscriptions.AddConnectionSubscriptions();
                }
            }
        }

        private Task OnWebsocketConnected(object sender, WebsocketConnectedArgs args)
        {
            return Task.Run(() =>
            {
                if (!args.IsRequestedReconnect)
                {
                    LogWriter.DebugLog("OnWebsocketConnected", DebugLogTypes.TwitchEventSub, "EventSub now connected.");

                    if (IsActive != true)
                    {
                        IsActive = true;
                        AddConnectedSubscriptions();

                        InvokeBotStarted();
                        _EventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                        _EventSubMessageIdsLogger.MsgLogCleanup();
                    }
                }
            });
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnErrorOccurred", DebugLogTypes.TwitchEventSub, $"Websocket session {_EventSubWebsocketClient?.SessionId} encountered an error:\r\n" +
                 $"Exception: {args.Exception}\r\n" +
                 $"Message: {args.Message}");

                InvokeBotStopped();
                ErrorFound = true;
            });
        }

        private Task OnWebsocketReconnected(object sender, EventArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnWebsocketReconnected", DebugLogTypes.TwitchEventSub, $"Websocket session {_EventSubWebsocketClient?.SessionId} reconnected!");
            });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            return Task.Run(async () =>
            {
                try
                {
                    LogWriter.DebugLog("OnWebsocketDisconnected", DebugLogTypes.TwitchEventSub, $"Websocket session {_EventSubWebsocketClient.SessionId} disconnected.");

                    if (IsActive == true && !ErrorFound)
                    {
                        await StopBot();
                        Thread.Sleep(1000);
                        await StartBot();
                    }
                    else
                    {
                        ErrorFound = false; // stopping bot, clear the error-found
                        await StopBot();
                    }
                }
                catch (TokenExpiredException ex)
                {
                    LogWriter.LogException(ex, "StartBot");
                    tokenBot.CheckToken();

                    if (IsActive == true && !ErrorFound)
                    {
                        await StopBot();
                        Thread.Sleep(1000);
                        await StartBot();
                        InvokeBotStarted();
                    }
                    else
                    {
                        IsActive = false;
                        ErrorFound = false; // stopping bot, clear the error-found
                        await StopBot();
                    }

                    if (_EventSubMessageIdsLogger != null)
                    {
                        _EventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                    }
                }
            });
        }

        public override Task StartBot()
        {
            return Task.Run(async () =>
            {
                try
                {
                    try
                    {
#if BROKEN_EVENTSUB
                        if (IsActive != true)
                        {
                            BuildClient();

                            OnInitialBotStartupSubHandlers?.Invoke(this, new());
#else
                    if (IsActive == null)
                    {
                        OnInitialBotStartupSubHandlers?.Invoke(this, new());
                    }

                    if (IsActive != true)
                    {
#endif
                            if (BotClientName == Bots.TwitchEventSubBot)
                            {
                                tokenBot.UpdateActiveTokens(BotType.BotAccount, true);
                            }
                            else if (BotClientName == Bots.TwitchEventSubStreamer)
                            {
                                tokenBot.UpdateActiveTokens(BotType.StreamerAccount, true);
                                tokenBot.UpdateActiveTokens(BotType.StreamerNoScopes, true);
                            }

                            tokenBot.CheckToken();

                            LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchEventSub, "Now starting EventSub bot.");

                            await StartAsync(new());
                        }
                    }
                    catch (TokenExpiredException ex)
                    {
                        LogWriter.LogException(ex, "StartBot");
                        tokenBot.CheckToken();

                        await StartAsync(new());

                        IsActive = true;
                        _EventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "StartBot");
                    IsActive = false;
                    InvokeBotFailedStart();
                    LogWriter.DebugLog("StartBot", DebugLogTypes.TwitchEventSub, $"Websocket failed to start.");
                }
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_EventSubWebsocketClient == null)
            {
                BuildClient();
            }
            await _EventSubWebsocketClient.ConnectAsync();
        }

        public override Task StopBot()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (IsActive == true)
                    {

#if BROKEN_EVENTSUB
                        DisposeClient();
                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchEventSub, "Stopping EventSub bot.");
#else
                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchEventSub, "Stopping EventSub bot.");
                        await StopAsync(new());
#endif

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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _EventSubWebsocketClient?.DisconnectAsync();
        }

        internal void AddSubscriptionHandler(ITwitchBotEventSubSubscriptions twitchBotEventSubSubscription)
        {
            twitchBotEventSubSubscription
                .AddEventHandlers(_EventSubWebsocketClient)
                .ConfigureMessageLogger(_EventSubMessageIdsLogger)
                .ConfigureTokenBot(tokenBot);
            SubscriptionHandlers.Add(twitchBotEventSubSubscription);
        }

        private void AddConnectedSubscriptions()
        {
            LogWriter.DebugLog("AddConnectedSubscriptions", DebugLogTypes.TwitchEventSub, "Adding EventSub subscriptions as EventSub is now connected.");

            foreach (var subscription in SubscriptionHandlers)
            {
                subscription.AddConnectionSubscriptions();
            }
        }

        public void AddStreamOnlineSubscriptions()
        {
            LogWriter.DebugLog("AddStreamOnlineSubscriptions", DebugLogTypes.TwitchEventSub, "Adding EventSub subscriptions now stream is online.");

            foreach (var subscription in SubscriptionHandlers)
            {
                subscription.RemoveSubscriptions(); // remove any active offline subscriptions before online mode subscriptions
                subscription.AddSubscriptions();
            }
        }
    }
}
