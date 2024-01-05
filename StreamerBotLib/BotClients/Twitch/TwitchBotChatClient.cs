
#define TwitchLib_ConnectProblem

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Enums;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace StreamerBotLib.BotClients.Twitch
{
    public class TwitchBotChatClient : TwitchBotsBase, INotifyPropertyChanged
    {
        private static TwitchTokenBot twitchTokenBot;

        /// <summary>
        /// The client connection to the server.
        /// </summary>
        public TwitchClient TwitchChat { get; private set; } // chat bot

        private Logger<TwitchClient> LogData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public string StatusLog { get; set; } = "";
        private const int maxlength = 8000;
        private const int SingleChatLength = 500;

        private readonly Queue<Task> TaskSend = new();

#if !TwitchLib_ConnectProblem
        private bool IsInitialized = false;
        private string ConnectedChannelName = "";
#endif

        public event EventHandler<EventArgs> UnRegisterHandlers;

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

#if !TwitchLib_ConnectProblem
            CreateClient();
#endif
        }

        /// <summary>
        /// Sets the Twitch Token bot used for the automatic refreshing access token.
        /// </summary>
        /// <param name="tokenBot">An instance of the token bot, to use the same token bot across chat bots.</param>
        internal override void SetTokenBot(TwitchTokenBot tokenBot)
        {
            twitchTokenBot = tokenBot;
            twitchTokenBot.BotAccessTokenChanged += TwitchTokenBot_BotAccessTokenChanged;
            twitchTokenBot.BotAccessTokenUnChanged += TwitchTokenBot_BotAccessTokenUnChanged;
        }

        private void TwitchTokenBot_BotAccessTokenUnChanged(object sender, EventArgs e)
        {
            SendChatMessages();
        }

        private void SendChatMessages()
        {
            lock (TaskSend)
            {
                while (TaskSend.Count > 0)
                {
                    TaskSend.Dequeue().Start();
                }
            }
        }

        private void TwitchTokenBot_BotAccessTokenChanged(object sender, EventArgs e)
        {
            if (IsInitialStart && IsStarted)
            {
                StopBot();
                StartBot();

                SendChatMessages();
            }
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
                WhisperThrottlingPeriod = TimeSpan.FromSeconds(30),
                ReconnectionPolicy = new(30)
            };

            TwitchChat = new TwitchClient(new WebSocketClient(options), ClientProtocol.WebSocket, LogData);
            TwitchChat.OnLog += TwitchChat_OnLog;
            TwitchChat.OnDisconnected += TwitchChat_OnDisconnected;
            TwitchChat.AutoReListenOnException = true;

            TwitchChat.OnError += TwitchChat_OnError;
        }


        // TODO: work with this exception regarding 401 authorization invalid HTTP error return - review other bots for handling httpresponseexception for unauthorized access tokens - account for bots already started or not started
        private void TwitchChat_OnError(object sender, OnErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("Unauthorized"))
            {
                twitchTokenBot.CheckToken();
            }
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The payload of the event.</param>
        internal void TwitchChat_OnLog(object sender, OnLogArgs e)
        {
            void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (StatusLog.Length + e.DateTime.ToLocalTime().ToString().Length + e.Data.Length + 2 >= maxlength)
            {
                StatusLog = StatusLog[StatusLog.IndexOf('\n')..];
            }

            StatusLog += $@"{e.DateTime.ToLocalTime().ToString(CultureInfo.CurrentCulture)} {e.Data}
";

            if (OptionFlags.LogBotStatus)
            {
                LogWriter.WriteLog(LogType.LogBotStatus, e.DateTime.ToLocalTime().ToString() + ": " + e.Data);
            }

            NotifyPropertyChanged(nameof(StatusLog));
        }

#if !TwitchLib_ConnectProblem
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
                if (!IsInitialized && TwitchChannelName != ConnectedChannelName)
                {
                    TwitchChat.Initialize(credentials, TwitchChannelName);
                    IsInitialized = true;
                }

                ConnectedChannelName = TwitchChannelName;
                TwitchChat.Connect();
             //   TwitchChat.JoinChannel(ConnectedChannelName);
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
            bool Stopped;
            try
            {
                if (IsStarted)
                {
                    IsStarted = false;
                    IsStopped = true;
                    TwitchChat.Disconnect();
                    InvokeBotStopped();
                }
                Stopped = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                Stopped = false;
            }

            return Stopped;
        }
#else
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
                TwitchChat.Initialize(credentials, TwitchChannelName);
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
            bool Connected = false;

            try
            {
                if (IsStopped || !IsStarted)
                {
                    IsInitialStart = true;
                    IsStarted = true;
                    CreateClient();
                    Connected = Connect();
                    if (Connected)
                    {
                        IsStopped = false;
                        InvokeBotStarted();
                    }
                }
            }
            catch (HttpRequestException hrEx)
            {
                if (hrEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    twitchTokenBot.CheckToken();
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
            bool Stopped;

            try
            {
                if (IsStarted)
                {
                    InvokeBotStopping();
                    IsStarted = false;
                    IsStopped = true;
                    TwitchChat.Disconnect();
                    InvokeBotStopped();
                }
                Stopped = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                Stopped = false;
            }

            return Stopped;
        }

#endif

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

        private readonly List<string> newSendMsg = new();

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
                newSendMsg.Clear();
                string tempSend = s;

                string prefix = (s.StartsWith("/me ") ? "/me " : "");

                while (tempSend.Length > SingleChatLength)
                {
                    string temp = tempSend[..SingleChatLength];
                    string AddSend = temp[..(temp.LastIndexOf(' ') - 1)];
                    newSendMsg.Add(AddSend);
                    tempSend = prefix + tempSend.Replace(AddSend, "").Trim();
                }

                if (tempSend.Length > 0)
                {
                    newSendMsg.Add(tempSend);
                }

                foreach (string D in newSendMsg)
                {
                    SendData(D);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                // if we get an exception sending, need to check if unauthorized; queue the chat message for when the bot restarts
                if (OptionFlags.TwitchTokenUseAuth)
                {
                    lock (TaskSend)
                    {
                        TaskSend.Enqueue(new(() =>
                        {
                            foreach (string D in newSendMsg)
                            {
                                SendData(D);
                            }
                        }));
                    }
                }

                twitchTokenBot.CheckToken();
                //CreateClient();
                //StartBot();
                //SendData(s);
                BackOffSend(); // if exception, perform backoff send
            }

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
            void BackOffSend(int BackOff = 1)
            {
                Thread.Sleep(BackOff * 1000); // block thread and wait for exponential time each loop while continuing to throw exceptions

                try
                {
                    foreach (string D in newSendMsg)
                    {
                        SendData(D);
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                    BackOffSend(BackOff * 2);
                }
            }

            return true;
        }

        /// <summary>
        /// Exit the bot when the app closes.
        /// </summary>
        /// <returns></returns>
        public override bool ExitBot()
        {
            if (TwitchChat != null && TwitchChat.IsConnected)
            {
                StopBot();
                TwitchChat = null;
            }

            return base.ExitBot();
        }

        /// <summary>
        /// Reconnect the bot if in use but chat gets disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchChat_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {

#if !TwitchLib_ConnectProblem
            // the TwitchClient reports disconnected but user didn't click the 'stop bot' button
            // the client should be started but is now disconnected
            // check is required so the bot doesn't keep restarting when the user actually clicked stop
            if (IsStarted)// && !TwitchChat.IsConnected)
            {
                Connect();    // restart the bot
            }
#else
            if (IsStarted && !IsStopped) // && !TwitchChat.IsConnected)
            {
                IsStarted = false;
                UnregisterHandlers();
                StartBot();
            }
            else
            {
                UnregisterHandlers();
                InvokeBotStopped();
            }
#endif
        }

#if TwitchLib_ConnectProblem
        private void UnregisterHandlers()
        {
            TwitchChat.OnLog -= TwitchChat_OnLog;
            TwitchChat.OnDisconnected -= TwitchChat_OnDisconnected;
            TwitchChat.LeaveChannel(TwitchChannelName);

            UnRegisterHandlers?.Invoke(this, new());
        }
#endif

    }
}
