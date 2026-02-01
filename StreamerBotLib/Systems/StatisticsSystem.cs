
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

using System.Diagnostics;

namespace StreamerBotLib.Systems
{
    public partial class ActionSystem
    {
        private delegate void BotOperation();
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
                    DataManage.UserJoined(StreamViewers.GetCurrentActiveUsers(true), SpecifyTime.ToLocalTime());
                }
            }
        }

        public static bool CheckStreamDate(DateTime TimeStream)
        {
            LogWriter.DebugLog("CheckStreamTime", DebugLogTypes.StatSystem, "Checking if the stream time is already in the database.");
            return DataManage.CheckStreamDate(TimeStream);
        }

        public void SetCategory(CategoryData categoryData)
        {
            LogWriter.DebugLog("SetCategory", DebugLogTypes.SystemController, $"Setting category to {categoryData.CategoryName}.");
            if (CurrCategory != categoryData)
            {
                LogWriter.DebugLog("SetCategory", DebugLogTypes.SystemController, "Updating category.");
                CurrCategory = categoryData;
                Category = categoryData.CategoryName;
                DataManage.PostCategory(categoryData);
                UpdateCategory();
            }
            LogWriter.DebugLog("SetCategory", DebugLogTypes.StatSystem, $"Current category is {CurrCategory.CategoryName}.");
        }

        public void PostCategoryStream(CategoryData categoryData)
        {
            LogWriter.DebugLog("PostCategoryStream", DebugLogTypes.StatSystem, "Posting category stream data to the database.");
            DataManage.PostCategoryStream(categoryData);
        }

        public void PostViewerCategory(CategoryData categoryData)
        {
            LogWriter.DebugLog("PostViewerCategory", DebugLogTypes.StatSystem, "Posting viewer category data to the database.");

            DataManage.PostCategory(categoryData);
        }

        public void UserJoined(List<LiveUser> UserNames)
        {
            LogWriter.DebugLog("UserJoined", DebugLogTypes.SystemController, "User joined.");
            DateTime Curr = DateTime.Now.ToLocalTime();

            var FirstUsers = StreamViewers.AddUsersFirstJoinedChannel(UserNames);

            UserJoined(UserNames, Curr);
            StreamViewers.RegisterUsers(FirstUsers);

            if (OptionFlags.FirstUserJoinedMsg)
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.SystemController, "Checking for first user joined message.");
                foreach (LiveUser user in FirstUsers)
                {
                    LogWriter.DebugLog("UserJoined", DebugLogTypes.SystemController, "Sending first user joined message.");
                    UserWelcomeMessage(user);
                }
            }

            UpdateUserJoinedList();

            foreach (LiveUser L in StreamViewers.GetUsersLeft(UserNames))
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.SystemController, $"User left, {L.UserName}.");
                UserLeft(L, Curr);
            }
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        private void UserJoined(LiveUser User, DateTime CurrTime)
        {
            LogWriter.DebugLog("UserJoined", DebugLogTypes.StatSystem, "Adding to database a user now joined to the channel.");
            if (OptionFlags.IsStreamOnline)
            {
                CurrStream.MaxUsers = Math.Max(CurrStream.MaxUsers, StreamViewers.Count);
                if (OptionFlags.ManageUsers)
                {
                    DataManage.UserJoined([User], CurrTime);
                }
            }
        }

        /// <summary>
        /// Adds user to the database by name, or updates existing user, and the time they joined the channel
        /// </summary>
        /// <param name="User">User's DisplayName</param>
        /// <param name="CurrTime">The current time the user joined</param>
        /// <returns></returns>
        private void UserJoined(IEnumerable<LiveUser> User, DateTime CurrTime)
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

        private void UserWelcomeMessage(LiveUser User)
        {
            LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Checking user welcome message.");

            if ((!User.UserName.Equals(ChannelName, StringComparison.CurrentCultureIgnoreCase)
               && (!User.UserName.Equals(BotUserName, StringComparison.CurrentCultureIgnoreCase)))
               || OptionFlags.MsgWelcomeStreamer)
            {
                string msg = CheckWelcomeUser(User.UserId);

                ChannelEventActions selected = ChannelEventActions.UserJoined;

                if (OptionFlags.WelcomeCustomMsg)
                {
                    LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Using custom welcome message.");
                    selected =
                        IsFollower(User.UserName) ?
                        ChannelEventActions.SupporterJoined :
                            IsReturningUser(User) ?
                                ChannelEventActions.ReturnUserJoined : ChannelEventActions.UserJoined;
                }

                string TempWelcomeMsg = LocalizedMsgSystem.GetEventMsg(selected, out bool Enabled, out short Multi);

                msg = msg == "" ? TempWelcomeMsg : msg;

                LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, $"Welcome message: {msg}");

                LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, $"Checking if sending welcome message is enabled, {Enabled}.");
                if (Enabled)
                {
                    LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Sending welcome message.");
                    SendMessage(
                        VariableParser.ParseReplace(
                            msg,
                            VariableParser.BuildDictionary(
                                new Tuple<MsgVars, string>[]
                                    {
                                        new( MsgVars.user, User.UserName )
                                    }
                            )
                        )
                    , Repeat: Multi);
                }

                LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Checking for overlay event.");
                CheckForOverlayEvent(OverlayTypes.ChannelEvents, selected.ToString(), User);
            }

            if (OptionFlags.AutoShout)
            {
                LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Checking for auto shout.");
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new(() =>
                    {
                        CheckShout(User, out string response);
                    }));
                }
            }
        }
        private void UpdateUserJoinedList()
        {
            try
            {
                LogWriter.DebugLog("UpdateUserJoinedList", DebugLogTypes.SystemController, "Updating user joined list.");
                ThreadManager.CreateThreadStart("UpdateUserJoinedList", () =>
                {
                    ThreadManager.AddTaskToGUIDispatcher(() =>
                    {
                        _ = new BotOperation(() =>
                        {
                            LogWriter.DebugLog("UpdateUserJoinedList", DebugLogTypes.SystemController, "Updating GUI current users.");
                            UpdateGUICurrUsers();
                        });
                    }
                    );
                });
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, "UpdateUserJoinedList");
            }
        }

        public void ModJoined(string User)
        {
            LogWriter.DebugLog("ModJoined", DebugLogTypes.StatSystem, "Adding a moderator to the list of moderators.");
            if (OptionFlags.IsStreamOnline)
            {
                ModUsers.UniqueAdd(User);
            }
        }

        public void SubJoined(string User)
        {
            LogWriter.DebugLog("SubJoined", DebugLogTypes.StatSystem, "Adding a subscriber to the list of subscribers.");
            if (OptionFlags.IsStreamOnline)
            {
                SubUsers.UniqueAdd(User);
            }
        }

        public void VIPJoined(string User)
        {
            LogWriter.DebugLog("VIPJoined", DebugLogTypes.StatSystem, "Adding a VIP to the list of VIPs.");
            if (OptionFlags.IsStreamOnline)
            {
                VIPUsers.UniqueAdd(User);
            }
        }

        public void UserLeft(LiveUser User)
        {
            LogWriter.DebugLog("UserLeft", DebugLogTypes.StatSystem, "User left.");
            UserLeft(User, DateTime.Now.ToLocalTime());
            UpdateUserJoinedList();
        }

        private void UserLeft(LiveUser User, DateTime CurrTime)
        {
            LogWriter.DebugLog("UserLeft", DebugLogTypes.StatSystem, "Posting to the database a user that left the channel.");
            lock (StreamViewers)
            {
                PostDataUserLeft(User, CurrTime);
            }
        }

        public void ClearUserList(DateTime Stopped)
        {
            LogWriter.DebugLog("ClearUserList", DebugLogTypes.StatSystem, "Register current viewers as leaving the stream.");
            lock (StreamViewers)
            {
                foreach (LiveUser U in StreamViewers.GetCurrentActiveUsers(true))
                {
                    PostDataUserLeft(U, Stopped);
                }

                StreamViewers.GetUsersLeft([]);
            }
        }

        private void PostDataUserLeft(LiveUser User, DateTime CurrTime)
        {
            LogWriter.DebugLog("PostDataUserLeft", DebugLogTypes.StatSystem, "Posting to the database a user that left the channel.");
            if (OptionFlags.ManageUsers && OptionFlags.IsStreamOnline)
            {
                DataManage.UserLeft(User, CurrTime);
            }
        }

        public bool IsFollower(string User)
        {
            LogWriter.DebugLog("IsFollower", DebugLogTypes.StatSystem, "Checking if the user is a follower of the channel.");
            return DataManage.CheckFollower(User, CurrStream.StreamStart);
        }

        public bool IsReturningUser(LiveUser User)
        {
            LogWriter.DebugLog("IsReturningUser", DebugLogTypes.StatSystem, "Checking if the user is a returning user to the channel.");
            return DataManage.CheckUser(User, CurrStream.StreamStart);
        }

        #region Incoming-Outgoing - Raids

        public void PostIncomingRaid(LiveUser User, DateTime RaidTime, int Viewers, CategoryData GameName)
        {
            lock (ProcMsgQueue)
            {
                ProcMsgQueue.Enqueue(new(() =>
                {
                    LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Processing incoming raid.");
                    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out bool Enabled, out short Multi);
                    if (Enabled)
                    {
                        LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Sending message about incoming raid.");
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                            new(MsgVars.user, User.UserName ),
                            new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers)),
                            new(MsgVars.category, GameName.CategoryName )
                            });

                        SendMessage(VariableParser.ParseReplace(msg, dictionary), DataManage.GetEventAnnounce(ChannelEventActions.Raid), Multi);
                    }

                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Raid.ToString(), User);

                    UpdatedStat(StreamStatType.Raids, StreamStatType.AutoEvents);

                    if (OptionFlags.TwitchRaidShoutOut)
                    {
                        LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Posting raid shout out of incoming raider.");
                        CheckShout(User, out string response, false);
                    }
                }));
            }
            if (OptionFlags.ManageRaidData)
            {
                LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.StatSystem, "Posting the incoming raid data to the database.");
                DataManage.PostCategory(GameName); // ensure category exists
                DataManage.PostInRaidData(User, RaidTime, Viewers, GameName);
            }
            if (OptionFlags.ManageOverlayTicker)
            {
                LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Adding new overlay ticker item.");
                AddNewOverlayTickerItem(OverlayTickerItem.LastInRaid, User.UserName);
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            if (OptionFlags.ManageOutRaidData)
            {
                LogWriter.DebugLog("PostOutgoingRaid", DebugLogTypes.SystemController, "Posting outgoing raid data.");
                DataManage.PostOutgoingRaid(HostedChannel, dateTime);
            }
        }

        #endregion

        public List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            LogWriter.DebugLog("GetDiscordWebhooks", DebugLogTypes.StatSystem, "Retrieving the Discord webhooks for posting the current stream.");
            return DataManage.GetWebhooks(WebhooksSource.Discord, webhooksKind);
        }

#if DEBUG
        bool testAddFirstStream = false;
#endif

        public bool StreamOnline(DateTime Started)
        {
            LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Detected a new livestream and starting up activites.");

            CurrStream = new(); // start over

            StartElapsedTimerThread();

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
#if DEBUG
                    //Debug.Assert(testAddFirstStream == true, "The stream should've already been added.");
#endif
                    LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Retrieving the stream data from the database.");
                    CurrStream = DataManage.GetStreamData(Started);

                    found = true;
                }
                else
                {
#if DEBUG
                    Debug.Assert(testAddFirstStream == false, "The stream should be new and not already added.");
                    testAddFirstStream = true; // stream added
#endif

                    LogWriter.DebugLog("StreamOnline", DebugLogTypes.StatSystem, "Posting the stream data to the database.");
                    DataManage.PostStream(CurrStream.StreamStart, Category);
                    DataManage.PostCategoryStream(CurrCategory); // new stream, update category stream count

                    found = false;
                    NotifyRepeatManager_StreamOnline();
                }
            }
            MonitorWatchTime();
            StartCurrencyClock();

            CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Live.ToString(), null);

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

        public void StreamOffline(DateTime Stopped)
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

        public void ResetCategoryStreamCount()
        {
            LogWriter.DebugLog("ResetCategoryStreamCount", DebugLogTypes.StatSystem, "Resetting category stream count in the database.");
            DataManage.ResetCategoryStreamCount();
        }

        #region Stream Stat Methods
        public void AddFollow()
        {
            lock (CurrStream)
            {
                LogWriter.DebugLog("AddFollow", DebugLogTypes.StatSystem, "Adding a new follow to the current stream stats.");
                CurrStream.NewFollows++;
                StreamDataUpdate();
            }
        }

        public void AddSub()
        {
            lock (CurrStream)
            {
                LogWriter.DebugLog("AddSub", DebugLogTypes.StatSystem, "Adding a new subscription to the current stream stats.");
                CurrStream.NewSubscribers++;
                StreamDataUpdate();
            }
        }

        public void AddGiftSubs(int Gifted = 1)
        {
            lock (CurrStream)
            {
                LogWriter.DebugLog("AddGiftSubs", DebugLogTypes.StatSystem, "Adding new gift sub count to the current stream stats.");
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
                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    result = DataManage.PostDeathCounterUpdate(FormatData.AddEscapeFormat(Category));
                });

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
            int result = 0;
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                result = DataManage.PostDeathCounterUpdate(FormatData.AddEscapeFormat(Category), true, Counter);
            });
            return result;
        }

        #endregion

    }
}
