
using StreamerBotLib.Enums;
using StreamerBotLib.Static;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
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

        public event PropertyChangedEventHandler PropertyChanged;
        public string StatusLog { get; set; } = "";
        private const int maxlength = 8000;
        private const int SingleChatLength = 500;

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
            BotClientName = Bots.TwitchChatBot;
            CreateClient();
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

        /// <summary>
        /// When the token is unchanged send any pending message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwitchTokenBot_BotAccessTokenUnChanged(object sender, EventArgs e)
        {
            SendChatMessages();
        }

        /// <summary>
        /// Send all pending messages following an access token refresh.
        /// </summary>
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
            if (IsStarted)
            {
                StopBot();
                TwitchChat.SetConnectionCredentials(new(TwitchBotUserName, TwitchAccessToken));
                StartBot();

                SendChatMessages();
            }
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The payload of the event.</param>
        internal Task TwitchChat_OnLog(object sender, OnSendReceiveDataArgs e)
        {
            return new(() =>
            {
                DateTime curr = DateTime.Now.ToLocalTime();
                void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }

                if (StatusLog.Length + curr.ToString().Length + e.Data.Length + 2 >= maxlength)
                {
                    StatusLog = StatusLog[StatusLog.IndexOf('\n')..];
                }

                StatusLog += $@"{curr.ToString(CultureInfo.CurrentCulture)} {e.Data}
";

                if (OptionFlags.LogBotStatus)
                {
                    LogWriter.WriteLog($"{curr}: {e.Data}");
                }

                NotifyPropertyChanged(nameof(StatusLog));
            });
        }

        /// <summary>
        /// Create the initial client and connect events.
        /// </summary>
        private void CreateClient()
        {
            TwitchChat = new TwitchClient();
            TwitchChat.OnSendReceiveData += TwitchChat_OnLog;
            TwitchChat.OnError += TwitchChat_OnError;
        }

        private Task TwitchChat_OnError(object sender, OnErrorEventArgs e)
        {
            return new(() =>
            {
                if (e.Exception.Message.Contains("Unauthorized"))
                {
                    twitchTokenBot.CheckToken();
                }
            });
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
                        // sets the data into a queue to ensure it'll be sent once the token is refreshed, if in auth token mode
                        TaskSend.Enqueue(new(() =>
                        {
                            foreach (string D in newSendMsg)
                            {
                                SendData(D);
                            }
                        }));
                    }
                }
                else
                {
                    BackOffSend(); // if exception, perform backoff send
                }

                twitchTokenBot.CheckToken();
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
        /// Make sure TwitchChannelName is set!
        /// </summary>
        /// <returns><code>true</code> for successful bot start</returns>
        public override bool StartBot()
        {
            bool Connected = true;

            try
            {
                if ((IsStopped || !IsStarted) && Connect())
                {
                    IsStarted = true;
                    IsStopped = false;
                    InvokeBotStarted();
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
        /// <returns><code>true</code> when successful.</returns>
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
    }
}
