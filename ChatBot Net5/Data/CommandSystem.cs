using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchLib.Client.Models;

namespace ChatBot_Net5.Data
{
    public class CommandSystem
    {
        private DataManager datamanager;


        internal CommandSystem(DataManager dataManager)
        {
            datamanager = dataManager;
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

            return "not finished";
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
