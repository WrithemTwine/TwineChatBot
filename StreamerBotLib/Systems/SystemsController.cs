using MediaOverlayServer.Enums;

using StreamerBotLib.BotClients;
using StreamerBotLib.Data;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

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

        private static Tuple<string, string> CurrCategory { get; set; } = new("","");

        private StatisticsSystem Stats { get; set; }
        private CommandSystem Command { get; set; }
        private CurrencySystem Currency { get; set; }
        private ModerationSystem Moderation { get; set; }
        private OverlaySystem Overlay { get; set; }

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
            SystemsBase.DataManage = DataManage;
            LocalizedMsgSystem.SetDataManager(DataManage);
            DataManage.Initialize();
            Stats = new();
            Command = new();
            Currency = new();
            Moderation = new();
            Overlay = new();

            Command.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;
            Command.ProcessedCommand += Command_ProcessedCommand;
            Stats.BeginCurrencyClock += Stats_BeginCurrencyClock;
            Stats.BeginWatchTime += Stats_BeginWatchTime;
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

        public void NotifyBotStart()
        {
            StatisticsSystem.ClearUserList(DateTime.Now.ToLocalTime());

            ChatBotStarted = true;

            // prevent starting another thread
            if (ProcessMsgs == null || !ProcessMsgs.IsAlive)
            {
                ProcessMsgs = ThreadManager.CreateThread(ActionProcessCmds, ThreadWaitStates.Wait, ThreadExitPriority.VeryHigh);
                ProcessMsgs.Start();
            }

            Command.StartElapsedTimerThread();
            Moderation.ManageLearnedMsgList();
        }

        public void NotifyBotStop()
        {
            ChatBotStarted = false;

            Command.StopElapsedTimerThread();
            ProcessMsgs?.Join();
        }

        #endregion

        #region Currency System

        private void Stats_BeginWatchTime(object sender, EventArgs e)
        {
            Currency.MonitorWatchTime();
        }

        private void Stats_BeginCurrencyClock(object sender, EventArgs e)
        {
            Currency.StartCurrencyClock();
        }

        #endregion

        #region Followers

        public static void StartBulkFollowers()
        {
            DataManage.StartFollowers();
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
                foreach(Follow F in FollowList)
                {
                    // TODO: FIX - because users will be banned just for bot retrieving data
                    //RequestBanUser(Bots.TwitchChatBot, F.FromUserName, BanReasons.FollowBot);
                    LogWriter.WriteLog(Enums.LogType.LogBotStatus, $"TwineBot would have banned {F.FromUserName}, testing experimental feature.");
                }
            }
            else
            {
                List<string> UserList = new();

                foreach (Follow f in FollowList.Where(f => DataManage.AddFollower(f.FromUserName, f.FollowedAt.ToLocalTime())))
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
                                SendMessage(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) })));
                            }
                        }

                        UpdatedStat(StreamStatType.Follow, StreamStatType.AutoEvents);
                    }

                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.NewFollow, f.FromUserName);
                }

                if (UserList.Count > 0)
                {
                    int Pick = 5;
                    int i = 0;

                    while (i * Pick < UserList.Count)
                    {
                        SendMessage(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, string.Join(',', UserList.Skip(i * Pick).Take(Pick))) })));
                        i++;
                    }
                }

            }
        }

        #endregion

        #region Database Ops

        public static void ManageDatabase()
        {
            SystemsBase.ManageDatabase();
        }

        public static void ClearWatchTime()
        {
            SystemsBase.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            SystemsBase.ClearAllCurrenciesValues();
        }

        internal static void ClearUsersNonFollowers()
        {
            SystemsBase.ClearUsersNonFollowers();
        }

        public static void SetSystemEventsEnabled(bool Enabled)
        {
            SystemsBase.SetSystemEventsEnabled(Enabled);
        }

        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            SystemsBase.SetBuiltInCommandsEnabled(Enabled);
        }

        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            SystemsBase.SetUserDefinedCommandsEnabled(Enabled);
        }

        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            SystemsBase.SetDiscordWebhooksEnabled(Enabled);
        }

        public static void PostUpdatedDataRow(bool RowChanged)
        {
            SystemsBase.PostUpdatedDataRow(RowChanged);
        }

        public static void DeleteRows(IEnumerable<DataRow> dataRows)
        {
            SystemsBase.DeleteRows(dataRows);
        }

        public static void AddNewAutoShoutUser(string UserName)
        {
            SystemsBase.AddNewAutoShoutUser(UserName);
        }

        #endregion

        #region Statistics

        public bool StreamOnline(DateTime CurrTime)
        {
            bool streamstart = Stats.StreamOnline(CurrTime);

            if(OptionFlags.ManageStreamStats)
            {
                BeginPostingStreamUpdates();
            }

            CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Live);

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
                        Stats.StreamDataUpdate();
                    }), null);

                    Thread.Sleep(SleepWait); // wait 10 seconds
                }
            });
        }

        public static void StreamOffline(DateTime CurrTime)
        {
            StatisticsSystem.StreamOffline(CurrTime);
        }

        public static void SetCategory(string GameId, string GameName)
        {
            if (CurrCategory.Item1 != GameId && CurrCategory.Item2 != GameName)
            {
                StatisticsSystem.SetCategory(GameId, GameName);
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
            Stats.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.InvokeMethod, null, Stats, null);
        }

        public void UpdatedStat(StreamStatType streamStat, int value)
        {
            Stats.GetType()?.InvokeMember("Add" + streamStat.ToString(), BindingFlags.InvokeMethod, null, Stats, new object[] { value });
        }

        public void UserJoined(List<string> UserNames, Bots Source)
        {
            DateTime Curr = DateTime.Now.ToLocalTime();

            foreach (string user in UserNames)
            {
                if (RegisterJoinedUser(user, Curr, Source, JoinedUserMsg: true))
                {
                    UserWelcomeMessage(user, Source);
                }
            }
            UpdateUserJoinedList();
        }

        private void UpdateUserJoinedList()
        {
            ThreadManager.CreateThreadStart(() =>
            {
                AppDispatcher.BeginInvoke(new BotOperation(() =>
                {
                    SystemsBase.UpdateGUICurrUsers();
                }));
            });
        }

        public void UserLeft(string UserName, Bots Source)
        {
            StatisticsSystem.UserLeft(UserName, DateTime.Now.ToLocalTime(), Source);
            UpdateUserJoinedList();
        }

        #endregion

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(webhooksKind);
        }

        private bool RegisterJoinedUser(string UserName, DateTime UserTime, Bots Source, bool JoinedUserMsg = false, bool ChatUserMessage = false)
        {
            bool FoundUserJoined = false;
            bool FoundUserChat = false;

            if (JoinedUserMsg) // use a straight flag for user to join the channel
            {
                FoundUserJoined = StatisticsSystem.UserJoined(UserName, UserTime, Source);
            }

            if (ChatUserMessage)
            {
                // have to separate, else the user registered before actually registered

                FoundUserChat = StatisticsSystem.UserChat(UserName);
            }
            // use the OptionFlags.FirstUserJoinedMsg flag to determine the welcome message is through user joined
            return (OptionFlags.FirstUserJoinedMsg && FoundUserJoined) || (ChatUserMessage && FoundUserChat);
        }

        private void UserWelcomeMessage(string UserName, Bots Source)
        {
            if ((UserName.ToLower(CultureInfo.CurrentCulture) != SystemsBase.ChannelName.ToLower(CultureInfo.CurrentCulture) && (UserName.ToLower(CultureInfo.CurrentCulture) != SystemsBase.BotUserName?.ToLower(CultureInfo.CurrentCulture))) || OptionFlags.MsgWelcomeStreamer)
            {
                string msg = Command.CheckWelcomeUser(UserName);

                ChannelEventActions selected = ChannelEventActions.UserJoined;

                if (OptionFlags.WelcomeCustomMsg)
                {
                    selected =
                        StatisticsSystem.IsFollower(UserName) ?
                        ChannelEventActions.SupporterJoined :
                            StatisticsSystem.IsReturningUser(UserName) ?
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
                                        new( MsgVars.user, UserName )
                                    }
                            )
                        )
                    , Repeat: Multi);
                }

                CheckForOverlayEvent(OverlayTypes.ChannelEvents, selected, UserName);
            }

            if (OptionFlags.AutoShout)
            {
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new(() =>
                    {
                        Command.CheckShout(UserName, out string response, Source);
                    }));
                }
            }
        }

        public void MessageReceived(CmdMessage MsgReceived, Bots Source)
        {
            MsgReceived.UserType = CommandSystem.ParsePermission(MsgReceived);

            if ((OptionFlags.ModerateUsersAction || OptionFlags.ModerateUsersWarn) && MsgReceived.DisplayName != OptionFlags.TwitchBotUserName)
            {
                Tuple<ModActions, int, MsgTypes, BanReasons> action = Moderation.ModerateMessage(MsgReceived);

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
                        RequestBanUser(Source, MsgReceived.DisplayName, action.Item4);
                    }
                    else if (action.Item1 == ModActions.Timeout)
                    {
                        RequestBanUser(Source, MsgReceived.DisplayName, action.Item4, action.Item2);
                    }
                }
            }

            if (OptionFlags.ModerateUserLearnMsgs)
            {
                DataManage.AddLearnMsgsRow(MsgReceived.Message, MsgTypes.UnidentifiedChatInput);
            }

            SystemsBase.AddChatString(MsgReceived.DisplayName, MsgReceived.Message);
            UpdatedStat(StreamStatType.TotalChats);

            if (MsgReceived.IsSubscriber)
            {
                StatisticsSystem.SubJoined(MsgReceived.DisplayName);
            }
            if (MsgReceived.IsVip)
            {
                StatisticsSystem.VIPJoined(MsgReceived.DisplayName);
            }
            if (MsgReceived.IsModerator)
            {
                StatisticsSystem.ModJoined(MsgReceived.DisplayName);
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

                        CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Bits, MsgReceived.DisplayName); 
                    }));
                }
            }

            if (RegisterJoinedUser(MsgReceived.DisplayName, DateTime.Now.ToLocalTime(), Source, ChatUserMessage: OptionFlags.FirstUserChatMsg))
            {
                UserWelcomeMessage(MsgReceived.DisplayName, Source);
            }

        }

        private void RequestBanUser(Bots Source, string UserName, BanReasons Reason, int Duration = 0)
        {
            BanUserRequest?.Invoke(this, new() { Source = Source, UserName = UserName, BanReason = Reason, Duration = Duration });
        }

        public void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName, Bots Source)
        {
            lock (ProcMsgQueue) {
                ProcMsgQueue.Enqueue(new(() =>
                {
                    string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out bool Enabled, out short Multi);
                    if (Enabled)
                    {
                        Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                            new(MsgVars.user, UserName ),
                            new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers))
                            });

                        SendMessage(VariableParser.ParseReplace(msg, dictionary), Multi);
                    }

                    CheckForOverlayEvent(OverlayTypes.ChannelEvents, ChannelEventActions.Raid, UserName);

                    UpdatedStat(StreamStatType.Raids, StreamStatType.AutoEvents);

                    if (OptionFlags.TwitchRaidShoutOut)
                    {
                        Command.CheckShout(UserName, out string response, Source, false);
                    }
                }));
            }
            if (OptionFlags.ManageRaidData)
            {
                StatisticsSystem.PostIncomingRaid(UserName, RaidTime, Viewers, GameName);
            }
        }

        public static void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            if (OptionFlags.ManageOutRaidData)
            {
                DataManage.PostOutgoingRaid(HostedChannel, dateTime);
            }
        }

        public void ProcessCommand(CmdMessage cmdMessage, Bots Source)
        {
            try
            {
                lock (ProcMsgQueue)
                {
                    ProcMsgQueue.Enqueue(new Task(() =>
                    {
                       Command.EvalCommand(cmdMessage, Source);
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

        #region Giveaway
        /// <summary>
        /// Initialize and start accepting giveaway entries
        /// </summary>
        public void BeginGiveaway()
        {
            GiveawayStarted = true;
            GiveawayCollectionList.Clear();
            SystemsBase.GiveawayCollection.Clear();

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
                SystemsBase.GiveawayCollection.Add(DisplayName);
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

                    foreach(string W in WinnerList)
                    {
                        CheckForOverlayEvent(OverlayTypes.Giveaway, OverlayTypes.Giveaway, W);
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
                if (SystemsBase.AddClip(c))
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

                    CheckForOverlayEvent(OverlayTypes.Clip, OverlayTypes.Clip, ProvidedURL: c.Url);
                }
            }
        }

        #endregion

        #region Media Overlay Server

        public void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> eventHandler)
        {
            Overlay.NewOverlayEvent += eventHandler;
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            return Overlay.GetOverlayActions();
        }

        public void SetChannelRewardList(List<string> RewardList)
        {
            Overlay.SetChannelRewardList(RewardList);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, Enum enumvalue, string UserName="", string UserMsg="", string ProvidedURL = "")
        {
            CheckForOverlayEvent(overlayType, enumvalue.ToString(), UserName ,UserMsg, ProvidedURL);
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string Action, string UserName = "", string UserMsg="", string ProvidedURL = "")
        {
            Overlay.CheckForOverlayEvent(overlayType, Action, UserName, UserMsg, ProvidedURL);
        }

        #endregion

        }
}
