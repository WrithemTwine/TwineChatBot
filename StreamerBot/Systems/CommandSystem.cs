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

using static StreamerBot.Data.DataSource;

namespace StreamerBot.Systems
{
    public class CommandSystem : SystemsBase, INotifyPropertyChanged
    {
        private Thread ElapsedThread;

        // TODO: add "!time" command for current time per the streamer's location

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

        private const int ChatCount = 20;
        private const int ViewerCount = 25;
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
            // TODO: fix repeating commands not updating their time, duplicate commands running


            // TODO: consider some AI bot chat when channel is slower
            List<TimerCommand> RepeatList = new();

            chattime = DateTime.Now.ToLocalTime(); // the time to check chats sent
            viewertime = DateTime.Now.ToLocalTime(); // the time to check viewers
            chats = GetCurrentChatCount;
            viewers = GetUserCount;

            double DiluteTime;

            while (OptionFlags.ActiveToken)
            {
                DiluteTime = CheckDilute();

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
                        }
                    }
                }

                Thread.Sleep(ThreadSleep * (1 + DateTime.Now.Second / 60)); // wait for awhile before checking commands again
            }
        }

        private void RepeatCmd(TimerCommand cmd)
        {
            int repeat = 0;
            bool InCategory = false;

            lock (cmd) // lock the cmd because it's referenced in other threads
            {
                repeat = cmd.RepeatTime;
                // verify if the category is different and the command is no longer applicable
                InCategory = cmd.CategoryList.Contains(Category) || cmd.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
            }

            while (OptionFlags.ActiveToken && repeat != 0 && InCategory)
            {
                if (cmd.NextRun <= DateTime.Now.ToLocalTime())
                {
                    OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = ParseCommand(cmd.Command, BotUserName, null, DataManage.GetCommand(cmd.Command), out short multi, true), RepeatMsg = multi });
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
                        cmd.ModifyTime(repeat, CheckDilute());
                    }
                    // verify if the category is different and the command is no longer applicable
                    InCategory = cmd.CategoryList.Contains(Category) || cmd.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                }
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

        public string EvalCommand(CmdMessage cmdMessage, out short multi)
        {
            string result = "";
            ViewerTypes InvokerPermission = ParsePermission(cmdMessage);

            CommandsRow cmdrow = DataManage.GetCommand(cmdMessage.CommandText);
            multi = 0;

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
                result = ParseCommand(cmdMessage.CommandText, cmdMessage.DisplayName, cmdMessage.CommandArguments, cmdrow, out multi);
            }

            result = (((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "") + result;
            return result;
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
                response = ParseCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so), UserName, new(), DataManage.GetCommand(LocalizedMsgSystem.GetVar(DefaultCommand.so)), out short multi);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string ParseCommand(string command, string DisplayName, List<string> arglist, CommandsRow cmdrow, out short multi, bool ElapsedTimer = false)
        {
            string result;
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

            result = (((OptionFlags.MsgPerComMe && cmdrow.AddMe) || OptionFlags.MsgAddMe) && !result.StartsWith("/me ") ? "/me " : "") + result;

            multi = cmdrow.RepeatMsg;
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
                List<string> JoinChatUsers = new();
                foreach(UserJoin u in JoinCollection)
                {
                    JoinChatUsers.Add(u.ChatUser);
                }

                response = string.Format("There are {0} users in the join queue: {1}", JoinCollection.Count, JoinCollection.Count==0 ? "no users!" : string.Join(", ", JoinChatUsers ) );
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
    }
}
