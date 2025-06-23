
namespace StreamerBotLib.Systems
{
    using StreamerBotLib.Models;
    using StreamerBotLib.Models.Enums;
    using StreamerBotLib.Static;

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

            // Combine removal and flag reset into a single loop
            ManageViewers.RemoveAll(viewer =>
            {
                if (!viewer.FirstJoinedChannel)
                {
                    return true; // Remove viewers who didn't join the channel
                }

                // Reset flags for remaining viewers
                viewer.EvaluateCurrentCheck = false;
                viewer.FirstChatMessage = false;
                viewer.FirstJoinedChannel = false;
                viewer.InStreamNow = false;
                viewer.Registered = false;

#if DEBUG
                LogWriter.DebugLog("EndStreamResetList", DebugLogTypes.SpecialPurpose, $"User: {viewer.LiveUser.UserName} - {viewer.LiveUser.Platform}");
#endif
                return false;
            });
        }

        /// <summary>
        /// Add the specific user list into the "ManageViewers" list, and add a new record or edit existing record
        /// to indicate the users in the current evaluation and now in stream.
        /// </summary>
        /// <param name="users"></param>
        private void AddNewUsers(List<LiveUser> users)
        {
            LogWriter.DebugLog("AddNewUsers", DebugLogTypes.ManageStreamViewers, "Adding new users to the list.");

            // Use a HashSet for quick lookup of existing users
            var existingUsers = new HashSet<LiveUser>(ManageViewers.Select(v => v.LiveUser));

            foreach (var viewer in ManageViewers)
            {
                viewer.InStreamNow = false;
                viewer.EvaluateCurrentCheck = false;
            }

            foreach (var user in users)
            {
                var existingViewer = ManageViewers.FirstOrDefault(v => v.LiveUser == user);
                if (existingViewer != null)
                {
                    // Update existing viewer
                    existingViewer.EvaluateCurrentCheck = true;
                    existingViewer.InStreamNow = true;
                }
                else
                {
                    // Add new viewer
                    ManageViewers.Add(new ManageStreamViewer(user, evaluateCurrentCheck: true, inStreamNow: true));
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

            var result = new List<LiveUser>();

            foreach (var viewer in ManageViewers.Where(v => v.EvaluateCurrentCheck && !v.FirstJoinedChannel))
            {
                viewer.FirstJoinedChannel = true;
                viewer.EvaluateCurrentCheck = false;
                result.Add(viewer.LiveUser);
            }

#if DEBUG
            foreach (var user in result)
            {
                LogWriter.DebugLog("AddUsersFirstJoinedChannel", DebugLogTypes.SpecialPurpose, $"User: {user.UserName} - {user.Platform}");
            }
#endif

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

            var result = new List<LiveUser>();

            foreach (var viewer in ManageViewers.Where(v => v.EvaluateCurrentCheck && !v.FirstChatMessage))
            {
                viewer.FirstChatMessage = true;
                viewer.EvaluateCurrentCheck = false;
                result.Add(viewer.LiveUser);
            }

#if DEBUG
            foreach (var user in result)
            {
                LogWriter.DebugLog("AddUsersFirstChatMessage", DebugLogTypes.SpecialPurpose, $"User: {user.UserName} - {user.Platform}");
            }
#endif

            return result;
        }

        public void RegisterUsers(List<LiveUser> liveUsers)
        {
            LogWriter.DebugLog("RegisterUsers", DebugLogTypes.ManageStreamViewers, "Registering users in the database.");
            foreach (var user in liveUsers)
            {
                var viewer = ManageViewers.FirstOrDefault(v => v.LiveUser == user);
                if (viewer != null)
                {
                    viewer.Registered = true;
                }
            }
        }

        /// <summary>
        /// Review managed user list for users no longer in stream as specified by provided user list.
        /// </summary>
        /// <param name="users">User list to determine current in stream users.</param>
        /// <returns>List of users no longer in the stream.</returns>
        public List<LiveUser> GetUsersLeft(List<LiveUser> users)
        {
            LogWriter.DebugLog("GetUsersLeft", DebugLogTypes.ManageStreamViewers, "Checking for users who have left the stream.");

            // Add missing users from the incoming list to ManageViewers
            foreach (var user in users)
            {
                if (!ManageViewers.Any(v => v.LiveUser == user))
                {
                    ManageViewers.Add(new ManageStreamViewer(user, evaluateCurrentCheck: false, inStreamNow: true));
                }
            }

            var currentUsersSet = new HashSet<LiveUser>(users);
            var result = new List<LiveUser>();

            // determine which users are currently in stream; determine those not in stream
            foreach (var viewer in ManageViewers)
            {
                if (currentUsersSet.Contains(viewer.LiveUser))
                {
                    viewer.InStreamNow = true;
                }
                else
                {
                    if (viewer.InStreamNow)
                    {
                        result.Add(viewer.LiveUser);
                    }
                    viewer.InStreamNow = false;
                    viewer.Registered = false;
                }
            }

#if DEBUG
            foreach (var user in result)
            {
                LogWriter.DebugLog("GetUsersLeft", DebugLogTypes.SpecialPurpose, $"User: {user.UserName} - {user.Platform}");
            }
#endif

            return result;
        }

        /// <summary>
        /// Retrieve the current users active in the current stream.
        /// </summary>
        /// <param name="isRegistered">Indicates if the user is registered as joined in the database.</param>
        /// <returns>List of users found in the current stream.</returns>
        public List<LiveUser> GetCurrentActiveUsers(bool isRegistered = false)
        {
            LogWriter.DebugLog("GetCurrentActiveUsers", DebugLogTypes.ManageStreamViewers, "Retrieving the current active users.");

            var activeUsers = ManageViewers
                .Where(v => v.InStreamNow && v.Registered == isRegistered)
                .Select(v => v.LiveUser)
                .ToList();

#if DEBUG
            foreach (var user in activeUsers)
            {
                LogWriter.DebugLog("GetCurrentActiveUsers", DebugLogTypes.SpecialPurpose, $"User: {user.UserName} - {user.Platform}");
            }
#endif

            return activeUsers;
        }
    }
}
