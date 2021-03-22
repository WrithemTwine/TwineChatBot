
using ChatBot_Net5.Properties;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;


namespace ChatBot_Net5.Clients
{
    public class IOModuleTwitch : IOModule, INotifyPropertyChanged
    {
        /// <summary>
        /// The client connection to the server.
        /// </summary>
        internal TwitchClient TwitchChat { get; private set; } // chat bot

        /// <summary>
        /// Listens for new followers.
        /// </summary>
        internal static FullFollowerService FollowerService { get; private set; } 

        /// <summary>
        /// Listens for new stream activity, such as going live, updated live stream, and stream goes offline.
        /// </summary>
        internal static LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

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

        public IOModuleTwitch()
        {
            ChatClientName = "Twitch";

            ClientOptions options = new ClientOptions()
            {
                UseSsl = true,
                ClientType = TwitchLib.Communication.Enums.ClientType.Chat,

                MessagesAllowedInPeriod = ClientID == ChannelName ? 100 : 20,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),
                SendQueueCapacity = 100,
                SendDelay = 5,

                WhisperQueueCapacity = 200,
                WhispersAllowedInPeriod = 200,
                WhisperThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

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

            TwitchChat = new TwitchClient(new WebSocketClient(options), TwitchLib.Client.Enums.ClientProtocol.WebSocket, LogData);
            TwitchChat.OnLog += TwitchChat_OnLog;

            AccessToken = Settings.Default.TwitchAccessToken;
            BotUserName = Settings.Default.TwitchBotUserName;
            ChannelName = Settings.Default.TwitchChannelName;
            ClientID = Settings.Default.TwitchClientID;
            FrequencyFollowerTime = Settings.Default.TwitchFrequency;
            FrequencyLiveNotifyTime = Settings.Default.TwitchGoLiveFrequency;
            RefreshToken = Settings.Default.TwitchRefreshToken;
            RefreshDate = Settings.Default.TwitchRefreshDate;
            ShowConnectionMsg = Settings.Default.BotConnectionMsg;
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The payload of the event.</param>
        private void TwitchChat_OnLog(object sender, TwitchLib.Client.Events.OnLogArgs e)
        {
            void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (StatusLog.Length + e.DateTime.ToLocalTime().ToString().Length + e.Data.Length + 2 >= maxlength)
            {
                StatusLog = StatusLog[StatusLog.IndexOf('\n')..];
            }
            
            StatusLog += e.DateTime.ToString() + " " + e.Data + "\n";

            NotifyPropertyChanged(nameof(StatusLog));
        }

        /// <summary>
        /// Initializes and connects the Twitch client
        /// </summary>
        /// <returns>True for a successful connection.</returns>
        public override bool Connect()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(BotUserName, AccessToken);
            if (ChannelName == null)
            {
                throw new NoUserDataException();
            }
            else
            {
                if (TwitchChat.ConnectionCredentials == null)
                {
                    TwitchChat.Initialize(credentials, ChannelName);
                }

                TwitchChat.OverrideBeingHostedCheck = (ChannelName != BotUserName);

                if (!TwitchChat.IsConnected)
                {
                    TwitchChat.Connect();
                }
                ConnectServices();
            }

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
                SaveParams();
                StartServices();

                return true;
            } 
            catch (Exception ex)
            {
                string s = ex.Message;
                return s.Length == 0;
            }
        }

        /// <summary>
        /// Stops the Twitch client and the services.
        /// </summary>
        /// <returns>True when successful.</returns>
        public override bool StopBot()
        {
            if (TwitchChat.IsConnected)
            {
                TwitchChat.Disconnect();
                StopServices();
                //SaveParams();
            }
            return true;
        }

        /// <summary>
        /// Attempt to send the whisper to a user.
        /// </summary>
        /// <param name="user">The user to send the whisper.</param>
        /// <param name="s">The message to send.</param>
        /// <returns>True when succesulf whisper sent.</returns>
        public override bool SendWhisper(string user, string s)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send a message to the connected channels.
        /// </summary>
        /// <param name="s">The message to send.</param>
        /// <returns>True when message is sent.</returns>
        public override bool Send(string s)
        {
            if(TwitchChat.IsConnected == false)
            {
                TwitchChat.Reconnect();
            }

            foreach (JoinedChannel j in TwitchChat.JoinedChannels)
            {
                TwitchChat.SendMessage(j, s);
            }
            return true;
        }

        /// <summary>
        /// Establish all of the services attached to this Twitch client.
        /// </summary>
        internal void ConnectServices()
        {
            ApiSettings apifollow = new() { AccessToken = AccessToken, ClientId = ClientID };
            FollowerService = new FullFollowerService(new TwitchAPI(null, null, apifollow, null), (int)Math.Round(FrequencyFollowerTime, 0));
            FollowerService.SetChannelsByName(new List<string>() { ChannelName });
            
            ApiSettings apilive = new() { AccessToken = AccessToken, ClientId = ClientID };
            LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), (int)Math.Round(FrequencyLiveNotifyTime, 0));
            LiveStreamMonitor.SetChannelsByName(new List<string>() { ChannelName });
        }

        /// <summary>
        /// Start all of the services attached to the client.
        /// </summary>
        internal static void StartServices()
        {
            FollowerService.Start();
            LiveStreamMonitor.Start();
        }

        /// <summary>
        /// Stop all of the services attached to the client.
        /// </summary>
        internal static void StopServices()
        {
            FollowerService.Stop();
            LiveStreamMonitor.Stop();
        }

        internal async Task<List<Follow>> GetAllFollowersAsync()
        {
            return await FollowerService.GetAllFollowers(ChannelName);
        }
    }
}
