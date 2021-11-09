using StreamerBot.BotClients.Twitch;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace StreamerBot.BotClients
{
    public class BotsTwitch : BotsBase
    {
        public event EventHandler<OnNewFollowersDetectedArgs> OnCompletedDownloadFollowers;
        public event EventHandler<ClipFoundEventArgs> OnClipFound;

        public static TwitchBotFollowerSvc TwitchFollower { get; private set; } = new();
        public static TwitchBotLiveMonitorSvc TwitchLiveMonitor { get; private set; } = new();

        public BotsTwitch()
        {
            AddBot(TwitchFollower);

            TwitchFollower.OnServiceConnected += TwitchFollower_OnServiceConnected;
            OnCompletedDownloadFollowers += BotsTwitch_OnCompletedDownloadFollowers;

            GetAllFollowers();
        }

        private void TwitchFollower_OnServiceConnected(object sender, EventArgs e)
        {
            TwitchFollower.FollowerService.OnNewFollowersDetected += FollowerService_OnNewFollowersDetected;
        }

        private void BotsTwitch_OnCompletedDownloadFollowers(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchBulkPostFollowers, e);
        }

        private void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            InvokeBotEvent(this, BotEvents.TwitchPostNewFollowers, e);
        }

        #region Threaded Ops
        
        public void GetAllFollowers()
        {
            if (OptionFlags.ManageFollowers && OptionFlags.TwitchAddFollowersStart && TwitchFollower.IsStarted)
            {
                new Thread(new ThreadStart(()=> {
                    string ChannelName = TwitchBotsBase.TwitchChannelName;

                    List<Follow> follows = TwitchFollower.GetAllFollowersAsync().Result;

                    OnCompletedDownloadFollowers?.Invoke(this, new() { NewFollowers = follows });
                })).Start();
            }
        }

        #endregion

        #region MultiLive Bot

        public static MultiUserLiveBot.Data.DataManager MultiLiveDataManager { get; private set; }

        private const int maxlength = 8000;

        public string MultiLiveStatusLog { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public void MultiConnect()
        {
            MultiLiveDataManager = new();
            TwitchLiveMonitor.IsMultiConnected = true;
            NotifyPropertyChanged(nameof(MultiLiveDataManager));
        }

        public void MultiDisconnect()
        {
            StopMultiLive();
            MultiLiveDataManager = null;
            TwitchLiveMonitor.IsMultiConnected = false;
            NotifyPropertyChanged(nameof(MultiLiveDataManager));
        }

        public void UpdateChannels()
        {
            if (TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                MultiLiveDataManager.SaveData();
                TwitchLiveMonitor.SetLiveMonitorChannels(MultiLiveDataManager.GetChannelNames());
            }
            else
            {
                TwitchLiveMonitor.SetLiveMonitorChannels(new());
            }
            // TODO: localize the multilive bot data
            LogEntry(string.Format(CultureInfo.CurrentCulture, "MultiLive Bot started and monitoring {0} channels.", TwitchLiveMonitor.LiveStreamMonitor.ChannelsToMonitor.Count.ToString(CultureInfo.CurrentCulture)), DateTime.Now.ToLocalTime());
        }

        public void StartMultiLive()
        {
            if (TwitchLiveMonitor.IsMultiConnected && !TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                TwitchLiveMonitor.IsMultiLiveBotActive = true;
                UpdateChannels();
            }
        }

        public void StopMultiLive()
        {
            if (TwitchLiveMonitor.IsMultiConnected && TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                TwitchLiveMonitor.IsMultiLiveBotActive = false;
                UpdateChannels();
                LogEntry("MultiLive Bot stopped.", DateTime.Now.ToLocalTime());
            }
        }

        public void SendMultiLiveMsg(OnStreamOnlineArgs e)
        {
            if (TwitchLiveMonitor.IsMultiLiveBotActive)
            {
                // true posted new event, false did not post
                bool PostedLive = MultiLiveDataManager.PostStreamDate(e.Stream.UserName, e.Stream.StartedAt);

                if (PostedLive)
                {

                    bool MultiLive = MultiLiveDataManager.CheckStreamDate(e.Channel, e.Stream.StartedAt);

                    if ((OptionFlags.PostMultiLive && MultiLive) || !MultiLive)
                    {
                        // get message, set a default if otherwise deleted/unavailable
                        string msg = OptionFlags.LiveMsg ?? "@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url";

                        // keys for exchanging codes for representative names
                        Dictionary<string, string> dictionary = new()
                        {
                            { "#user", e.Stream.UserName },
                            { "#category", e.Stream.GameName },
                            { "#title", e.Stream.Title },
                            { "#url", "https://www.twitch.tv/" + e.Stream.UserName }
                        };

                        LogEntry(VariableParser.ParseReplace(msg, dictionary), e.Stream.StartedAt);
                        foreach (Tuple<string, Uri> u in MultiLiveDataManager.GetWebLinks())
                        {
                            if (u.Item1 == "Discord")
                            {
                                DiscordWebhook.SendMessage(u.Item2, VariableParser.ParseReplace(msg, dictionary));
                            }
                        }
                    }
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event to handle when the Twitch client sends and event. Updates the StatusLog property with the logged activity.
        /// </summary>
        /// <param name="data">The string of the message.</param>
        /// <param name="dateTime">The time of the event.</param>
        public void LogEntry(string data, DateTime dateTime)
        {
            if (MultiLiveStatusLog.Length + dateTime.ToString().Length + data.Length + 2 >= maxlength)
            {
                MultiLiveStatusLog = MultiLiveStatusLog[MultiLiveStatusLog.IndexOf('\n')..];
            }

            MultiLiveStatusLog += dateTime.ToString() + " " + data + "\n";

            NotifyPropertyChanged(nameof(MultiLiveStatusLog));
        }

        #endregion

    }
}
