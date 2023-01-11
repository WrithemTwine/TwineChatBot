using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
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

        private void BuildPubSubClient()
        {
            if (TwitchPubSub == null)
            {
                TwitchPubSub = new(LogData);

                TwitchPubSub.OnLog += TwitchPubSub_OnLog;
                TwitchPubSub.OnPubSubServiceConnected += TwitchPubSub_OnPubSubServiceConnected;
                TwitchPubSub.OnPubSubServiceClosed += TwitchPubSub_OnPubSubServiceClosed;
                IsConnected = false;
            }
        }

        private void TwitchPubSub_OnLog(object sender, OnLogArgs e)
        {
            BotsTwitch.TwitchBotChatClient.TwitchChat_OnLog(sender, new global::TwitchLib.Client.Events.OnLogArgs() { Data = $"PubSub {e.Data}", DateTime = DateTime.Now });
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
                            RefreshSettings();
                            BuildPubSubClient();

                            UserId = BotsTwitch.TwitchBotUserSvc.GetUserId(TwitchChannelName);

                            // add Listen to Topics here
                            if (OptionFlags.TwitchPubSubChannelPoints)
                            {
                                TwitchPubSub.ListenToChannelPoints(UserId);
                            }


                            TwitchPubSub.Connect();
                            Connected = true;
                            IsStarted = true;
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchPubSub_OnPubSubServiceConnected(object sender, EventArgs e)
        {
            if (!IsConnected && IsStarted)
            {
                Token = OptionFlags.TwitchStreamerUseToken ? OptionFlags.TwitchStreamOauthToken : OptionFlags.TwitchBotAccessToken;
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

            //TwitchPubSub = null;
            IsConnected = false;
            RefreshSettings();
            InvokeBotStopped();

        }
    }
}
