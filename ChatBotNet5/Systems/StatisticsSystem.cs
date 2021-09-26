using ChatBot_Net5.BotClients;
using ChatBot_Net5.BotClients.TwitchLib.Events.ClipService;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Events;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Services.Events.FollowerService;

namespace ChatBot_Net5.Systems
{
    public class StatisticsSystem : BotSystems
    {
        /// <summary>
        /// Currency system instantiates through Statistic System, it's active when the stream is active - the StreamOnline and StreamOffline activity starts the Currency clock <- currency is (should be) earned when online.
        /// </summary>
        private static CurrencySystem CurrencySystem { get; set; }
        private Thread StreamUpdateThread;
        private bool StreamUpdateClockStarted;
        private const int SecondsDelay = 5000;

        private readonly List<string> CurrUsers = new();
        private readonly List<string> UniqueUserJoined = new();
        private readonly List<string> UniqueUserChat = new();
        private readonly List<string> ModUsers = new();
        private readonly List<string> SubUsers = new();
        private readonly List<string> VIPUsers = new();
        private StreamStat CurrStream;
        public string Category { get; set; }

        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;

        public StatisticsSystem()
        {
            CurrStream = new();
            CurrencySystem = new(CurrUsers);
        }

        /// <summary>
        /// Attempt to start the currency clock. The setting "TwitchCurrencyOnline" can be user set and changed during bot operation. This method checks and starts the clock if not already started. The Currency System "StartClock()" method has checks for whether this setting is enabled.
        /// </summary>
        public static void StartCurrencyClock()
        {
            CurrencySystem.StartCurrencyClock(); // try to start clock, in case accrual is started for offline mode
        }

        public void ManageUsers()
        {
            ManageUsers(DateTime.Now.ToLocalTime());
            CurrencySystem.MonitorWatchTime();
        }

        public void ManageUsers(DateTime SpecifyTime)
        {
            lock (CurrUsers)
            {
                foreach (string U in CurrUsers)
                {
                    if (OptionFlags.ManageUsers)
                    {
                        DataManage.UserJoined(U, SpecifyTime.ToLocalTime());
                    }
                }
            }
        }

        public static bool CheckStreamTime(DateTime TimeStream)
        {
            return DataManage.CheckMultiStreams(TimeStream);
        }

        public void SetCategory(string categoryId, string category)
        {
            Category = category;
            DataManage.UpdateCategory(categoryId, category);
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        public bool UserJoined(string User, DateTime CurrTime)
        {
            lock (CurrUsers)
            {
                CurrUsers.Add(User);
            }

            if (OptionFlags.ManageUsers && OptionFlags.IsStreamOnline)
            {
                DataManage.UserJoined(User, CurrTime);
            }

            return UserChat(User);
        }

        /// <summary>
        /// Retrieves the current users within the channel during the stream.
        /// </summary>
        /// <returns>The current user count as of now.</returns>
        public int GetUserCount()
        {
            lock (CurrUsers)
            {
                return CurrUsers.Count;
            }
        }

        /// <summary>
        /// Retrieve how many chats have occurred in the current live stream to now.
        /// </summary>
        /// <returns>Current total chats as of now.</returns>
        public int GetCurrentChatCount()
        {
            lock (CurrStream)
            {
                return CurrStream.TotalChats;
            }
        }

        public bool UserChat(string User)
        {
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, CurrUsers.Count);
                if (!UniqueUserChat.Contains(User))
                {
                    UniqueUserChat.Add(User);
                    return true;
                }
            }
            return false;
        }

        public void ModJoined(string User)
        {
            if (OptionFlags.IsStreamOnline && !ModUsers.Contains(User))
            {
                ModUsers.Add(User);
            }
        }

        public void SubJoined(string User)
        {
            if (OptionFlags.IsStreamOnline && !SubUsers.Contains(User))
            {
                SubUsers.Add(User);
            }
        }

        public void VIPJoined(string User)
        {
            if (OptionFlags.IsStreamOnline && !VIPUsers.Contains(User))
            {
                VIPUsers.Add(User);
            }
        }

        public void UserLeft(string User, DateTime CurrTime)
        {
            lock (CurrUsers)
            {
                PostDataUserLeft(User, CurrTime);
                CurrUsers.Remove(User);
            }
        }

        private static void PostDataUserLeft(string User, DateTime CurrTime)
        {
            if (OptionFlags.ManageUsers && OptionFlags.IsStreamOnline)
            {
                DataManage.UserLeft(User, CurrTime);
            }
        }

        public static bool IsFollower(string User)
        {
            return DataManage.CheckFollower(User, DateTime.Now.ToLocalTime());
        }

        public static bool IsReturningUser(string User)
        {
            return DataManage.CheckUser(User, DateTime.Now.ToLocalTime());
        }

        #region Follower
        public void FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled);

            while (DataManage.UpdatingFollowers) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

            foreach (Follow f in e.NewFollowers.Where(f => DataManage.AddFollower(f.FromUserName, f.FollowedAt.ToLocalTime())))
            {
                if (OptionFlags.ManageFollowers)
                {
                    if (FollowEnabled)
                    {
                        PostChannelMessage?.Invoke(this,new() { Msg = VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) })) });
                    }

                    AddFollow();
                    AddAutoEvents();
                }

                //if (OptionFlags.TwitchFollowerFollowBack)
                //{
                //    FollowbackOp(f.FromUserName);
                //}
            }
        }
        #endregion Follower

        #region Incoming Raids

        public void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName)
        {
            DataManage.AddRaidData(UserName, RaidTime, Viewers, GameName);
        }

        #endregion

        #region Clips
        ///// <summary>
        ///// Default to all users or a specific user to register "DateTime.Now.ToLocalTime()" as the current watch date.
        ///// </summary>
        ///// <param name="User">User to update "Now" or null to update all users watch time.</param>
        //public void UpdateWatchTime(string User = null)
        //{
        //    if (OptionFlags.IsStreamOnline && OptionFlags.ManageUsers)
        //    {
        //        UpdateWatchTime(User, DateTime.Now.ToLocalTime());
        //    }
        //}

        //public void UpdateWatchTime(string User, DateTime Seen)
        //{
        //    if (OptionFlags.IsStreamOnline && OptionFlags.ManageUsers)
        //    {
        //        if (User != null)
        //        {
        //            datamanager.UpdateWatchTime(User, Seen);
        //        }
        //        else
        //        {
        //            foreach (string s in CurrUsers)
        //            {
        //                datamanager.UpdateWatchTime(s, Seen);
        //            }
        //        }
        //    }
        //}
        public void ClipMonitorService_OnNewClipFound(object sender, OnNewClipsDetectedArgs e)
        {
            ClipHelper(e.Clips);
        }

        public void ClipHelper(IEnumerable<Clip> Clips)
        {
            foreach (Clip c in Clips)
            {
                if (AddClip(c))
                {
                    if (OptionFlags.TwitchClipPostChat)
                    {
                        PostChannelMessage?.Invoke(this, new() { Msg = c.Url });
                    }

                    if (OptionFlags.TwitchClipPostDiscord)
                    {
                        foreach (Tuple<bool, Uri> u in GetDiscordWebhooks(WebhooksKind.Clips))
                        {
                            DiscordWebhook.SendMessage(u.Item2, c.Url);
                            AddDiscord();
                        }
                    }
                }
            }
        }

        public void RegisterNewClip(Clip clip)
        {
            DataManage.AddClip(clip.Id, clip.CreatedAt, clip.Duration, clip.GameId, clip.Language, clip.Title, clip.Url);
            AddClips();
        }
        #endregion Clips

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(webhooksKind);
        }

        public bool StreamOnline(DateTime Started)
        {
            OptionFlags.IsStreamOnline = true;
            CurrStream.StreamStart = Started;
            CurrStream.StreamEnd = Started; // temp assign ending time as start

            ManageUsers(Started);

            bool found;

            // retrieve existing stream or start a new stream entry
            if (DataManage.CheckStreamTime(Started))
            {
                if (OptionFlags.ManageStreamStats)
                {
                    CurrStream = DataManage.GetStreamData(Started);
                }
                found = true;
            }
            else
            {
                if (OptionFlags.ManageStreamStats)
                {
                    DataManage.AddStream(CurrStream.StreamStart);
                }
                found = false;
            }

            // TODO: fix updating a new stream online stat - start time and end time
            //PostStreamUpdates();
            CurrencySystem.MonitorWatchTime();
            CurrencySystem.StartCurrencyClock();

            // setting if user wants to save Stream Stat data
            return OptionFlags.ManageStreamStats && !found;
        }

        public void EndPostingStreamUpdates()
        {
            StreamUpdateThread.Join();
        }

        private void PostStreamUpdates()
        {
            if (!StreamUpdateClockStarted)
            {
                StreamUpdateClockStarted = true;
                CurrencySystem.MonitorWatchTime();
                CurrencySystem.StartCurrencyClock();

                StreamUpdateThread = new Thread(new ThreadStart(() =>
                {
                    while (OptionFlags.IsStreamOnline && OptionFlags.ManageStreamStats)
                    {
                        lock (CurrStream)
                        {
                            DataManage.PostStreamStat(ref CurrStream);
                        }
                        Thread.Sleep(SecondsDelay * (1 + (DateTime.Now.Second / 60)));
                    }
                    StreamUpdateClockStarted = false;
                }));
                StreamUpdateThread.Start();
            }
        }

        public void StreamOffline(DateTime Stopped)
        {
            if (OptionFlags.IsStreamOnline)
            {
                // TODO: add option to stop bot when stream goes offline

                lock (CurrUsers)
                {
                    foreach (string U in CurrUsers)
                    {
                        PostDataUserLeft(U, Stopped);
                    }
                    CurrUsers.Clear();
                }

                OptionFlags.IsStreamOnline = false;
                //EndPostingStreamUpdates(); // wait until the posting thread stops
                CurrStream.StreamEnd = Stopped;
                CurrStream.ModeratorsPresent = ModUsers.Count;
                CurrStream.VIPsPresent = VIPUsers.Count;
                CurrStream.SubsPresent = SubUsers.Count;

                // setting if user wants to save Stream Stat data
                if (OptionFlags.ManageStreamStats)
                {
                    DataManage.PostStreamStat(ref CurrStream);
                }

                CurrStream.Clear();
                ModUsers.Clear();
                SubUsers.Clear();
                VIPUsers.Clear();
                UniqueUserJoined.Clear();
                UniqueUserChat.Clear();
            }
        }

        public DateTime GetCurrentStreamStart()
        {
            return CurrStream.StreamStart;
        }

        #region Stream Stat Methods
        public void AddFollow()
        {
            lock (CurrStream)
            {
                CurrStream.NewFollows++;
            }
        }

        public void AddSub()
        {
            lock (CurrStream)
            {
                CurrStream.NewSubscribers++;
            }
        }

        public void AddGiftSubs(int Gifted = 1)
        {
            lock (CurrStream)
            {
                CurrStream.GiftSubs += Gifted;
            }
        }

        public void AddBits(int BitCount)
        {
            lock (CurrStream)
            {
                CurrStream.Bits += BitCount;
            }
        }

        public void AddRaids()
        {
            lock (CurrStream)
            {
                CurrStream.Raids++;
            }
        }

        public void AddHosted()
        {
            lock (CurrStream)
            {
                CurrStream.Hosted++;
            }
        }

        public void AddUserBanned()
        {
            lock (CurrStream)
            {
                CurrStream.UsersBanned++;
            }
        }

        public void AddUserTimedOut()
        {
            lock (CurrStream)
            {
                CurrStream.UsersTimedOut++;
            }
        }

        public void AddTotalChats()
        {
            lock (CurrStream)
            {
                CurrStream.TotalChats++;
            }
        }

        public void AddCommands()
        {
            lock (CurrStream)
            {
                CurrStream.Commands++;
            }
        }

        public void AddAutoEvents()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedEvents++;
            }
        }

        public void AddAutoCommands()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedCommands++;
            }
        }

        public void AddDiscord()
        {
            lock (CurrStream)
            {
                CurrStream.DiscordMsgs++;
            }
        }

        public void AddClips()
        {
            lock (CurrStream)
            {
                CurrStream.ClipsMade++;
            }
        }

        public void AddChannelPtsCount()
        {
            lock (CurrStream)
            {
                CurrStream.ChannelPtCount++;
            }
        }

        public void AddChannelChallenge()
        {
            lock (CurrStream)
            {
                CurrStream.ChannelChallenge++;
            }
        }
        #endregion
    }
}
