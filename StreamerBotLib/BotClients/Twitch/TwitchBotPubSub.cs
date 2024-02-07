using Microsoft.Extensions.Logging;

using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotPubSub : TwitchBotsBase
    {
        public TwitchPubSub TwitchPubSub { get; private set; }
        private Logger<TwitchPubSub> LogData { get; set; }

        private bool IsConnected;
        private string UserId;

        private string Token;

        public TwitchBotPubSub()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Constructing PubSub instance.");

            BotClientName = Bots.TwitchPubSub;

            LogData = (Logger<TwitchPubSub>)LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TwitchPubSub>();

            /*  new Logger<TwitchPubSub>(
            new LoggerFactory(
                new List<ILoggerProvider>() {
                    new ConsoleLoggerProvider(
                        new OptionsMonitor<ConsoleLoggerOptions>(
                            new OptionsFactory<ConsoleLoggerOptions>(
                                new List<ConfigureOptions<ConsoleLoggerOptions>>(),
                                new List<PostConfigureOptions<ConsoleLoggerOptions>>()
                            ),
                            new List<ConfigurationChangeTokenSource<ConsoleLoggerOptions>>(),
                            new OptionsCache<ConsoleLoggerOptions>()
                            )
                        )
                }
                )
            );*/

            twitchTokenBot.BotAccessTokenChanged += TwitchTokenBot_BotAccessTokenChanged;
            twitchTokenBot.StreamerAccessTokenChanged += TwitchTokenBot_StreamerAccessTokenChanged;
        }

        /// <summary>
        /// When the Streamer Access Token is refreshed, we have to restart the PubSub bot using the new token.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchTokenBot_StreamerAccessTokenChanged(object sender, EventArgs e)
        {
            if (IsStarted)
            {
                if (OptionFlags.TwitchStreamerUseToken)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Detected token changed. Updating bot.");
                    StopBot(); // stop the PubSub bot
                    StartBot(); // restart the PubSub bot
                }
            }
        }

        /// <summary>
        /// When the Bot Access Token is refreshed, we have to restart the PubSub bot using the new token.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchTokenBot_BotAccessTokenChanged(object sender, EventArgs e)
        {
            if (IsStarted)
            {
                if (!OptionFlags.TwitchStreamerUseToken)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Detected token changed. Updating bot.");

                    StopBot(); // stop the PubSub bot
                    StartBot(); // restart the PubSub bot
                }
            }
        }

        /// <summary>
        /// Build the PubSub instance. Will add an output logger and attach events.
        /// </summary>
        private void BuildPubSubClient()
        {
            if (TwitchPubSub == null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Building the PubSub client.");

                TwitchPubSub = new(LogData);

                TwitchPubSub.OnLog += TwitchPubSub_OnLog;
                TwitchPubSub.OnPubSubServiceConnected += TwitchPubSub_OnPubSubServiceConnected;
                TwitchPubSub.OnPubSubServiceClosed += TwitchPubSub_OnPubSubServiceClosed;
                TwitchPubSub.OnPubSubServiceError += TwitchPubSub_OnPubSubServiceError;

                IsConnected = false;
            }
        }

        /// <summary>
        /// The likely error we guess is an unauthorized token. Call and refresh the token.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchPubSub_OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name,
                DebugLogTypes.TwitchPubSubBot, "Found a service error. Checking the token for issues.");
            twitchTokenBot.CheckToken();
        }

        /// <summary>
        /// Handled event when PubSub logs a status message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchPubSub_OnLog(object sender, OnLogArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Posting a new logged message.");

            static string Clean(string message)
            {
                return message.Replace("\n", "").Replace("\r", "");
            }

            if (e.Data.Contains("reconnect", StringComparison.CurrentCultureIgnoreCase))
            {
                ReconnectService();
            }

            BotsTwitch.TwitchBotChatClient.TwitchChat_OnLog(sender,
                new global::TwitchLib.Client.Events.OnLogArgs()
                {
                    Data = $"PubSub {Clean(e.Data)}",
                    DateTime = DateTime.Now
                });
        }

        /// <summary>
        /// Create a new client when there's an error or receive a signal to reconnect.
        /// 
        /// Reconnects the PubSub service.
        /// First, remove event handlers.
        /// Second, start the bot.
        /// </summary>
        private void ReconnectService()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Reconnecting the service.");

            IsStarted = false;
            IsStopped = true;
            UnregisterHandlers();
            HandlersAdded = false;
            TwitchPubSub = null;

            StartBot();
        }

        /// <summary>
        /// Start the PubSub bot service.
        /// </summary>
        /// <returns><code>true: if the service is successfully started. false: if the service isn't started.</code></returns>
        public override bool StartBot()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Starting the PubSub bot.");

            bool Connected = true;

            ThreadManager.CreateThreadStart(() =>
            {
                lock (this)
                {
                    try
                    {
                        if (IsStopped || !IsStarted)
                        {
                            IsStarted = true;
                            BuildPubSubClient();

                            UserId = BotsTwitch.TwitchBotUserSvc.GetUserId(TwitchChannelName);

                            // add Listen to Topics here
                            if (OptionFlags.TwitchPubSubChannelPoints)
                            {
                                TwitchPubSub.ListenToChannelPoints(UserId);
                            }

                            TwitchPubSub.Connect();
                            Connected = true;
                            IsStopped = false;
                        }
                        else
                        {
                            Connected = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                        InvokeBotFailedStart();
                        Connected = false;
                    }
                }
            });

            return Connected;
        }

        /// <summary>
        /// Stop the PubSub bot service.
        /// </summary>
        /// <returns><code>true: from successfully stopping the bot; false: if unable to stop the bot.</code></returns>
        public override bool StopBot()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Checking to stop the PubSub bot.");

            bool Stopped = true;

            ThreadManager.CreateThreadStart(() =>
            {
                lock (this)
                {
                    try
                    {
                        if (IsStarted)
                        {
                            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "PubSub found and started. Attempting to stop.");

                            IsStarted = false;
                            IsStopped = true;
                            TwitchPubSub.Disconnect();
                            UnregisterHandlers();
                            HandlersAdded = false; 
                            TwitchPubSub = null;
                        }
                        else
                        {
                            Stopped = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                        InvokeBotFailedStart();
                        Stopped = false;
                    }
                }
            });

            return Stopped;
        }

        /// <summary>
        /// Event handler used to send all of the user selected 'listen to topics' to the server. This must be performed within 15 seconds of the connection, otherwise, the Twitch server disconnects the connection.
        /// </summary>
        /// <param name="sender">Object sending the event.</param>
        /// <param name="e">Parameters for event.</param>
        private void TwitchPubSub_OnPubSubServiceConnected(object sender, EventArgs e)
        {
            if (!IsConnected && IsStarted)
            {
                Token = OptionFlags.TwitchStreamerUseToken ? TwitchStreamerAccessToken : TwitchAccessToken;
                if (Token != null)
                {
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Detected PubSub service connected. Now sending topics to listen.");
                    // send the topics to listen
                    TwitchPubSub?.SendTopics(Token);
                    InvokeBotStarted();
                }
                else
                {
                    InvokeBotFailedStart();
                }

                IsConnected = true;
            }
        }

        /// <summary>
        /// Recieved event when PubSub service is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchPubSub_OnPubSubServiceClosed(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchPubSubBot, "Stopping PubSub service and stop listening to topics.");

            TwitchPubSub?.SendTopics(Token, true);
            UnregisterHandlers();
            IsConnected = false;
            InvokeBotStopped();
        }

        private void UnregisterHandlers()
        {
            TwitchPubSub.OnLog -= TwitchPubSub_OnLog;
            TwitchPubSub.OnPubSubServiceConnected -= TwitchPubSub_OnPubSubServiceConnected;
            TwitchPubSub.OnPubSubServiceClosed -= TwitchPubSub_OnPubSubServiceClosed;
            TwitchPubSub.OnPubSubServiceError -= TwitchPubSub_OnPubSubServiceError;
        }
    }
}
