#if DEBUG
#define LogDataManager_Actions
#endif

using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace StreamerBotLib.Systems
{
    internal partial class ActionSystem : INotifyPropertyChanged
    {
        private static Thread ElapsedThread;
        private bool ChatBotStarted;

        private const int ThreadSleep = 5000;
        private DateTime chattime;
        private DateTime viewertime;
        private int chats;
        private int priorchats;
        private int viewers;
        private double diluteTime;
        private readonly string RepeatLock = "";

        private static int LastLiveViewerCount = 0;

        // TODO: add user quotes - retrieve sayings saved during stream

        // bubbles up messages from the event timers because there is no invoking method to receive this output message 
        public event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<PostChannelMessageEventArgs> ProcessedCommand;

        public void NotifyPropertyChanged(string ParamName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        public void StartElapsedTimerThread()
        {
            // don't start another thread if the current is still active
            if (ElapsedThread == null || !ElapsedThread.IsAlive)
            {
                ChatBotStarted = true;
                ElapsedThread = ThreadManager.CreateThread(ElapsedCommandTimers);
                ElapsedThread.Start();
            }
        }

        public void StopElapsedTimerThread()
        {
            ChatBotStarted = false;
            ElapsedThread?.Join();
            ElapsedThread = null;
        }

        /// <summary>
        /// Performs the commands with timers > 0 seconds. Runs on a separate thread.
        /// </summary>
        private void ElapsedCommandTimers()
        {
            // TODO: consider some AI bot chat when channel is slower
            // TODO: repeat command still performs when command not enabled

            List<TimerCommand> RepeatList = new();
            DateTime now = DateTime.Now;
            chattime = now; // the time to check chats sent
            viewertime = now; // the time to check viewers
            chats = GetCurrentChatCount;
            priorchats = chats;
            viewers = GetUserCount;

            try
            {
                while (ComputeRerunLoop())
                {
                    diluteTime = CheckDilute();
                    foreach (var item in from Tuple<string, int, string[]> Timers in DataManage.GetTimerCommands()
                                         where Timers.Item3.Contains(Category) || Timers.Item3.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory))
                                         let item = new TimerCommand(Timers, diluteTime)
                                         select item)
                    {
                        if (RepeatList.UniqueAdd(item))
                        {
                            ThreadManager.CreateThreadStart(() => RepeatCmd(item));
                        }
                        else
                        {
                            // if repeat command already added, check and remove from list if the repeat time is set to '0' (user changed)
                            lock (item)
                            {
                                RepeatList.Remove(RepeatList.Find((f) => f.Equals(item) && f.RepeatTime == 0));
                            }
                        }
                    }
                    // wait a while before checking commands again
                    Thread.Sleep(ThreadSleep * (1 + (DateTime.Now.Second / 60)));
                }
            }
            catch (ThreadInterruptedException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private bool ComputeRerunLoop()
        {
            return OptionFlags.ActiveToken
                    && ChatBotStarted
                    && OptionFlags.RepeatTimerCommands
                    && ((OptionFlags.RepeatWhenLive && OptionFlags.IsStreamOnline) || !OptionFlags.RepeatWhenLive);
        }
        private bool ComputeRerunLoop(List<string> CategoryList)
        {
            return ComputeRerunLoop()
                    && (CategoryList.Contains(Category) || CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCategory)));
        }

        private bool ComputeRepeat()
        {
            return OptionFlags.RepeatNoAdjustment // no limits, just perform repeat command
              || OptionFlags.RepeatTimerComSlowdown // diluted command, performance time
              || (OptionFlags.RepeatUseThresholds
                  && (!OptionFlags.RepeatAboveUserCount || viewers >= OptionFlags.RepeatUserCount) // if user threshold, check threshold, else, accept the check
                  && (!OptionFlags.RepeatAboveChatCount || chats >= OptionFlags.RepeatChatCount) // if chat threshold, check threshold, else, accept the check
                  );
        }

        private void RepeatCmd(TimerCommand cmd)
        {
            int repeat = 0;  // determined seconds for the repeat timer commands
            bool ResetLive = false; // flag to check reset when going live and going offline, to avoid continuous resets

            lock (cmd) // lock the cmd because it's referenced in other threads
            {
                repeat = cmd.RepeatTime;
            }

            try
            {
                while (repeat != 0 && ComputeRerunLoop(cmd.CategoryList))
                {
                    if (OptionFlags.IsStreamOnline && OptionFlags.RepeatLiveReset && !ResetLive)
                    {
                        if (OptionFlags.RepeatLiveResetShow) // perform command when repeat timers are reset based on live online stream
                        {
                            lock (cmd)
                            {
                                cmd.SetNow(); // cause command to fire immediately
                            }
                        }
                        ResetLive = true;
                    }
                    else if (!OptionFlags.IsStreamOnline && ResetLive)
                    {
                        ResetLive = false;
                    }

                    if (cmd.CheckFireTime())
                    {
                        lock (GUI.GUIDataManagerLock.Lock) // lock it up because accessing a DataManage row
                        {
                            lock (RepeatLock)
                            {
                                if (ComputeRepeat())
                                {
                                    OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = ParseCommand(cmd.Command, new(BotUserName, Platform.Default), new(), DataManage.GetCommand(cmd.Command), out short multi, true), RepeatMsg = multi });
                                }
                            }
                        }
                        lock (cmd)
                        {
                            // update for any changes
                            cmd.UpdateTime(diluteTime);
                        }
                    }
                    Thread.Sleep(ThreadSleep * (1 + (DateTime.Now.Second / 60)));

                    lock (cmd) // lock the cmd because it's referenced in other threads
                    {
                        repeat = DataManage.GetTimerCommandTime(cmd.Command) ?? 0;
                        cmd.ModifyTime(repeat, diluteTime);
                    }
                }
                cmd.ModifyTime(0, diluteTime);
            }
            catch (ThreadInterruptedException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private double CheckDilute()
        {
            double temp = 1.0; // return 1.0 if the user chooses not to dilute the timers

            UpdateChatUserStats();

            if (OptionFlags.RepeatTimerComSlowdown) // only calculate if user wants diluted/smart-mode repeat commands
            {
                double factor = (chats + viewers) / ((OptionFlags.RepeatChatCount + OptionFlags.RepeatUserCount) == 0 ? 1 : OptionFlags.RepeatChatCount + OptionFlags.RepeatUserCount);

                temp = 1.0 + (factor > 1.0 ? 0 : 1.0 - factor);
            }
            return temp;
        }

        private void UpdateChatUserStats()
        {
            lock (RepeatLock)
            {
                DateTime now = DateTime.Now;

                if ((now - chattime) >= new TimeSpan(0, OptionFlags.RepeatChatMinutes, 0))
                {
                    chattime = now;
                    int currChats = GetCurrentChatCount;
                    chats = currChats - priorchats;
                    priorchats = currChats;
                }

                if ((now - viewertime) >= new TimeSpan(0, OptionFlags.RepeatUserMinutes, 0))
                {
                    viewertime = now;
                    viewers = GetUserCount;
                }
            }
        }

        /// <summary>
        /// Establishes the permission level for the user who sends the message.
        /// </summary>
        /// <param name="chatMessage">The ChatMessage holding the characteristics of the user who invoked the chat command, which parses out the user permissions.</param>
        /// <returns>The ViewerType corresponding to the user's highest permission.</returns>
        public static ViewerTypes ParsePermission(CmdMessage chatMessage)
        {
            if (chatMessage.IsBroadcaster)
            {
                return ViewerTypes.Broadcaster;
            }
            else if (chatMessage.IsModerator)
            {
                return ViewerTypes.Mod;
            }
            else if (chatMessage.IsVip)
            {
                return ViewerTypes.VIP;
            }
            else if (DataManage.CheckFollower(chatMessage.DisplayName))
            {
                return ViewerTypes.Follower;
            }
            else if (chatMessage.IsSubscriber)
            {
                return ViewerTypes.Sub;
            }
            else
            {
                return ViewerTypes.Viewer;
            }
        }

        public void EvalCommand(CmdMessage cmdMessage, Platform source)
        {
            string result;
            cmdMessage.UserType = ParsePermission(cmdMessage);
            short multi = 0;

            CommandData cmdrow = DataManage.GetCommand(cmdMessage.CommandText);

            if (cmdrow == null)
            {
                result = OptionFlags.MsgCommandNotFound ? LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound) : "";
            }
            else if (!cmdrow.IsEnabled)
            {
                result = "";
            }
            else if ((ViewerTypes)Enum.Parse(typeof(ViewerTypes), cmdrow.Permission) < cmdMessage.UserType)
            {
                Tuple<string, string> ApproveAction = GetApprovalRule(ModActionType.Commands, cmdMessage.CommandText);
                if (ApproveAction == null)
                {
                    result = LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidCommand);
                }
                else
                {
                    AddApprovalRequest($"{cmdMessage.CommandText} {cmdMessage.DisplayName} {cmdMessage.Message}",
                        new(() => { FormatResult(ParseCommand(cmdMessage.CommandText, new(cmdMessage.DisplayName, source), cmdMessage.CommandArguments, cmdrow, out multi), multi, cmdrow); }));
                    result = ParseCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve), new LiveUser(BotController.GetBotName(source), source), new(), DataManage.GetCommand(LocalizedMsgSystem.GetVar(DefaultCommand.approve)), out multi);
                }
            }
            else
            {
                // parse commands, either built-in or custom
                result = ParseCommand(cmdMessage.CommandText, new(cmdMessage.DisplayName, source), cmdMessage.CommandArguments, cmdrow, out multi);
            }

            FormatResult(result, multi, cmdrow);
        }

        private void FormatResult(string result, short multi, CommandData cmdrow)
        {
            result = $"{(cmdrow != null && cmdrow.IsEnabled && ((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "")}{result}";

            OnProcessCommand(result, multi);
        }

        /// <summary>
        /// Call to check all users in the stream, and shout them.
        /// </summary>
        /// <param name="Source">The name of the Bot calling the shout-outs, for purposes of which platform to call the category.</param>
        public void AutoShoutUsers()
        {

            List<LiveUser> CurrActiveUsers;
            lock (CurrUsers)
            {
                CurrActiveUsers = new();
                CurrActiveUsers.UniqueAddRange(CurrUsers);
            }

#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, "Received AutoShoutUsers command. Current active users.");

            foreach (LiveUser u in CurrActiveUsers)
            {
                LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Contains {u.UserName}, {u.UserId}, {u.Source}");
            }

            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, "Now checking if the user is on the shout list.");
#endif

            ThreadManager.CreateThreadStart(() =>
            {
                foreach (LiveUser U in CurrActiveUsers)
                {
                    CheckShout(U, out _);
                }
            });
        }

        /// <summary>
        /// See if the user is part of the user's auto-shout out list to determine if the message should be called, or shout-out from a raid or other similar event.
        /// </summary>
        /// <param name="UserName">The user to check</param>
        /// <param name="response">the response message template</param>
        /// <param name="AutoShout">true-check if the user is on the autoshout list, false-the method call is from a command, no autoshout check</param>
        /// <returns></returns>
        public void CheckShout(LiveUser User, out string response, bool AutoShout = true)
        {
            response = "";
            if (DataManage.CheckShoutName(User.UserName) || !AutoShout)
            {
                if (OptionFlags.MsgSendSOToChat)
                {
                    OnProcessCommand($"!{LocalizedMsgSystem.GetVar(DefaultCommand.so)} {User.UserName}");
                }
                response = ParseCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so), User, new(), DataManage.GetCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so)), out short multi);

                // handle when returned without #category in the message
                if (response != "" && response != "/me ")
                {
                    OnProcessCommand(response, multi);

#if LogDataManager_Actions
                    LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, "Sent message with no #category symbol.");
#endif

                }
            }
        }

        public string CheckWelcomeUser(string User)
        {
            return DataManage.CheckWelcomeUser(User);
        }

        public string ParseCommand(string command, LiveUser User, List<string> arglist, CommandData cmdrow, out short multi, bool ElapsedTimer = false)
        {
            string result = "";
            string tempHTMLResponse = "";
            Dictionary<string, string> datavalues = null;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                result = DataManage.PostCommand(newcom[1..], CommandParams.Parse(arglist));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.settitle))
            {
                if (arglist.Count > 0)
                {
                    bool success = BotController.ModifyChannelInformation(User.Source, Title: string.Join(' ', arglist));
                    result = success ? cmdrow.Message : LocalizedMsgSystem.GetVar("MsgNoSuccess");
                }
                else
                {
                    result = LocalizedMsgSystem.GetVar("MsgNoTitleCategory");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.raid))
            {
                if (arglist.Count > 0)
                {
                    BotController.RaidChannel(arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0], User.Source);
                    result = cmdrow.Message;
                }
                else
                {
                    result = DataManage.GetUsage(command);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.cancelraid))
            {
                BotController.CancelRaidChannel(User.Source);
                result = cmdrow.Message;
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.approve))
            {
                if (arglist.Count == 0)
                {
                    datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.list, string.Join(", ",GetDescriptions()) ),
                        new(MsgVars.usage, cmdrow.Usage)
                    });

                    result = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                }
                else if (arglist.Count == 1)
                {
                    string AppLabel = GetLabel(arglist[0]);
                    if (AppLabel != null)
                    {
                        RunApprovedRequest(AppLabel);
                        result = LocalizedMsgSystem.GetVar(Msg.MsgModApproved);
                    }
                    else
                    {
                        result = LocalizedMsgSystem.GetVar(Msg.MsgModApproveNotFound);
                    }
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.setintro))
            {
                if (arglist.Count > 2)
                {
                    string adduser = arglist[0].Replace("@", "");
                    string message = string.Join(' ', arglist.Skip(1));

                    DataManage.PostUserCustomWelcome(adduser, message);
                }
                else
                {
                    result = cmdrow.Usage;
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.mergeaccounts))
            {
                bool? output = null;
                if (arglist.Count == 0)
                {
                    result = cmdrow.Usage;
                }
                else
                {
                    /* -p:Mod 
                     * -use:(Mod level) !mergeaccounts <currname> <previousname> 'or' (user level) !mergeaccounts <previousname>
                     */
                    string CurrUser, SrcUsr;

                    switch (arglist.Count)
                    {
                        case 1:
                            CurrUser = User.UserName;
                            SrcUsr = arglist[0];
                            break;
                        case 2:
                        default:
                            CurrUser = arglist[0];
                            SrcUsr = arglist[1];
                            break;
                    }

                    output = DataManage.PostMergeUserStats(CurrUser.Replace("@", ""), SrcUsr.Replace("@", ""), User.Source);
                }
                result = output == null ? result : output == true ? LocalizedMsgSystem.GetVar(Msg.MsgMergeSuccessful) : LocalizedMsgSystem.GetVar(Msg.MsgMergeFailed);
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.setcategory))
            {
                if (arglist.Count > 0)
                {
                    if (int.TryParse(arglist[0], out int GameId))
                    {
                        BotController.ModifyChannelInformation(User.Source, CategoryId: GameId.ToString());
                        result = cmdrow.Message;
                    }
                    else
                    {
                        bool success = false;
                        string CategoryName = string.Join(' ', arglist);

                        Tuple<string, string> found = DataManage.GetGameCategories().Find((x) => x.Item2 == CategoryName);

                        if (found != null)
                        {
                            success = BotController.ModifyChannelInformation(User.Source, CategoryId: found.Item1);
                        }
                        else
                        {
                            success = BotController.ModifyChannelInformation(User.Source, CategoryName: CategoryName);
                        }

                        result = success ? cmdrow.Message : LocalizedMsgSystem.GetVar("MsgNoSuccess");
                    }
                }
                else
                {
                    result = LocalizedMsgSystem.GetVar("MsgNoTitleCategory");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.editcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                result = DataManage.EditCommand(newcom[1..], arglist);
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.removecommand))
            {
                if (!LocalizedMsgSystem.CheckDefaultCommand(arglist[0]))
                {
                    result = DataManage.RemoveCommand(arglist[0])
                        ? LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.removecommand)
                        : LocalizedMsgSystem.GetVar("Msgcommandnotfound");
                }
                else
                {
                    result = LocalizedMsgSystem.GetVar("Msgdefaultcommand");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.accountage))
            {
                string ParamUser = arglist.Count == 1 ? arglist[0].Replace("@", "") : User.UserName;

                ThreadManager.CreateThreadStart(() =>
                {
                    DateTime created = BotController.GetUserAccountAge(ParamUser, User.Source);
                    datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.user,ParamUser),
                        new(MsgVars.date, FormatData.FormatTimes(created))
                    });

                    OnProcessCommand(VariableParser.ParseReplace(cmdrow.Message, datavalues));

                });

            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.socials))
            {
                // User chose to send separate messages for the socials
                if (OptionFlags.MsgSocialSeparate)
                {
                    string Socialresult = "";
                    foreach (string Social in DataManage.GetSocialComs())
                    {
                        CommandData SocialRow = DataManage.GetCommand(Social);
                        Socialresult = ParseCommand(Social, User, null, SocialRow, out multi);
                        FormatResult(Socialresult, SocialRow.SendMsgCount, SocialRow);
                    }
                }
                else
                {
                    result = cmdrow.Message + " " + DataManage.GetSocials();
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.uptime))
            {
                if (arglist.Count == 0 && cmdrow.Message.Contains(MsgVars.viewers.ToString()))
                {
                    BotController.GetViewerCount(User.Source);
                    result = "";
                }
                else
                {
                    int DeltaViewers = Convert.ToInt32(arglist[0]) - LastLiveViewerCount;

                    result = VariableParser.ParseReplace(OptionFlags.IsStreamOnline ? (DataManage.GetCommand(command).Message ?? LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.uptime)) : LocalizedMsgSystem.GetVar(Msg.Msgstreamoffline), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                    new( MsgVars.user, ChannelName ),
                    new( MsgVars.uptime, FormatData.FormatTimes(GetCurrentStreamStart) ),
                    new( MsgVars.viewers, FormatData.Plurality(arglist.Count > 0 ? arglist[0] : "", MsgVars.Pluralviewers) ),
                    new( MsgVars.deltaviewers, $"{(DeltaViewers>0?'+':"")}{DeltaViewers}" )
                    }));

                    LastLiveViewerCount = Convert.ToInt32(arglist[0]);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.commands))
            {
                result = DataManage.GetCommands();
            }
            // capture all of the join queue commands
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.join)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.enqueue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.leave)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.dequeue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.queue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo))
            {
                result = OptionFlags.UserPartyStart
                    ? PartyCommand(command, User.UserName, arglist.Count > 0 ? arglist[0] : "", cmdrow)
                    : ElapsedTimer ? "" : LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstop);
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart) || command == LocalizedMsgSystem.GetVar(DefaultCommand.qstop))
            {
                result = cmdrow.Message;
                OptionFlags.SetParty(command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart));
                NotifyPropertyChanged("UserPartyStart");
                NotifyPropertyChanged("UserPartyStop");
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.soactive))
            {
                AutoShoutUsers();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.blackjack))
            {
                bool TryConvertInt = int.TryParse(arglist[0], out int Wager);

                if (arglist.Count != 1 || !TryConvertInt)
                {
                    result = cmdrow.Usage;
                }
                else
                {
                    GamePlayBlackJack(cmdrow, User, Wager);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.death))
            {
                int counter = AddDeathCounter();

                if (counter != -1)
                {
                    result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]{
                        new(MsgVars.user, ChannelName),
                        new(MsgVars.value, FormatData.Plurality(counter,MsgVars.Pluraltime)),
                        new(MsgVars.category, Category)
                    }));
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.resetdeath))
            {
                int counter = ResetDeathCounter(arglist.Count != 0 ? Convert.ToInt32(arglist[0]) : 0);

                result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]{
                        new(MsgVars.value, FormatData.Plurality(counter,MsgVars.Pluraltime)),
                        new(MsgVars.category, Category)
                    }));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.viewdeath))
            {
                int counter = DataManage.GetDeathCounter(FormatData.AddEscapeFormat(Category));

                result = VariableParser.ParseReplace(counter != -1 ? cmdrow.Message : LocalizedMsgSystem.GetVar(Msg.MsgNoDeathCounter), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]{
                        new(MsgVars.user, ChannelName),
                        new(MsgVars.value, FormatData.Plurality( counter ,MsgVars.Pluraltime)),
                        new(MsgVars.category, Category)
                    }));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addquote))
            {
                if (arglist.Count == 0)
                {
                    result = cmdrow.Usage;
                }
                else
                {
                    int quoteNum = DataManage.PostQuote(string.Join(' ', arglist));

                    result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                    {
                        new(MsgVars.quotenum, quoteNum.ToString())
                    }));
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.quote))
            {
                if (arglist.Count > 1)
                {
                    result = cmdrow.Usage;
                }
                else if (arglist.Count == 0)
                {
                    result = VariableParser.ParseReplace(LocalizedMsgSystem.GetVar(Msg.MsgQuoteNumber), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                       {
                            new(MsgVars.quotenum, FormatData.Plurality(DataManage.GetQuoteCount(), MsgVars.Pluralquote))
                       }));
                }
                else
                {
                    result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                        {
                            new(MsgVars.quote, DataManage.GetQuote(Convert.ToInt32(arglist[0])) ?? LocalizedMsgSystem.GetVar(Msg.MsgDefaultQuote))
                        }));
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.removequote))
            {
                result = VariableParser.ParseReplace(cmdrow.Message, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                       {
                            new(MsgVars.quotenum, DataManage.RemoveQuote(Convert.ToInt32(arglist[0])) ? arglist[0] : LocalizedMsgSystem.GetVar(Msg.MsgDefaultQuote))
                       }));
            }
            else
            {
                string paramvalue = cmdrow.AllowParam
                    ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                        ? User.UserName
                        : arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0]
                    : User.UserName;
                datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new( MsgVars.user, paramvalue ),
                    new( MsgVars.url, paramvalue ),
                    new( MsgVars.time, DateTime.Now.ToLocalTime().ToShortTimeString() ),
                    new( MsgVars.date, DateTime.Now.ToLocalTime().ToShortDateString() ),
                    new( MsgVars.com, paramvalue )
                });

                if (command == LocalizedMsgSystem.GetVar(DefaultCommand.so) && !BotController.VerifyUserExist(paramvalue, User.Source))
                {
                    result = LocalizedMsgSystem.GetVar(Msg.MsgNoUserFound);
                }
                else
                {
                    if (cmdrow.Lookupdata)
                    {
                        LookupQuery(cmdrow, paramvalue, ref datavalues);
                    }

                    if (cmdrow.Message.Contains(MsgVars.category.ToString()))
                    {
                        ThreadManager.CreateThreadStart(() =>
                        {
                            lock (GUI.GUIDataManagerLock.Lock)
                            {
                                VariableParser.AddData(ref datavalues,
                                new Tuple<MsgVars, string>[] { new(MsgVars.category, BotController.GetUserCategory(ChannelName: paramvalue, UserId: DataManage.GetUserId(new(paramvalue, User.Source)), bots: User.Source) ?? LocalizedMsgSystem.GetVar(Msg.MsgNoCategory)) });

                                string resultcat = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                                tempHTMLResponse = VariableParser.ParseReplace(cmdrow.Message, datavalues, true);
                                resultcat = (((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !resultcat.StartsWith("/me ") ? "/me " : "") + resultcat;

                                OnProcessCommand(resultcat, cmdrow.SendMsgCount);

#if LogDataManager_Actions
                                LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Found !so message with a category, {resultcat}.");
#endif

                                CheckForOverlayEvent(overlayType: OverlayTypes.Commands,
                                    Action: DefaultCommand.so.ToString(),
                                    UserName: User.UserName, UserMsg: tempHTMLResponse);
                            }
                        });

                        result = "";
                    }
                    else
                    {
                        result = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                        tempHTMLResponse = VariableParser.ParseReplace(cmdrow.Message, datavalues, true);


#if LogDataManager_Actions
                        if (command == LocalizedMsgSystem.GetVar(DefaultCommand.so))
                        {
                            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Found !so message without a category, {result}");
                        }
#endif

                    }
                }
            }

            if (result != "")
            {
                CheckForOverlayEvent(overlayType: OverlayTypes.Commands, Action: command, UserName: User.UserName, UserMsg: tempHTMLResponse);
            }

            result = ((((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") && result != "") ? "/me " : "") + result;
            multi = cmdrow.SendMsgCount;

            return result;
        }

        private void OnProcessCommand(string Message, int repeatMsg = 0)
        {
            ProcessedCommand?.Invoke(this, new() { Msg = Message, RepeatMsg = repeatMsg });
        }

        private static string PartyCommand(string command, string DisplayName, string argument, CommandData cmdrow)
        {
            UserJoin newuser = new() { ChatUser = DisplayName };
            if (argument != "")
            {
                newuser.GameUserName = argument;
            }

            string response;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.queue))
            {
                List<string> JoinChatUsers = new();
                foreach (UserJoin u in JoinCollection)
                {
                    JoinChatUsers.Add(u.ChatUser);
                }

                response = string.Format("There are {0} users in the join queue: {1}", JoinCollection.Count, JoinCollection.Count == 0 ? "no users!" : string.Join(", ", JoinChatUsers));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo))
            {
                response = cmdrow.Message;
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.enqueue) || command == LocalizedMsgSystem.GetVar(DefaultCommand.join))
            {
                if (JoinCollection.Contains(newuser))
                {
                    response = $"You have already joined. You are currently number {JoinCollection.IndexOf(newuser) + 1}.";
                }
                else
                {
                    response = $"You have joined the queue. You are currently {JoinCollection.Count + 1}.";
                    JoinCollection.Add(newuser);
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.leave) || command == LocalizedMsgSystem.GetVar(DefaultCommand.dequeue))
            {
                if (JoinCollection.Contains(newuser))
                {
                    JoinCollection.Remove(newuser);
                    response = "You are no longer in the queue.";
                }
                else
                {
                    response = "You are not in the queue.";
                }
            }
            else
            {
                response = "Command not understood!";
            }

            return response;
        }

        private static void LookupQuery(CommandData CommData, string paramvalue, ref Dictionary<string, string> datavalues)
        {
            //TODO: the commands with data lookup needs a lot of work!

            switch (CommData.Top)
            {
                case > 0:
                case -1:
                    {
                        if (CommData.Action != CommandAction.Get.ToString())
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidComUsage), CommData.CmdName, CommData.Action, CommandAction.Get.ToString()));
                        }

                        // convert multi-row output to a string
                        string queryoutput = "";
                        foreach (Tuple<object, object> bundle in from object r in DataManage.PerformQuery(CommData, CommData.Top)
                                                                 let bundle = r as Tuple<object, object>
                                                                 where bundle.Item1 == bundle.Item2
                                                                 select bundle)
                        {
                            queryoutput += bundle.Item1 + ", ";
                        }

                        queryoutput = queryoutput.Remove(queryoutput.LastIndexOf(','));
                        VariableParser.AddData(ref datavalues, new Tuple<MsgVars, string>[] { new(MsgVars.query, queryoutput) });
                        break;
                    }

                default:
                    {
                        object querydata = DataManage.PerformQuery(CommData, paramvalue);

                        string output = "";
                        if (querydata.GetType() == typeof(string))
                        {
                            output = (string)querydata;
                        }
                        else if (querydata.GetType() == typeof(TimeSpan))
                        {
                            output = FormatData.FormatTimes((TimeSpan)querydata);
                        }
                        else if (querydata.GetType() == typeof(DateTime))
                        {
                            output = FormatData.FormatTimes((DateTime)querydata);
                        }
                        else if (querydata.GetType() == typeof(int) || querydata.GetType() == typeof(double))
                        {
                            output = ((double)querydata).ToString("N2");
                        }
                        else
                        {
                            output = querydata.ToString();
                        }

                        if (output != null)
                        {
                            VariableParser.AddData(ref datavalues, new Tuple<MsgVars, string>[] { new(MsgVars.query, output) });
                        }
                        break;
                    }
            }
        }

    }
}
