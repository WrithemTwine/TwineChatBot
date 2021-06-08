using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Clients;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Events;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using TwitchLib.Client.Models;
using System.Linq;
using System.Globalization;

namespace ChatBot_Net5.Data
{
    public class CommandSystem : INotifyPropertyChanged
    {
        private readonly DataManager datamanager;
        private readonly string BotUserName;
        private Thread ElapsedThread;

        internal event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;
        internal event EventHandler<UserJoinArgs> UserJoinCommand;
        internal event EventHandler<UpTimeCommandArgs> GetUpTimeCommand;
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string ParamName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        internal CommandSystem(DataManager dataManager, string BotName)
        {
            datamanager = dataManager;
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
            List<TimerCommand> RepeatList = new();

            foreach (Tuple<string, int> Timers in datamanager.GetTimerCommands())
            {
                RepeatList.Add(new(Timers));
            }

            while (OptionFlags.ProcessOps)
            {
                foreach (TimerCommand timer in RepeatList)
                {
                    if (timer.CheckFireTime())
                    {
                        timer.UpdateTime();
                        string output = PerformCommand(timer.Command, BotUserName, null, true);

                        OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = output });
                    }
                }

                // check if any commands are added to the repeat timers, does not remove until bot is stopped and started again
                foreach (Tuple<string, int> Timers in datamanager.GetTimerCommands())
                {
                    TimerCommand command = new(Timers);
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

                for (int x = RepeatList.Count - 1; x >= 0 && RepeatList.Count > 0; x--)
                {
                    if (RepeatList[x].RepeatTime == 0)
                    {
                        RepeatList.RemoveAt(x);
                    }
                }

                Thread.Sleep(5000); // wait for awhile before checking commands again
            }
        }

        public bool CheckShout(string UserName, out string response, bool AutoShout = true)
        {
            response = "";
            if (datamanager.CheckShoutName(UserName) || !AutoShout)
            {
                response = PerformCommand("so", UserName, new());
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
                throw new InvalidOperationException("No permission to invoke this command.");
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

            switch (command)
            {
                case "addcommand":
                    {
                        string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                        arglist.RemoveAt(0);

                        return datamanager.AddCommand(newcom[1..], CommandParams.Parse(arglist));
                    }

                case "socials":
                    return datamanager.GetSocials();
                case "uptime":
                    GetUpTimeCommand?.Invoke(this, new() { Message = datamanager.GetCommand(command).Message, User = IOModule.TwitchChannelName });
                    return ""; // the message is handled at the botcontroller
                case "join":
                case "leave":
                case "queue":
                case "qinfo" when OptionFlags.UserPartyStop && !ElapsedTimer:
                    UserParty(command, arglist, DisplayName);
                    return ""; // the message is handled in the GUI thread
                default:
                    {
                        switch (command) // case when an elapsed timer tries to invoke the qinfo for stopped queue, just blank-no response
                        {
                            case "qinfo" when OptionFlags.UserPartyStop:
                                return ""; // skip the queue info if it's a recurring message
                            case "qstart":
                            case "qstop":
                                OptionFlags.SetParty(command == "qstart");
                                NotifyPropertyChanged("UserPartyStart");
                                NotifyPropertyChanged("UserPartyStop");
                                break;
                        }

                        DataSource.CommandsRow CommData = datamanager.GetCommand(command);

                        string paramvalue = CommData.AllowParam
                            ? arglist == null || arglist.Count == 0 || arglist[0] == string.Empty
                                ? DisplayName
                                : arglist[0].Contains('@') ? arglist[0].Remove(0, 1) : arglist[0]
                            : DisplayName;

                        //TODO: research and consider a dictionary static class to keep these keys uniform and scalable
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

                //TODO: research and consider a dictionary static class to keep these keys uniform and scalable
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

            //return "not finished";
        }

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
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The command {0} is configured for {1}, but can only perform {2}", CommData.CmdName, CommData.action, CommandAction.Get.ToString()));
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
                        datavalues.Add("#query", queryoutput);
                        break;
                    }

                default:
                    {
                        var querydata = datamanager.PerformQuery(CommData, paramvalue);

                        string output = "";
                        if (querydata.GetType() == typeof(string))
                        {
                            output = (string)querydata;
                        }
                        else if (querydata.GetType() == typeof(TimeSpan))
                        {
                            output = BotController.FormatTimes((TimeSpan)querydata);
                        }
                        else if (querydata.GetType() == typeof(DateTime))
                        {
                            output = ((DateTime)querydata).ToShortDateString();
                        }
                        else if (querydata.GetType() == typeof(int))
                        {
                            output = querydata.ToString();
                        }

                        datavalues.Add("#query", output);
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
