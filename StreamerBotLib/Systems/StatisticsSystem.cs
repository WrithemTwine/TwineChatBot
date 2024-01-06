using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.Systems
{
    internal partial class ActionSystem
    {
        private bool UpdateDeathCounter;

        /// <summary>
        /// Currency system instantiates through Statistic System, it's active when the stream is active - the StreamOnline and StreamOffline activity starts the Currency clock < - currency is (should be) earned when online.
        /// </summary>
        // private Thread StreamUpdateThread;

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
                        DataManage.UserJoined(U, SpecifyTime.ToLocalTime());
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
                DataManage.PostCategory(categoryId, category);
            }
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        public static bool UserJoined(LiveUser User, DateTime CurrTime)
        {
            bool result = false;
            lock (CurrUsers)
            {
                result = CurrUsers.UniqueAdd(User);
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

        public static bool UserChat(LiveUser User)
        {
            bool result = false;
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, CurrUsers.Count);
                result = UniqueUserChat.UniqueAdd(User.UserName);
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

        public static void UserLeft(LiveUser User, DateTime CurrTime)
        {
            lock (CurrUsers)
            {
                PostDataUserLeft(User, CurrTime);
                CurrUsers.Remove(User);
            }
        }

        public static void ClearUserList(DateTime Stopped)
        {
            lock (CurrUsers)
            {
                foreach (LiveUser U in CurrUsers)
                {
                    PostDataUserLeft(U, Stopped);
                }
                CurrUsers.Clear();
            }
        }

        private static void PostDataUserLeft(LiveUser User, DateTime CurrTime)
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

        public static bool IsReturningUser(LiveUser User)
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
            LastLiveViewerCount = 0; // reset count to 0 for new stream

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
                    DataManage.PostStream(CurrStream.StreamStart);

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

        #region death counter

        /// <summary>
        /// Only works when stream is online. Increases death count for the current category.
        /// Has a built-in 30 second delay to prevent multi-counting the same death.
        /// </summary>
        /// <returns><code>-1</code>: if no update, <code>int value</code>: if counter updated</returns>
        internal int AddDeathCounter()
        {
            int result = -1;
            if (!UpdateDeathCounter)
            {
                UpdateDeathCounter = true;
                result = DataManage.PostDeathCounterUpdate(FormatData.AddEscapeFormat(Category));

                ThreadManager.CreateThreadStart(() =>
                {
                    Thread.Sleep(30 * 1000); // wait 30 seconds
                    UpdateDeathCounter = false; // reset flag for next update
                });
            }

            return result;
        }

        /// <summary>
        /// Resets the game death counter for the current counter. Can work offline. Typically moderator+ invoked, so likely safe
        /// against getting called multiple times in a short time period
        /// </summary>
        /// <param name="Counter">The value to update the death counter for the current category.</param>
        /// <returns>Current value of the counter after reset.</returns>
        internal int ResetDeathCounter(int Counter)
        {
            return DataManage.PostDeathCounterUpdate(FormatData.AddEscapeFormat(Category), true, Counter);
        }

        #endregion

    }
}
