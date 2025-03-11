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
            LogWriter.DebugLog("ManageUsers", DebugLogTypes.StatSystem, "Starting the user management process.");
            ManageUsers(DateTime.Now.ToLocalTime());
            MonitorWatchTime();
        }

        public static void ManageUsers(DateTime SpecifyTime)
        {
            LogWriter.DebugLog("ManageUsers", DebugLogTypes.StatSystem, "Managing users at a specific time.");
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
            LogWriter.DebugLog("CheckStreamTime", DebugLogTypes.StatSystem, "Checking if the stream time is already in the database.");
            return DataManage.CheckMultiStreams(TimeStream);
        }

        public static void SetCategory(CategoryData categoryData)
        {
            LogWriter.DebugLog("SetCategory", DebugLogTypes.StatSystem, "Setting the current category for the stream.");
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
            LogWriter.DebugLog("UserJoined", DebugLogTypes.StatSystem, "Adding to database a user now joined to the channel.");
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
            LogWriter.DebugLog("UserJoined", DebugLogTypes.StatSystem, $"Adding to database {User.Count()} users now joined to the channel.");
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
            LogWriter.DebugLog("ModJoined", DebugLogTypes.StatSystem, "Adding a moderator to the list of moderators.");
            if (OptionFlags.IsStreamOnline)
            {
                ModUsers.UniqueAdd(User);
            }
        }

        public static void SubJoined(string User)
        {
            LogWriter.DebugLog("SubJoined", DebugLogTypes.StatSystem, "Adding a subscriber to the list of subscribers.");
            if (OptionFlags.IsStreamOnline)
            {
                SubUsers.UniqueAdd(User);
            }
        }

        public static void VIPJoined(string User)
        {
            LogWriter.DebugLog("VIPJoined", DebugLogTypes.StatSystem, "Adding a VIP to the list of VIPs.");
            if (OptionFlags.IsStreamOnline)
            {
                VIPUsers.UniqueAdd(User);
            }
        }

        public static void UserLeft(LiveUser User, DateTime CurrTime)
        {
            LogWriter.DebugLog("UserLeft", DebugLogTypes.StatSystem, "Posting to the database a user that left the channel.");
            lock (StreamViewers)
            {
                PostDataUserLeft(User, CurrTime);
            }
        }

        public static void ClearUserList(DateTime Stopped)
        {
            LogWriter.DebugLog("ClearUserList", DebugLogTypes.StatSystem, "Register current viewers as leaving the stream.");
            lock (StreamViewers)
            {
                foreach (LiveUser U in StreamViewers.GetCurrentActiveUsers())
                {
                    PostDataUserLeft(U, Stopped);
                }

                StreamViewers.GetUsersLeft([]);
            }
        }

        private static void PostDataUserLeft(LiveUser User, DateTime CurrTime)
        {
            LogWriter.DebugLog("PostDataUserLeft", DebugLogTypes.StatSystem, "Posting to the database a user that left the channel.");
            if (OptionFlags.ManageUsers && OptionFlags.IsStreamOnline)
            {
                DataManage.UserLeft(User, CurrTime);
            }
        }

        public static bool IsFollower(string User)
        {
            LogWriter.DebugLog("IsFollower", DebugLogTypes.StatSystem, "Checking if the user is a follower of the channel.");
            return DataManage.CheckFollower(User, CurrStream.StreamStart);
        }

        public static bool IsReturningUser(LiveUser User)
        {
            LogWriter.DebugLog("IsReturningUser", DebugLogTypes.StatSystem, "Checking if the user is a returning user to the channel.");
            return DataManage.CheckUser(User, CurrStream.StreamStart);
        }

        #region Incoming Raids

        public static void PostIncomingRaid(LiveUser liveUser, DateTime RaidTime, int Viewers, CategoryData GameName)
        {
            LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.StatSystem, "Posting the incoming raid data to the database.");
            DataManage.PostInRaidData(liveUser, RaidTime, Viewers, GameName);
        }

        #endregion

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            LogWriter.DebugLog("GetDiscordWebhooks", DebugLogTypes.StatSystem, "Retrieving the Discord webhooks for posting the current stream.");
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
                LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Checking if the stream time is already in the database.");
                // retrieve existing stream or start a new stream entry
                if (DataManage.CheckStreamTime(Started))
                {
                    LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Retrieving the stream data from the database.");
                    CurrStream = DataManage.GetStreamData(Started);

                    found = true;
                }
                else
                {
                    LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Posting the stream data to the database.");
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
        public void AddFollow()
        {
            lock (CurrStream)
            {
                CurrStream.NewFollows++;
                StreamDataUpdate();
            }
        }

        public void AddSub()
        {
            lock (CurrStream)
            {
                CurrStream.NewSubscribers++;
                StreamDataUpdate();
            }
        }

        public void AddGiftSubs(int Gifted = 1)
        {
            lock (CurrStream)
            {
                CurrStream.GiftSubs += Gifted;
                StreamDataUpdate();
            }
        }

        public void AddBits(int BitCount)
        {
            lock (CurrStream)
            {
                CurrStream.Bits += BitCount;
                StreamDataUpdate();
            }
        }

        public void AddRaids()
        {
            lock (CurrStream)
            {
                CurrStream.Raids++;
                StreamDataUpdate();
            }
        }

        public void AddHosted()
        {
            lock (CurrStream)
            {
                CurrStream.Hosted++;
                StreamDataUpdate();
            }
        }

        public void AddUserBanned()
        {
            lock (CurrStream)
            {
                CurrStream.UsersBanned++;
                StreamDataUpdate();
            }
        }

        public void AddUserTimedOut()
        {
            lock (CurrStream)
            {
                CurrStream.UsersTimedOut++;
                StreamDataUpdate();
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
                CurrStream.CommandMsgs++;
            }
        }

        public void AddAutoEvents()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedEvents++;
                StreamDataUpdate();
            }
        }

        public void AddAutoCommands()
        {
            lock (CurrStream)
            {
                CurrStream.AutomatedCommands++;
                StreamDataUpdate();
            }
        }

        public void AddDiscord()
        {
            lock (CurrStream)
            {
                CurrStream.WebhookMsgs++;
            }
        }

        public void AddClips()
        {
            lock (CurrStream)
            {
                CurrStream.ClipsMade++;
                StreamDataUpdate();
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
