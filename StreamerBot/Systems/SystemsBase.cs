using StreamerBot.Data;
using StreamerBot.Models;
using StreamerBot.Static;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;

namespace StreamerBot.Systems
{
    /// <summary>
    /// The common shared operations class between each of the subsystems. 
    /// Should not be referenced outside of <c>ChatBot_Net5.Systems</c> namespace.
    /// Perform direct DataManager tasks here.
    /// Each Subsystem class derives from this base class and can access the 
    /// DataManager and static properties here to share data between systems.
    /// </summary>
    public class SystemsBase
    {
        public static DataManager DataManage { get; set; }
        public static FlowDocument ChatData { get; private set; } = new();
        public static ObservableCollection<UserJoin> JoinCollection { get; set; } = new();
        public static ObservableCollection<string> GiveawayCollection { get; set; } = new();


        public static string Category { get; set; }

        /// <summary>
        /// The streamer channel monitored.
        /// </summary>
        public static string ChannelName { get; set; }
        public static string BotUserName { get; set; }

        protected const int SecondsDelay = 5000;
        protected static bool StreamUpdateClockStarted;

        protected static List<string> CurrUsers { get; private set; } = new();
        protected static List<string> UniqueUserJoined { get; private set; } = new();
        protected static List<string> UniqueUserChat { get; private set; } = new();
        protected static List<string> ModUsers { get; private set; } = new();
        protected static List<string> SubUsers { get; private set; } = new();
        protected static List<string> VIPUsers { get; private set; } = new();

        protected static StreamStat CurrStream { get; set; } = new();

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
        }

        /// <summary>
        /// Add currency accrual rows for every user when a new currency type is added to the database
        /// </summary>
        public static void UpdateCurrencyTable()
        {
            DataManage.AddCurrencyRows();
        }

        public static void ClearWatchTime()
        {
            DataManage.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            DataManage.ClearAllCurrencyValues();
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

        public static bool AddClip(Clip c)
        {
            return DataManage.AddClip(c.ClipId, c.CreatedAt, c.Duration, c.GameId, c.Language, c.Title, c.Url);
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
        public DateTime GetCurrentStreamStart => CurrStream.StreamStart;


        private delegate void ProcMessage(string UserName, string Message);

        internal static void AddChatString(string UserName, string Message)
        {
            Application.Current.Dispatcher.BeginInvoke(new ProcMessage(UpdateMessage), UserName, Message);
        }

        private static void UpdateMessage(string UserName, string Message)
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

    }
}
