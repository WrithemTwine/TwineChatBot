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
using System.Windows;

namespace StreamerBot.Systems
{
    public class SystemsController
    {
        public event EventHandler<PostChannelMessageEventArgs> PostChannelMessage;

        public static DataManager DataManage { get; private set; } = new();

        private StatisticsSystem Stats { get; set; }
        private CommandSystem Command { get; set; }
        private CurrencySystem Currency { get; set; }

        public SystemsController()
        {
            SystemsBase.DataManage = DataManage;
            LocalizedMsgSystem.SetDataManager(DataManage);
            DataManage.Initialize();
            Stats = new();
            Command = new();
            Currency = new();

            Command.OnRepeatEventOccured += ProcessCommands_OnRepeatEventOccured;
            Stats.BeginCurrencyClock += Stats_BeginCurrencyClock;
            Stats.BeginWatchTime += Stats_BeginWatchTime;
        }

        private void SendMessage(string message)
        {
            PostChannelMessage?.Invoke(this, new() { Msg = message });
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

        public void StartBulkFollowers()
        {
            DataManage.StartFollowers();
        }

        public void UpdateFollowers(IEnumerable<Follow> Follows)
        {
            DataManage.UpdateFollowers(Follows);
        }

        public void StopBulkFollowers()
        {
            DataManage.StopBulkFollows();
        }


        private delegate void ProcFollowDelegate(IEnumerable<Follow> FollowList, string msg, bool FollowEnabled);

        public void AddNewFollowers(IEnumerable<Follow> FollowList)
        {
            string msg = LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out bool FollowEnabled);

            if (DataManage.UpdatingFollowers)
            { // capture any followers found after starting the bot and before completing the bulk follower load
                Application.Current.Dispatcher.BeginInvoke( (ProcFollowDelegate) PerformFollow, FollowList, msg, FollowEnabled);
            }
            else
            {
                ProcessFollow(FollowList, msg, FollowEnabled);
            }
        }

        private void PerformFollow(IEnumerable<Follow> FollowList, string msg, bool FollowEnabled)
        {
            new Thread(new ThreadStart(() =>
            {
                while (DataManage.UpdatingFollowers && OptionFlags.ActiveToken) { } // spin until the 'add followers when bot starts - this.ProcessFollows()' is finished

                ProcessFollow(FollowList, msg, FollowEnabled);
            })).Start();
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

        public void ManageDatabase()
        {
            SystemsBase.ManageDatabase();
        }

        public void ClearWatchTime()
        {
            SystemsBase.ClearWatchTime();
        }

        public void ClearAllCurrenciesValues()
        {
            SystemsBase.ClearAllCurrenciesValues();
        }

        #endregion

        #region Statistics

        public bool StreamOnline(DateTime CurrTime)
        {
            return Stats.StreamOnline(CurrTime);
        }

        public void StreamOffline(DateTime CurrTime)
        {
            Stats.StreamOffline(CurrTime);
        }

        public void SetCategory(string GameId, string GameName)
        {
            Stats.SetCategory(GameId, GameName);
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

        public void UserJoined(List<string> users)
        {
            foreach (string user in from string user in users
                                    where Stats.UserJoined(user, DateTime.Now.ToLocalTime())
                                    select user)
            {
                RegisterJoinedUser(user);
            }
        }

        public void UserLeft(string User)
        {
            Stats.UserLeft(User, DateTime.Now.ToLocalTime());
        }

        #endregion

        public List<Tuple<bool, Uri>> GetDiscordWebhooks(WebhooksKind webhooksKind)
        {
            return DataManage.GetWebhooks(webhooksKind);
        }

        public void AddChat(string Username)
        {
            if (Stats.UserChat(Username) && OptionFlags.FirstUserChatMsg)
            {
                RegisterJoinedUser(Username);
            }
        }

        private void RegisterJoinedUser(string Username)
        {
            // TODO: fix welcome message if user just joined as a follower, then says hello, welcome message says -welcome back to channel
            if (OptionFlags.FirstUserJoinedMsg || OptionFlags.FirstUserChatMsg)
            {
                if ((Username.ToLower(CultureInfo.CurrentCulture) != SystemsBase.ChannelName) || OptionFlags.MsgWelcomeStreamer)
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
                bool output = Command.CheckShout(Username, out string response);
                if (output)
                {
                    SendMessage(response);
                }
            }
        }

        public void MessageReceived(string UserName, bool IsSubscriber, bool IsVip, bool IsModerator, int Bits, string Message)
        {
            SystemsBase.AddChatString(UserName, Message);
            UpdatedStat(StreamStatType.TotalChats);

            if (IsSubscriber)
            {
                Stats.SubJoined(UserName);
            }
            if (IsVip)
            {
                Stats.VIPJoined(UserName);
            }
            if (IsModerator)
            {
                Stats.ModJoined(UserName);
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

            AddChat(UserName);
        }

        public void PostIncomingRaid(string UserName, DateTime RaidTime, string Viewers, string GameName)
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
                Stats.UserJoined(UserName, RaidTime);
                bool output = Command.CheckShout(UserName, out string response, false);
                if (output)
                {
                    SendMessage(response);
                }
            }

            if (OptionFlags.ManageRaidData)
            {
                Stats.PostIncomingRaid(UserName, RaidTime, Viewers, GameName);
            }
        }

        public void ProcessCommand(CmdMessage cmdMessage)
        {
            try
            {
                string response = Command.EvalCommand(cmdMessage);
                if (response != "" && response != "/me ")
                {
                    SendMessage(response);
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
                SendMessage(e.Message);
                UpdatedStat(StreamStatType.AutoCommands);
            }
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
