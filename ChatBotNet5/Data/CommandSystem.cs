using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.Data
{
    public class CommandSystem
    {
        private DataManager datamanager;

        public ObservableCollection<UserJoin> JoinCollection { get; set; } = new();
        private string BotUserName;

        internal event EventHandler<TimerCommandsEventArgs> OnRepeatEventOccured;

        internal CommandSystem(DataManager dataManager, string BotName)
        {
            datamanager = dataManager;
            BotUserName = BotName;

            new Thread(new ThreadStart(MonitorJoinCollection)).Start();
            new Thread(new ThreadStart(ElapsedCommandTimers)).Start();
        }

        private void MonitorJoinCollection()
        {
            while (OptionFlags.ProcessOps)
            {
                List<UserJoin> removelist = new();

                lock (JoinCollection)
                {
                    foreach (UserJoin u in JoinCollection)
                    {
                        if (u.Remove)
                        {
                            removelist.Add(u);
                        }
                    }

                    foreach (UserJoin u in removelist)
                    {
                        JoinCollection.Remove(u);
                    }
                }

                Thread.Sleep(5000);
            }
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

                for (int x = RepeatList.Count; x==0; x--)
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
                return UserParty(command, arglist, DisplayName);
            }
            else 
            {
                //string comuser = arglist.Count > 0 ? (arglist[0].Contains('@') ? arglist[0] : string.Empty) : null;

                //return datamanager.PerformCommand(command, DisplayName ?? BotUserName, comuser, arglist);

                datamanager.GetCommand(command, out string Usage, out string Message, out string ParamQuery, out bool AllowParam);

                string user = AllowParam && arglist[0]?.Contains('@')==true ? arglist[0].Remove(0, 1) : DisplayName;

                Dictionary<string, string> datavalues = new()
                {
                    { "#user", user },
                    { "#url", "http://www.twitch.tv/" + user },
                    { "#time", DateTime.Now.TimeOfDay.ToString() },
                    { "#date", DateTime.Now.Date.ToString() }
                };

                object[] comparam = null;
                if (ParamQuery != null || ParamQuery != string.Empty)
                {
                    CommandParams query = CommandParams.Parse(ParamQuery);


                }

                return BotController.ParseReplace(Message, datavalues);
            }

            return "not finished";
        }

        internal string UserParty(string command, List<string> arglist, string UserName)
        {
            lock (JoinCollection)
            {
                switch (command)
                {
                    case "join":
                        int x = 1;
                        foreach (UserJoin u in JoinCollection)
                        {
                            if (u.ChatUser == UserName) { return "You have already joined. You are currently number " + x.ToString() + "."; }
                            x++;
                        }
                        JoinCollection.Add(new UserJoin() { ChatUser = UserName, GameUserName = (arglist.Count > 0 ? arglist[0] : UserName) });
                        return "You have joined the queue. You are currently " + JoinCollection.Count + ".";

                    case "leave":
                        UserJoin remove = null;
                        foreach (UserJoin u in JoinCollection)
                        {
                            if (u.ChatUser == UserName) { remove = u; }
                        }
                        if (remove == null)
                        {
                            return "You are not in the queue.";
                        }
                        else
                        {
                            JoinCollection.Remove(remove);
                            return "You are no longer in the queue.";
                        }
                    case "queue":
                        int y = 1;
                        string queuelist = string.Empty;
                        foreach (UserJoin u in JoinCollection)
                        {
                            queuelist += y.ToString() + ". " + u.ChatUser + " ";
                            y++;
                        }
                        return "The current users in the join queue: " + (queuelist == string.Empty ? "no users!" : queuelist);
                }
            }
            return "You have reached an unknown spot.";
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
