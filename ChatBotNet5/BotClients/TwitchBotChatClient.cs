
using ChatBot_Net5.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ChatBot_Net5.BotClients
{
    public class TwitchBotChatClient : TwitchBots, INotifyPropertyChanged
    {
        /// <summary>
        /// The client connection to the server.
        /// </summary>
        internal TwitchClient TwitchChat { get; private set; } // chat bot

        private Logger<TwitchClient> LogData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public string StatusLog { get; set; } = "";
        private const int maxlength = 8000;

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
            BotClientName = Enum.Bots.TwitchChatBot;

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

            if (StatusLog.Length + e.DateTime.ToLocalTime().ToString().Length + e.Data.Length + 2 >= maxlength)
            {
                StatusLog = StatusLog[StatusLog.IndexOf('\n')..];
            }

            StatusLog += e.DateTime.ToLocalTime().ToString() + " " + e.Data + "\n";

            NotifyPropertyChanged(nameof(StatusLog));
        }

        /// <summary>
        /// Initializes and connects the Twitch client
        /// </summary>
        /// <returns>True for a successful connection.</returns>
        public override bool Connect()
        {
            ConnectionCredentials credentials = new(TwitchBotUserName, TwitchAccessToken);
            if (TwitchChannelName == null)
            {
                throw new NoUserDataException();
            }
            else if (TwitchChat.TwitchUsername == null)
            {
                TwitchChat.Initialize(credentials, TwitchChannelName);
                TwitchChat.OverrideBeingHostedCheck = TwitchChannelName != TwitchBotUserName;
            }
            TwitchChat.Connect();

            return true;
        }

        /// <summary>
        /// call Connect() first!
        /// </summary>
        /// <returns>true for successful bot start</returns>
        public override bool StartBot()
        {
            try
            {
                if (IsStopped || !IsStarted)
                {
                    RefreshSettings();
                    Connect();
                    IsStarted = true;
                    IsStopped = false;
                    InvokeBotStarted();
                }
                return true;
            }
            catch
            { return false; }
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
            catch { return false; }
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
            catch
            {
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
                catch
                {
                    BackOffSend(ToSend, BackOff * 2);
                }
            }

        }

        public override bool ExitBot()
        {
            if (TwitchChat.IsConnected)
            {
                TwitchChat.Disconnect();
            }
            TwitchChat = null;

            return base.ExitBot();
        }

        private void TwitchChat_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            // the TwitchClient reports disconnected but user didn't click the 'start bot' button
            // the client should be started but is now disconnected
            // check is required so the bot doesn't keep restarting when the user actually clicked stop
            if (IsStarted && !TwitchChat.IsConnected)
            {
                Connect();    // restart the bot
                HandlersAdded = false;
            }
        }
    }
}
