using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Properties;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
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
    public partial class ActionSystem
    {
        internal static IDataManager DataManage { get; set; }
        public static FlowDocument ChatData { get; private set; } = new();
        public static ObservableCollection<UserJoin> JoinCollection { get; set; } = [];
        public static ObservableCollection<LiveUser> GiveawayCollection { get; set; } = [];
        public static ObservableCollection<string> CurrUserJoin { get; private set; } = [];

        public static string Category { get; set; }
        /// <summary>
        /// The streamer channel monitored.
        /// </summary>
        public static string ChannelName => OptionFlags.TwitchChannelName;
        /// <summary>
        /// The account user name of the bot account.
        /// </summary>
        public static string BotUserName => OptionFlags.TwitchBotUserName;
        /// <summary>
        /// Time delays to use in threads
        /// </summary>
        protected const int SecondsDelay = 4000;


        internal static ManageStreamViewers StreamViewers { get; } = new();

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
        { }

        public static void ManageDatabase()
        {
            LogWriter.DebugLog("ManageDatabase", Enums.DebugLogTypes.CommonSystem, "Managing Database.");
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

        public static void ClearWatchTime()
        {
            LogWriter.DebugLog("ClearWatchTime", Enums.DebugLogTypes.CommonSystem, "Clearing Watch Time.");
            DataManage.ClearWatchTime();
        }

        public static void ClearAllCurrenciesValues()
        {
            LogWriter.DebugLog("ClearAllCurrenciesValues", Enums.DebugLogTypes.CommonSystem, "Clearing All Currency Values.");
            DataManage.ClearAllCurrencyValues();
        }

        public static void ClearUsersNonFollowers()
        {
            LogWriter.DebugLog("ClearUsersNonFollowers", Enums.DebugLogTypes.CommonSystem, "Clearing Users Non-Followers.");
            DataManage.ClearUsersNotFollowers();
        }

        public static void SetSystemEventsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetSystemEventsEnabled", Enums.DebugLogTypes.CommonSystem, $"Setting System Events Enabled: {Enabled}");
            DataManage.SetSystemEventsEnabled(Enabled);
        }

        public static void SetBuiltInCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetBuiltInCommandsEnabled", Enums.DebugLogTypes.CommonSystem, $"Setting Built-In Commands Enabled: {Enabled}");
            DataManage.SetBuiltInCommandsEnabled(Enabled);
        }

        public static void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetUserDefinedCommandsEnabled", Enums.DebugLogTypes.CommonSystem, $"Setting User Defined Commands Enabled: {Enabled}");
            DataManage.SetUserDefinedCommandsEnabled(Enabled);
        }

        public static void SetDiscordWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetDiscordWebhooksEnabled", Enums.DebugLogTypes.CommonSystem, $"Setting Discord Webhooks Enabled: {Enabled}");
            DataManage.SetWebhooksEnabled(Enabled);
        }

        public static void PostUpdatedDataRow(bool RowChanged)
        {
            LogWriter.DebugLog("PostUpdatedDataRow", Enums.DebugLogTypes.CommonSystem, $"Posting Updated DataRow: {RowChanged}");
            //DataManage.PostUpdatedDataRow(RowChanged);
        }

        public static void DeleteRows(IEnumerable<DataRow> dataRows)
        {
            LogWriter.DebugLog("DeleteRows", Enums.DebugLogTypes.CommonSystem, $"Deleting Rows: {dataRows.Count()}");
            DataManage.DeleteDataRows(dataRows);
        }

        public static void AddNewAutoShoutUser(string UserId, Enums.Platform platform)
        {
            LogWriter.DebugLog("AddNewAutoShoutUser", Enums.DebugLogTypes.CommonSystem, $"Adding New AutoShout User: {UserId}");
            DataManage.PostNewAutoShoutUser(UserId, platform);
        }

        internal static void UpdatedIsEnabledRows(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            LogWriter.DebugLog("UpdatedIsEnabledRows", Enums.DebugLogTypes.CommonSystem, $"Updating IsEnabled Rows: {dataRows.Count()}");
            DataManage.SetIsEnabled(dataRows, IsEnabled);
        }

        internal static bool CheckField(string dataTable, string fieldName)
        {
            LogWriter.DebugLog("CheckField", Enums.DebugLogTypes.CommonSystem, $"Checking Field: {fieldName}");
            return DataManage.CheckField(dataTable, fieldName);
        }

        public static bool AddClip(Clip c)
        {
            LogWriter.DebugLog("AddClip", Enums.DebugLogTypes.CommonSystem, $"Adding Clip: {c.Title}");
            return DataManage.PostClip(c.ClipId, DateTime.Parse(c.CreatedAt).ToLocalTime(), (decimal)c.Duration, c.GameId, c.Language, c.Title, c.Url, c.FromUserId, c.FromUserName);
        }

        /// <summary>
        /// Retrieves the current users within the channel during the stream.
        /// </summary>
        /// <returns>The current user count as of now.</returns>
        public static int GetUserCount
        {
            get
            {
                LogWriter.DebugLog("GetUserCount", Enums.DebugLogTypes.CommonSystem, "Getting User Count.");
                lock (StreamViewers)
                {
                    return StreamViewers.GetCurrentActiveUsers().Count;
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
                LogWriter.DebugLog("GetCurrentChatCount", Enums.DebugLogTypes.CommonSystem, "Getting Current Chat Count.");
                lock (CurrStream)
                {
                    return CurrStream.TotalChats;
                }
            }
        }

        public static void UpdateGUICurrUsers()
        {
            LogWriter.DebugLog("UpdateGUICurrUsers", Enums.DebugLogTypes.CommonSystem, "Updating GUI Current Users.");
            CurrUserJoin.Clear();
            var curr = StreamViewers.GetCurrentActiveUsers();
            curr.Sort();

            foreach (LiveUser liveUser in curr)
            {
                CurrUserJoin.Add(liveUser.UserName);
            }
        }

        internal static void AddChatString(string UserName, string Message)
        {
            LogWriter.DebugLog("AddChatString", Enums.DebugLogTypes.CommonSystem, $"Adding Chat String: {UserName}: {Message}");
            ThreadManager.AddTaskToGUIDispatcher(() => UpdateGUIChatMessages(UserName, Message));
        }

        private static void UpdateGUIChatMessages(string UserName, string Message)
        {
            LogWriter.DebugLog("UpdateGUIChatMessages", Enums.DebugLogTypes.CommonSystem, $"Updating GUI Chat Messages: {UserName}: {Message}");

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
            LogWriter.DebugLog("OutputSentToBotsHandler", Enums.DebugLogTypes.CommonSystem, $"Output Sent To Bots Handler: {e.Msg}");
            AddChatString(Settings.Default.TwitchBotUserName, e.Msg);
        }

    }
}
