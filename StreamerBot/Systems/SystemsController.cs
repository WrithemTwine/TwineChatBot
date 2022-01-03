using StreamerBot.BotClients;
using StreamerBot.Data;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Models;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace StreamerBot.Systems
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
        }

        /// <summary>
        /// Handle if message is processed as multithreaded, due to one or more bot calls and wait for 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_ProcessedCommand(object sender, PostChannelMessageEventArgs e)
        {
            SendMessage(e.Msg);
        }

        public void SetDispatcher(Dispatcher dispatcher)
        {
            AppDispatcher = dispatcher;
        }

        private void SendMessage(string message)
        {
            if (message is not "" and not "/me ")
            {
                PostChannelMessage?.Invoke(this, new() { Msg = message });
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
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled);

            if (DataManage.UpdatingFollowers)
            { // capture any followers found after starting the bot and before completing the bulk follower load
                HoldNewFollowsForBulkAdd = new Thread(new ThreadStart(() =>
                {
                    while (DataManage.UpdatingFollowers && OptionFlags.ActiveToken) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

                    ProcessFollow(FollowList, msg, FollowEnabled);
                }));

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

        #endregion

        #region Statistics

        public bool StreamOnline(DateTime CurrTime)
        {
            return Stats.StreamOnline(CurrTime);
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

        public void UserJoined(List<string> users, Bots Source)
        {

            foreach (string user in from string user in users
                                    where StatisticsSystem.UserJoined(user, DateTime.Now.ToLocalTime())
                                    select user)
            {
                if (OptionFlags.FirstUserJoinedMsg)
                {
                    RegisterJoinedUser(user, Source);
                }
            }
        }

        public static void UserLeft(string User)
        {
            StatisticsSystem.UserLeft(User, DateTime.Now.ToLocalTime());
        }

        #endregion

        public static List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(webhooksKind);
        }

        public void AddChat(string Username, Bots Source)
        {
            if (StatisticsSystem.UserChat(Username) && OptionFlags.FirstUserChatMsg)
            {
                RegisterJoinedUser(Username, Source);
            }
        }

        private void RegisterJoinedUser(string Username, Bots Source)
        {
            // TODO: fix welcome message if user just joined as a follower, then says hello, welcome message says -welcome back to channel
            if (OptionFlags.FirstUserJoinedMsg || OptionFlags.FirstUserChatMsg)
            {
                if ((Username.ToLower(CultureInfo.CurrentCulture) != SystemsBase.ChannelName.ToLower(CultureInfo.CurrentCulture)) || OptionFlags.MsgWelcomeStreamer)
                {
                    ChannelEventActions selected = ChannelEventActions.UserJoined;

                    if (OptionFlags.WelcomeCustomMsg)
                    {
                        selected =
                            StatisticsSystem.IsFollower(Username) ?
                            ChannelEventActions.SupporterJoined :
                                StatisticsSystem.IsReturningUser(Username) ?
                                    ChannelEventActions.ReturnUserJoined : ChannelEventActions.UserJoined;
                    }

                    string msg = LocalizedMsgSystem.GetEventMsg(selected, out _);
                    SendMessage(
                        VariableParser.ParseReplace(
                            msg,
                            VariableParser.BuildDictionary(
                                new Tuple<MsgVars, string>[]
                                    {
                                        new( MsgVars.user, Username )
                                    }
                            )
                        )
                    );
                }
            }

            if (OptionFlags.AutoShout)
            {
                bool output = Command.CheckShout(Username, out string response, Source);
                if (output)
                {
                    SendMessage(response);
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
                string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out bool Enabled);
                if (Enabled)
                {
                    Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                    new(MsgVars.user, UserName),
                    new(MsgVars.bits, FormatData.Plurality(Bits, MsgVars.Pluralbits) )
                    });

                    SendMessage(VariableParser.ParseReplace(msg, dictionary));

                    UpdatedStat(StreamStatType.Bits, Bits);
                    UpdatedStat(StreamStatType.AutoEvents);
                }
            }

            AddChat(UserName, Source);
        }

        public void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName, Bots Source)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out bool Enabled);
            if (Enabled)
            {
                Dictionary<string, string> dictionary = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[] {
                new(MsgVars.user, UserName ),
                new(MsgVars.viewers, FormatData.Plurality(Viewers, MsgVars.Pluralviewers))
                });

                SendMessage(VariableParser.ParseReplace(msg, dictionary));
            }
            UpdatedStat(StreamStatType.Raids, StreamStatType.AutoEvents);

            if (OptionFlags.TwitchRaidShoutOut)
            {
                StatisticsSystem.UserJoined(UserName, RaidTime);
                bool output = Command.CheckShout(UserName, out string response, Source, false);
                if (output)
                {
                    SendMessage(response);
                }
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
                string response = Command.EvalCommand(cmdMessage, out short multi, Source);
                short x = 0;

                do
                {
                    SendMessage(response);
                    x++;
                } while (x <= multi);
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

        #region Clips
        public void ClipHelper(IEnumerable<Clip> Clips)
        {
            foreach (Clip c in Clips)
            {
                if (SystemsBase.AddClip(c))
                {
                    if (OptionFlags.TwitchClipPostChat)
                    {
                        SendMessage(c.Url);
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
