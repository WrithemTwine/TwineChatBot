using ChatBot_Net5.Events;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System.Collections.ObjectModel;
using System.Windows;

namespace ChatBot_Net5.BotIOController
{
    public partial class BotController
    {
        public ObservableCollection<UserJoin> JoinCollection { get; set; } = new();

        //public CommandSystem ProcessCommands { get; private set; }

        private delegate void UpdateUsers(UserJoin userJoin);

        private void AddJoinUser(UserJoin userJoin)
        {
            JoinCollection.Add(userJoin);
        }

        private void RemoveJoinUser(UserJoin userJoin)
        {
            JoinCollection.Remove(userJoin);
        }

        private void ProcessCommands_OnRepeatEventOccured(object sender, TimerCommandsEventArgs e)
        {
            if (OptionFlags.RepeatTimer && (!OptionFlags.RepeatWhenLive || OptionFlags.IsStreamOnline))
            {
                Send(e.Message);
                SystemsController.UpdatedStat(Enum.StreamStatType.AutoCommands);
            }
        }

        private void ProcessCommands_UserJoinCommand(object sender, UserJoinEventArgs e)
        {
            string response = "";

            if(OptionFlags.MsgPerComMe==true && e.AddMe)
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
                                response = string.Format("You have already joined. You are currently number {0}." , x.ToString());
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

            Send(response);
        }
    }
}
