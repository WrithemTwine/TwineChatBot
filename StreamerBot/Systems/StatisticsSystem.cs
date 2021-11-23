using StreamerBot.Enum;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Threading;

namespace StreamerBot.Systems
{
    public class StatisticsSystem : SystemsBase
    {
        /// <summary>
        /// Currency system instantiates through Statistic System, it's active when the stream is active - the StreamOnline and StreamOffline activity starts the Currency clock <- currency is (should be) earned when online.
        /// </summary>
        private Thread StreamUpdateThread;

        public event EventHandler BeginCurrencyClock;
        public event EventHandler BeginWatchTime;

        public StatisticsSystem()
        {
        }

        /// <summary>
        /// Attempt to start the currency clock. The setting "TwitchCurrencyOnline" can be user set and changed during bot operation. This method checks and starts the clock if not already started. The Currency System "StartClock()" method has checks for whether this setting is enabled.
        /// </summary>
        public void StartCurrencyClock()
        {
            BeginCurrencyClock?.Invoke(this,new()); // try to start clock, in case accrual is started for offline mode
        }

        public void MonitorWatchTime()
        {
            BeginWatchTime?.Invoke(this, new());
        }

        public void ManageUsers()
        {
            ManageUsers(DateTime.Now.ToLocalTime());
            MonitorWatchTime();
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

        public bool UserChat(string User)
        {
            bool result = false;
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, CurrUsers.Count);
                if (!UniqueUserChat.Contains(User))
                {
                    UniqueUserChat.Add(User);
                    result = true;
                }
            }
            return result;
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

        //public void AddNewFollower(List<Follow> FollowList)
        //{
        //    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled);

        //    while (DataManage.UpdatingFollowers) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

        //    foreach (Follow f in FollowList.Where(f => DataManage.AddFollower(f.FromUserName, f.FollowedAt.ToLocalTime())))
        //    {
        //        if (OptionFlags.ManageFollowers)
        //        {
        //            if (FollowEnabled)
        //            {
        //                CallbackSendMsg?.Invoke(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) })));
        //            }

        //            AddFollow();
        //            AddAutoEvents();
        //        }
        //    }
        //}

        #endregion Follower

        #region Incoming Raids

        public void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName)
        {
            DataManage.AddRaidData(UserName, RaidTime, Viewers, GameName);
        }

        #endregion

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
            MonitorWatchTime();
            StartCurrencyClock();

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
                MonitorWatchTime();
                StartCurrencyClock();

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
