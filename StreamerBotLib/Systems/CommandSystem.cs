using StreamerBotLib.BotIOController;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

using static StreamerBotLib.Data.DataSource;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace StreamerBotLib.Systems
{
    public class CommandSystem : SystemsBase, INotifyPropertyChanged
    {
        private static Thread ElapsedThread;
        private bool ChatBotStarted;

        // TODO: add account merging for a user, approval by a mod+ (moderator, broadcaster)
        // TODO: add approval to manage channel point redemption for custom welcome message
        // TODO: add quotes

        // bubbles up messages from the event timers because there is no invoking method to receive this output message 
        public event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<PostChannelMessageEventArgs> ProcessedCommand;
        public event EventHandler<CheckOverlayEventArgs> CheckOverlayEvent;

        public void NotifyPropertyChanged(string ParamName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        public CommandSystem()
        {
            //StartElapsedTimerThread();
        }

        private void OnCheckOverlayEvent(CheckOverlayEventArgs e)
        {
            CheckOverlayEvent?.Invoke(this, e);
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
        }

        private const int ChatCount = 20;
        private const int ViewerCount = 15;
        private const int ThreadSleep = 5000;
        private DateTime chattime;
        private DateTime viewertime;
        private int chats;
        private int viewers;

        /// <summary>
        /// Performs the commands with timers > 0 seconds. Runs on a separate thread.
        /// </summary>
        private void ElapsedCommandTimers()
        {
            // TODO: consider some AI bot chat when channel is slower
            List<TimerCommand> RepeatList = new();

            chattime = DateTime.Now.ToLocalTime(); // the time to check chats sent
            viewertime = DateTime.Now.ToLocalTime(); // the time to check viewers
            chats = GetCurrentChatCount;
            viewers = GetUserCount;

            double DiluteTime;

            try
            {
                while (OptionFlags.ActiveToken && ChatBotStarted && OptionFlags.RepeatTimer)
                {
                    DiluteTime = CheckDilute();

                    foreach (Tuple<string, int, string[]> Timers in DataManage.GetTimerCommands())
                    {
                        if (Timers.Item3.Contains(Category) || Timers.Item3.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry)))
                        {
                            TimerCommand item = new(Timers, DiluteTime);
                            if (RepeatList.UniqueAdd(item))
                            {
                                ThreadManager.CreateThreadStart(() => RepeatCmd(item));
                            }
                            else
                            {
                                lock (item)
                                {
                                    TimerCommand Listcmd = RepeatList.Find((f) => f.Equals(item));
                                    if (Listcmd.RepeatTime == 0)
                                    {
                                        RepeatList.Remove(Listcmd);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(ThreadSleep * (1 + DateTime.Now.Second / 60)); // wait for awhile before checking commands again
                }
            }
            catch (ThreadInterruptedException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void RepeatCmd(TimerCommand cmd)
        {
            int repeat = 0;  // determined seconds for the repeat timer commands
            bool InCategory = false; // flag to determine category matching
            bool ResetLive = false; // flag to check reset when going live and going offline, to avoid continuous resets

            lock (cmd) // lock the cmd because it's referenced in other threads
            {
                repeat = cmd.RepeatTime;
                // verify if the category is different and the command is no longer applicable
                InCategory = cmd.CategoryList.Contains(Category) || cmd.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
            }

            try
            {
                while (OptionFlags.ActiveToken && repeat != 0 && InCategory && ChatBotStarted && OptionFlags.RepeatTimer)
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
                        OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = ParseCommand(cmd.Command, BotUserName, null, DataManage.GetCommand(cmd.Command), out short multi, Bots.Default, true), RepeatMsg = multi });
                        lock (cmd)
                        {
                            cmd.UpdateTime(CheckDilute());
                        }
                    }
                    Thread.Sleep(ThreadSleep * (1 + (DateTime.Now.Second / 60)));

                    lock (cmd) // lock the cmd because it's referenced in other threads
                    {
                        Tuple<string, int, string[]> command = DataManage.GetTimerCommand(cmd.Command);
                        if (command == null) // when command disappears
                        {
                            repeat = 0;
                        }
                        else
                        {
                            repeat = command.Item2;
                            cmd.ModifyTime(repeat, CheckDilute());
                        }
                        // verify if the category is different and the command is no longer applicable
                        InCategory = cmd.CategoryList.Contains(Category) || cmd.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                    }
                }
                cmd.ModifyTime(0, CheckDilute());
            }
            catch (ThreadInterruptedException ex)
            {
                LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private double CheckDilute()
        {
            double temp = 1.0;

            if (OptionFlags.RepeatTimerDilute)
            {
                DateTime now = DateTime.Now.ToLocalTime();

                // 10+ viewers per 1/2 hr, or 20chats in 15 minutes; == 1.0 dilute

                int newchats = chats, newviewers = viewers;

                if ((now - chattime) >= new TimeSpan(0, 15, 0))
                {
                    chattime = now;
                    newchats = GetCurrentChatCount - chats;
                }

                if ((now - viewertime) >= new TimeSpan(0, 30, 0))
                {
                    viewertime = now;
                    newviewers = GetUserCount;
                }

                double factor = (newchats + newviewers) / (ChatCount + ViewerCount);

                temp = 1.0 + (factor > 1.0 ? 0 : 1.0 - factor);
            }
            return temp;
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

        public void EvalCommand(CmdMessage cmdMessage, Bots source)
        {
            string result;
            cmdMessage.UserType = ParsePermission(cmdMessage);

            CommandsRow cmdrow = DataManage.GetCommand(cmdMessage.CommandText);
            short multi = 0;

            if (cmdrow == null)
            {
                result = LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound);
            }
            else if (!cmdrow.IsEnabled)
            {
                result = "";
            }
            else if ((ViewerTypes)Enum.Parse(typeof(ViewerTypes), cmdrow.Permission) < cmdMessage.UserType)
            {
                result = LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidCommand);
            }
            else
            {
                // parse commands, either built-in or custom
                result = ParseCommand(cmdMessage.CommandText, cmdMessage.DisplayName, cmdMessage.CommandArguments, cmdrow, out multi, source);
            }

            result = $"{(cmdrow.IsEnabled && ((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "")}{result}";

            ProcessedCommand?.Invoke(this, new() { Msg = result, RepeatMsg = multi });
        }

        /// <summary>
        /// Call to check all users in the stream, and shout them.
        /// </summary>
        /// <param name="Source">The name of the Bot calling the shout-outs, for purposes of which platform to call the category.</param>
        public void AutoShoutUsers()
        {
            // TODO: if adding non-Twitch platforms, need to adjust to call the correct platform-to get the channel category

            List<LiveUser> CurrActiveUsers;
            lock (CurrUsers)
            {
                CurrActiveUsers = new(CurrUsers);
            }

            ThreadManager.CreateThreadStart(() =>
            {
                foreach (LiveUser U in CurrActiveUsers)
                {
                    CheckShout(U.UserName, out _, U.Source);
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
        public void CheckShout(string UserName, out string response, Bots Source, bool AutoShout = true)
        {
            response = "";
            if (DataManage.CheckShoutName(UserName) || !AutoShout)
            {
                if(AutoShout && OptionFlags.MsgSendSOToChat)
                {
                    ProcessedCommand?.Invoke(this, new() { RepeatMsg = 0, Msg = $"!{LocalizedMsgSystem.GetVar(DefaultCommand.so)} {UserName}" });
                }
                response = ParseCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so), UserName, new(), DataManage.GetCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so)), out short multi, Source);

                // handle when returned without #category in the message
                if (response != "")
                {
                    ProcessedCommand?.Invoke(this, new() { Msg = response, RepeatMsg = multi });
                }
            }
        }

        public string CheckWelcomeUser(string User)
        {
            return DataManage.CheckWelcomeUser(User);
        }

        public string ParseCommand(string command, string DisplayName, List<string> arglist, CommandsRow cmdrow, out short multi, Bots Source, bool ElapsedTimer = false)
        {
            string result = "";
            string tempHTMLResponse = "";
            Dictionary<string, string> datavalues = null;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                result = DataManage.AddCommand(newcom[1..], CommandParams.Parse(arglist));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.settitle))
            {
                if (arglist.Count > 0)
                {
                    bool success = BotController.ModifyChannelInformation(Source, Title: string.Join(' ', arglist));
                    result = success ? cmdrow.Message : LocalizedMsgSystem.GetVar("MsgNoSuccess");
                }
                else
                {
                    result = LocalizedMsgSystem.GetVar("MsgNoTitleCategory");
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.setcategory))
            {
                if (arglist.Count > 0)
                {
                    if (int.TryParse(arglist[0], out int GameId))
                    {
                        BotController.ModifyChannelInformation(Source, CategoryId: GameId.ToString());
                        result = cmdrow.Message;
                    }
                    else
                    {
                        bool success = false;
                        string CategoryName = string.Join(' ', arglist);

                        Tuple<string, string> found = DataManage.GetGameCategories().Find((x) => x.Item2 == CategoryName);

                        if (found != null)
                        {
                            success = BotController.ModifyChannelInformation(Source, CategoryId: found.Item1);
                        }
                        else
                        {
                            success = BotController.ModifyChannelInformation(Source, CategoryName: CategoryName);
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
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.socials))
            {
                result = cmdrow.Message + " " + DataManage.GetSocials();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.uptime))
            {
                result = VariableParser.ParseReplace(OptionFlags.IsStreamOnline ? (DataManage.GetCommand(command).Message ?? LocalizedMsgSystem.GetVar(Msg.Msguptime)) : LocalizedMsgSystem.GetVar(Msg.Msgstreamoffline), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new( MsgVars.user, ChannelName ),
                    new( MsgVars.uptime, FormatData.FormatTimes(GetCurrentStreamStart) )
                }));
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
                    ? PartyCommand(command, DisplayName, arglist.Count > 0 ? arglist[0] : "", cmdrow)
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
            else
            {
                string paramvalue = cmdrow.AllowParam
                    ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                        ? DisplayName
                        : arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0]
                    : DisplayName;
                datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                    new( MsgVars.user, paramvalue ),
                    new( MsgVars.url, paramvalue ),
                    new( MsgVars.time, DateTime.Now.ToLocalTime().ToShortTimeString() ),
                    new( MsgVars.date, DateTime.Now.ToLocalTime().ToShortDateString() ),
                    new( MsgVars.com, paramvalue)
                });

                if (command == LocalizedMsgSystem.GetVar(DefaultCommand.so) && !BotController.VerifyUserExist(paramvalue, Source))
                {
                    result = LocalizedMsgSystem.GetVar(Msg.MsgNoUserFound);
                }
                else
                {
                    if (cmdrow.lookupdata)
                    {
                        LookupQuery(cmdrow, paramvalue, ref datavalues);
                    }

                    if (cmdrow.Message.Contains(MsgVars.category.ToString()))
                    {
                        ThreadManager.CreateThreadStart(() =>
                        {
                            VariableParser.AddData(ref datavalues,
                            new Tuple<MsgVars, string>[] { new(MsgVars.category, BotController.GetUserCategory(paramvalue, Source) ?? LocalizedMsgSystem.GetVar(Msg.MsgNoCategory)) });

                            result = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                            tempHTMLResponse = VariableParser.ParseReplace(cmdrow.Message, datavalues, true);
                            result = (((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "") + result;

                            ProcessedCommand?.Invoke(this, new() { Msg = result, RepeatMsg = cmdrow.SendMsgCount });

                            OnCheckOverlayEvent(new() { OverlayType = MediaOverlayServer.Enums.OverlayTypes.Commands, Action = DefaultCommand.so.ToString(), UserName = DisplayName, UserMsg = tempHTMLResponse });
                        });

                        result = "";
                    }
                    else
                    {
                        result = VariableParser.ParseReplace(cmdrow.Message, datavalues);
                        tempHTMLResponse = VariableParser.ParseReplace(cmdrow.Message, datavalues, true);
                    }
                }
            }
            result = (((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "") + result;
            multi = cmdrow.SendMsgCount;

            if (result != "")
            {
                OnCheckOverlayEvent(new() { OverlayType = MediaOverlayServer.Enums.OverlayTypes.Commands, Action = command, UserName = DisplayName, UserMsg = tempHTMLResponse });
            }

            return result;
        }

        private static string PartyCommand(string command, string DisplayName, string argument, CommandsRow cmdrow)
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

        private static void LookupQuery(CommandsRow CommData, string paramvalue, ref Dictionary<string, string> datavalues)
        {
            //TODO: the commands with data lookup needs a lot of work!

            switch (CommData.top)
            {
                case > 0:
                case -1:
                    {
                        if (CommData.action != CommandAction.Get.ToString())
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidComUsage), CommData.CmdName, CommData.action, CommandAction.Get.ToString()));
                        }

                        // convert multi-row output to a string
                        string queryoutput = "";
                        foreach (Tuple<object, object> bundle in from object r in DataManage.PerformQuery(CommData, CommData.top)
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
                        else if (querydata.GetType() == typeof(int))
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
