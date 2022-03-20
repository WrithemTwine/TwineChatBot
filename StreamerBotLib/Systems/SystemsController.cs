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
    public class SystemsController
    {
        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;

        public static DataManager DataManage { get; private set; } = new();

        private Thread HoldNewFollowsForBulkAdd;

        private StatisticsSystem Stats { get; set; }
        private CommandSystem Command { get; set; }
        private CurrencySystem Currency { get; set; }

        internal Dispatcher AppDispatcher { get; set; }

        private Queue<Task> ProcMsgQueue { get; set; } = new();
        private readonly Thread ProcessMsgs;

        private const int SleepWait = 6000;

        private delegate void BotOperation();

        private bool GiveawayStarted = false;
        private readonly List<string> GiveawayCollectionList = new();


        public SystemsController()
        {
            SystemsBase.DataManage = DataManage;
            LocalizedMsgSystem.SetDataManager(DataManage);
            DataManage.Initialize();
            Stats = new();
            Command = new();
            Currency = new();

            Command.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;
            Command.ProcessedCommand += Command_ProcessedCommand;
            Stats.BeginCurrencyClock += Stats_BeginCurrencyClock;
            Stats.BeginWatchTime += Stats_BeginWatchTime;

            ProcessMsgs = ThreadManager.CreateThread(ActionProcessCmds, ThreadWaitStates.Wait, 10);
            ProcessMsgs.Start();
        }

        private void ActionProcessCmds()
        {
            while (OptionFlags.ActiveToken)
            {
                while (ProcMsgQueue.Count > 0)
                {
                    lock (ProcMsgQueue)
                    {
                        ProcMsgQueue.Dequeue().Start();
                    }
                    Thread.Sleep(300);
                }

                Thread.Sleep(1000);
            }
        }

        public void Exit()
        {
            ProcessMsgs.Join();
        }

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
                }, ThreadWaitStates.Wait, 50);

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
            foreach (Follow f in FollowList.Where(f => DataManage.AddFollower(f.FromUserName, f.FollowedAt.ToLocalTime())))
            {
                if (OptionFlags.ManageFollowers)
                {
                    if (FollowEnabled)
                    {
                        SendMessage(VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] { new(MsgVars.user, f.FromUserName) })));
                    }

                    UpdatedStat(StreamStatType.Follow, StreamStatType.AutoEvents);
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

        public static void PostUpdatedDataRow(DataRow UpdatedData)
        {
            DataManage.PostUpdatedDataRow(UpdatedData);
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
            StatisticsSystem.SetCategory(GameId, GameName);
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
                if (RegisterJoinedUser(user, Curr, JoinedUserMsg: true))
                {
                    UserWelcomeMessage(user, Source);
                }
            }
        }

        public static void UserLeft(string UserName)
        {
            StatisticsSystem.UserLeft(UserName, DateTime.Now.ToLocalTime());
        }

        #endregion

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(webhooksKind);
        }

        private bool RegisterJoinedUser(string UserName, DateTime UserTime, bool JoinedUserMsg = false, bool ChatUserMessage = false)
        {
            bool FoundUserJoined = false;
            bool FoundUserChat = false;

            if (JoinedUserMsg) // use a straight flag for user to join the channel
            {
                FoundUserJoined = StatisticsSystem.UserJoined(UserName, UserTime);
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
            if ((UserName.ToLower(CultureInfo.CurrentCulture) != SystemsBase.ChannelName.ToLower(CultureInfo.CurrentCulture) && (UserName.ToLower(CultureInfo.CurrentCulture) != SystemsBase.BotUserName.ToLower(CultureInfo.CurrentCulture))) || OptionFlags.MsgWelcomeStreamer)
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

         public void MessageReceived(string UserName, bool IsSubscriber, bool IsVip, bool IsModerator, int Bits, string Message, Bots Source)
        {
            SystemsBase.AddChatString(UserName, Message);
            UpdatedStat(StreamStatType.TotalChats);

            if (IsSubscriber)
            {
                StatisticsSystem.SubJoined(UserName);
            }
            if (IsVip)
            {
                StatisticsSystem.VIPJoined(UserName);
            }
            if (IsModerator)
            {
                StatisticsSystem.ModJoined(UserName);
            }

            // handle bit cheers
            if (Bits > 0)
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
                                new(MsgVars.user, UserName),
                                new(MsgVars.bits, FormatData.Plurality(Bits, MsgVars.Pluralbits) )
                            });

                            SendMessage(VariableParser.ParseReplace(msg, dictionary), Multi);

                            UpdatedStat(StreamStatType.Bits, Bits);
                            UpdatedStat(StreamStatType.AutoEvents);
                        }
                    }));
                }
            }

            if (RegisterJoinedUser(UserName, DateTime.Now.ToLocalTime(), ChatUserMessage: OptionFlags.FirstUserChatMsg))
            {
                UserWelcomeMessage(UserName, Source);
            }
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
                    UpdatedStat(StreamStatType.Raids, StreamStatType.AutoEvents);

                    if (OptionFlags.TwitchRaidShoutOut)
                    {
                        StatisticsSystem.UserJoined(UserName, RaidTime);
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

        public void PostGiveawayResult()
        {
            Random random = new();

            string DisplayName = "";

            if (GiveawayCollectionList.Count > 0)
            {
                int x = 0;
                while (x < OptionFlags.GiveawayCount)
                {
                    string winner = GiveawayCollectionList[random.Next(GiveawayCollectionList.Count)];
                    GiveawayCollectionList.RemoveAll((w) => w == winner);
                    DisplayName += (OptionFlags.GiveawayCount > 1 && x > 0 ? ", " : "") + winner;
                    x++;
                }

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
                }
            }
        }        

        #endregion
    }
}
