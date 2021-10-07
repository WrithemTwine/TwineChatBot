using ChatBot_Net5.Data;
using ChatBot_Net5.Enum;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Reflection;

using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Systems
{
    /// <summary>
    /// The common shared operations class between each of the subsystems. Should not be referenced outside of <c>ChatBot_Net5.Systems</c> namespace.
    /// Perform direct DataManager tasks here.
    /// Each Subsystem class derives from this base class and can access the DataManager and static properties here to share data between systems.
    /// </summary>
    public class SystemsBase
    {
        public static DataManager DataManage { get; set; }

        public static string Category { get; set; }
        public static Action<string> CallbackSendMsg;
        protected const int SecondsDelay = 5000;
        protected static bool StreamUpdateClockStarted;

        protected static List<string> CurrUsers { get; private set; } = new();
        protected static List<string> UniqueUserJoined { get; private set; } = new();
        protected static List<string> UniqueUserChat { get; private set; } = new();
        protected static List<string> ModUsers { get; private set; } = new();
        protected static List<string> SubUsers { get; private set; } = new();
        protected static List<string> VIPUsers { get; private set; } = new();

        protected StreamStat CurrStream = new();

        public SystemsBase() { }

        public static void ManageDatabase()
        {
            // TODO: add fixes if user re-enables 'managing { users || followers || stats }' to restart functions without restarting the bot

            if (!OptionFlags.ManageUsers)
            {
                // if ManageUsers is False, then remove users!
                DataManage.RemoveAllUsers();
            }

            // if ManageFollowers is False, then remove followers!, upstream code stops the follow bot
            if (!OptionFlags.ManageFollowers)
            {
                DataManage.RemoveAllFollowers();
            }

            // when management resumes, code upstream enables the startbot process

            //  if ManageStreamStats is False, then remove all Stream Statistics!

            if (!OptionFlags.ManageStreamStats)
            {
                // when the LiveStream Online event fires again, the datacollection will restart
                //  if ManageStreamStats is False, then remove all Stream Statistics!
                DataManage.RemoveAllStreamStats();
                // when the LiveStream Online event fires again, the datacollection will restart
            }

            if (!OptionFlags.ManageRaidData)
            {
                DataManage.RemoveAllRaidData();
            }
        }

        public static void UpdateFollowers(string ChannelName, List<Follow> Follows)
        {
            DataManage.UpdateFollowers(ChannelName, new() { { ChannelName, Follows } });
        }
        /// <summary>
        /// Add currency accrual rows for every user when a new currency type is added to the database
        /// </summary>
        public void UpdateCurrencyTable()
        {
            DataManage.AddCurrencyRows();
        }

        public void ClearWatchTime()
        {
            DataManage.ClearWatchTime();
        }

        public void ClearAllCurrenciesValues()
        {
            DataManage.ClearAllCurrencyValues();
        }

        public bool AddClip(Clip c)
        {
            return DataManage.AddClip(c.Id, c.CreatedAt, c.Duration, c.GameId, c.Language, c.Title, c.Url);
        }

        /// <summary>
        /// Retrieves the current users within the channel during the stream.
        /// </summary>
        /// <returns>The current user count as of now.</returns>
        public int GetUserCount
        {
            get
            {
                lock (CurrUsers)
                {
                    return CurrUsers.Count;
                }
            }
        }
        /// <summary>
        /// Retrieve how many chats have occurred in the current live stream to now.
        /// </summary>
        /// <returns>Current total chats as of now.</returns>
        public int GetCurrentChatCount
        {
            get
            {
                lock (CurrStream)
                {
                    return CurrStream.TotalChats;
                }
            }
        }

        /// <summary>
        /// Returns the start of the current active online stream.
        /// </summary>
        /// <returns>The DateTime of the stream start time.</returns>
        public DateTime GetCurrentStreamStart
        {
            get
            {
                return CurrStream.StreamStart;
            }
        }
    }
}
