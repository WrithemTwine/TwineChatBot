using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.Systems
{
    public class ManageStreamViewers
    {
        private readonly List<ManageStreamViewer> ManageViewers = [];
        public int Count => (from V in ManageViewers where V.InStreamNow select V).Count();

        /// <summary>
        /// Ending the stream, remove users who didn't join the stream, clear all the user flags for current users
        /// </summary>
        public void EndStreamResetList()
        {
            LogWriter.DebugLog("EndStreamResetList", DebugLogTypes.ManageStreamViewers, "Ending the stream, resetting the user list.");
            // remove the viewers that didn't join the channel - primarily stream to stream list management
            ManageViewers.RemoveAll((v) => v.FirstJoinedChannel == false);

            foreach (var viewer in ManageViewers)
            {
                viewer.EvaluateCurrentCheck = false;
                viewer.FirstChatMessage = false;
                viewer.FirstJoinedChannel = false;
                viewer.InStreamNow = false;
            }
        }

        /// <summary>
        /// Add the specific user list into the "ManageViewers" list, and add a new record or edit existing record
        /// to indicate the users in the current evaluation and now in stream.
        /// </summary>
        /// <param name="users"></param>
        private void AddNewUsers(List<LiveUser> users)
        {
            LogWriter.DebugLog("AddNewUsers", DebugLogTypes.ManageStreamViewers, "Adding new users to the list.");
            ManageViewers.ForEach(v => {
                v.InStreamNow = false;
                v.EvaluateCurrentCheck = false;
            }); // set all users in stream to false, set true with source users

            foreach (var (user, Curr) in from user in users
                                         let Curr = (from V in ManageViewers where V.LiveUser == user select V).FirstOrDefault()
                                         select (user, Curr))
            {
                if (Curr == null)
                {
                    ManageViewers.Add(new(user, evaluateCurrentCheck: true, inStreamNow: true));
                }
                else
                {
                    Curr.EvaluateCurrentCheck = true;
                    Curr.InStreamNow = true;
                }
            }
        }

        /// <summary>
        /// Add new users and review the current users list and determine if these users have 
        /// first joined the channel for the current stream.
        /// </summary>
        /// <param name="users">List of users to check</param>
        /// <returns>A list of the LiveUsers who have first joined the channel in the current stream.</returns>
        public List<LiveUser> AddUsersFirstJoinedChannel(List<LiveUser> users)
        {
            LogWriter.DebugLog("AddUsersFirstJoinedChannel", DebugLogTypes.ManageStreamViewers, "Adding users to the first joined channel list.");
            AddNewUsers(users);

            var result = (from V in ManageViewers
                          where V.EvaluateCurrentCheck && !V.FirstJoinedChannel
                          select V.LiveUser).ToList();

            foreach (var user in (from V in ManageViewers
                                  where V.EvaluateCurrentCheck
                                  select V))
            {
                user.FirstJoinedChannel = true;
                user.EvaluateCurrentCheck = false;
            }

            return result;
        }

        /// <summary>
        /// Add new users and review current users list to determine if the provided
        /// users have not chatted until now.
        /// </summary>
        /// <param name="users">The user list to add and check if they haven't chatted yet.</param>
        /// <returns>A LiveUser list of active users who have not yet chatted.</returns>
        public List<LiveUser> AddUsersFirstChatMessage(List<LiveUser> users)
        {
            LogWriter.DebugLog("AddUsersFirstChatMessage", DebugLogTypes.ManageStreamViewers, "Adding users to the first chat message list.");
            AddNewUsers(users);

            var result = (from V in ManageViewers
                          where V.EvaluateCurrentCheck && !V.FirstChatMessage
                          select V.LiveUser).ToList();

            foreach (var user in from V in ManageViewers
                                 where V.EvaluateCurrentCheck && !V.FirstChatMessage
                                 select V)
            {
                user.FirstChatMessage = true;
                user.EvaluateCurrentCheck = false;
            }

            return result;
        }

        /// <summary>
        /// Review managed user list for users no longer in stream as specified by provided user list.
        /// </summary>
        /// <param name="users">User list to determine current in stream users.</param>
        /// <returns>List of users no longer in the stream.</returns>
        public List<LiveUser> GetUsersLeft(List<LiveUser> users)
        {
            LogWriter.DebugLog("GetUsersLeft", DebugLogTypes.ManageStreamViewers, "Checking for users who have left the stream.");

            // find the users marked in stream, but not in the supplied user list to check
            List<LiveUser> result = [];

            foreach (var user in users)
            {
                if (!(from V in ManageViewers  // add any missing users
                      where V.LiveUser == user
                      select V).Any())
                {
                    ManageViewers.Add(new(user, evaluateCurrentCheck: false, inStreamNow: true));
                }
            }

            foreach (var MV in (from V in ManageViewers
                                where users.Contains(V.LiveUser)
                                select V))
            {
                MV.InStreamNow = true;
            }

            foreach (var MV in (from V in ManageViewers
                                where !users.Contains(V.LiveUser)
                                select V))
            {
                MV.InStreamNow = false;
                result.Add(MV.LiveUser);
            }

            return result;
        }

        /// <summary>
        /// Retrieve the current users active in the current stream.
        /// </summary>
        /// <returns>List of users found in the current stream.</returns>
        public List<LiveUser> GetCurrentActiveUsers()
        {
            LogWriter.DebugLog("GetCurrentActiveUsers", DebugLogTypes.ManageStreamViewers, "Retrieving the current active users.");
            return [.. (from V in ManageViewers
                    where V.InStreamNow
                    select V.LiveUser)];
        }

    }
}
