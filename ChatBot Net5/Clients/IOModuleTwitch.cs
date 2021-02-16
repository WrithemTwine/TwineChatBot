﻿
using ChatBot_Net5.Properties;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using TwitchLib.Api;
using TwitchLib.Api.Core;
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
        /// The websocket to connect to the server.
        /// </summary>
        internal TwitchClient TwitchChat { get; private set; } // chat bot
        internal FollowerService FollowerService { get; private set; } // checks followers
        internal LiveStreamMonitorService LiveStreamMonitor { get; private set; } // check for live stream activity

        private Logger<TwitchClient> LogData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public string StatusLog { get; set; } = "";
        private const int maxlength = 16000;

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
            TwitchChat.OnConnected += TwitchChat_OnConnected;

            AccessToken = Settings.Default.TwitchAccessToken;
            BotUserName = Settings.Default.TwitchBotUserName;
            ChannelName = Settings.Default.TwitchChannelName;
            ClientID = Settings.Default.TwitchClientID;
            FrequencyTime = Settings.Default.TwitchFrequency;
            RefreshToken = Settings.Default.TwitchRefreshToken;
            RefreshDate = Settings.Default.TwitchRefreshDate;
        }

        private void TwitchChat_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            TwitchChat.JoinChannel(ChannelName);
        }

        private void TwitchChat_OnLog(object sender, TwitchLib.Client.Events.OnLogArgs e)
        {
            void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (StatusLog.Length + e.DateTime.ToString().Length + e.Data.Length + 2 >= maxlength)
            {
                StatusLog = StatusLog[StatusLog.IndexOf('\n')..];
            }
            
            StatusLog += e.DateTime.ToString() + " " + e.Data + "\n";

            NotifyPropertyChanged(nameof(StatusLog));
        }

        public override bool Connect()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(BotUserName, AccessToken);
            if (ChannelName == null)
            {
                throw new NoUserDataException();
            }
            else
            {
                TwitchChat.Initialize(credentials);
            }

            return true;
        }

        public override bool StartBot()
        {
            try
            {
                ApiSettings apifollow = new ApiSettings() { AccessToken = AccessToken, ClientId = ClientID, Scopes = new List<TwitchLib.Api.Core.Enums.AuthScopes>() };
                apifollow.Scopes.Add(TwitchLib.Api.Core.Enums.AuthScopes.Channel_Read);

                ApiSettings apilive = new ApiSettings() { AccessToken = AccessToken, ClientId = ClientID, Scopes = new List<TwitchLib.Api.Core.Enums.AuthScopes>() };
                apilive.Scopes.Add(TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Read_Broadcast);

                int checkIntervalInSeconds = (int)Math.Round(FrequencyTime, 0);
                FollowerService = new FollowerService(new TwitchAPI(null,null, apifollow, null), checkIntervalInSeconds);
                LiveStreamMonitor = new LiveStreamMonitorService(new TwitchAPI(null, null, apilive, null), checkIntervalInSeconds);

                // initiate follower and livestream service, connect to provided channel
                FollowerService.SetChannelsByName(new List<string>() { ChannelName });
                LiveStreamMonitor.SetChannelsByName(new List<string>() { ChannelName });

                SaveParams();
                Connect();

                TwitchChat.Connect();

                FollowerService.Start();
                LiveStreamMonitor.Start();
                return true;
            } 
            catch (Exception ex)
            {
                string s = ex.Message;
                return s.Length == 0;
            }
        }

        public override bool StopBot()
        {
            if (TwitchChat.IsConnected)
            {
                TwitchChat.Disconnect();
                FollowerService.Stop();
                LiveStreamMonitor.Stop();
                SaveParams();
            }
            return true;
        }

        public override bool SendWhisper(string user, string s)
        {
            throw new NotImplementedException();
        }

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

        internal override bool SaveParams()
        {
            Settings.Default.TwitchAccessToken = AccessToken;
            Settings.Default.TwitchChannelName = ChannelName;
            Settings.Default.TwitchBotUserName = BotUserName;
            Settings.Default.TwitchClientID = ClientID;
            Settings.Default.TwitchRefreshToken = RefreshToken;
            Settings.Default.TwitchRefreshDate = RefreshDate;
            Settings.Default.TwitchFrequency = FrequencyTime;

            Settings.Default.Save();

            return true;
        }
    }
}
