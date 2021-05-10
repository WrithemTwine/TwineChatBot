using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.Data
{
    public class CommandSystem : INotifyPropertyChanged
    {
        private DataManager datamanager;

        //private ObservableCollection<UserJoin> JoinCollection;

        private string BotUserName;

        internal event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;
        internal event EventHandler<UserJoinArgs> UserJoinCommand;
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(string ParamName="" )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }

        internal CommandSystem(DataManager dataManager, string BotName)
        {
            datamanager = dataManager;
            BotUserName = BotName;

            new Thread(new ThreadStart(ElapsedCommandTimers)).Start();
        }


        /// <summary>
        /// Performs the commands with timers > 0 seconds. Runs on a separate thread.
        /// </summary>
        private void ElapsedCommandTimers()
        {
            List<TimerCommand> RepeatList = new();

            foreach(Tuple<string, int> Timers in datamanager.GetTimerCommands())
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
                        string output = PerformCommand(timer.Command, BotUserName, null);

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
                        string output = PerformCommand(command.Command, BotUserName, null);

                        OnRepeatEventOccured?.Invoke(this, new TimerCommandsEventArgs() { Message = output });
                    }
                    else
                    {
                        RepeatList.Find((a) => a.Command == command.Command).RepeatTime = command.RepeatTime;
                    }
                }

                for (int x = RepeatList.Count; x==0 && RepeatList.Count>0; x--)
                {
                    if (RepeatList[x].RepeatTime == 0)
                    {
                        RepeatList.RemoveAt(x);
                    }
                }

                Thread.Sleep(5000); // wait for awhile before checking commands again
            }
        }

        public bool CheckShout(string UserName, out string response)
        {
            response = "";
            if (datamanager.CheckShoutName(UserName))
            {
                response = PerformCommand("so", UserName, new());
                return true;
            } else
            return false;
        }

        /// <summary>
        /// Parses the command and performs the operation of the command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arglist"></param>
        /// <param name="chatMessage"></param>
        /// <exception cref="InvalidOperationException">The user calling the chat command does not have permission.</exception>
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

        private string PerformCommand(string command, string DisplayName, List<string> arglist)
        {
            arglist.ForEach((s) => s = s.Trim());

            if (command == "addcommand")
            {
                string newcom = arglist[0][0] == '!' ? arglist[0] : string.Empty;
                arglist.RemoveAt(0);

                CommandParams addparams = CommandParams.Parse(arglist);

                datamanager.AddCommand(newcom.Substring(1), addparams);
            }
            else if (command == "socials")
            {
                return datamanager.GetSocials();
            }
            else if (command == "join" || command == "leave" || command == "queue")
            {
                UserParty(command, arglist, DisplayName);
                return ""; // the message is handled in the GUI thread
            }
            else 
            {
                if(command=="qinfo" && OptionFlags.UserPartyStop)
                {
                    return ""; // skip the queue info if it's a recurring message
                }

                if (command == "qstart" || command == "qstop")
                {
                    OptionFlags.SetParty(command == "qstart");
                    NotifyPropertyChanged("UserPartyStart");
                    NotifyPropertyChanged("UserPartyStop");
                }

                datamanager.GetCommand(command, out DataSource.CommandsRow CommData);

                string paramvalue;

                if (CommData.AllowParam)
                {
                    if (arglist == null || arglist.Count == 0 || arglist[0] == string.Empty)
                    {
                        paramvalue = DisplayName;
                    }
                    else if (arglist[0].Contains('@'))
                    {
                        paramvalue = arglist[0].Remove(0, 1);
                    }
                    else
                    {
                        paramvalue = arglist[0];
                    }
                }
                else
                {
                    paramvalue = DisplayName;
                }

                Dictionary<string, string> datavalues = new()
                {
                    { "#user", paramvalue },
                    { "#url", "http://www.twitch.tv/" + paramvalue },
                    { "#time", DateTime.Now.TimeOfDay.ToString() },
                    { "#date", DateTime.Now.Date.ToString() }
                };

                object querydata = null;

                if (CommData.lookupdata)
                {
                    if (CommData.top > 0)
                    {
                        if (CommData.action != CommandAction.Get.ToString())
                        {
                            throw new InvalidOperationException(string.Format("The command {0} is configured for {1}, but can only perform {2}", CommData.CmdName, CommData.action, CommandAction.Get.ToString()));
                        }
                    }
                    else
                    {
                        querydata = datamanager.PerformQuery(CommData, paramvalue);

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
                    }
                }

                string response = BotController.ParseReplace(CommData.Message, datavalues);

                return (OptionFlags.PerComMeMsg && CommData.AddMe ? "/me " : "" ) + response;
            }

            return "not finished";
        }

        internal void UserParty(string command, List<string> arglist, string UserName)
        {
            datamanager.GetCommand(command, out DataSource.CommandsRow CommData);

            UserJoinArgs userJoinArgs = new();
            userJoinArgs.Command = command;
            userJoinArgs.AddMe = CommData.AddMe;
            userJoinArgs.ChatUser = UserName;
            userJoinArgs.GameUserName = arglist.Count == 0 ? UserName : arglist[0];

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
