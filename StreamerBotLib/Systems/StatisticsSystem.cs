using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;

namespace StreamerBotLib.Systems
{
    public class StatisticsSystem : SystemsBase
    {
        /// <summary>
        /// Currency system instantiates through Statistic System, it's active when the stream is active - the StreamOnline and StreamOffline activity starts the Currency clock < - currency is (should be) earned when online.
        /// </summary>
        // private Thread StreamUpdateThread;

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
            BeginCurrencyClock?.Invoke(this, new()); // try to start clock, in case accrual is started for offline mode
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

        public static void ManageUsers(DateTime SpecifyTime)
        {
            lock (CurrUsers)
            {
                foreach (LiveUser U in CurrUsers)
                {
                    if (OptionFlags.ManageUsers)
                    {
                        DataManage.UserJoined(U.UserName, SpecifyTime.ToLocalTime());
                    }
                }
            }
        }

        public static bool CheckStreamTime(DateTime TimeStream)
        {
            return DataManage.CheckMultiStreams(TimeStream);
        }

        public static void SetCategory(string categoryId, string category)
        {
            Category = category;

            if (OptionFlags.ManageStreamStats)
            {
                DataManage.AddCategory(categoryId, category);
            }
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        public static bool UserJoined(string User, DateTime CurrTime, Bots Source)
        {
            bool result = false;
            lock (CurrUsers)
            {
                result = CurrUsers.UniqueAdd(new(User, Source));
            }

            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, CurrUsers.Count);
                if (OptionFlags.ManageUsers)
                {
                    DataManage.UserJoined(User, CurrTime);
                }
            }

            return result;
        }

        public static bool UserChat(string User)
        {
            bool result = false;
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, CurrUsers.Count);
                result = UniqueUserChat.UniqueAdd(User);
            }
            return result;
        }

        public static void ModJoined(string User)
        {
            if (OptionFlags.IsStreamOnline)
            {
                ModUsers.UniqueAdd(User);
            }
        }

        public static void SubJoined(string User)
        {
            if (OptionFlags.IsStreamOnline)
            {
                SubUsers.UniqueAdd(User);
            }
        }

        public static void VIPJoined(string User)
        {
            if (OptionFlags.IsStreamOnline)
            {
                VIPUsers.UniqueAdd(User);
            }
        }

        public static void UserLeft(string User, DateTime CurrTime, Bots Source)
        {
            lock (CurrUsers)
            {
                PostDataUserLeft(User, CurrTime);
                CurrUsers.Remove(new(User, Source));
            }
        }

        public static void ClearUserList(DateTime Stopped)
        {
            lock (CurrUsers)
            {
                foreach (LiveUser U in CurrUsers)
                {
                    PostDataUserLeft(U.UserName, Stopped);
                }
                CurrUsers.Clear();
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
            return DataManage.CheckFollower(User, CurrStream.StreamStart);
        }

        public static bool IsReturningUser(string User)
        {
            return DataManage.CheckUser(User, CurrStream.StreamStart);
        }
        
        #region Incoming Raids

        public static void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName)
        {
            DataManage.PostInRaidData(UserName, RaidTime, Viewers, GameName);
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

            bool found = false;
            if (OptionFlags.ManageStreamStats)
            {
                // retrieve existing stream or start a new stream entry
                if (DataManage.CheckStreamTime(Started))
                {
                    CurrStream = DataManage.GetStreamData(Started);

                    found = true;
                }
                else
                {
                    DataManage.AddStream(CurrStream.StreamStart);

                    found = false;
                }
            }
            MonitorWatchTime();
            StartCurrencyClock();

            // setting if user wants to save Stream Stat data
            return OptionFlags.ManageStreamStats && !found;
        }

        public void StreamDataUpdate()
        {
            CurrStream.ModeratorsPresent = ModUsers.Count;
            CurrStream.VIPsPresent = VIPUsers.Count;
            CurrStream.SubsPresent = SubUsers.Count;
            DataManage.PostStreamStat(CurrStream);
        }

        public static void StreamOffline(DateTime Stopped)
        {
            ClearUserList(Stopped);

            OptionFlags.IsStreamOnline = false;
            CurrStream.StreamEnd = Stopped;
            CurrStream.ModeratorsPresent = ModUsers.Count;
            CurrStream.VIPsPresent = VIPUsers.Count;
            CurrStream.SubsPresent = SubUsers.Count;

            // setting if user wants to save Stream Stat data
            if (OptionFlags.ManageStreamStats)
            {
                DataManage.PostStreamStat(CurrStream);
            }

            CurrStream.Clear();
            ModUsers.Clear();
            SubUsers.Clear();
            VIPUsers.Clear();
            UniqueUserJoined.Clear();
            UniqueUserChat.Clear();
        }

        #region Stream Stat Methods
        public static void AddFollow()
        {
            lock (CurrStream)
            {
                CurrStream.NewFollows++;
            }
        }

        public static void AddSub()
        {
            lock (CurrStream)
            {
                CurrStream.NewSubscribers++;
            }
        }

        public static void AddGiftSubs(int Gifted = 1)
        {
            lock (CurrStream)
            {
                CurrStream.GiftSubs += Gifted;
            }
        }

        public static void AddBits(int BitCount)
        {
            lock (CurrStream)
            {
                CurrStream.Bits += BitCount;
            }
        }

        public static void AddRaids()
        {
            lock (CurrStream)
            {
                CurrStream.Raids++;
            }
        }

        public static void AddHosted()
        {
            lock (CurrStream)
            {
                CurrStream.Hosted++;
            }
        }

        public static void AddUserBanned()
        {
            lock (CurrStream)
            {
                CurrStream.UsersBanned++;
            }
        }

        public static void AddUserTimedOut()
        {
            lock (CurrStream)
            {
                CurrStream.UsersTimedOut++;
            }
        }

        public static void AddTotalChats()
        {
            lock (CurrStream)
            {
                CurrStream.TotalChats++;
            }
        }

        public static void AddCommands()
        {
            lock (CurrStream)
            {
                CurrStream.Commands++;
            }
        }

        public static void AddAutoEvents()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedEvents++;
            }
        }

        public static void AddAutoCommands()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedCommands++;
            }
        }

        public static void AddDiscord()
        {
            lock (CurrStream)
            {
                CurrStream.DiscordMsgs++;
            }
        }

        public static void AddClips()
        {
            lock (CurrStream)
            {
                CurrStream.ClipsMade++;
            }
        }

        public static void AddChannelPtsCount()
        {
            lock (CurrStream)
            {
                CurrStream.ChannelPtCount++;
            }
        }

        public static void AddChannelChallenge()
        {
            lock (CurrStream)
            {
                CurrStream.ChannelChallenge++;
            }
        }
        #endregion
    }
}
