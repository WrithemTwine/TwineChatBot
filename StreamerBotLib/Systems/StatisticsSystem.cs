using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        private bool UpdateDeathCounter;

        public void ManageUsers()
        {
            ManageUsers(DateTime.Now.ToLocalTime());
            MonitorWatchTime();
        }

        public static void ManageUsers(DateTime SpecifyTime)
        {
            lock (StreamViewers)
            {
                if (OptionFlags.ManageUsers)
                {
                    DataManage.UserJoined(StreamViewers.GetCurrentActiveUsers(), SpecifyTime.ToLocalTime());
                }
            }
        }

        public static bool CheckStreamTime(DateTime TimeStream)
        {
            return DataManage.CheckMultiStreams(TimeStream);
        }

        public static void SetCategory(CategoryData categoryData)
        {
            Category = categoryData.CategoryName;

            if (OptionFlags.ManageStreamStats)
            {
                DataManage.PostCategoryStream(categoryData);
            }
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        public static void UserJoined(LiveUser User, DateTime CurrTime)
        {
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, StreamViewers.Count);
                if (OptionFlags.ManageUsers)
                {
                    DataManage.UserJoined(User, CurrTime);
                }
            }
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        public static void UserJoined(IEnumerable<LiveUser> User, DateTime CurrTime)
        {
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, StreamViewers.Count);
                if (OptionFlags.ManageUsers)
                {
                    LogWriter.DebugLog("UserJoined", DebugLogTypes.StatSystem,
                        $"Adding to database {User.Count()} users now joined to the channel.");
                    DataManage.UserJoined(User, CurrTime);
                }
            }
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
            lock (StreamViewers)
            {
                PostDataUserLeft(User, CurrTime);
            }
        }

        public static void ClearUserList(DateTime Stopped)
        {
            lock (StreamViewers)
            {
                foreach (LiveUser U in StreamViewers.GetCurrentActiveUsers())
                {
                    PostDataUserLeft(U, Stopped);
                }
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

        public static void PostIncomingRaid(LiveUser liveUser, DateTime RaidTime, int Viewers, CategoryData GameName)
        {
            DataManage.PostInRaidData(liveUser, RaidTime, Viewers, GameName);
        }

        #endregion

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(WebhooksSource.Discord, webhooksKind);
        }

        public bool StreamOnline(DateTime Started)
        {
            LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Detected a new livestream and starting up activites.");

            CurrStream.Clear();

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
                    DataManage.PostStream(CurrStream.StreamStart, Category);

                    found = false;
                }
            }
            MonitorWatchTime();
            StartCurrencyClock();

            // setting if user wants to save Stream Stat data
            return OptionFlags.ManageStreamStats && !found;
        }

        internal static void StreamDataUpdate()
        {
            LogWriter.DebugLog("StreamDataUpdate", DebugLogTypes.StatSystem, "Updating the current stream data to the database.");

            if (OptionFlags.ManageStreamStats)
            {
                CurrStream.ModeratorsPresent = ModUsers.Count;
                CurrStream.VIPsPresent = VIPUsers.Count;
                CurrStream.SubsPresent = SubUsers.Count;
                DataManage.PostStreamStat(CurrStream);
            }
        }

        public static void StreamOffline(DateTime Stopped)
        {
            LogWriter.DebugLog("StreamOffline", DebugLogTypes.StatSystem, "Received notice event the channel livestream is offline.");
            LogWriter.DebugLog("StreamOffline", DebugLogTypes.StatSystem, "Clearing user list with stream end time, to end watchtime & currency accrual.");

            ClearUserList(Stopped);

            OptionFlags.IsStreamOnline = false;
            CurrStream.StreamEnd = Stopped;
            CurrStream.ModeratorsPresent = ModUsers.Count;
            CurrStream.VIPsPresent = VIPUsers.Count;
            CurrStream.SubsPresent = SubUsers.Count;

            LogWriter.DebugLog("StreamOffline", DebugLogTypes.StatSystem, "Gathering user stats from the recent livestream.");

            // setting if user wants to save Stream Stat data
            if (OptionFlags.ManageStreamStats)
            {
                LogWriter.DebugLog("StreamOffline", DebugLogTypes.StatSystem, "Posting the final livestream stats.");
                DataManage.PostStreamStat(StreamStat.Create(CurrStream)); // create new instance due to clearing stats in async operation before posted to database
            }

            LogWriter.DebugLog("StreamOffline", DebugLogTypes.StatSystem, "Wrap up clearing the stream stats.");

            ClearStreamStatState();
        }

        /// <summary>
        /// Clears stream stats and user details for a stream
        /// </summary>
        private static void ClearStreamStatState()
        {
            LogWriter.DebugLog("ClearStreamStatState", DebugLogTypes.StatSystem, "Clear all of the stream data lists.");

            CurrStream.Clear();
            ModUsers.Clear();
            SubUsers.Clear();
            VIPUsers.Clear();
            StreamViewers.EndStreamResetList();
        }

        #region Stream Stat Methods
        public static void AddFollow()
        {
            lock (CurrStream)
            {
                CurrStream.NewFollows++;
                StreamDataUpdate();
            }
        }

        public static void AddSub()
        {
            lock (CurrStream)
            {
                CurrStream.NewSubscribers++;
                StreamDataUpdate();
            }
        }

        public static void AddGiftSubs(int Gifted = 1)
        {
            lock (CurrStream)
            {
                CurrStream.GiftSubs += Gifted;
                StreamDataUpdate();
            }
        }

        public static void AddBits(int BitCount)
        {
            lock (CurrStream)
            {
                CurrStream.Bits += BitCount;
                StreamDataUpdate();
            }
        }

        public static void AddRaids()
        {
            lock (CurrStream)
            {
                CurrStream.Raids++;
                StreamDataUpdate();
            }
        }

        public static void AddHosted()
        {
            lock (CurrStream)
            {
                CurrStream.Hosted++;
                StreamDataUpdate();
            }
        }

        public static void AddUserBanned()
        {
            lock (CurrStream)
            {
                CurrStream.UsersBanned++;
                StreamDataUpdate();
            }
        }

        public static void AddUserTimedOut()
        {
            lock (CurrStream)
            {
                CurrStream.UsersTimedOut++;
                StreamDataUpdate();
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
                CurrStream.CommandMsgs++;
            }
        }

        public static void AddAutoEvents()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedEvents++;
                StreamDataUpdate();
            }
        }

        public static void AddAutoCommands()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedCommands++;
                StreamDataUpdate();
            }
        }

        public static void AddDiscord()
        {
            lock (CurrStream)
            {
                CurrStream.WebhookMsgs++;
            }
        }

        public static void AddClips()
        {
            lock (CurrStream)
            {
                CurrStream.ClipsMade++;
                StreamDataUpdate();
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
            LogWriter.DebugLog("AddDeathCounter", DebugLogTypes.StatSystem, "Received request to update death counter.");

            int result = -1;
            if (!UpdateDeathCounter)
            {
                LogWriter.DebugLog("AddDeathCounter", DebugLogTypes.StatSystem, "Starting a check to prevent multiple death counter updates.");

                UpdateDeathCounter = true;
                result = DataManage.PostDeathCounterUpdate(FormatData.AddEscapeFormat(Category));

                ThreadManager.CreateThreadStart("AddDeathCounter", () =>
                {
                    LogWriter.DebugLog("AddDeathCounter", DebugLogTypes.StatSystem, "Waiting for 30 seconds to prevent another death counter update.");

                    Thread.Sleep(30 * 1000); // wait 30 seconds
                    UpdateDeathCounter = false; // reset flag for next update

                    LogWriter.DebugLog("AddDeathCounter", DebugLogTypes.StatSystem, "Finished the 30 seconds. Ready to accept another death counter update.");
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
        internal static int ResetDeathCounter(int Counter)
        {
            LogWriter.DebugLog("ResetDeathCounter", DebugLogTypes.StatSystem, $"Request to update the current {Category} death counter to {Counter}.");

            return DataManage.PostDeathCounterUpdate(FormatData.AddEscapeFormat(Category), true, Counter);
        }

        #endregion

    }
}
