using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.Reflection;

using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotPubSub : TwitchBotsBase
    {
        private static TwitchTokenBot twitchTokenBot;

        public TwitchPubSub TwitchPubSub { get; private set; }
        private Logger<TwitchPubSub> LogData { get; set; }

        private bool IsConnected;
        private string UserId;

        private string Token;

        public TwitchBotPubSub()
        {
            BotClientName = Bots.TwitchPubSub;

            LogData = new Logger<TwitchPubSub>(
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
                );

            BuildPubSubClient();
        }

        /// <summary>
        /// Sets the Twitch Token bot used for the automatic refreshing access token.
        /// </summary>
        /// <param name="tokenBot">An instance of the token bot, to use the same token bot across chat bots.</param>
        internal override void SetTokenBot(TwitchTokenBot tokenBot)
        {
            twitchTokenBot = tokenBot;
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
                    StopBot(); // stop the PubSub bot
                    StartBot(); // restart the PubSub bot
                }
            }
        }

        private void BuildPubSubClient()
        {
            if (TwitchPubSub == null)
            {
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
            twitchTokenBot.CheckToken();
        }

        private void TwitchPubSub_OnLog(object sender, OnLogArgs e)
        {
            static string Clean(string message)
            {
                return message.Replace("\n", "").Replace("\r", "");
            }

            if (e.Data.Contains("reconnect", StringComparison.CurrentCultureIgnoreCase))
            {
                ReconnectService();
            }

            BotsTwitch.TwitchBotChatClient.TwitchChat_OnLog(sender,
                new global::TwitchLib.Client.Events.OnSendReceiveDataArgs()
                {
                    Data = $"PubSub {Clean(e.Data)}"
                });
        }

        private void ReconnectService()
        {
            IsStarted = false;
            IsStopped = true;
            TwitchPubSub.OnLog -= TwitchPubSub_OnLog;
            TwitchPubSub.OnPubSubServiceConnected -= TwitchPubSub_OnPubSubServiceConnected;
            TwitchPubSub.OnPubSubServiceClosed -= TwitchPubSub_OnPubSubServiceClosed;
            TwitchPubSub.OnPubSubServiceError -= TwitchPubSub_OnPubSubServiceError;
            HandlersAdded = false;
            TwitchPubSub = null;

            StartBot();
        }

        public override bool StartBot()
        {
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

                        Connected = false;
                    }
                }
            });

            return Connected;
        }

        public override bool StopBot()
        {
            bool Stopped = true;

            ThreadManager.CreateThreadStart(() =>
            {
                lock (this)
                {
                    try
                    {
                        if (IsStarted)
                        {
                            IsStarted = false;
                            IsStopped = true;
                            TwitchPubSub.Disconnect();

                            TwitchPubSub.OnLog -= TwitchPubSub_OnLog;
                            TwitchPubSub.OnPubSubServiceConnected -= TwitchPubSub_OnPubSubServiceConnected;
                            TwitchPubSub.OnPubSubServiceClosed -= TwitchPubSub_OnPubSubServiceClosed;
                            HandlersAdded = false;
                            TwitchPubSub = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
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
                    // send the topics to listen
                    TwitchPubSub?.SendTopics(Token);
                    InvokeBotStarted();
                }

                IsConnected = true;
            }
        }

        private void TwitchPubSub_OnPubSubServiceClosed(object sender, EventArgs e)
        {
            TwitchPubSub?.SendTopics(Token, true);

            IsConnected = false;
            InvokeBotStopped();
        }
    }
}
