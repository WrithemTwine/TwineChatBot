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

                LogWriter.DebugLog("RefreshSubscriptions", DebugLogTypes.TwitchEventSub, "Removing all EventSub subscriptions to prepare for refresh.");
                subscriptions.RemoveSubscriptions();
                subscriptions.ClearSubscriptions();


                if (OptionFlags.IsStreamOnline) // restore subscriptions based on if stream is online
                {
                    LogWriter.DebugLog("RefreshSubscriptions", DebugLogTypes.TwitchEventSub, "Stream is online, adding online EventSub subscriptions.");
                    subscriptions.AddSubscriptions();
                }
                else
                { // restore stream offline, EventSub connected, subscriptions
                    LogWriter.DebugLog("RefreshSubscriptions", DebugLogTypes.TwitchEventSub, "Stream is offline, adding offline EventSub subscriptions.");
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
                        LogWriter.DebugLog("OnWebsocketConnected", DebugLogTypes.TwitchEventSub, $"Adding immediately connected subscriptions.");
                        AddConnectedSubscriptions();

                        LogWriter.DebugLog("OnWebsocketConnected", DebugLogTypes.TwitchEventSub, $"Notifying the bot is active.");
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

                    if (IsActive == true && !ErrorFound && OptionFlags.ActiveToken)
                    {
                        LogWriter.DebugLog("OnWebsocketDisconnected", DebugLogTypes.TwitchEventSub, $"Websocket disconnected unexpectedly, restarting bot.");
                        await StopBot();
                        await Task.Delay(1000);
                        await StartBot();
                    }
                    //else
                    //{
                    //    LogWriter.DebugLog("OnWebsocketDisconnected", DebugLogTypes.TwitchEventSub, $"Websocket disconnected, bot stopping the bot.");
                    //    ErrorFound = false; // stopping bot, clear the error-found
                    //    await StopBot();
                    //}
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
                        if (IsActive == null)
                        {
                            OnInitialBotStartupSubHandlers?.Invoke(this, new());
                        }

                        if (IsActive != true)
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
                        RemoveClearSubscriptions();

                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchEventSub, "Stopping EventSub bot.");
                        await StopAsync(new());

                        IsActive = false;

                        LogWriter.DebugLog("StopBot", DebugLogTypes.TwitchEventSub, "Notifying the GUI the bot stopped.");
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
            LogWriter.DebugLog("StopAsync", DebugLogTypes.TwitchEventSub, "Disconnecting EventSub websocket client.");

            await _EventSubWebsocketClient?.DisconnectAsync();
        }

        internal void AddSubscriptionHandler(ITwitchBotEventSubSubscriptions twitchBotEventSubSubscription)
        {
            LogWriter.DebugLog("AddSubscriptionHandler", DebugLogTypes.TwitchEventSub, $"Adding EventSub subscription handler for {twitchBotEventSubSubscription.CurrBot}.");
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

        private void RemoveClearSubscriptions()
        {
            LogWriter.DebugLog("RemoveClearSubscriptions", DebugLogTypes.TwitchEventSub, "Removing all EventSub subscriptions.");
            foreach (var subscription in SubscriptionHandlers)
            {
                subscription.RemoveSubscriptions();
                subscription.ClearSubscriptions();
            }
        }
    }
}
