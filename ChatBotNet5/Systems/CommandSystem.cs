using ChatBot_Net5.BotClients;
using ChatBot_Net5.Data;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Events;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.Systems
{
    public class CommandSystem : INotifyPropertyChanged
    {
        private readonly DataManager datamanager;
        private readonly StatisticsSystem StatData;
        private readonly string BotUserName;
        private Thread ElapsedThread;

        // bubbles up messages from the event timers because there is no invoking method to receive this output message 
        internal event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;

        // bubble up the user request for joining the game queue list
        internal event EventHandler<UserJoinArgs> UserJoinCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string ParamName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        internal CommandSystem(DataManager dataManager, StatisticsSystem statistics, string BotName)
        {
            datamanager = dataManager;
            StatData = statistics;
            BotUserName = BotName;

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
            // TODO: consider slower timers (dilute timers, make them longer) when channel isn't as active
            // TODO: consider some AI bot chat when channel is slower
            List<TimerCommand> RepeatList = new();

            foreach (Tuple<string, int, string[]> Timers in datamanager.GetTimerCommands())
            {
                if (Timers.Item3.Contains(StatData.Category) || Timers.Item3.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry)))
                {
                    RepeatList.Add(new(Timers));
                }
            }

            while (OptionFlags.ProcessOps)
            {
                foreach (TimerCommand timer in RepeatList)
                {
                    if (timer.CheckFireTime() && (timer.CategoryList.Contains(StatData.Category) || timer.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry))))
                    {
                        timer.UpdateTime();
                        string output = PerformCommand(timer.Command, BotUserName, null, true);

                        OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = output });
                    }
                }

                // check if any commands are added to the repeat timers, does not remove until bot is stopped and started again
                foreach (Tuple<string, int, string[]> Timers in datamanager.GetTimerCommands())
                {
                    TimerCommand command = new(Timers);
                    if (command.CategoryList.Contains(StatData.Category) || command.CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry)))
                    {
                        if (!RepeatList.Contains(command))
                        {
                            RepeatList.Add(command);
                            string output = PerformCommand(command.Command, BotUserName, null, true);

                            OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = output });
                        }
                        else
                        {
                            TimerCommand update = RepeatList.Find((a) => a.Command == command.Command);
                            update.RepeatTime = command.RepeatTime;
                        }
                    }
                }

                for (int x = RepeatList.Count - 1; x >= 0 && RepeatList.Count > 0; x--)
                {
                    if (RepeatList[x].RepeatTime == 0 && (!RepeatList[x].CategoryList.Contains(StatData.Category) || !RepeatList[x].CategoryList.Contains(LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry))))
                    {
                        RepeatList.RemoveAt(x);
                    }
                }

                Thread.Sleep(5000); // wait for awhile before checking commands again
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
            if (datamanager.CheckShoutName(UserName) || !AutoShout)
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
        /// <param name="chatMessage"></param>
        /// <exception cref="InvalidOperationException">The user calling the chat command does not have permission.</exception>
        /// <exception cref="NullReferenceException">A referenced object is null and cannot be accessed.</exception>
        /// <exception cref="KeyNotFoundException">Command is not found in the command listing.</exception>
        /// <returns>The resulting value of the command.</returns>
        public string ParseCommand(string command, List<string> arglist, ChatMessage chatMessage)
        {
            ViewerTypes InvokerPermission = ParsePermission(chatMessage);

            // no permission, stop processing
            if (!CheckPermission(command, InvokerPermission))
            {
                throw new InvalidOperationException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidCommand));
            }

            return PerformCommand(command, chatMessage.DisplayName, arglist);
        }

        /// <summary>
        /// Review the permission of the invoker to activate the command
        /// </summary>
        /// <param name="command">the command to check</param>
        /// <param name="chatMessage">From the invoker, contains different flags indicating permission.</param>
        /// <returns></returns>
        private bool CheckPermission(string command, ViewerTypes InvokerPermission)
        {
            return datamanager.CheckPermission(command, InvokerPermission);
        }

        private string PerformCommand(string command, string DisplayName, List<string> arglist, bool ElapsedTimer = false)
        {
            arglist?.ForEach((s) => s = s.Trim());

            if (command == LocalizedMsgSystem.GetVar(DefaultCommand.addcommand))
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);
                return datamanager.AddCommand(newcom[1..], CommandParams.Parse(arglist));
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.socials))
            {
                return datamanager.GetSocials();
            }
            else if (command == LocalizedMsgSystem.GetVar(DefaultCommand.uptime))
            {
                string msg = OptionFlags.IsStreamOnline ? datamanager.GetCommand(command).Message ?? LocalizedMsgSystem.GetVar(Msg.Msguptime) : LocalizedMsgSystem.GetVar(Msg.Msgstreamoffline);


                return VariableParser.ParseReplace(msg, VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                {
                            new( MsgVars.user, TwitchBots.TwitchChannelName ),
                            new( MsgVars.uptime, FormatData.FormatTimes(StatData.GetCurrentStreamStart()) )
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

                DataSource.CommandsRow CommData = datamanager.GetCommand(command);

                string paramvalue = CommData.AllowParam
                    ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                        ? DisplayName
                        : arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0]
                    : DisplayName;

                Dictionary<string, string> datavalues = VariableParser.BuildDictionary(new Tuple<MsgVars, string>[]
                            {
                            new( MsgVars.user, paramvalue ),
                            new( MsgVars.url, paramvalue ),
                            new( MsgVars.time, DateTime.Now.ToLocalTime().TimeOfDay.ToString() ),
                            new( MsgVars.date, DateTime.Now.Date.ToLocalTime().ToString() )
                            });

                if (CommData.lookupdata)
                {
                    LookupQuery(CommData, paramvalue, ref datavalues);
                }

                return (OptionFlags.MsgPerComMe && CommData.AddMe ? "/me " : "") + VariableParser.ParseReplace(CommData.Message, datavalues);
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
                { "#time", DateTime.Now.TimeOfDay.ToString() },
                { "#date", DateTime.Now.Date.ToString() }
            };

            if (CommData.lookupdata)
            {
                LookupQuery(CommData, paramvalue, ref datavalues);
            }

            string response = BotController.ParseReplace(CommData.Message, datavalues);

            return (OptionFlags.PerComMeMsg && CommData.AddMe ? "/me " : "") + response;
        } 
         */

        private void LookupQuery(DataSource.CommandsRow CommData, string paramvalue, ref Dictionary<string, string> datavalues)
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
                        foreach (Tuple<object, object> bundle in from object r in datamanager.PerformQuery(CommData, CommData.top)
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
                        object querydata = datamanager.PerformQuery(CommData, paramvalue);

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

        internal void UserParty(string command, List<string> arglist, string UserName)
        {
            DataSource.CommandsRow CommData = datamanager.GetCommand(command);

            UserJoinArgs userJoinArgs = new();
            userJoinArgs.Command = command;
            userJoinArgs.AddMe = CommData.AddMe;
            userJoinArgs.ChatUser = UserName;
            userJoinArgs.GameUserName = arglist == null || arglist.Count == 0 ? UserName : arglist[0];

            // we have to invoke an event, because the GUI thread must be used to manipulate the data collection for the user list
            UserJoinCommand?.Invoke(this, userJoinArgs);
        }

        /// <summary>
        /// Establishes the permission level for the user who sends the message.
        /// </summary>
        /// <param name="chatMessage">The ChatMessage holding the characteristics of the user who invoked the chat command, which parses out the user permissions.</param>
        /// <returns>The ViewerType corresponding to the user's highest permission.</returns>
        internal ViewerTypes ParsePermission(ChatMessage chatMessage)
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
            else if (datamanager.CheckFollower(chatMessage.DisplayName))
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
    }
}
