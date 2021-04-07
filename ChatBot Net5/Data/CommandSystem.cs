using ChatBot_Net5.BotIOController;
using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.Data
{
    public class CommandSystem
    {
        private DataManager datamanager;

        public ObservableCollection<UserJoin> JoinCollection { get; private set; } = new();

        internal CommandSystem(DataManager dataManager)
        {
            datamanager = dataManager;
            new Thread(new ThreadStart(MonitorJoinCollection)).Start();
        }

        private void MonitorJoinCollection()
        {
            while (ThreadFlags.ProcessOps)
            {
                List<UserJoin> removelist = new();

                lock (JoinCollection)
                {
                    foreach(UserJoin u in JoinCollection)
                    {
                        if(u.Remove) { removelist.Add(u); }
                    }

                    foreach (UserJoin u in removelist)
                    {
                        JoinCollection.Remove(u);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public bool CheckShout(string UserName, string InvokedUserName, out string response)
        {
            response = "";
            if (datamanager.CheckShoutName(UserName))
            {
                response = datamanager.PerformCommand("so", InvokedUserName, UserName, new());
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
            ViewerTypes Chatter = ParsePermission(chatMessage);

            if (!datamanager.CheckPermission(command, Chatter))
            {
                throw new InvalidOperationException("No permission to invoke this command.");
            }

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
                return UserParty(command, arglist, chatMessage);
            }
            else
            {
                string comuser = arglist.Count>0 ? (arglist[0].Contains('@') ? arglist[0] : string.Empty) : null;

                return datamanager.PerformCommand(command, chatMessage.DisplayName, comuser, arglist);
            }

            return "not finished";
        }

        internal string UserParty(string command, List<string> arglist, ChatMessage chatMessage)
        {
            lock (JoinCollection)
            {
                switch (command)
                {
                    case "join":
                        int x = 1;
                        foreach (UserJoin u in JoinCollection)
                        {
                            if (u.ChatUser == chatMessage.Username) { return "You have already joined. You are currently number " + x.ToString() + "."; }
                            x++;
                        }
                        JoinCollection.Add(new UserJoin() { ChatUser = chatMessage.Username, GameUserName = (arglist.Count > 0 ? arglist[0] : chatMessage.Username) });
                        return "You have joined the queue. You are currently " + JoinCollection.Count + ".";

                    case "leave":
                        UserJoin remove = null;
                        foreach (UserJoin u in JoinCollection)
                        {
                            if (u.ChatUser == chatMessage.Username) { remove = u; }
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
