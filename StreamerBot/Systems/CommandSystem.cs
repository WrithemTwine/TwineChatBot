using StreamerBot.Data;
using StreamerBot.Enum;
using StreamerBot.Events;
using StreamerBot.Models;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

using TwitchLib.PubSub.Models.Responses;

using static StreamerBot.Data.DataSource;

namespace StreamerBot.Systems
{
    public class CommandSystem : SystemsBase, INotifyPropertyChanged
    {
        private Thread ElapsedThread;

        // bubbles up messages from the event timers because there is no invoking method to receive this output message 
        public event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string ParamName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        public CommandSystem()
        {
            StartElapsedTimerThread();
        }

        private void StartElapsedTimerThread()
        {
            ElapsedThread = new Thread(new ThreadStart(ElapsedCommandTimers));
            ElapsedThread.Start();
        }

        public void StopElapsedTimerThread()
        {
            ElapsedThread.Join();
        }

        /// <summary>
        /// Performs the commands with timers > 0 seconds. Runs on a separate thread.
        /// </summary>
        private void ElapsedCommandTimers()
        {
            // TODO: fix repeating commands not updating their time, duplicate commands running


            // TODO: consider some AI bot chat when channel is slower
            List<TimerCommand> RepeatList = new();

            DateTime chattime = DateTime.Now.ToLocalTime(); // the time to check chats sent
            DateTime viewertime = DateTime.Now.ToLocalTime(); // the time to check viewers
            int chats = GetCurrentChatCount;
            int viewers = GetUserCount;

            const int ChatCount = 20;
            const int ViewerCount = 10;
            const int ThreadSleep = 5000;

            double CheckDilute()
            {
                if (OptionFlags.RepeatTimerDilute)
                {
                    // 10+ viewers per hr, or 20chats in 15 minutes; == 1.0 dilute
                    if (chats >= ChatCount || viewers >= ViewerCount)
                    {
                        return 1.0;
                    }

                    DateTime now = DateTime.Now.ToLocalTime();
                    if (now - chattime >= new TimeSpan(0, 15, 0) || now - viewertime >= new TimeSpan(1, 0, 0))
                    {
                        int newchats, newviewers;
                        if (now - chattime >= new TimeSpan(0, 15, 0)) { chattime = now; newchats = GetCurrentChatCount; }
                        else
                        {
                            newchats = chats;
                        }

                        if (now - viewertime >= new TimeSpan(1, 0, 0)) { viewertime = now; newviewers = GetUserCount; }
                        else
                        {
                            newviewers = viewers;
                        }

                        double temp = 1.0 + newchats >= ChatCount || newviewers >= ViewerCount ? 0 : (1.0 - (Math.Abs(newchats + newviewers) / (ChatCount + ViewerCount)));
                        chats = newchats;
                        viewers = newviewers;

                        return temp;
                    }
                }
                return 1.0;
            }

            void RepeatCmd(TimerCommand cmd)
            {
                int repeat = 0;
                bool InCategory = false;

                lock (cmd) // lock the cmd because it's referenced in other threads
                {
                    repeat = cmd.RepeatTime;
                    // verify if the category is different and the command is no longer applicable
                    InCategory = cmd.CategoryList.Contains(Category) || cmd.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                }

                while (
                    OptionFlags.ProcessOps
                    && repeat != 0
                    && InCategory)
                {
                    if ((cmd.NextRun - DateTime.Now.ToLocalTime()).Seconds <= 0)
                    {
                        OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = PerformCommand(cmd.Command, BotUserName, null, true) });
                        lock (cmd)
                        {
                            cmd.UpdateTime(CheckDilute());
                        }
                    }
                    Thread.Sleep(ThreadSleep * (1 + (DateTime.Now.Second / 60)));

                    lock (cmd) // lock the cmd because it's referenced in other threads
                    {
                        Tuple<string, int, string[]> command = DataManage.GetTimerCommand(cmd.Command);
                        if (command == null)
                        {
                            repeat = 0;
                        }
                        else
                        {
                            repeat = command.Item2;
                        }
                        // verify if the category is different and the command is no longer applicable
                        InCategory = cmd.CategoryList.Contains(Category) || cmd.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                    }
                }
            }

            double DiluteTime = CheckDilute();

            while (OptionFlags.ProcessOps)
            {
                foreach (Tuple<string, int, string[]> Timers in DataManage.GetTimerCommands())
                {
                    if (Timers.Item3.Contains(Category) || Timers.Item3.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry)))
                    {
                        TimerCommand item = new(Timers, DiluteTime);
                        if (!RepeatList.Contains(item))
                        {
                            RepeatList.Add(item);
                            new Thread(new ThreadStart(() => RepeatCmd(item))).Start();
                        }
                        else
                        {
                            TimerCommand Listcmd = RepeatList.Find((f) => f.Command == item.Command);

                            if (Listcmd.RepeatTime == 0)
                            {
                                RepeatList.Remove(Listcmd);
                            }
                            else
                            {
                                lock (Listcmd)
                                {
                                    Listcmd.RepeatTime = item.RepeatTime; // update repeat time, the task will update time after it runs one loop of repeat
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(ThreadSleep * (1 + DateTime.Now.Second / 60)); // wait for awhile before checking commands again
            }
        }

        /// <summary>
        /// See if the user is part of the user's auto-shout out list to determine if the message should be called
        /// </summary>
        /// <param name="UserName">The user to check</param>
        /// <param name="response">the response message template</param>
        /// <param name="AutoShout">true-check if the user is on the autoshout list, false-the method call is from a command, no autoshout check</param>
        /// <returns></returns>
        public bool CheckShout(string UserName, out string response, bool AutoShout = true)
        {
            response = "";
            if (DataManage.CheckShoutName(UserName) || !AutoShout)
            {
                response = PerformCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so), UserName, new());
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Parses the command and performs the operation of the command. Some exceptions can bubble up from underlying database.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arglist"></param>
        /// <param name="cmdMessage"></param>
        /// <exception cref="InvalidOperationException">The user calling the chat command does not have permission.</exception>
        /// <exception cref="NullReferenceException">A referenced object is null and cannot be accessed.</exception>
        /// <exception cref="KeyNotFoundException">Command is not found in the command listing.</exception>
        /// <returns>The resulting value of the command.</returns>
        public string ParseCommand(CmdMessage cmdMessage)
        {
            //TODO: fix StreamerBot Commands
            ViewerTypes InvokerPermission = ParsePermission(cmdMessage);

            // no permission, stop processing
            if (!CheckPermission(cmdMessage.CommandText, InvokerPermission))
            {
               return LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidCommand);
            }

            return PerformCommand(cmdMessage.CommandText, cmdMessage.DisplayName, cmdMessage.CommandArguments);
        }

        /// <summary>
        /// Review the permission of the invoker to activate the command
        /// </summary>
        /// <param name="command">the command to check</param>
        /// <param name="chatMessage">From the invoker, contains different flags indicating permission.</param>
        /// <returns></returns>
        private bool CheckPermission(string command, ViewerTypes InvokerPermission)
        {
            return DataManage.CheckPermission(command, InvokerPermission);
        }

        private string PerformCommand(string command, string DisplayName, List<string> arglist, bool ElapsedTimer = false)
        {
            arglist?.ForEach((s) => s = s.Trim());

            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                return DataManage.AddCommand(newcom[1..], CommandParams.Parse(arglist));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.socials))
            {
                return DataManage.GetSocials();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.uptime))
            {
                string msg = OptionFlags.IsStreamOnline ? DataManage.GetCommand(command).Message ?? LocalizedMsgSystem.GetVar(Msg.Msguptime) : LocalizedMsgSystem.GetVar(Msg.Msgstreamoffline);


                return VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                            new( MsgVars.user, ChannelName ),
                            new( MsgVars.uptime, FormatData.FormatTimes(GetCurrentStreamStart) )
                })); // the message is handled at the botcontroller
            }

            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.join)
         || command == LocalizedMsgSystem.GetVar(DefaultCommand.leave)
         || command == LocalizedMsgSystem.GetVar(DefaultCommand.queue)
         || (command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo) && OptionFlags.UserPartyStop && !ElapsedTimer)) // handle case when a viewer tries to view qinfo--it's not started
            {
                UserParty(command, arglist, DisplayName);
                return ""; // the message is handled in the GUI thread
            }
            else
            {
                if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo) && OptionFlags.UserPartyStop) // case when an elapsed timer tries to invoke the qinfo for stopped queue, just blank-no response
                {
                    return ""; // skip the queue info if it's a recurring message
                }

                if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart) || command == LocalizedMsgSystem.GetVar(DefaultCommand.qstop))
                {
                    OptionFlags.SetParty(command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart));
                    NotifyPropertyChanged("UserPartyStart");
                    NotifyPropertyChanged("UserPartyStop");
                }

                CommandsRow CommData = DataManage.GetCommand(command);
                Dictionary<string, string> datavalues = null;

                if (CommData != null)
                {
                    string paramvalue = CommData.AllowParam
                        ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                            ? DisplayName
                            : arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0]
                        : DisplayName;

                    datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                                {
                            new( MsgVars.user, paramvalue ),
                            new( MsgVars.url, paramvalue ),
                            new( MsgVars.time, DateTime.Now.ToLocalTime().TimeOfDay.ToString() ),
                            new( MsgVars.date, DateTime.Now.ToLocalTime().Date.ToString() )
                                });

                    if (CommData.lookupdata)
                    {
                        LookupQuery(CommData, paramvalue, ref datavalues);
                    }
                }

                return (OptionFlags.MsgPerComMe && CommData.AddMe ? "/me " : "") + VariableParser.ParseReplace(CommData?.Message ?? LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound), datavalues);
            }
        }

        /*
         if (command == "addcommand")
        {
            string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
            arglist.RemoveAt(0);

            CommandParams addparams = CommandParams.Parse(arglist);

            return datamanager.AddCommand(newcom[1..], addparams);
        }
        else if (command == "socials")
        {
            return datamanager.GetSocials();
        }
        else if (command == "uptime")
        {
            GetUpTimeCommand?.Invoke(this, new() { Message = datamanager.GetCommand(command).Message, User = IOModule.TwitchChannelName });
            return ""; // the message is handled at the botcontroller
        }
        else if (command == "join"
                 || command == "leave"
                 || command == "queue"
                 || (command == "qinfo" && OptionFlags.UserPartyStop && !ElapsedTimer)) // handle case when a viewer tries to view qinfo--it's not started
        {
            UserParty(command, arglist, DisplayName);
            return ""; // the message is handled in the GUI thread
        }
        else
        {
            if (command == "qinfo" && OptionFlags.UserPartyStop) // case when an elapsed timer tries to invoke the qinfo for stopped queue, just blank-no response
            {
                return ""; // skip the queue info if it's a recurring message
            }

            if (command == "qstart" || command == "qstop")
            {
                OptionFlags.SetParty(command == "qstart");
                NotifyPropertyChanged("UserPartyStart");
                NotifyPropertyChanged("UserPartyStop");
            }

            DataSource.CommandsRow CommData = datamanager.GetCommand(command);

            string paramvalue = CommData.AllowParam
                ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                    ? DisplayName
                    : arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0]
                : DisplayName;

            Dictionary<string, string> datavalues = new()
            {
                { "#user", paramvalue },
                { "#url", "http://www.twitch.tv/" + paramvalue },
                { "#time", DateTime.Now.ToLocalTime().TimeOfDay.ToString() },
                { "#date", DateTime.Now.ToLocalTime().Date.ToString() }
            };

            if (CommData.lookupdata)
            {
                LookupQuery(CommData, paramvalue, ref datavalues);
            }

            string response = BotController.ParseReplace(CommData.Message, datavalues);

            return (OptionFlags.PerComMeMsg && CommData.AddMe ? "/me " : "") + response;
        } 
         */



        public void UserParty(string command, List<string> arglist, string UserName)
        {
            DataSource.CommandsRow CommData = DataManage.GetCommand(command);

            UserJoinEventArgs userJoinArgs = new();
            userJoinArgs.Command = command;
            userJoinArgs.AddMe = CommData?.AddMe ?? false;
            userJoinArgs.ChatUser = UserName;
            userJoinArgs.GameUserName = arglist == null || arglist.Count == 0 ? UserName : arglist[0];

            // we have to invoke an event, because the GUI thread must be used to manipulate the data collection for the user list
            ProcessCommands_UserJoinCommand(userJoinArgs);
        }

        #region New Command Code
       /// <summary>
        /// Establishes the permission level for the user who sends the message.
        /// </summary>
        /// <param name="chatMessage">The ChatMessage holding the characteristics of the user who invoked the chat command, which parses out the user permissions.</param>
        /// <returns>The ViewerType corresponding to the user's highest permission.</returns>
        public ViewerTypes ParsePermission(CmdMessage chatMessage)
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

        public string EvalCommand(CmdMessage cmdMessage)
        {
            string result = "";
            ViewerTypes InvokerPermission = ParsePermission(cmdMessage);

            CommandsRow cmdrow = DataManage.GetCommand(cmdMessage.CommandText);

            if (cmdrow == null)
            {
                result = LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound);
            }
            else if ((ViewerTypes)System.Enum.Parse(typeof(ViewerTypes), cmdrow.Permission) < InvokerPermission)
            {
                result = LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidCommand);
            }
            else
            {
                // parse commands, either built-in or custom
                result = ParseCommand(cmdMessage.CommandText, cmdMessage.DisplayName, cmdMessage.CommandArguments, cmdrow);
            }

            result = ((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe ? "/me " : "") + result;

            return result;
        }

        public string ParseCommand(string command, string DisplayName, List<string> arglist, CommandsRow cmdrow, bool ElapsedTimer = false)
        {
            string result = "";
            Dictionary<string, string> datavalues = null;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                result = DataManage.AddCommand(newcom[1..], CommandParams.Parse(arglist));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.socials))
            {
                result = cmdrow.Message + " " + DataManage.GetSocials();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.uptime))
            {
                result = VariableParser.ParseReplace(OptionFlags.IsStreamOnline ? DataManage.GetCommand(command).Message ?? LocalizedMsgSystem.GetVar(Msg.Msguptime) : LocalizedMsgSystem.GetVar(Msg.Msgstreamoffline), VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                            new( MsgVars.user, ChannelName ),
                            new( MsgVars.uptime, FormatData.FormatTimes(GetCurrentStreamStart) )
                }));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.commands))
            {

            }
            // capture all of the join queue commands
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.join)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.enqueue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.leave)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.dequeue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.queue)
                || command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo))
            {
                if (OptionFlags.UserPartyStart)
                {
                    result = PartyCommand(command, DisplayName, arglist.Count > 0 ? arglist[0] : "", cmdrow);
                }
                else
                {
                    if (ElapsedTimer)
                    {
                        result = "";
                    }
                    else
                    {
                        result = LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstop);
                    }
                }
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart) || command == LocalizedMsgSystem.GetVar(DefaultCommand.qstop))
            {
                result = cmdrow.Message;
                OptionFlags.SetParty(command == LocalizedMsgSystem.GetVar(DefaultCommand.qstart));
                NotifyPropertyChanged("UserPartyStart");
                NotifyPropertyChanged("UserPartyStop");
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
                    new( MsgVars.time, DateTime.Now.ToLocalTime().TimeOfDay.ToString() ),
                    new( MsgVars.date, DateTime.Now.ToLocalTime().Date.ToString() )
                });

                if (cmdrow.lookupdata)
                {
                    LookupQuery(cmdrow, paramvalue, ref datavalues);
                }

                result = VariableParser.ParseReplace(cmdrow.Message, datavalues);
            }

            return result;
        }

        private string PartyCommand(string command, string DisplayName, string argument, CommandsRow cmdrow)
        {
            UserJoin newuser = new() { ChatUser = DisplayName };
            if (argument != "")
            {
                newuser.GameUserName = argument;
            }

            string response;
            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.queue))
            {
                response = string.Format("There are {0} users in the join queue: {1}", JoinCollection.Count, JoinCollection.Count==0 ? "no users!" : string.Join(", ", JoinCollection) );
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.qinfo))
            {
                response = cmdrow.Message;
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.enqueue) || command == LocalizedMsgSystem.GetVar(DefaultCommand.join))
            {
                if (JoinCollection.Contains(newuser))
                {
                    response = string.Format("You have already joined. You are currently number {0}.", JoinCollection.IndexOf(newuser) + 1);
                }
                else
                {
                    response = string.Format("You have joined the queue. You are currently {0}.", (JoinCollection.Count + 1));
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

        private void LookupQuery(CommandsRow CommData, string paramvalue, ref Dictionary<string, string> datavalues)
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

                        VariableParser.AddData(ref datavalues, new Tuple<MsgVars, string>[] { new(MsgVars.query, output) });
                        break;
                    }
            }
        }

        #endregion


        #region Join Collection

        private delegate void UpdateUsers(UserJoin userJoin);

        private void AddJoinUser(UserJoin userJoin)
        {
            JoinCollection.Add(userJoin);
        }

        private void RemoveJoinUser(UserJoin userJoin)
        {
            JoinCollection.Remove(userJoin);
        }

        private void ProcessCommands_UserJoinCommand(UserJoinEventArgs e)
        {
            string response = "";

            if (OptionFlags.MsgPerComMe && e.AddMe)
            {
                response = "/me ";
            }

            // TODO: convert the queue join messages to localized strings
            if (OptionFlags.UserPartyStart)
            {
                switch (e.Command)
                {
                    case "join":
                        int x = 1;

                        foreach (UserJoin u in JoinCollection)
                        {
                            if (u.ChatUser == e.ChatUser)
                            {
                                response = string.Format("You have already joined. You are currently number {0}.", x.ToString());
                                break;
                            }
                            x++;
                        }

                        response = "You have joined the queue. You are currently " + (JoinCollection.Count + 1) + ".";
                        UpdateUsers AddUpdate = AddJoinUser;
                        Application.Current.Dispatcher.BeginInvoke(AddUpdate, new UserJoin() { ChatUser = e.ChatUser, GameUserName = e.GameUserName });
                        break;
                    case "leave":
                        UserJoin remove = null;
                        foreach (UserJoin u in JoinCollection)
                        {
                            if (u.ChatUser == e.ChatUser) { remove = u; }
                        }
                        if (remove == null)
                        {
                            response = "You are not in the queue.";
                        }
                        else
                        {
                            UpdateUsers RemUpdate = RemoveJoinUser;
                            Application.Current.Dispatcher.BeginInvoke(RemUpdate, remove);
                            response = "You are no longer in the queue.";
                        }
                        break;
                    case "queue":
                        int y = 1;
                        string queuelist = string.Empty;
                        if (JoinCollection != null)
                        {
                            foreach (UserJoin u in JoinCollection)
                            {
                                queuelist += y.ToString() + ". " + u.ChatUser + " ";
                                y++;
                            }
                        }
                        response = "The current users in the join queue: " + (queuelist == string.Empty ? "no users!" : queuelist);
                        break;
                    default:
                        response = "Command not understood!";
                        break;
                }
            }
            else
            {
                response = "The join queue list is not started.";
            }

            //CallbackSendMsg(response);
        }

        #endregion
    }
}
