
#define TwitchLib_ConnectProblem

using Microsoft.Extensions.Logging;

using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace StreamerBotLib.BotClients.Twitch
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
        private const int SingleChatLength = 500;

        private bool resetToken;

#if !TwitchLib_ConnectProblem
        private bool IsInitialized = false;
        private string ConnectedChannelName = "";
#endif

        public event EventHandler<EventArgs> UnRegisterHandlers;
        private readonly Queue<Task> TaskSend = new();

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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Building the TwitchBotChatClient bot.");

            BotClientName = Bots.TwitchChatBot;

            LogData = (Logger<TwitchClient>)LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TwitchClient>();

            twitchTokenBot.BotAccessTokenChanged += TwitchTokenBot_BotAccessTokenChanged;
            twitchTokenBot.BotAccessTokenUnChanged += TwitchTokenBot_BotAccessTokenUnChanged;

#if !TwitchLib_ConnectProblem
            CreateClient();
#endif
        }

        /// <summary>
        /// When the token is unchanged send any pending message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchTokenBot_BotAccessTokenUnChanged(object sender, EventArgs e)
        {
            if (IsStarted)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Access token unchanged. Sending chat messages.");
                SendChatMessages();
            }
        }

        /// <summary>
        /// Send all pending messages following an access token refresh.
        /// </summary>
        private void SendChatMessages()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Sending chat messages.");

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
            if (IsStarted)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Detected new access token. Updating and reseting TwitchChatClient.");

                ThreadManager.CreateThreadStart(() =>
                {
                    try
                    {
                        resetToken = true;
                        UnregisterHandlers();
                        TwitchChat.Disconnect();
                        CreateClient();
                        Connect();
                        resetToken = false;

                        SendChatMessages();
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                    }
                });
            }
        }

        /// <summary>
        /// Create the initial client and connect events.
        /// </summary>
        private void CreateClient()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Creating a TwitchChat client.");

            TwitchChat = new TwitchClient(null, ClientProtocol.WebSocket, LogData);
            TwitchChat.OnLog += TwitchChat_OnLog;
            TwitchChat.OnDisconnected += TwitchChat_OnDisconnected;
            TwitchChat.AutoReListenOnException = false;
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
                LogWriter.WriteLog(e.DateTime.ToLocalTime().ToString() + ": " + e.Data);
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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Beginning to connect client.");

            bool isConnected;

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Establishing credentials with bot user name and access token.");
            ConnectionCredentials credentials = new(TwitchBotUserName, TwitchAccessToken);
            if (TwitchChannelName == null)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "The TwitchChannelName is blank. Not connected.");
                isConnected = false;
            }
            else
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "TwitchChannelName available. Beginning to connect to channel.");

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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Starting chat bot.");
            bool Connected = false;

            try
            {
                if (IsStopped || !IsStarted)
                {
                    CreateClient();
                    if (Connected = Connect())
                    {
                        LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Connection complete. Notifying GUI about ProcessFollowQueuestarted bot.");

                        IsStarted = true;
                        IsStopped = false;
                        InvokeBotStarted();
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Found exception.");

                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                InvokeBotFailedStart();
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
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Found a ProcessFollowQueuestarted bot, now stopping the chat bot.");

                    InvokeBotStopping();
                    IsStarted = false;
                    IsStopped = true;
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Disconnecting the bot.");
                    TwitchChat.Disconnect();
                    LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Bot disconnected. Notifying the GUI.");
                    InvokeBotStopped();
                }
                Stopped = true;
            }
            catch (Exception ex)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Found exception.");

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

        private readonly List<string> newSendMsg = [];

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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Sending a message.");

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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Found exception.");

                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);

                //CreateClient();
                //StartBot();
                //SendData(s);
                BackOffSend(); // if exception, perform backoff send
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

        }

        /// <summary>
        /// Exit the bot when the app closes.
        /// </summary>
        /// <returns></returns>
        public override bool ExitBot()
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Exiting the chat bot.");

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
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Chat client disconnected. Attempting restart.");

                IsStarted = false;
                if (!resetToken)
                {
                    UnregisterHandlers();
                }
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
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.TwitchChatBot, "Unregistering event handlers to switch chat clients.");

            TwitchChat.OnLog -= TwitchChat_OnLog;
            TwitchChat.OnDisconnected -= TwitchChat_OnDisconnected;
            TwitchChat.LeaveChannel(TwitchChannelName);

            UnRegisterHandlers?.Invoke(this, new());
        }
#endif

    }
}