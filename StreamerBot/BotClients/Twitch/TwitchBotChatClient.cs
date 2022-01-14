
using StreamerBot.Enum;
using StreamerBot.Static;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Enums;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace StreamerBot.BotClients.Twitch
{
    public class TwitchBotChatClient : TwitchBotsBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The client connection to the server.
        /// </summary>
        public TwitchClient TwitchChat { get; private set; } // chat bot

        private Logger<TwitchClient> LogData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public string StatusLog { get; set; } = "";
        private const int maxlength = 8000;

        private bool IsInitialized = false;
        private string ConnectedChannelName = "";

        // limits of the number of IRC commands or messages you are allowed to send to the server
        //Limit Applies to …
        //20 per 30 seconds Users sending commands or messages to channels in which they do not have Moderator or Operator status
        //100 per 30 seconds Users sending commands or messages to channels in which they have Moderator or Operator status
        //50 per 30 seconds Known bots
        //7500 per 30 seconds Verified bots


        // For Whispers(private chat message between two users):
        //Limit Applies to …
        //3 per second, up to 100 per minute
        //40 accounts per day Users(not bots)
        //10 per second, up to 200 per minute
        //500 accounts per day Known bots
        //20 per second, up to 1200 per minute
        //100,000 accounts per day Verified bots
        public TwitchBotChatClient()
        {
            BotClientName = Bots.TwitchChatBot;

            LogData = new Logger<TwitchClient>(
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

            CreateClient();

            RefreshSettings();
        }

        /// <summary>
        /// Create the initial client and connect events.
        /// </summary>
        private void CreateClient()
        {
            ClientOptions options = new()
            {
                UseSsl = true,
                ClientType = ClientType.Chat,

                MessagesAllowedInPeriod = TwitchClientID == TwitchChannelName ? 100 : 20,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                SendQueueCapacity = 100,
                SendDelay = 5,

                WhisperQueueCapacity = 200,
                WhispersAllowedInPeriod = 200,
                WhisperThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            TwitchChat = new TwitchClient(new WebSocketClient(options), ClientProtocol.WebSocket, LogData);
            TwitchChat.OnLog += TwitchChat_OnLog;
            TwitchChat.OnDisconnected += TwitchChat_OnDisconnected;
            TwitchChat.AutoReListenOnException = true;
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The payload of the event.</param>
        private void TwitchChat_OnLog(object sender, OnLogArgs e)
        {
            void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (StatusLog.Length + e.DateTime.ToString().Length + e.Data.Length + 2 >= maxlength)
            {
                StatusLog = StatusLog[StatusLog.IndexOf('\n')..];
            }

            StatusLog += $@"{e.DateTime.ToString(CultureInfo.CurrentCulture)} {e.Data}
";

            if (OptionFlags.LogBotStatus)
            {
                LogWriter.WriteLog(LogType.LogBotStatus, e.DateTime.ToString() + ": " + e.Data);
            }

            NotifyPropertyChanged(nameof(StatusLog));
        }

        /// <summary>
        /// Initializes and connects the Twitch client
        /// </summary>
        /// <returns>True for a successful connection.</returns>
        public override bool Connect()
        {
            bool isConnected;
            ConnectionCredentials credentials = new(TwitchBotUserName, TwitchAccessToken);
            if (TwitchChannelName == null)
            {
                isConnected = false;
            }
            else
            {
                if (!IsInitialized)
                {
                    TwitchChat.Initialize(credentials, TwitchChannelName);
                    IsInitialized = true;
                }
                else if (ConnectedChannelName != TwitchChannelName) // if the user changes the channel to monitor, we need to disconnect the prior channel review
                {
                    TwitchChat.LeaveChannel(ConnectedChannelName);
                    TwitchChat.JoinChannel(TwitchChannelName);
                }
                TwitchChat.OverrideBeingHostedCheck = TwitchChannelName != TwitchBotUserName;
                ConnectedChannelName = TwitchChannelName;
                TwitchChat.Connect();
                isConnected = true;
            }

            return isConnected;
        }

        /// <summary>
        /// call Connect() first!
        /// </summary>
        /// <returns>true for successful bot start</returns>
        public override bool StartBot()
        {
            bool Connected;

            try
            {
                if (IsStopped || !IsStarted)
                {
                    RefreshSettings();
                    Connected = Connect();
                    if (Connected) // only connect if the joining channel name isn't null
                    {
                        IsStarted = true;
                        IsStopped = false;
                        InvokeBotStarted();
                    }
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

            return Connected;
        }

        /// <summary>
        /// Stops the Twitch client and the services.
        /// </summary>
        /// <returns>True when successful.</returns>
        public override bool StopBot()
        {
            try
            {
                if (IsStarted)
                {
                    IsStarted = false;
                    IsStopped = true;
                    TwitchChat.Disconnect();
                    RefreshSettings();
                    InvokeBotStopped();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        /// <summary>
        /// Attempt to send the whisper to a user.
        /// </summary>
        /// <param name="user">The user to send the whisper.</param>
        /// <param name="s">The message to send.</param>
        /// <returns>True when succesulf whisper sent.</returns>
        public override bool SendWhisper(string user, string s)
        {
            throw new();
        }

        /// <summary>
        /// Send a message to the connected channels.
        /// Note: When Twitch gets busy/unstable, there are many exceptions from unaccepted responses. Twitchlib throws the http handler exceptions, which we 
        /// catch here and start backing off by exponential time until success.
        /// </summary>
        /// <param name="s">The message to send.</param>
        /// <returns>True when message is sent.</returns>
        public override bool Send(string s)
        {
            try
            {
                SendData(s);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                //CreateClient();
                //StartBot();
                //SendData(s);
                BackOffSend(s); // if exception, perform backoff send
            }
            return true;

            void SendData(string s)
            {
                if (IsStarted)
                {
                    foreach (JoinedChannel j in TwitchChat.JoinedChannels)
                    {
                        TwitchChat.SendMessage(j, s);
                    }
                }
            }

            // need recursive call to send data while unable to send message (returning exceptions from Twitch)
            void BackOffSend(string ToSend, int BackOff = 1)
            {
                Thread.Sleep(BackOff * 1000); // block thread and wait for exponential time each loop while continuing to throw exceptions

                try
                {
                    SendData(ToSend);
                } 
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                    BackOffSend(ToSend, BackOff * 2);
                }
            }

        }

        /// <summary>
        /// Exit the bot when the app closes.
        /// </summary>
        /// <returns></returns>
        public override bool ExitBot()
        {
            if (TwitchChat.IsConnected)
            {
                StopBot();
            }
            TwitchChat = null;

            return base.ExitBot();
        }

        /// <summary>
        /// Reconnect the bot if in use but chat gets disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchChat_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            // the TwitchClient reports disconnected but user didn't click the 'stop bot' button
            // the client should be started but is now disconnected
            // check is required so the bot doesn't keep restarting when the user actually clicked stop
            if (IsStarted && !TwitchChat.IsConnected)
            {
                Connect();    // restart the bot
            }
        }
    }
}
