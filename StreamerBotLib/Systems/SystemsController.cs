using StreamerBotLib.BotClients;
using StreamerBotLib.DataSQL;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;

using System.Data;

namespace StreamerBotLib.Systems
{
    /// <summary>
    /// Primary entry controller point to the app systems, managing data updates for the actions performed during a stream
    /// </summary>
    public class SystemsController
    {
        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;
        public event EventHandler<BanUserRequestEventArgs> BanUserRequest;
        public event EventHandler<TwitchShoutOutUsersEventArgs> TwitchShoutOutUser;

        public static IDataManager DataManage { get; private set; }

        private static CategoryData CurrCategory { get; set; } = new("", "");

        private ActionSystem SystemActions { get; set; }

        private Queue<Task> ProcMsgQueue { get; set; } = new();
        private Thread ProcessMsgs;
        private bool ChatBotStarted;

        private const int SleepWait = 6000;

        private delegate void BotOperation();

        private bool GiveawayStarted = false;
        private readonly List<LiveUser> GiveawayCollectionList = [];

        private static ManageStreamViewers StreamViewers => ActionSystem.StreamViewers;

        /// <summary>
        /// Builds and initalizes the controller, instantiates all of the systems
        /// </summary>
        public SystemsController()
        {
            LogWriter.DebugLog(".ctor_SystemsController", DebugLogTypes.SystemController, "Initializing Systems Controller.");

            DataManage = new DataManagerSQL();
            ActionSystem.DataManage = DataManage;
            LocalizedMsgSystem.SetDataManager(DataManage);
            DataManage.Initialize();

            SystemActions = new();

            SystemActions.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;
            SystemActions.ProcessedCommand += Command_ProcessedCommand;
            SystemActions.TwitchShoutOutUser += SystemActions_TwitchShoutOutUser;

            DataManage.OnBulkFollowersAddFinished += DataManage_OnBulkFollowersAddFinished;
        }

        private void SystemActions_TwitchShoutOutUser(object sender, TwitchShoutOutUsersEventArgs e)
        {
            TwitchShoutOutUser?.Invoke(sender, e);
        }

#if DEBUG
        public void TestAddUsers()
        {
            LogWriter.DebugLog("TestAddUsers", DebugLogTypes.SystemController, "Adding test users to the system.");
            int getUsers = 20;
            Random random = new();

            UserJoined([.. ((IDataManagerTestMethods)DataManage).TestGetRandomUsers(random.Next(getUsers))]);

        }
#endif

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
        /// Closing actions when the application is exiting
        /// </summary>
        public void Exit()
        {
            LogWriter.DebugLog("Exit", DebugLogTypes.SystemController, "Conclude processing any commands.");
            ProcessMsgs?.Join();
            LogWriter.DebugLog("Exit", DebugLogTypes.SystemController, "Closing the database connection.");
            ((DataManagerSQL)DataManage).Exit();
        }

        #region Chatbot
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

        public void ActivateRepeatTimers()
        {
            LogWriter.DebugLog("ActivateRepeatTimers", DebugLogTypes.SystemController, "Activating repeat timers.");
            SystemActions.StartElapsedTimerThread();
        }

        public void NotifyBotStart()
        {
            LogWriter.DebugLog("NotifyBotStart", DebugLogTypes.SystemController, "Starting the bot.");
            ActionSystem.ClearUserList(DateTime.Now.ToLocalTime());

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
            SystemActions.StartElapsedTimerThread();
            ActionSystem.ManageLearnedMsgList();
        }

        public void NotifyBotStop()
        {
            LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.SystemController, "Stopping the bot.");
            ChatBotStarted = false;

            LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.SystemController, "Stopping the elapsed timer thread.");
            SystemActions.StopElapsedTimerThread();
            LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.SystemController, "Stopping the command processing thread.");
            ProcessMsgs?.Join();
        }

        #endregion

        #region Currency System

        #endregion

        #region Followers

        public static void StartBulkFollowers()
        {
            LogWriter.DebugLog("StartBulkFollowers", DebugLogTypes.SystemController, "Starting bulk followers procedure.");
            DataManage.StartBulkFollowers();
        }

        public static void UpdateFollowers(List<Follow> Follows)
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

        public static void StopBulkFollowers()
        {
            LogWriter.DebugLog("StopBulkFollowers", DebugLogTypes.SystemController, "Stopping bulk followers procedure.");
            DataManage.NotifyStopBulkFollowers();
        }

        private delegate void ProcFollowDelegate();

        public void AddNewFollowers(List<Follow> FollowList)
        {
            LogWriter.DebugLog("AddNewFollowers", DebugLogTypes.SystemController, "Adding new followers.");
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled, out _);
            FollowList.ForEach((f) => { f.Category = CurrCategory.CategoryName; }); // add category into follow object(s)
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
                foreach (Follow f in DataManage.PostFollowers(FollowList))
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

                                SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow.ToString(), f.FromUser, UserMsg: message);
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
                        SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow.ToString(), null, UserMsg: message);

                        i++;
                    }
                }
            }
        }

        #endregion

        #region Database Ops

        public static void ManageDatabase()
        {
            LogWriter.DebugLog("ManageDatabase", DebugLogTypes.SystemController, "Managing database.");
            ActionSystem.ManageDatabase();
        }

        public static void ClearWatchTime()
        {
            LogWriter.DebugLog("ClearWatchTime", DebugLogTypes.SystemController, "Clearing watch time.");
            ActionSystem.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            LogWriter.DebugLog("ClearAllCurrenciesValues", DebugLogTypes.SystemController, "Clearing all currency values.");
            ActionSystem.ClearAllCurrenciesValues();
        }

        internal static void ClearUsersNonFollowers()
        {
            LogWriter.DebugLog("ClearUsersNonFollowers", DebugLogTypes.SystemController, "Clearing non-followers.");
            ActionSystem.ClearUsersNonFollowers();
        }

        public static void SetSystemEventsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetSystemEventsEnabled", DebugLogTypes.SystemController, $"Setting system events enabled: {Enabled}.");
            ActionSystem.SetSystemEventsEnabled(Enabled);
        }

        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetBuiltInCommandsEnabled", DebugLogTypes.SystemController, $"Setting built-in commands enabled: {Enabled}.");
            ActionSystem.SetBuiltInCommandsEnabled(Enabled);
        }

        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetUserDefinedCommandsEnabled", DebugLogTypes.SystemController, $"Setting user-defined commands enabled: {Enabled}.");
            ActionSystem.SetUserDefinedCommandsEnabled(Enabled);
        }

        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetDiscordWebhooksEnabled", DebugLogTypes.SystemController, $"Setting Discord webhooks enabled: {Enabled}.");
            ActionSystem.SetDiscordWebhooksEnabled(Enabled);
        }

        public static void PostUpdatedDataRow(bool RowChanged)
        {
            LogWriter.DebugLog("PostUpdatedDataRow", DebugLogTypes.SystemController, $"Posting updated data row: {RowChanged}.");
            ActionSystem.PostUpdatedDataRow(RowChanged);
        }

        public static void DeleteRows(IEnumerable<DataRow> dataRows)
        {
            LogWriter.DebugLog("DeleteRows", DebugLogTypes.SystemController, $"Deleting {dataRows.Count()} rows.");
            ActionSystem.DeleteRows(dataRows);
        }

        public static void AddNewAutoShoutUser(string UserId, Platform platform)
        {
            LogWriter.DebugLog("AddNewAutoShoutUser", DebugLogTypes.SystemController, $"Adding new auto shout user: {UserId}.");
            ActionSystem.AddNewAutoShoutUser(UserId, platform);
        }

        public static void UpdateIsEnabledRows(IEnumerable<DataRow> dataRows, bool IsEnabled)
        {
            LogWriter.DebugLog("UpdateIsEnabledRows", DebugLogTypes.SystemController, $"Updating {dataRows.Count()} rows to enabled: {IsEnabled}.");
            ActionSystem.UpdatedIsEnabledRows(dataRows, IsEnabled);
        }

        public static bool CheckField(string dataTable, string FieldName)
        {
            LogWriter.DebugLog("CheckField", DebugLogTypes.SystemController, $"Checking field: {FieldName}.");
            return ActionSystem.CheckField(dataTable, FieldName);
        }

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksSource source, WebhooksKind webhooksKind)
        {
            LogWriter.DebugLog("GetDiscordWebhooks", DebugLogTypes.SystemController, $"Getting Discord webhooks for source: {source} and kind: {webhooksKind}.");
            return DataManage.GetWebhooks(source, webhooksKind);
        }

        internal static void DataGridUpdatedRow(object sender, AddNewRowEventArgs e)
        {
            LogWriter.DebugLog("DataGridUpdatedRow", DebugLogTypes.SystemController, "Updating data grid row.");
            DataManage.PostDataGridGUIAddRow(e.NewRow);
        }

        #endregion

        #region Mod Approval
        public static Tuple<string, string> GetApprovalRule(ModActionType actionType, string ActionName)
        {
            LogWriter.DebugLog("GetApprovalRule", DebugLogTypes.SystemController, $"Getting approval rule for {ActionName}.");
            return ActionSystem.GetApprovalRule(actionType, ActionName);
        }

        public void PostApproval(string Description, Task Action)
        {
            LogWriter.DebugLog("PostApproval", DebugLogTypes.SystemController, $"Posting approval for {Description}.");
            SystemActions.AddApprovalRequest(Description, Action);
        }

        #endregion

        #region Statistics

        public bool StreamOnline(DateTime CurrTime)
        {
            LogWriter.DebugLog("StreamOnline", DebugLogTypes.SystemController, "Starting stream.");
            bool streamstart = SystemActions.StreamOnline(CurrTime);

            SystemActions.StartElapsedTimerThread();

            LogWriter.DebugLog("StreamOnline", DebugLogTypes.SystemController, "Checking for overlay event.");
            SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Live.ToString(), null);

            return streamstart;
        }

        //private void BeginPostingStreamUpdates()
        //{
        //    ThreadManager.CreateThreadStart(() =>
        //    {
        //        while (OptionFlags.IsStreamOnline)
        //        {
        //            AppDispatcher.BeginInvoke(new BotOperation(() =>
        //            {
        //                ActionSystem.StreamDataUpdate();
        //            }), null);

        //            Thread.Sleep(SleepWait); // wait 6 seconds
        //        }
        //    });
        //}

        public static void StreamOffline(DateTime CurrTime)
        {
            LogWriter.DebugLog("StreamOffline", DebugLogTypes.SystemController, "Ending stream.");
            ActionSystem.StreamOffline(CurrTime);

            // reset category to empty, so next time stream starts, the "streamed category" counter
            // updates - a streamer may have consecutive streams with same category,
            // not doing this locks the counter from incrementing each stream
            CurrCategory = new("", "");
        }

        public static void SetCategory(CategoryData categoryData)
        {
            LogWriter.DebugLog("SetCategory", DebugLogTypes.SystemController, $"Setting category to {categoryData.CategoryName}.");
            if (CurrCategory != categoryData)
            {
                LogWriter.DebugLog("SetCategory", DebugLogTypes.SystemController, "Updating category.");
                ActionSystem.SetCategory(categoryData);
                CurrCategory = categoryData;
            }
            LogWriter.DebugLog("SetCategory", DebugLogTypes.SystemController, $"Current category is {CurrCategory.CategoryName}.");
        }

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
                    SystemActions.AddFollow();
                    break;
                case StreamStatType.Sub:
                    SystemActions.AddSub();
                    break;
                case StreamStatType.GiftSubs:
                    SystemActions.AddGiftSubs(Value);
                    break;
                case StreamStatType.Bits:
                    SystemActions.AddBits(Value);
                    break;
                case StreamStatType.Raids:
                    SystemActions.AddRaids();
                    break;
                case StreamStatType.Hosted:
                    SystemActions.AddHosted();
                    break;
                case StreamStatType.UserBanned:
                    SystemActions.AddUserBanned();
                    break;
                case StreamStatType.UserTimedOut:
                    SystemActions.AddUserTimedOut();
                    break;
                case StreamStatType.TotalChats:
                    SystemActions.AddTotalChats();
                    break;
                case StreamStatType.Commands:
                    SystemActions.AddCommands();
                    break;
                case StreamStatType.AutoEvents:
                    SystemActions.AddAutoEvents();
                    break;
                case StreamStatType.AutoCommands:
                    SystemActions.AddAutoCommands();
                    break;
                case StreamStatType.Discord:
                    SystemActions.AddDiscord();
                    break;
                case StreamStatType.Clips:
                    SystemActions.AddClips();
                    break;
                case StreamStatType.ChannelPtsCount:
                    SystemActions.AddChannelPtsCount();
                    break;
                case StreamStatType.ChannelChallenge:
                    SystemActions.AddChannelChallenge();
                    break;
            }
        }

        public void UserJoined(List<LiveUser> UserNames)
        {
            LogWriter.DebugLog("UserJoined", DebugLogTypes.SystemController, "User joined.");
            DateTime Curr = DateTime.Now.ToLocalTime();

            var FirstUsers = StreamViewers.AddUsersFirstJoinedChannel(UserNames);

            ActionSystem.UserJoined(UserNames, Curr);
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
                ActionSystem.UserLeft(L, Curr);
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
                            ActionSystem.UpdateGUICurrUsers();
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

        public void UserLeft(LiveUser User)
        {
            LogWriter.DebugLog("UserLeft", DebugLogTypes.SystemController, "User left.");
            ActionSystem.UserLeft(User, DateTime.Now.ToLocalTime());
            UpdateUserJoinedList();
        }

        #endregion

        #region User Related

        private void UserWelcomeMessage(LiveUser User)
        {
            LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Checking user welcome message.");

            if ((!User.UserName.Equals(ActionSystem.ChannelName, StringComparison.CurrentCultureIgnoreCase)
               && (!User.UserName.Equals(ActionSystem.BotUserName, StringComparison.CurrentCultureIgnoreCase)))
               || OptionFlags.MsgWelcomeStreamer)
            {
                string msg = ActionSystem.CheckWelcomeUser(User.UserId);

                ChannelEventActions selected = ChannelEventActions.UserJoined;

                if (OptionFlags.WelcomeCustomMsg)
                {
                    LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Using custom welcome message.");
                    selected =
                        ActionSystem.IsFollower(User.UserName) ?
                        ChannelEventActions.SupporterJoined :
                            ActionSystem.IsReturningUser(User) ?
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
                SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, selected.ToString(), User);
            }

            if (OptionFlags.AutoShout)
            {
                LogWriter.DebugLog("UserWelcomeMessage", DebugLogTypes.SystemController, "Checking for auto shout.");
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new(() =>
                    {
                        SystemActions.CheckShout(User, out string response);
                    }));
                }
            }
        }

        public void MessageReceived(CmdMessage MsgReceived, LiveUser User)
        {
            LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Message received.");
            UpdateUserStats(DBUserStats.Chats, User.UserId, User.Platform);

            MsgReceived.UserType = ActionSystem.ParsePermission(MsgReceived);

            if ((OptionFlags.ModerateUsersAction || OptionFlags.ModerateUsersWarn) && MsgReceived.DisplayName != OptionFlags.TwitchBotUserName)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Moderating message.");
                Tuple<ModActions, int, MsgTypes, BanReasons> action = SystemActions.ModerateMessage(MsgReceived);

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

            ActionSystem.AddChatString(MsgReceived.DisplayName, MsgReceived.Message);

            LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Updating statistics.");
            if (User.UserName != OptionFlags.TwitchBotUserName)
            {
                UpdatedStat(StreamStatType.TotalChats);
            }

            if (MsgReceived.IsSubscriber)
            {
                ActionSystem.SubJoined(MsgReceived.DisplayName);
            }
            if (MsgReceived.IsVip)
            {
                ActionSystem.VIPJoined(MsgReceived.DisplayName);
            }
            if (MsgReceived.IsModerator)
            {
                ActionSystem.ModJoined(MsgReceived.DisplayName);
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

            if (SystemActions.BlackJackActive)
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.SystemController, "Evaluating user message for blackjack game.");
                SystemActions.GameCheckBlackJackResponse(User, MsgReceived.Message);
            }

            #endregion
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
                        SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Bits.ToString(), User, User.UserName ?? "anonymous");
                    }));
                }

                AddNewOverlayTickerItem(OverlayTickerItem.LastBits, User.UserName ?? "anonymous");
            }
        }

        private void RequestBanUser(LiveUser User, BanReasons Reason, int Duration = 0)
        {
            LogWriter.DebugLog("RequestBanUser", DebugLogTypes.SystemController, "Requesting to ban user.");
            BanUserRequest?.Invoke(this, new() { User = User, BanReason = Reason, Duration = Duration });
        }

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
                            new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers))
                            });

                        SendMessage(VariableParser.ParseReplace(msg, dictionary), DataManage.GetEventAnnounce(ChannelEventActions.Raid), Multi);
                    }

                    SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Raid.ToString(), User);

                    UpdatedStat(StreamStatType.Raids, StreamStatType.AutoEvents);

                    if (OptionFlags.TwitchRaidShoutOut)
                    {
                        LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Posting raid shout out of incoming raider.");
                        SystemActions.CheckShout(User, out string response, false);
                    }
                }));
            }
            if (OptionFlags.ManageRaidData)
            {
                LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Posting incoming raid data.");
                ActionSystem.PostIncomingRaid(User, RaidTime, Viewers, GameName);
            }
            if (OptionFlags.ManageOverlayTicker)
            {
                LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.SystemController, "Adding new overlay ticker item.");
                AddNewOverlayTickerItem(OverlayTickerItem.LastInRaid, User.UserName);
            }
        }

        public static void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            if (OptionFlags.ManageOutRaidData)
            {
                LogWriter.DebugLog("PostOutgoingRaid", DebugLogTypes.SystemController, "Posting outgoing raid data.");
                DataManage.PostOutgoingRaid(HostedChannel, dateTime);
            }
        }

        public void ProcessCommand(CmdMessage cmdMessage, Platform Source)
        {
            try
            {
                LogWriter.DebugLog("ProcessCommand", DebugLogTypes.SystemController, "Processing command.");
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new Task(() =>
                    {
                        LogWriter.DebugLog("ProcessCommand", DebugLogTypes.SystemController, "Evaluating command.");
                        SystemActions.EvalCommand(cmdMessage, Source);
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

        #endregion

        #region Giveaway
        /// <summary>
        /// Initialize and start accepting giveaway entries
        /// </summary>
        public void BeginGiveaway()
        {
            LogWriter.DebugLog("BeginGiveaway", DebugLogTypes.SystemController, "Starting giveaway.");

            GiveawayStarted = true;
            GiveawayCollectionList.Clear();
            ActionSystem.GiveawayCollection.Clear();

            SendMessage(OptionFlags.GiveawayBegMsg);
        }

        /// <summary>
        /// Adds a viewer DisplayName to the active giveaway list. The giveaway must be started through <code>BeginGiveaway()</code>.
        /// </summary>
        /// <param name="DisplayName"></param>
        public void ManageGiveaway(LiveUser User)
        {
            LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Managing giveaway.");

            if (GiveawayStarted && ((OptionFlags.GiveawayMultiUser && GiveawayCollectionList.FindAll((e) => e == User).Count < OptionFlags.GiveawayMaxEntries) || GiveawayCollectionList.UniqueAdd(User)))
            {
                LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Adding user to giveaway list.");
                ActionSystem.GiveawayCollection.Add(User);
            }

            LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Checking for max entries for user.");
            while (GiveawayCollectionList.FindAll((e) => e == User).Count > OptionFlags.GiveawayMaxEntries)
            {
                LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.SystemController, "Removing extra user entries from giveaway list.");
                GiveawayCollectionList.RemoveAt(GiveawayCollectionList.FindLastIndex((s) => s == User));
            }
        }

        /// <summary>
        /// End the Giveaway event.
        /// </summary>
        public void EndGiveaway()
        {
            LogWriter.DebugLog("EndGiveaway", DebugLogTypes.SystemController, "Ending giveaway.");
            GiveawayStarted = false;
            SendMessage(OptionFlags.GiveawayEndMsg);
        }

        /// <summary>
        /// Pick a winner and send the winner notice to the channel chat
        /// </summary>
        public void PostGiveawayResult()
        {
            LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Posting giveaway result.");
            Random random = new();

            string DisplayName = "";

            if (GiveawayCollectionList.Count > 0)
            {
                List<LiveUser> WinnerList = [];
                int x = 0;
                while (x < OptionFlags.GiveawayCount)
                {
                    LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Picking winner.");
                    LiveUser winner = GiveawayCollectionList[random.Next(GiveawayCollectionList.Count)];
                    GiveawayCollectionList.RemoveAll((w) => w == winner);
                    WinnerList.Add(winner);
                    // DisplayName += (OptionFlags.GiveawayCount > 1 && x > 0 ? ", " : "") + winner;
                    if (OptionFlags.ManageGiveawayUsers)
                    {
                        LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Posting giveaway data to database.");
                        DataManage.PostGiveawayData(winner.UserId, DateTime.Now.ToLocalTime());
                    }
                    x++;
                }

                DisplayName = string.Join(", ", WinnerList);

                if (DisplayName != "")
                {
                    LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Sending winner message.");
                    SendMessage(
                        VariableParser.ParseReplace(
                            OptionFlags.GiveawayWinMsg ?? "",
                            VariableParser.BuildDictionary(
                                new Tuple<MsgVars, string>[]
                                {
                                new(MsgVars.winner, DisplayName)
                                }
                                )));

                    foreach (LiveUser W in WinnerList)
                    {
                        LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.SystemController, "Checking for overlay event.");
                        SystemActions.CheckForOverlayEvent(OverlayTypes.Giveaway, OverlayTypes.Giveaway.ToString(), W);
                    }

                }
            }
        }

        #endregion

        #region Clips
        public void ClipHelper(IEnumerable<Clip> Clips)
        {
            LogWriter.DebugLog("ClipHelper", DebugLogTypes.SystemController, "Processing clips.");
            foreach (Clip c in Clips)
            {
                if (ActionSystem.AddClip(c))
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

        #region Media Overlay Server

        public void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> NewOverlayeventHandler, EventHandler<UpdatedTickerItemsEventArgs> UpdatedTickerEventHandler)
        {
            LogWriter.DebugLog("SetNewOverlayEventHandler", DebugLogTypes.SystemController, "Setting new overlay event handlers.");
            SystemActions.NewOverlayEvent += NewOverlayeventHandler;
            ActionSystem.UpdatedTickerItems += UpdatedTickerEventHandler;
        }

        /// <summary>
        /// Initialize the ticker items. Called when first starting the overlay server.
        /// </summary>
        public void SendInitialTickerItems()
        {
            LogWriter.DebugLog("SendInitialTickerItems", DebugLogTypes.SystemController, "Sending initial ticker items.");
            SystemActions.SendInitialTickerItems();
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            LogWriter.DebugLog("GetOverlayActions", DebugLogTypes.SystemController, "Getting overlay actions.");
            return SystemActions.GetOverlayActions();
        }

        public void SetChannelRewardList(List<string> RewardList)
        {
            LogWriter.DebugLog("SetChannelRewardList", DebugLogTypes.SystemController, "Setting channel reward list.");
            SystemActions.SetChannelRewardList(RewardList);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, Enum enumvalue, LiveUser User, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.SystemController, "Checking for overlay event.");
            CheckForOverlayEvent(overlayType, enumvalue.ToString(), User, UserMsg, ProvidedURL, UrlDuration);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, LiveUser User, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.SystemController, "Checking for overlay event.");
            SystemActions.CheckForOverlayEvent(overlayType, Action, User, UserMsg, ProvidedURL, UrlDuration);
        }

        public static void AddNewOverlayTickerItem(OverlayTickerItem item, string UserName)
        {
            LogWriter.DebugLog("AddNewOverlayTickerItem", DebugLogTypes.SystemController, "Adding new overlay ticker item.");
            ActionSystem.AddNewOverlayTickerItem(item, UserName);
        }

        #endregion

        #region MultiLive 

        public void AddNewMonitorChannel(IEnumerable<LiveUser> liveUsers)
        {
            LogWriter.DebugLog("AddNewMonitorChannel", DebugLogTypes.SystemController, "Adding new monitor channel.");
            DataManage.PostMonitorChannel(liveUsers);
        }

        /// <summary>
        /// Summarize the multi-live data.
        /// </summary>
        /// <param name="multiLiveSummarizeEventArgs">Defines data, if null then all date records are summarized, and 
        /// a callback action to invoke after querying the database. 
        /// See also: <seealso cref="MultiLiveSummarizeEventArgs"/></param>
        public static void MultiSummarize(MultiLiveSummarizeEventArgs multiLiveSummarizeEventArgs)
        {
            LogWriter.DebugLog("MultiSummarize", DebugLogTypes.SystemController, "Summarizing multi-live data.");
            if (multiLiveSummarizeEventArgs.Data == null)
            {
                DataManage.SummarizeStreamData();
                multiLiveSummarizeEventArgs.CallbackAction.Invoke();
            }
            else
            {
                DataManage.SummarizeStreamData(multiLiveSummarizeEventArgs.Data);
                multiLiveSummarizeEventArgs.CallbackAction.Invoke();
            }
        }

        #endregion

        #region GUI

        /// <summary>
        /// Save the data grid edits to the database. If the edit involved a command, update the repeat command list.
        /// </summary>
        /// <param name="CommandUpdate">Notification if the edit involved a command.</param>
        public void GUISaveDataGridEdits(bool CommandUpdate)
        {
            LogWriter.DebugLog("GUISaveDataGridEdits", DebugLogTypes.SystemController, "Saving data grid edits.");
            DataManage.GUIRowEditSave();

            if (CommandUpdate)
            {
                SystemActions.UpdateCommandsChanged();
            }
        }

        #endregion
    }
}
