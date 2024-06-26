﻿using StreamerBotLib.Data;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;

namespace StreamerBotLib.Systems
{

    /// <summary>
    /// The common shared operations class between each of the subsystems. 
    /// Should not be referenced outside of <c>StreamerBotLib.Systems</c> namespace.
    /// Perform direct DataManager tasks here.
    /// Each Subsystem class derives from this base class and can access the 
    /// DataManager and static properties here to share data between systems.
    /// </summary>
    internal partial class ActionSystem
    {
        internal static DataManager DataManage { get; set; }
        public static FlowDocument ChatData { get; private set; } = new();
        public static ObservableCollection<UserJoin> JoinCollection { get; set; } = [];
        public static ObservableCollection<string> GiveawayCollection { get; set; } = [];
        public static ObservableCollection<string> CurrUserJoin { get; private set; } = [];

        public static string Category { get; set; }
        /// <summary>
        /// The streamer channel monitored.
        /// </summary>
        public static string ChannelName { get; set; }
        /// <summary>
        /// The account user name of the bot account.
        /// </summary>
        public static string BotUserName { get; set; }
        /// <summary>
        /// Time delays to use in threads
        /// </summary>
        protected const int SecondsDelay = 2000;

        protected static List<LiveUser> CurrUsers { get; private set; } = [];
        protected static List<string> UniqueUserJoined { get; private set; } = [];
        protected static List<string> UniqueUserChat { get; private set; } = [];
        protected static List<string> ModUsers { get; private set; } = [];
        protected static List<string> SubUsers { get; private set; } = [];
        protected static List<string> VIPUsers { get; private set; } = [];

        protected static StreamStat CurrStream { get; set; } = new();

        /// <summary>
        /// Returns the start of the current active online stream.
        /// </summary>
        /// <returns>The DateTime of the stream start time.</returns>
        private static DateTime GetCurrentStreamStart => CurrStream.StreamStart;

        private delegate void ProcMessage(string UserName, string Message);

        public ActionSystem()
        {
        }

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
                DataManage.RemoveAllInRaidData();
            }

            if (!OptionFlags.ManageOutRaidData)
            {
                DataManage.RemoveAllOutRaidData();
            }

            if (!OptionFlags.ManageGiveawayUsers)
            {
                DataManage.RemoveAllGiveawayData();
            }

            if (!OptionFlags.ManageOverlayTicker)
            {
                DataManage.RemoveAllOverlayTickerData();
            }
        }

        /// <summary>
        /// Add currency accrual rows for every user when a new currency type is added to the database
        /// </summary>
        public static void UpdateCurrencyTable()
        {
            DataManage.PostCurrencyRows();
        }

        public static void ClearWatchTime()
        {
            DataManage.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            DataManage.ClearAllCurrencyValues();
        }

        public static void ClearUsersNonFollowers()
        {
            DataManage.ClearUsersNotFollowers();
        }

        public static void SetSystemEventsEnabled(bool Enabled)
        {
            DataManage.SetSystemEventsEnabled(Enabled);
        }

        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            DataManage.SetBuiltInCommandsEnabled(Enabled);
        }

        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            DataManage.SetUserDefinedCommandsEnabled(Enabled);
        }

        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            DataManage.SetDiscordWebhooksEnabled(Enabled);
        }

        public static void PostUpdatedDataRow(bool RowChanged)
        {
            DataManage.PostUpdatedDataRow(RowChanged);
        }

        public static void DeleteRows(IEnumerable<DataRow> dataRows)
        {
            DataManage.DeleteDataRows(dataRows);
        }

        public static void AddNewAutoShoutUser(string UserName, string UserId, string platform)
        {
            DataManage.PostNewAutoShoutUser(UserName, UserId, platform);
        }

        internal static void UpdatedIsEnabledRows(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            lock (GUI.GUIDataManagerLock.Lock)
            {
                DataManage.SetIsEnabled(dataRows, IsEnabled);
            }
        }

        internal static bool CheckField(string dataTable, string fieldName)
        {
            return DataManage.CheckField(dataTable, fieldName);
        }

        public static bool AddClip(Clip c)
        {
            return DataManage.PostClip(c.ClipId, c.CreatedAt, c.Duration, c.GameId, c.Language, c.Title, c.Url);
        }

        /// <summary>
        /// Retrieves the current users within the channel during the stream.
        /// </summary>
        /// <returns>The current user count as of now.</returns>
        public static int GetUserCount
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
        public static int GetCurrentChatCount
        {
            get
            {
                lock (CurrStream)
                {
                    return CurrStream.TotalChats;
                }
            }
        }

        public static void UpdateGUICurrUsers()
        {
            CurrUserJoin.Clear();
            CurrUsers.Sort();
            foreach (LiveUser liveUser in CurrUsers)
            {
                CurrUserJoin.Add(liveUser.UserName);
            }
        }

        internal static void AddChatString(string UserName, string Message)
        {
            Application.Current?.Dispatcher.BeginInvoke(new ProcMessage(UpdateGUIChatMessages), UserName, Message);
        }

        private static void UpdateGUIChatMessages(string UserName, string Message)
        {
            Paragraph p = new();
            string time = DateTime.Now.ToLocalTime().ToString("h:mm", CultureInfo.CurrentCulture) + " ";
            p.Inlines.Add(new Run(time));
            p.Inlines.Add(new Run(UserName + ": "));
            p.Inlines.Add(new Run(Message));
            //p.Foreground = new SolidColorBrush(Color.FromArgb(a: s.Color.A,
            //                                                  r: s.Color.R,
            //                                                  g: s.Color.G,
            //                                                  b: s.Color.B));
            ChatData.Blocks.Add(p);
        }

        internal static void OutputSentToBotsHandler(object sender, PostChannelMessageEventArgs e)
        {
            AddChatString(Settings.Default.TwitchBotUserName, e.Msg);
        }

    }
}
