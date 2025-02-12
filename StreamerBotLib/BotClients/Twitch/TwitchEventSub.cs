using Microsoft.Extensions.Logging;

using StreamerBotLib.Enums;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Logger;
using StreamerBotLib.Static;

using TwitchLib.Api.Core.Exceptions;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchEventSub : TwitchBotsBase
    {
        private IEventSubMessageIdsLogger EventSubMessageIdsLogger { get; }
        private EventSubWebsocketClient EventSubWebsocketClient { get; }
        private readonly TwitchTokenBot tokenBot;

        private List<ITwitchBotEventSubSubscriptions> SubscriptionHandlers = [];

        internal event EventHandler OnInitialBotStartupSubHandlers;

        private bool ErrorFound { get; set; } = false;

        internal TwitchEventSub(TwitchTokenBot TokenBot, Bots ClientName)
        {
            BotClientName = ClientName;
            tokenBot = TokenBot;

            ILoggerFactory StreamLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddStreamLogger();
            });

            EventSubMessageIdsLogger = new EventSubMessageIdsLogger();
            EventSubWebsocketClient = new(StreamLoggerFactory);

            EventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            EventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            EventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            EventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;


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
                subscriptions.RemoveSubscriptions();
                subscriptions.ClearSubscriptions();

                subscriptions.AddSubscriptions();
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
                        InvokeBotStarted();
                        EventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                        EventSubMessageIdsLogger.MsgLogCleanup();

                        AddConnectedSubscriptions();
                    }
                }
            });
        }

        private Task OnErrorOccurred(object sender, ErrorOccuredArgs args)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("OnErrorOccurred", Enums.DebugLogTypes.TwitchEventSub, $"Websocket session {EventSubWebsocketClient.SessionId} encountered an error:\r\n" +
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
                LogWriter.DebugLog("OnWebsocketReconnected", Enums.DebugLogTypes.TwitchEventSub, $"Websocket session {EventSubWebsocketClient.SessionId} reconnected!");
            });
        }

        private Task OnWebsocketDisconnected(object sender, EventArgs args)
        {
            return Task.Run(async () =>
            {
                try
                {
                    LogWriter.DebugLog("OnWebsocketDisconnected", Enums.DebugLogTypes.TwitchEventSub, $"Websocket session {EventSubWebsocketClient.SessionId} disconnected.");

                    if (IsActive == true && !ErrorFound)
                    {
                        Thread.Sleep(1000);
                        await StartAsync(new());
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

                    await StartAsync(new());

                    IsActive = true;
                    EventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                    InvokeBotStarted();
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
                        if (IsActive == null)
                        {
                            OnInitialBotStartupSubHandlers?.Invoke(this, new());
                        }

                        if (IsActive == null || IsActive == false)
                        {
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
                        EventSubMessageIdsLogger.MsgLogging |= IsActive == true;
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "StartBot");
                    IsActive = false;
                    InvokeBotFailedStart();
                    LogWriter.DebugLog("StartBot", Enums.DebugLogTypes.TwitchEventSub, $"Websocket failed to start.");
                }
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await EventSubWebsocketClient.ConnectAsync();
        }

        public override Task StopBot()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (IsActive == true)
                    {
                        LogWriter.DebugLog("StopBot", Enums.DebugLogTypes.TwitchEventSub, "Stopping EventSub bot.");

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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await EventSubWebsocketClient.DisconnectAsync();
        }

        internal void AddSubscriptionHandler(ITwitchBotEventSubSubscriptions twitchBotEventSubSubscription)
        {
            SubscriptionHandlers.Add(twitchBotEventSubSubscription);
            twitchBotEventSubSubscription
                .AddEventHandlers(EventSubWebsocketClient)
                .ConfigureMessageLogger(EventSubMessageIdsLogger)
                .ConfigureTokenBot(tokenBot);
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
