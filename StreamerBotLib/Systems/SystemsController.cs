using StreamerBotLib.BotClients;
using StreamerBotLib.Data;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using StreamerBotLibMediaOverlayServer.Enums;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace StreamerBotLib.Systems
{
    /// <summary>
    /// Primary entry controller point to the app systems, managing data updates for the actions performed during a stream
    /// </summary>
    public class SystemsController
    {
        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;
        public event EventHandler<BanUserRequestEventArgs> BanUserRequest;

        // TODO: fix - "stream started and live, user clicks 'enable repeat timers', 'repeat timers' should restart"

        public static DataManager DataManage { get; private set; } = new();

        private Thread HoldNewFollowsForBulkAdd;

        private static Tuple<string, string> CurrCategory { get; set; } = new("", "");

        private ActionSystem SystemActions { get; set; }
        internal Dispatcher AppDispatcher { get; set; }

        private Queue<Task> ProcMsgQueue { get; set; } = new();
        private Thread ProcessMsgs;
        private bool ChatBotStarted;

        private const int SleepWait = 6000;

        private delegate void BotOperation();

        private bool GiveawayStarted = false;
        private readonly List<string> GiveawayCollectionList = new();

        /// <summary>
        /// Builds and initalizes the controller, instantiates all of the systems
        /// </summary>
        public SystemsController()
        {
            ActionSystem.DataManage = DataManage;
            LocalizedMsgSystem.SetDataManager(DataManage);
            DataManage.Initialize();
            SystemActions = new();

            SystemActions.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;
            SystemActions.ProcessedCommand += Command_ProcessedCommand;
        }

        private void ActionProcessCmds()
        {
            while (OptionFlags.ActiveToken && ChatBotStarted)
            {
                lock (ProcMsgQueue)
                {
                    while (ProcMsgQueue.Count > 0)
                    {
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
            ProcessMsgs?.Join();
        }

        #region Chatbot
        /// <summary>
        /// Handle if message is processed as multithreaded, due to one or more bot calls and wait for 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_ProcessedCommand(object sender, PostChannelMessageEventArgs e)
        {
            SendMessage(e.Msg, e.RepeatMsg);
        }

        public void SetDispatcher(Dispatcher dispatcher)
        {
            AppDispatcher = dispatcher;
        }

        private void SendMessage(string message, int Repeat = 0)
        {
            if (message is not "" and not "/me ")
            {
                PostChannelMessage?.Invoke(this, new() { Msg = message, RepeatMsg = Repeat });
            }
        }

        public void ActivateRepeatTimers()
        {
            SystemActions.StartElapsedTimerThread();
        }

        public void NotifyBotStart()
        {
            ActionSystem.ClearUserList(DateTime.Now.ToLocalTime());

            ChatBotStarted = true;

            // prevent starting another thread
            if (ProcessMsgs == null || !ProcessMsgs.IsAlive)
            {
                ProcessMsgs = ThreadManager.CreateThread(ActionProcessCmds, ThreadWaitStates.Wait, ThreadExitPriority.VeryHigh);
                ProcessMsgs.Start();
            }

            SystemActions.StartElapsedTimerThread();
            SystemActions.ManageLearnedMsgList();
        }

        public void NotifyBotStop()
        {
            ChatBotStarted = false;

            SystemActions.StopElapsedTimerThread();
            ProcessMsgs?.Join();
        }

        #endregion

        #region Currency System

        #endregion

        #region Followers

        public static void StartBulkFollowers()
        {
            DataManage.StartBulkFollowers();
        }

        public static void UpdateFollowers(IEnumerable<Follow> Follows)
        {
            DataManage.UpdateFollowers(Follows);
        }

        public static void StopBulkFollowers()
        {
            DataManage.StopBulkFollows();
        }

        private delegate void ProcFollowDelegate();

        public void AddNewFollowers(IEnumerable<Follow> FollowList)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled, out _);

            if (DataManage.UpdatingFollowers)
            { // capture any followers found after starting the bot and before completing the bulk follower load
                HoldNewFollowsForBulkAdd = ThreadManager.CreateThread(() =>
                {
                    while (DataManage.UpdatingFollowers && OptionFlags.ActiveToken) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

                    ProcessFollow(FollowList, msg, FollowEnabled);
                }, ThreadWaitStates.Wait, ThreadExitPriority.High);

                _ = AppDispatcher.BeginInvoke(new ProcFollowDelegate(PerformFollow));
            }
            else
            {
                ProcessFollow(FollowList, msg, FollowEnabled);
            }
        }

        private void PerformFollow()
        {
            HoldNewFollowsForBulkAdd.Start();
        }

        private void ProcessFollow(IEnumerable<Follow> FollowList, string msg, bool FollowEnabled)
        {
            if (OptionFlags.TwitchFollowerAutoBanBots && FollowList.Count() >= OptionFlags.TwitchFollowerAutoBanCount)
            {
                foreach (Follow F in FollowList)
                {
                    // TODO: FIX - because users will be banned just for bot retrieving data
                    //RequestBanUser(Bots.TwitchChatBot, F.FromUserName, BanReasons.FollowBot);
                    LogWriter.WriteLog(Enums.LogType.LogBotStatus, $"TwineBot would have banned {F.FromUserName}, testing experimental feature.");
                }
            }
            else
            {
                List<string> UserList = new();

                foreach (Follow f in FollowList.Where(f => DataManage.PostFollower(f.FromUser,f.FollowedAt.ToLocalTime())))
                {
                    if (OptionFlags.ManageFollowers)
                    {
                        if (FollowEnabled)
                        {
                            if (OptionFlags.TwitchFollowerEnableMsgLimit && FollowList.Count() >= OptionFlags.TwitchFollowerMsgLimit)
                            {
                                UserList.Add(f.FromUserName);
                            }
                            else
                            {
                                string message = VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) }));
                                SendMessage(message);

                                SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow.ToString(), f.FromUserName, UserMsg: message);
                            }
                        }

                        UpdatedStat(StreamStatType.Follow, StreamStatType.AutoEvents);
                    }
                }

                AddNewOverlayTickerItem(OverlayTickerItem.LastFollower, FollowList.Last().FromUserName);

                if (UserList.Count > 0)
                {
                    int Pick = 5;
                    int i = 0;

                    while (i * Pick < UserList.Count)
                    {
                        string message = VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, string.Join(',', UserList.Skip(i * Pick).Take(Pick))) }));
                        SendMessage(message);
                        SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow.ToString(), UserMsg: message);

                        i++;
                    }
                }
            }
        }

        #endregion

        #region Database Ops

        public static void ManageDatabase()
        {
            ActionSystem.ManageDatabase();
        }

        public static void ClearWatchTime()
        {
            ActionSystem.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            ActionSystem.ClearAllCurrenciesValues();
        }

        internal static void ClearUsersNonFollowers()
        {
            ActionSystem.ClearUsersNonFollowers();
        }

        public static void SetSystemEventsEnabled(bool Enabled)
        {
            ActionSystem.SetSystemEventsEnabled(Enabled);
        }

        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            ActionSystem.SetBuiltInCommandsEnabled(Enabled);
        }

        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            ActionSystem.SetUserDefinedCommandsEnabled(Enabled);
        }

        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            ActionSystem.SetDiscordWebhooksEnabled(Enabled);
        }

        public static void PostUpdatedDataRow(bool RowChanged)
        {
            ActionSystem.PostUpdatedDataRow(RowChanged);
        }

        public static void DeleteRows(IEnumerable<DataRow> dataRows)
        {
            ActionSystem.DeleteRows(dataRows);
        }

        public static void AddNewAutoShoutUser(string UserName)
        {
            ActionSystem.AddNewAutoShoutUser(UserName);
        }

        public static void UpdateIsEnabledRows(IEnumerable<DataRow> dataRows, bool IsEnabled)
        {
            ActionSystem.UpdatedIsEnabledRows(dataRows, IsEnabled);
        }

        public static bool CheckField(string dataTable, string FieldName)
        {
            return ActionSystem.CheckField(dataTable, FieldName);
        }

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(webhooksKind);
        }
     

        #endregion

        #region Mod Approval
        public static Tuple<string, string> GetApprovalRule(ModActionType actionType, string ActionName)
        {
            return ActionSystem.GetApprovalRule(actionType, ActionName);
        }

        public void PostApproval(string Description, Task Action)
        {
            SystemActions.AddApprovalRequest(Description, Action);
        }

        #endregion

        #region Statistics

        public bool StreamOnline(DateTime CurrTime)
        {
            bool streamstart = SystemActions.StreamOnline(CurrTime);

            if (OptionFlags.ManageStreamStats)
            {
                BeginPostingStreamUpdates();
            }

            SystemActions.StartElapsedTimerThread();

            SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Live.ToString());

            return streamstart;
        }

        private void BeginPostingStreamUpdates()
        {
            ThreadManager.CreateThreadStart(() =>
            {
                while (OptionFlags.IsStreamOnline)
                {
                    AppDispatcher.BeginInvoke(new BotOperation(() =>
                    {
                        SystemActions.StreamDataUpdate();
                    }), null);

                    Thread.Sleep(SleepWait); // wait 10 seconds
                }
            });
        }

        public static void StreamOffline(DateTime CurrTime)
        {
            ActionSystem.StreamOffline(CurrTime);
        }

        public static void SetCategory(string GameId, string GameName)
        {
            if (CurrCategory.Item1 != GameId && CurrCategory.Item2 != GameName)
            {
                ActionSystem.SetCategory(GameId, GameName);
                CurrCategory = new(GameId, GameName);
            }
        }

        public void UpdatedStat(params StreamStatType[] streamStatTypes)
        {
            foreach (StreamStatType s in streamStatTypes)
            {
                UpdatedStat(s);
            }
        }

        public void UpdatedStat(StreamStatType streamStat)
        {
            SystemActions.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.InvokeMethod, null, SystemActions, null);
        }

        public void UpdatedStat(StreamStatType streamStat, int value)
        {
            SystemActions.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.InvokeMethod, null, SystemActions, new object[] { value });
        }

        public void UserJoined(List<LiveUser> UserNames)
        {
            DateTime Curr = DateTime.Now.ToLocalTime();

            foreach (LiveUser user in UserNames)
            {
                if (RegisterJoinedUser(user, Curr, JoinedUserMsg: true))
                {
                    UserWelcomeMessage(user);
                }
            }
            UpdateUserJoinedList();
        }

        private void UpdateUserJoinedList()
        {
            try
            {
                ThreadManager.CreateThreadStart(() =>
                {
                    AppDispatcher.BeginInvoke(new BotOperation(() =>
                    {
                        ActionSystem.UpdateGUICurrUsers();
                    }));
                });
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void UserLeft(LiveUser User)
        {
            ActionSystem.UserLeft(User, DateTime.Now.ToLocalTime());
            UpdateUserJoinedList();
        }

        #endregion

        #region User Related
        private bool RegisterJoinedUser(LiveUser User, DateTime UserTime, bool JoinedUserMsg = false, bool ChatUserMessage = false)
        {
            bool FoundUserJoined = false;
            bool FoundUserChat = false;

            if (JoinedUserMsg) // use a straight flag for user to join the channel
            {
                FoundUserJoined = ActionSystem.UserJoined(User, UserTime);
            }

            if (ChatUserMessage)
            {
                // have to separate, else the user registered before actually registered

                FoundUserChat = ActionSystem.UserChat(User);
            }
            // use the OptionFlags.FirstUserJoinedMsg flag to determine the welcome message is through user joined
            return (OptionFlags.FirstUserJoinedMsg && FoundUserJoined) || (ChatUserMessage && FoundUserChat);
        }

        private void UserWelcomeMessage(LiveUser User)
        {
            if ((User.UserName.ToLower(CultureInfo.CurrentCulture) != ActionSystem.ChannelName.ToLower(CultureInfo.CurrentCulture) && (User.UserName.ToLower(CultureInfo.CurrentCulture) != ActionSystem.BotUserName?.ToLower(CultureInfo.CurrentCulture))) || OptionFlags.MsgWelcomeStreamer)
            {
                string msg = SystemActions.CheckWelcomeUser(User.UserName);

                ChannelEventActions selected = ChannelEventActions.UserJoined;

                if (OptionFlags.WelcomeCustomMsg)
                {
                    selected =
                        ActionSystem.IsFollower(User.UserName) ?
                        ChannelEventActions.SupporterJoined :
                            ActionSystem.IsReturningUser(User) ?
                                ChannelEventActions.ReturnUserJoined : ChannelEventActions.UserJoined;
                }

                string TempWelcomeMsg = LocalizedMsgSystem.GetEventMsg(selected, out bool Enabled, out short Multi);

                msg = msg == "" ? TempWelcomeMsg : msg;

                if (Enabled)
                {
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

                SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, selected.ToString(), User.UserName);
            }

            if (OptionFlags.AutoShout)
            {
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
            MsgReceived.UserType = ActionSystem.ParsePermission(MsgReceived);

            if ((OptionFlags.ModerateUsersAction || OptionFlags.ModerateUsersWarn) && MsgReceived.DisplayName != OptionFlags.TwitchBotUserName)
            {
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
                DataManage.PostLearnMsgsRow(MsgReceived.Message, MsgTypes.UnidentifiedChatInput);
            }

            ActionSystem.AddChatString(MsgReceived.DisplayName, MsgReceived.Message);
            UpdatedStat(StreamStatType.TotalChats);

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

            // handle bit cheers
            if (MsgReceived.Bits > 0)
            {
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new(() =>
                    {
                        string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out bool Enabled, out short Multi);
                        if (Enabled)
                        {
                            Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                            {
                                new(MsgVars.user, MsgReceived.DisplayName),
                                new(MsgVars.bits, FormatData.Plurality(MsgReceived.Bits, MsgVars.Pluralbits) )
                            });

                            SendMessage(VariableParser.ParseReplace(msg, dictionary), Multi);

                            UpdatedStat(StreamStatType.Bits, MsgReceived.Bits);
                            UpdatedStat(StreamStatType.AutoEvents);
                        }

                        SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Bits.ToString(), MsgReceived.DisplayName);
                    }));
                }

                AddNewOverlayTickerItem(OverlayTickerItem.LastBits, MsgReceived.DisplayName);
            }

            if (RegisterJoinedUser(User, DateTime.Now.ToLocalTime(), ChatUserMessage: OptionFlags.FirstUserChatMsg))
            {
                UserWelcomeMessage(User);
            }
        }

        private void RequestBanUser(LiveUser User, BanReasons Reason, int Duration = 0)
        {
            BanUserRequest?.Invoke(this, new() { User = User, BanReason = Reason, Duration = Duration });
        }

        public void PostIncomingRaid(LiveUser User, DateTime RaidTime, string Viewers, string GameName)
        {
            lock (ProcMsgQueue)
            {
                ProcMsgQueue.Enqueue(new(() =>
                {
                    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out bool Enabled, out short Multi);
                    if (Enabled)
                    {
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                            new(MsgVars.user, User.UserName ),
                            new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers))
                            });

                        SendMessage(VariableParser.ParseReplace(msg, dictionary), Multi);
                    }

                    SystemActions.CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Raid.ToString(), User.UserName);

                    UpdatedStat(StreamStatType.Raids, StreamStatType.AutoEvents);

                    if (OptionFlags.TwitchRaidShoutOut)
                    {
                        SystemActions.CheckShout(User, out string response, false);
                    }
                }));
            }
            if (OptionFlags.ManageRaidData)
            {
                ActionSystem.PostIncomingRaid(User.UserName, RaidTime, Viewers, GameName);
            }
            if (OptionFlags.ManageOverlayTicker)
            {
                DataManage.UpdateOverlayTicker(OverlayTickerItem.LastInRaid, User.UserName);
            }
        }

        public static void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            if (OptionFlags.ManageOutRaidData)
            {
                DataManage.PostOutgoingRaid(HostedChannel, dateTime);
            }
        }

        public void ProcessCommand(CmdMessage cmdMessage, Platform Source)
        {
            try
            {
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new Task(() =>
                    {
                        SystemActions.EvalCommand(cmdMessage, Source);
                    }));
                }
            }
            catch (InvalidOperationException InvalidOp)
            {
                LogWriter.LogException(InvalidOp, MethodBase.GetCurrentMethod().Name);
                SendMessage(InvalidOp.Message);
            }
            catch (NullReferenceException NullRef)
            {
                LogWriter.LogException(NullRef, MethodBase.GetCurrentMethod().Name);
                SendMessage(NullRef.Message);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void ProcessCommands_OnRepeatEventOccured(object sender, TimerCommandsEventArgs e)
        {
            if (OptionFlags.RepeatTimer && (!OptionFlags.RepeatWhenLive || OptionFlags.IsStreamOnline))
            {
                short x = 0;

                do
                {
                    SendMessage(e.Message);
                    x++;
                } while (x <= e.RepeatMsg);
            }
            UpdatedStat(StreamStatType.AutoCommands);
        }

        #endregion

        #region Giveaway
        /// <summary>
        /// Initialize and start accepting giveaway entries
        /// </summary>
        public void BeginGiveaway()
        {
            GiveawayStarted = true;
            GiveawayCollectionList.Clear();
            ActionSystem.GiveawayCollection.Clear();

            SendMessage(OptionFlags.GiveawayBegMsg);
        }

        /// <summary>
        /// Adds a viewer DisplayName to the active giveaway list. The giveaway must be started through <code>BeginGiveaway()</code>.
        /// </summary>
        /// <param name="DisplayName"></param>
        public void ManageGiveaway(string DisplayName)
        {
            if (GiveawayStarted && ((OptionFlags.GiveawayMultiUser && GiveawayCollectionList.FindAll((e) => e == DisplayName).Count < OptionFlags.GiveawayMultiEntries) || GiveawayCollectionList.UniqueAdd(DisplayName)))
            {
                ActionSystem.GiveawayCollection.Add(DisplayName);
            }

            while (GiveawayCollectionList.FindAll((e) => e == DisplayName).Count > OptionFlags.GiveawayMultiEntries)
            {
                GiveawayCollectionList.RemoveAt(GiveawayCollectionList.FindLastIndex((s) => s == DisplayName));
            }
        }

        /// <summary>
        /// End the Giveaway event.
        /// </summary>
        public void EndGiveaway()
        {
            GiveawayStarted = false;
            SendMessage(OptionFlags.GiveawayEndMsg);
        }

        /// <summary>
        /// Pick a winner and send the winner notice to the channel chat
        /// </summary>
        public void PostGiveawayResult()
        {
            Random random = new();

            string DisplayName = "";

            if (GiveawayCollectionList.Count > 0)
            {
                List<string> WinnerList = new();
                int x = 0;
                while (x < OptionFlags.GiveawayCount)
                {
                    string winner = GiveawayCollectionList[random.Next(GiveawayCollectionList.Count)];
                    GiveawayCollectionList.RemoveAll((w) => w == winner);
                    WinnerList.Add(winner);
                    // DisplayName += (OptionFlags.GiveawayCount > 1 && x > 0 ? ", " : "") + winner;
                    x++;
                }

                DisplayName = string.Join(", ", WinnerList);

                if (DisplayName != "")
                {
                    SendMessage(
                        VariableParser.ParseReplace(
                            OptionFlags.GiveawayWinMsg ?? "",
                            VariableParser.BuildDictionary(
                                new Tuple<MsgVars, string>[]
                                {
                                new(MsgVars.winner, DisplayName)
                                }
                                )));

                    foreach (string W in WinnerList)
                    {
                        SystemActions.CheckForOverlayEvent(OverlayTypes.Giveaway, OverlayTypes.Giveaway.ToString(), W);
                    }

                    if (OptionFlags.ManageGiveawayUsers)
                    {
                        DataManage.PostGiveawayData(DisplayName, DateTime.Now.ToLocalTime());
                    }
                }
            }
        }

        #endregion

        #region Clips
        public void ClipHelper(IEnumerable<Clip> Clips)
        {
            foreach (Clip c in Clips)
            {
                if (ActionSystem.AddClip(c))
                {
                    if (OptionFlags.TwitchClipPostChat)
                    {
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
                        foreach (Tuple<bool, Uri> u in GetDiscordWebhooks(WebhooksKind.Clips))
                        {
                            DiscordWebhook.SendMessage(u.Item2, c.Url);
                            UpdatedStat(StreamStatType.Discord, StreamStatType.AutoEvents); // count how many times posted to Discord
                        }
                    }

                    UpdatedStat(StreamStatType.Clips, StreamStatType.AutoEvents);

                    //CheckForOverlayEvent(OverlayTypes.Clip, OverlayTypes.Clip, ProvidedURL: c.Url);
                }
            }
        }

        #endregion

        #region Media Overlay Server

        public void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> eventHandler)
        {
            SystemActions.NewOverlayEvent += eventHandler;
        }

        //public void SetChannelClipsHandler(EventHandler<GetChannelClipsEventArgs> eventHandler)
        //{
        //    Overlay.GetChannelClipsEvent += eventHandler;
        //}

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            return SystemActions.GetOverlayActions();
        }

        public void SetChannelRewardList(List<string> RewardList)
        {
            SystemActions.SetChannelRewardList(RewardList);
        }

        //public void CheckForOverlayEvent(object? sender, CheckOverlayEventArgs e)
        //{
        //    CheckForOverlayEvent(e.OverlayType, e.Action, e.UserName, e.UserMsg, e.ProvidedURL);
        //}

        public void CheckForOverlayEvent(OverlayTypes overlayType, Enum enumvalue, string UserName = null, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            CheckForOverlayEvent(overlayType, enumvalue.ToString(), UserName, UserMsg, ProvidedURL, UrlDuration);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, string UserName = null, string UserMsg = null, string ProvidedURL = null, float UrlDuration = 0)
        {
            SystemActions.CheckForOverlayEvent(overlayType, Action, UserName, UserMsg, ProvidedURL, UrlDuration);
        }

        public static void AddNewOverlayTickerItem(OverlayTickerItem item, string UserName)
        {
            if (OptionFlags.ManageOverlayTicker)
            {
                DataManage.UpdateOverlayTicker(item, UserName);
            }
        }

        #endregion

    }
}
