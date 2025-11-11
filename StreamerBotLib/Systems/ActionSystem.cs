using StreamerBotLib.BotClients;
using StreamerBotLib.DataSQL;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Documents;

namespace StreamerBotLib.Systems
{
    /// <summary>
    /// The common shared operations class between each of the subsystems. 
    /// Should not be referenced outside of <c>StreamerBotLib.Systems</c> namespace.
    /// Perform direct DataManager tasks here.
    /// Each Subsystem class derives from this base class and can access the 
    /// DataManager and static properties here to share data between systems.
    /// </summary>
    public partial class ActionSystem
    {
        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;
        public event EventHandler<BanUserRequestEventArgs> BanUserRequest;

        public static IDataManager DataManage { get; set; }
        public FlowDocument ChatData { get; private set; } = new();
        public ObservableCollection<UserJoin> JoinCollection { get; set; } = [];
        public ObservableCollection<LiveUser> GiveawayCollection { get; set; } = [];
        public ObservableCollection<string> CurrUserJoin { get; private set; } = [];

        private static CategoryData CurrCategory { get; set; } = new("", "");

        public static string Category { get; set; }
        /// <summary>
        /// The streamer channel monitored.
        /// </summary>
        public static string ChannelName => OptionFlags.TwitchChannelName;
        /// <summary>
        /// The account user name of the bot account.
        /// </summary>
        public static string BotUserName => OptionFlags.TwitchBotUserName;
        /// <summary>
        /// Time delays to use in threads
        /// </summary>
        protected const int SecondsDelay = 4000;
        private const int SleepWait = 6000;

        private bool ChatBotStarted;

        private Queue<Task> ProcMsgQueue { get; set; } = new();
        private Thread ProcessMsgs;

        internal static ManageStreamViewers StreamViewers { get; } = new();

        protected static List<string> ModUsers { get; private set; } = [];
        protected static List<string> SubUsers { get; private set; } = [];
        protected static List<string> VIPUsers { get; private set; } = [];

        protected static StreamStat CurrStream { get; set; } = new();

        /// <summary>
        /// Returns the start of the current active online stream.
        /// </summary>
        /// <returns>The DateTime of the stream start time.</returns>
        private static DateTime GetCurrentStreamStart => CurrStream.StreamStart;

        private delegate void ProcMessage(string UserName, string Message);

        public ActionSystem()
        {
            DataManage = new DataManagerSQL();
            LocalizedMsgSystem.SetDataManager(DataManage);

            ProcessedCommand += Command_ProcessedCommand;

            RepeatManager = new(this);
            RepeatManager.OnRepeatCheckStopped += RepeatManager_OnRepeatCheckStopped;

            DataManage.OnBulkFollowersAddFinished += DataManage_OnBulkFollowersAddFinished;
        }

        public async Task InitializeDataManager()
        {
            await ((DataManagerSQL)DataManage).InitializeDataManager();
        }

        public void InitializeDataManagerCollectionUpdateEvent(EventHandler<OnDataCollectionUpdatedEventArgs> eventHandler)
        {
            DataManage.OnDataCollectionUpdated += eventHandler;
        }

        public void InitializeUpdatedMonitoringChannelsEvent(EventHandler eventHandler)
        {
            ((DataManagerSQL)DataManage).UpdatedMonitoringChannels += eventHandler;
        }

        public object GetICollection(DataTables dataTable)
        {
            LogWriter.DebugLog("GetICollection", DebugLogTypes.CommonSystem, $"Getting ICollection for DataTable: {dataTable}");
            return DataManage.GetICollection(dataTable);
        }

        public void SetCleanupList(ref List<ArchiveMultiStream> archiveMultiStreams)
        {
            LogWriter.DebugLog("SetCleanupList", DebugLogTypes.SystemController, "Setting cleanup list for multi-live streams.");
            DataManage.SetCleanupList(ref archiveMultiStreams);
        }

        /// <summary>
        /// Closing actions when the application is exiting
        /// </summary>
        public void Exit()
        {
            LogWriter.DebugLog("Exit", DebugLogTypes.SystemController, "Conclude processing any commands.");
            ProcessMsgs?.Join();
            LogWriter.DebugLog("Exit", DebugLogTypes.SystemController, "Closing the database connection.");
            ((DataManagerSQL)DataManage).Exit();
        }


        #region Database Ops
        public void ManageDatabase()
        {
            LogWriter.DebugLog("ManageDatabase", DebugLogTypes.CommonSystem, "Managing Database.");
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            if (!OptionFlags.ManageRaidData)
            {
                DataManage.RemoveAllInRaidData();
            }

            if (!OptionFlags.ManageOutRaidData)
            {
                DataManage.RemoveAllOutRaidData();
            }

            if (!OptionFlags.ManageGiveawayUsers)
            {
                DataManage.RemoveAllGiveawayData();
            }

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            if (!OptionFlags.ManageFollowers)
            {
                DataManage.RemoveAllFollowers();
            }

            if (!OptionFlags.ManageUsers)
            {
                // if ManageUsers is False, then remove users!
                DataManage.RemoveAllUsers();
            }

            // when management resumes, code upstream enables the startbot process

            //  if ManageStreamStats is False, then remove all Stream Statistics!

            if (!OptionFlags.ManageStreamStats)
            {
                // when the LiveStream Online event fires again, the datacollection will restart
                //  if ManageStreamStats is False, then remove all Stream Statistics!
                DataManage.RemoveAllStreamStats();
                // when the LiveStream Online event fires again, the datacollection will restart
            }

            if (!OptionFlags.ManageOverlayTicker)
            {
                DataManage.RemoveAllOverlayTickerData();
            }
        }

        public void PostDataGridGUIAddRow(IDatabaseTableMeta NewRow)
        {
            LogWriter.DebugLog("PostDataGridGUIAddRow", DebugLogTypes.CommonSystem, "Posting DataGrid GUI Add Row.");
            DataManage.PostDataGridGUIAddRow(NewRow);
        }

        public void ClearWatchTime()
        {
            LogWriter.DebugLog("ClearWatchTime", DebugLogTypes.CommonSystem, "Clearing Watch Time.");
            DataManage.ClearWatchTime();
        }

        public void ClearAllCurrenciesValues()
        {
            LogWriter.DebugLog("ClearAllCurrenciesValues", DebugLogTypes.CommonSystem, "Clearing All Currency Values.");
            DataManage.ClearAllCurrencyValues();
        }

        public void ClearUsersNonFollowers()
        {
            LogWriter.DebugLog("ClearUsersNonFollowers", DebugLogTypes.CommonSystem, "Clearing Users Non-Followers.");
            DataManage.ClearUsersNotFollowers();
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetSystemEventsEnabled", DebugLogTypes.CommonSystem, $"Setting System Events Enabled: {Enabled}");
            DataManage.SetSystemEventsEnabled(Enabled);
        }

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetBuiltInCommandsEnabled", DebugLogTypes.CommonSystem, $"Setting Built-In Commands Enabled: {Enabled}");
            DataManage.SetBuiltInCommandsEnabled(Enabled);
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetUserDefinedCommandsEnabled", DebugLogTypes.CommonSystem, $"Setting User Defined Commands Enabled: {Enabled}");
            DataManage.SetUserDefinedCommandsEnabled(Enabled);
        }

        public void SetDiscordWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetDiscordWebhooksEnabled", DebugLogTypes.CommonSystem, $"Setting Discord Webhooks Enabled: {Enabled}");
            DataManage.SetWebhooksEnabled(Enabled);
        }

        public static void PostUpdatedDataRow(bool RowChanged)
        {
            LogWriter.DebugLog("PostUpdatedDataRow", DebugLogTypes.CommonSystem, $"Posting Updated DataRow: {RowChanged}");
            //DataManage.PostUpdatedDataRow(RowChanged);
        }

        public void DeleteRows(IEnumerable<object> dataRows, string TableName)
        {
            LogWriter.DebugLog("DeleteRows", DebugLogTypes.CommonSystem, $"Deleting Rows: {dataRows.Count()}");
            DataManage.DeleteDataRows(dataRows, TableName);
        }

        public void AddNewAutoShoutUser(string UserId, Platform platform)
        {
            LogWriter.DebugLog("AddNewAutoShoutUser", DebugLogTypes.CommonSystem, $"Adding New AutoShout User: {UserId}");
            DataManage.PostNewAutoShoutUser(UserId, platform);
        }

        internal bool CheckField(string dataTable, string fieldName)
        {
            LogWriter.DebugLog("CheckField", DebugLogTypes.CommonSystem, $"Checking Field: {fieldName}");
            return DataManage.CheckField(dataTable, fieldName);
        }

        public List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksSource source, WebhooksKind webhooksKind)
        {
            LogWriter.DebugLog("GetDiscordWebhooks", DebugLogTypes.SystemController, $"Getting Discord webhooks for source: {source} and kind: {webhooksKind}.");
            return DataManage.GetWebhooks(source, webhooksKind);
        }

        internal void DataGridUpdatedRow(object sender, AddNewRowEventArgs e)
        {
            LogWriter.DebugLog("DataGridUpdatedRow", DebugLogTypes.SystemController, "Updating data grid row.");
            DataManage.PostDataGridGUIAddRow(e.NewRow);
        }

        public bool GetEventAnnounce(ChannelEventActions channelEventAction)
        {
            return DataManage.GetEventAnnounce(channelEventAction);
        }

        public string GetUserId(LiveUser user)
        {
            LogWriter.DebugLog("GetUserId", DebugLogTypes.CommonSystem, $"Getting User ID for: {user.UserName}");
            return DataManage.GetUserId(user);
        }

        public List<CategoryData> GetGameCategories()
        {
            LogWriter.DebugLog("GetGameCategories", DebugLogTypes.CommonSystem, "Getting Game Categories.");
            return DataManage.GetGameCategories();
        }

        #endregion

        public bool AddClip(Clip c, bool LastClip)
        {
            LogWriter.DebugLog("AddClip", DebugLogTypes.CommonSystem, $"Adding Clip: {c.Title}");
            return DataManage.PostClip(c.ClipId, DateTime.Parse(c.CreatedAt).ToLocalTime(), (decimal)c.Duration, c.GameId, c.Language, c.Title, c.Url, c.FromUserId, c.FromUserName, LastClip);
        }

        /// <summary>
        /// Retrieves the current users within the channel during the stream.
        /// </summary>
        /// <returns>The current user count as of now.</returns>
        public int GetUserCount
        {
            get
            {
                LogWriter.DebugLog("GetUserCount", DebugLogTypes.CommonSystem, "Getting User Count.");
                lock (StreamViewers)
                {
                    return StreamViewers.GetCurrentActiveUsers(true).Count;
                }
            }
        }

        /// <summary>
        /// Retrieve how many chats have occurred in the current live stream to now.
        /// </summary>
        /// <returns>Current total chats as of now.</returns>
        public int GetCurrentChatCount
        {
            get
            {
                LogWriter.DebugLog("GetCurrentChatCount", DebugLogTypes.CommonSystem, "Getting Current Chat Count.");
                lock (CurrStream)
                {
                    return CurrStream.TotalChats;
                }
            }
        }

        public void UpdateGUICurrUsers()
        {
            LogWriter.DebugLog("UpdateGUICurrUsers", DebugLogTypes.CommonSystem, "Updating GUI Current Users.");
            CurrUserJoin.Clear();
            var curr = StreamViewers.GetCurrentActiveUsers(true);
            curr.Sort();

            foreach (LiveUser liveUser in curr)
            {
                CurrUserJoin.Add(liveUser.UserName);
            }
        }

        internal void AddChatString(string UserName, string Message)
        {
            LogWriter.DebugLog("AddChatString", DebugLogTypes.CommonSystem, $"Adding Chat String: {UserName}: {Message}");
            ThreadManager.AddTaskToGUIDispatcher(() => UpdateGUIChatMessages(UserName, Message));
        }

        private void UpdateGUIChatMessages(string UserName, string Message)
        {
            LogWriter.DebugLog("UpdateGUIChatMessages", DebugLogTypes.CommonSystem, $"Updating GUI Chat Messages: {UserName}: {Message}");

            Paragraph p = new();
            string time = DateTime.Now.ToLocalTime().ToString("h:mm", CultureInfo.CurrentCulture) + " ";
            p.Inlines.Add(new Run(time));
            p.Inlines.Add(new Run(UserName + ": "));
            p.Inlines.Add(new Run(Message));
            //p.Foreground = new SolidColorBrush(Color.FromArgb(a: s.Color.A,
            //                                                  r: s.Color.R,
            //                                                  g: s.Color.G,
            //                                                  b: s.Color.B));
            ChatData.Blocks.Add(p);
        }

        internal void OutputSentToBotsHandler(object sender, PostChannelMessageEventArgs e)
        {
            LogWriter.DebugLog("OutputSentToBotsHandler", DebugLogTypes.CommonSystem, $"Output Sent To Bots Handler: {e.Msg}");
            AddChatString(Settings.Default.TwitchBotUserName, e.Msg);
        }

        #region Chatbot

        private void ActionProcessCmds()
        {
            LogWriter.DebugLog("ActionProcessCmds", DebugLogTypes.SystemController, "Starting the command processing thread.");
            while (OptionFlags.ActiveToken && ChatBotStarted)
            {
                lock (ProcMsgQueue)
                {
                    while (ProcMsgQueue.Count > 0)
                    {
                        LogWriter.DebugLog("ActionProcessCmds", DebugLogTypes.SystemController, "Processing command from queue.");
                        ProcMsgQueue.Dequeue().Start();
                    }
                    Thread.Sleep(300);
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Handle if message is processed as multithreaded, due to one or more bot calls and wait for 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_ProcessedCommand(object sender, PostChannelMessageEventArgs e)
        {
            LogWriter.DebugLog("Command_ProcessedCommand", DebugLogTypes.SystemController, "Processing command message.");
            SendMessage(e.Msg, e.Announcement, e.RepeatMsg);
        }

        private void SendMessage(string message, bool Announcement = false, int Repeat = 0)
        {
            LogWriter.DebugLog("SendMessage", DebugLogTypes.SystemController, "Sending message.");
            if (message is not "" and not "/me ")
            {
                LogWriter.DebugLog("SendMessage", DebugLogTypes.SystemController, $"Sending message: {message}");
                PostChannelMessage?.Invoke(this, new() { Msg = message, Announcement = Announcement, RepeatMsg = Repeat });
            }
        }


        public void NotifyBotStart()
        {
            LogWriter.DebugLog("NotifyBotStart", DebugLogTypes.SystemController, "Starting the bot.");
            ClearUserList(DateTime.Now.ToLocalTime());

            ChatBotStarted = true;

            LogWriter.DebugLog("NotifyBotStart", DebugLogTypes.SystemController, "Checking the command processing thread.");
            // prevent starting another thread
            if (ProcessMsgs == null || !ProcessMsgs.IsAlive)
            {
                LogWriter.DebugLog("NotifyBotStart", DebugLogTypes.SystemController, "Starting the command processing thread.");
                ProcessMsgs = ThreadManager.CreateThread("NotifyBotStart", ActionProcessCmds, ThreadWaitStates.Wait, ThreadExitPriority.VeryHigh);
                ProcessMsgs.Start();
            }

            LogWriter.DebugLog("NotifyBotStart", DebugLogTypes.SystemController, "Starting the elapsed timer thread.");
            StartElapsedTimerThread();
            ManageLearnedMsgList();
        }

        public void NotifyBotStop()
        {
            LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.SystemController, "Stopping the bot.");
            ChatBotStarted = false;

            LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.SystemController, "Stopping the elapsed timer thread.");
            StopElapsedTimerThread();
            LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.SystemController, "Stopping the command processing thread.");
            ProcessMsgs?.Join();
        }

        #endregion

        #region Statistics

        public void UpdatedStat(params StreamStatType[] streamStatTypes)
        {
            LogWriter.DebugLog("UpdatedStat", DebugLogTypes.SystemController, "Updating statistics.");
            foreach (StreamStatType s in streamStatTypes)
            {
                UpdatedStat(s);
            }
        }

        public void UpdatedStat(StreamStatType streamStat, int Value = 0)
        {
            LogWriter.DebugLog("UpdatedStat", DebugLogTypes.SystemController, $"Updating statistic: {streamStat}{(Value == 0 ? "" : $", with value: {Value}")}.");

            switch (streamStat)
            {
                case StreamStatType.Follow:
                    AddFollow();
                    break;
                case StreamStatType.Sub:
                    AddSub();
                    break;
                case StreamStatType.GiftSubs:
                    AddGiftSubs(Value);
                    break;
                case StreamStatType.Bits:
                    AddBits(Value);
                    break;
                case StreamStatType.Raids:
                    AddRaids();
                    break;
                case StreamStatType.Hosted:
                    AddHosted();
                    break;
                case StreamStatType.UserBanned:
                    AddUserBanned();
                    break;
                case StreamStatType.UserTimedOut:
                    AddUserTimedOut();
                    break;
                case StreamStatType.TotalChats:
                    AddTotalChats();
                    break;
                case StreamStatType.Commands:
                    AddCommands();
                    break;
                case StreamStatType.AutoEvents:
                    AddAutoEvents();
                    break;
                case StreamStatType.AutoCommands:
                    AddAutoCommands();
                    break;
                case StreamStatType.Discord:
                    AddDiscord();
                    break;
                case StreamStatType.Clips:
                    AddClips();
                    break;
                case StreamStatType.ChannelPtsCount:
                    AddChannelPtsCount();
                    break;
                case StreamStatType.ChannelChallenge:
                    AddChannelChallenge();
                    break;
            }
        }

        #endregion

        #region User Related

#if DEBUG
        public void TestAddUsers()
        {
            LogWriter.DebugLog("TestAddUsers", DebugLogTypes.SystemController, "Adding test users to the system.");
            int getUsers = 20;
            Random random = new();

            UserJoined([.. ((IDataManagerTestMethods)DataManage).TestGetRandomUsers(random.Next(getUsers))]);
        }
#endif

        public void ProcessCommand(CmdMessage cmdMessage, Platform Source)
        {
            ThreadManager.AddTaskToGUIDispatcher(() =>
            {
                try
                {
                    LogWriter.DebugLog("ProcessCommand", DebugLogTypes.SystemController, "Processing command.");
                    lock (ProcMsgQueue)
                    {
                        ProcMsgQueue.Enqueue(new Task(() =>
                        {
                            LogWriter.DebugLog("ProcessCommand", DebugLogTypes.SystemController, "Evaluating command.");
                            EvalCommand(cmdMessage, Source);
                        }));
                    }
                }
                catch (InvalidOperationException InvalidOp)
                {
                    LogWriter.LogException(InvalidOp, "ProcessCommand");
                    SendMessage(InvalidOp.Message);
                }
                catch (NullReferenceException NullRef)
                {
                    LogWriter.LogException(NullRef, "ProcessCommand");
                    SendMessage(NullRef.Message);
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "ProcessCommand");
                }
            });
        }

        private void ProcessCommands_OnRepeatEventOccured(object sender, TimerCommandsEventArgs e)
        {
            if (OptionFlags.RepeatTimerCommands && (!OptionFlags.RepeatWhenLive || OptionFlags.IsStreamOnline))
            {
                LogWriter.DebugLog("ProcessCommands_OnRepeatEventOccured", DebugLogTypes.SystemController, "Processing repeat event.");
                short x = 0;

                do
                {
                    SendMessage(e.Message);
                    x++;
                } while (x <= e.RepeatMsg);
            }
            UpdatedStat(StreamStatType.AutoCommands);
        }

        public void UpdateUserStats(DBUserStats dBUserStats, string userId, Platform platform)
        {
            LogWriter.DebugLog("UpdateUserStats", DebugLogTypes.SystemController, "Updating user statistics in database.");
            DataManage.UpdateStats(dBUserStats, userId, platform);
        }

        private void RequestBanUser(LiveUser User, BanReasons Reason, int Duration = 0)
        {
            LogWriter.DebugLog("RequestBanUser", DebugLogTypes.SystemController, "Requesting to ban user.");
            BanUserRequest?.Invoke(this, new() { User = User, BanReason = Reason, Duration = Duration });
        }

        public void UserCheered(LiveUser User, int Bits)
        {
            // handle bit cheers
            if (Bits > 0)
            {
                LogWriter.DebugLog("UserCheered", DebugLogTypes.SystemController, "User cheered.");
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new(() =>
                    {
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out bool Enabled, out short Multi);
                        if (Enabled)
                        {
                            LogWriter.DebugLog("UserCheered", DebugLogTypes.SystemController, "Sending message about user cheering.");
                            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                            {
                                new(MsgVars.user, User.UserName ?? "anonymous"),
                                new(MsgVars.bits, FormatData.Plurality(Bits, MsgVars.Pluralbits) )
                            });

                            SendMessage(VariableParser.ParseReplace(msg, dictionary), DataManage.GetEventAnnounce(ChannelEventActions.Bits), Multi);

                            UpdatedStat(StreamStatType.Bits, Bits);
                            UpdatedStat(StreamStatType.AutoEvents);
                        }

                        LogWriter.DebugLog("UserCheered", DebugLogTypes.SystemController, "Checking for overlay event.");
                        CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Bits.ToString(), User, User.UserName ?? "anonymous");
                    }));
                }

                AddNewOverlayTickerItem(OverlayTickerItem.LastBits, User.UserName ?? "anonymous");
            }
        }

        public void MessageReceived(CmdMessage MsgReceived, LiveUser User)
        {
            LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Message received.");
            UpdateUserStats(DBUserStats.Chats, User.UserId, User.Platform);

            MsgReceived.UserType = ParsePermission(MsgReceived);

            if ((OptionFlags.ModerateUsersAction || OptionFlags.ModerateUsersWarn) && MsgReceived.DisplayName != OptionFlags.TwitchBotUserName)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Moderating message.");
                Tuple<ModActions, int, MsgTypes, BanReasons> action = ModerateMessage(MsgReceived);

                if (OptionFlags.ModerateUsersWarn)
                {
                    if (action.Item1 is ModActions.Ban or ModActions.Timeout)
                    {
                        SendMessage($"Moderator should {action.Item1} User for {action.Item4} due to {action.Item3} message.");
                    }
                    else if (action.Item3 == MsgTypes.LearnMore)
                    {
                        SendMessage("I am unable to make a determination. Please teach me more so I can better decide.");
                    }
                }
                else if (OptionFlags.ModerateUsersAction)
                {
                    LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Requesting ban or timeout.");
                    // don't fix it yet
                    if (action.Item1 == ModActions.Ban)
                    {
                        RequestBanUser(User, action.Item4);
                    }
                    else if (action.Item1 == ModActions.Timeout)
                    {
                        RequestBanUser(User, action.Item4, action.Item2);
                    }
                }
            }

            if (OptionFlags.ModerateUserLearnMsgs)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Posting a new message to learned.");
                DataManage.PostLearnMsgsRow(MsgReceived.Message, MsgTypes.UnidentifiedChatInput);
            }

            AddChatString(MsgReceived.DisplayName, MsgReceived.Message);

            LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Updating statistics.");
            if (User.UserName != OptionFlags.TwitchBotUserName)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, $"Incrementing total chat count for user {User.UserName}.");
                UpdatedStat(StreamStatType.TotalChats);
            }

            if (MsgReceived.IsSubscriber)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, $"User {User.UserName} is a subscriber.");
                SubJoined(MsgReceived.DisplayName);
            }
            if (MsgReceived.IsVip)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, $"User {User.UserName} is a VIP.");
                VIPJoined(MsgReceived.DisplayName);
            }
            if (MsgReceived.IsModerator)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, $"User {User.UserName} is a moderator.");
                ModJoined(MsgReceived.DisplayName);
            }

            if (OptionFlags.FirstUserChatMsg)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Checking for first user chat message.");
                foreach (LiveUser user in StreamViewers.AddUsersFirstChatMessage([User]))
                {
                    UserWelcomeMessage(user);
                }
            }

            #region Currency Games

            if (BlackJackActive)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Evaluating user message for blackjack game.");
                GameCheckBlackJackResponse(User, MsgReceived.Message);
            }

            #endregion
        }

        #endregion

        #region Followers

        public void StartBulkFollowers()
        {
            LogWriter.DebugLog("StartBulkFollowers", DebugLogTypes.SystemController, "Starting bulk followers procedure.");
            DataManage.StartBulkFollowers();
        }

        public void UpdateFollowers(List<Follow> Follows)
        {
            LogWriter.DebugLog("UpdateFollowers", DebugLogTypes.SystemController, "Updating followers.");
            Follows.ForEach((f) => { f.Category = CurrCategory.CategoryName; });
            DataManage.UpdateFollowers(Follows);
        }

        private void DataManage_OnBulkFollowersAddFinished(object sender, OnBulkFollowersAddFinishedEventArgs e)
        {
            LogWriter.DebugLog("DataManage_OnBulkFollowersAddFinished", DebugLogTypes.SystemController, "Notifying bulk followers.");
            AddNewOverlayTickerItem(OverlayTickerItem.LastFollower, e.LastFollowerUserName);
        }

        public void StopBulkFollowers()
        {
            LogWriter.DebugLog("StopBulkFollowers", DebugLogTypes.SystemController, "Stopping bulk followers procedure.");
            DataManage.NotifyStopBulkFollowers();
        }

        private delegate void ProcFollowDelegate();

        public void AddNewFollowers(List<Follow> FollowList)
        {
            LogWriter.DebugLog("AddNewFollowers", DebugLogTypes.SystemController, "Adding new followers.");
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled, out _);
            FollowList.ForEach((f) => { f.Category = f.Category ?? CurrCategory.CategoryName; }); // add category into follow object(s)
            ProcessFollow(FollowList, msg, FollowEnabled);
        }

        private void ProcessFollow(IEnumerable<Follow> FollowList, string msg, bool FollowEnabled)
        {
            LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Processing new followers.");
            if (OptionFlags.TwitchFollowerAutoBanBots && FollowList.Count() >= OptionFlags.TwitchFollowerAutoBanCount)
            {
                foreach (Follow F in FollowList)
                {
                    // TODO: FIX - because users will be banned just for bot retrieving data
                    //RequestBanUser(Bots.TwitchChatBot, F.FromUserName, BanReasons.FollowBot);
                    LogWriter.WriteLog($"TwineBot would have banned {F.FromUserName}, testing experimental feature.");
                }
            }
            else
            {
                List<string> UserList = [];

                LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Evaluating new followers.");

                IEnumerable<Follow> ResultList;

                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    ResultList = DataManage.PostFollowers(FollowList);

                    foreach (Follow f in ResultList)
                    {
                        if (OptionFlags.ManageFollowers)
                        {
                            LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Managing new followers.");
                            if (FollowEnabled)
                            {
                                if (OptionFlags.TwitchFollowerEnableMsgLimit && FollowList.Count() >= OptionFlags.TwitchFollowerMsgLimit)
                                {
                                    LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Adding user to group follower list.");
                                    UserList.Add(f.FromUserName);
                                }
                                else
                                {
                                    LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Sending message about user.");
                                    string message = VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) }));
                                    SendMessage(message);

                                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow.ToString(), f.FromUser, UserMsg: message);
                                }
                            }

                            LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Updating statistics.");
                            UpdatedStat(StreamStatType.Follow, StreamStatType.AutoEvents);
                        }
                    }

                    LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Checking for overlay follower event.");
                    AddNewOverlayTickerItem(OverlayTickerItem.LastFollower, FollowList.Last().FromUserName);


                    if (UserList.Count > 0)
                    {
                        LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Processing group follower list.");
                        int Pick = 5;
                        int i = 0;

                        LogWriter.DebugLog("ProcessFollow", DebugLogTypes.SystemController, "Sending to channel a group multi-follower message, i.e. 1 message with n-followers.");
                        while (i * Pick < UserList.Count)
                        {
                            string message = VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, string.Join(',', UserList.Skip(i * Pick).Take(Pick))) }));
                            SendMessage(message);
                            CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow.ToString(), null, UserMsg: message);

                            i++;
                        }
                    }
                });
            }
        }

        #endregion

        #region GUI

        /// <summary>
        /// Save the data grid edits to the database. If the edit involved a command, update the repeat command list.
        /// </summary>
        /// <param name="CommandUpdate">Notification if the edit involved a command.</param>
        public void GUISaveDataGridEdits(bool CommandUpdate, string TableName)
        {
            LogWriter.DebugLog("GUISaveDataGridEdits", DebugLogTypes.SystemController, "Saving data grid edits.");
            DataManage.GUIRowEditSave(TableName);

            if (CommandUpdate)
            {
                UpdateCommandsChanged();
            }
        }

        public void UpdateRepeatCommands()
        {
            UpdateCommandsChanged();
        }

        #endregion

        #region Clips
        public void ClipHelper(IEnumerable<Clip> Clips)
        {
            LogWriter.DebugLog("ClipHelper", DebugLogTypes.SystemController, "Processing clips.");
            foreach (Clip c in Clips)
            {
                if (AddClip(c, Clips.Last() == c))
                {
                    if (OptionFlags.TwitchClipPostChat)
                    {
                        LogWriter.DebugLog("ClipHelper", DebugLogTypes.SystemController, "Posting clip to chat.");
                        lock (ProcMsgQueue)
                        {
                            ProcMsgQueue.Enqueue(new Task(() =>
                            {
                                SendMessage(c.Url);
                            }));
                        }
                    }

                    if (OptionFlags.TwitchClipPostDiscord)
                    {
                        LogWriter.DebugLog("ClipHelper", DebugLogTypes.SystemController, "Posting clip to Discord.");
                        foreach (Tuple<bool, Uri> u in GetDiscordWebhooks(WebhooksSource.Discord, WebhooksKind.Clips))
                        {
                            // TODO: add into database->enable adding data
                            DiscordWebhook.SendMessage(u.Item2, c.Url);
                            UpdatedStat(StreamStatType.Discord, StreamStatType.AutoEvents); // count how many times posted to WebHooks
                        }
                    }

                    LogWriter.DebugLog("ClipHelper", DebugLogTypes.SystemController, "Updating statistics.");
                    UpdatedStat(StreamStatType.Clips, StreamStatType.AutoEvents);

                    // CheckForOverlayEvent(OverlayTypes.Clip, OverlayTypes.Clip, ProvidedURL: c.Url);
                }
            }
        }

        #endregion
    }
}
