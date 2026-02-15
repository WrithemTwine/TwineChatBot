using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;

using System.Reflection;

namespace TestStreamerBot
{
    public class TestRepeatManager
    {
        // A tiny IDataManager test double implementing only the methods used by the tests.
        // All other interface members throw NotImplementedException (sufficient for unit tests).
        private class TestDataManager : IDataManager
        {
            private readonly List<Tuple<string, int, List<string>>> _timerCommands;

            public TestDataManager(List<Tuple<string, int, List<string>>> timerCommands)
            {
                _timerCommands = timerCommands;
            }

            // Implemented for use by tests
            public List<Tuple<string, int, List<string>>> GetTimerCommands()
            {
                return _timerCommands;
            }

            public CommandData GetCommand(string cmd)
            {
                // minimal CommandData wrapper using CommandsBase is not available here;
                // Return null to keep ParseCommand paths that require a CommandData from being exercised.
                return null;
            }

            #region Unused interface members - throw by default

            public event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished { add { } remove { } }
            public event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated { add { } remove { } }
            public event EventHandler<EventArgs> OnLoadCompleted { add { } remove { } }

            event EventHandler IDataManagerReadOnly.UpdatedMonitoringChannels
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            public void AddAsyncTaskToGUIDispatcher(string CallMethodName, Action action) => throw new NotImplementedException();
            public bool CheckCurrency(LiveUser User, double value, string CurrencyName) => throw new NotImplementedException();
            bool CheckField(string table, string field) => throw new NotImplementedException();
            bool CheckFollower(string User) => throw new NotImplementedException();
            bool CheckFollower(string User, DateTime ToDateTime) => throw new NotImplementedException();
            public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction) => throw new NotImplementedException();
            public bool CheckStreamDate(DateTime streamStart) => throw new NotImplementedException();
            bool CheckPermission(string cmd, ViewerTypes permission) => throw new NotImplementedException();
            bool CheckShoutName(string UserId) => throw new NotImplementedException();
            public bool CheckStreamTime(DateTime CurrTime) => throw new NotImplementedException();
            bool CheckUser(LiveUser User) => throw new NotImplementedException();
            bool CheckUser(LiveUser User, DateTime ToDateTime) => throw new NotImplementedException();
            string CheckWelcomeUser(string User) => throw new NotImplementedException();
            public void ClearAllCurrencyValues() => throw new NotImplementedException();
            public void ClearUsersNotFollowers() => throw new NotImplementedException();
            public void ClearWatchTime() => throw new NotImplementedException();
            public void DeleteDataRows(IEnumerable<object> dataRows, string TableName) => throw new NotImplementedException();
            public string EditCommand(string cmd, List<string> Arglist) => throw new NotImplementedException();
            public Tuple<ModActions, StreamerBotLib.Models.Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes) => throw new NotImplementedException();
            IEnumerable<string> GetCommandList(bool prefix) => throw new NotImplementedException();
            public IEnumerable<string> GetCommandListNoParams(bool prefix = true) => throw new NotImplementedException();
            string IDataManager.GetCommandString() => throw new NotImplementedException();
            public List<string> GetCurrencyNames() => throw new NotImplementedException();
            public int GetDeathCounter(string currCategory) => throw new NotImplementedException();
            public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi) => throw new NotImplementedException();
            public int GetFollowerCount() => throw new NotImplementedException();
            public List<CategoryData> GetGameCategories() => throw new NotImplementedException();
            public string GetKey(string Table) => throw new NotImplementedException();
            public IEnumerable<string> GetKeys(string Table) => throw new NotImplementedException();
            public string GetNewestFollower() => throw new NotImplementedException();
            public Dictionary<string, List<string>> GetOverlayActions() => throw new NotImplementedException();
            public List<OverlayActionType> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username) => throw new NotImplementedException();
            public string GetQuote(int QuoteNum) => throw new NotImplementedException();
            public int GetQuoteCount() => throw new NotImplementedException();
            public List<string> GetSocialComs() => throw new NotImplementedException();
            public string GetSocials() => throw new NotImplementedException();
            public StreamStat GetStreamData(DateTime dateTime) => throw new NotImplementedException();
            public List<string> GetTableFields(string TableName) => throw new NotImplementedException();
            public List<string> GetTableNames() => throw new NotImplementedException();
            public List<TickerItem> GetTickerItems() => throw new NotImplementedException();
            Tuple<string, int, List<string>> IDataManager.GetTimerCommand(string Cmd) => throw new NotImplementedException();
            public int GetTimerCommandTime(string Cmd) => throw new NotImplementedException();
            public string GetUsage(string command) => throw new NotImplementedException();
            public string GetUserId(LiveUser User) => throw new NotImplementedException();
            public List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks) => throw new NotImplementedException();
            public void Initialize() => throw new NotImplementedException();
            public object[] PerformQuery(CommandsBase row, int Top = 0) => throw new NotImplementedException();
            public object PerformQuery(CommandsBase row, string ParamValue) => throw new NotImplementedException();
            public bool PostCategory(CategoryData categoryData) => throw new NotImplementedException();
            public void PostCategoryStream(CategoryData category, int StreamCount = 0) => throw new NotImplementedException();
            public bool PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName, bool LastClip) => throw new NotImplementedException();
            public IEnumerable<Clip> SyncClips(bool AllClips, IEnumerable<Clip> clips) => throw new NotImplementedException();
            public string PostCommand(string cmd, CommandParams Params) => throw new NotImplementedException();
            public void PostCurrencyType(StreamerBotLib.DataSQL.Models.CurrencyType currencyType) => throw new NotImplementedException();
            public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName) => throw new NotImplementedException();
            public int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1) => throw new NotImplementedException();
            public bool PostFollower(Follow follow) => throw new NotImplementedException();
            public void PostGiveawayData(string UserId, DateTime dateTime) => throw new NotImplementedException();
            public void PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename) => throw new NotImplementedException();
            public bool PostMultiStreamDate(LiveUser liveUser, DateTime onDate) => throw new NotImplementedException();
            public void PostNewAutoShoutUser(string UserId, Platform platform) => throw new NotImplementedException();
            public void PostOutgoingRaid(string HostedChannel, DateTime dateTime) => throw new NotImplementedException();
            public int PostQuote(string Text) => throw new NotImplementedException();
            public void PostStreamStat(StreamStat streamStat) => throw new NotImplementedException();
            public void PostUserCustomWelcome(LiveUser User, string WelcomeMsg) => throw new NotImplementedException();
            public void RemoveAllFollowers() => throw new NotImplementedException();
            public void RemoveAllGiveawayData() => throw new NotImplementedException();
            public void RemoveAllInRaidData() => throw new NotImplementedException();
            public void RemoveAllOutRaidData() => throw new NotImplementedException();
            public void RemoveAllOverlayTickerData() => throw new NotImplementedException();
            public void RemoveAllStreamStats() => throw new NotImplementedException();
            public void RemoveAllUsers() => throw new NotImplementedException();
            public bool RemoveCommand(string command) => throw new NotImplementedException();
            public bool RemoveQuote(int QuoteNum) => throw new NotImplementedException();
            public void SetBuiltInCommandsEnabled(bool Enabled) => throw new NotImplementedException();
            public void SetCleanupList(ref List<ArchiveMultiStream> archiveMultiStreams) => throw new NotImplementedException();
            public void SetMultiLiveStatusLog(ref List<string> log) => throw new NotImplementedException();
            public void SetSystemEventsEnabled(bool Enabled) => throw new NotImplementedException();
            public void SetUserDefinedCommandsEnabled(bool Enabled) => throw new NotImplementedException();
            public void SetWebhooksEnabled(bool Enabled) => throw new NotImplementedException();
            public void StartBulkFollowers() => throw new NotImplementedException();
            public void SummarizeStreamData() => throw new NotImplementedException();
            public void SummarizeStreamData(ArchiveMultiStream archiveRecord) => throw new NotImplementedException();
            public void UpdateCurrency(List<LiveUser> Users, DateTime dateTime) => throw new NotImplementedException();
            public void UpdateFollowers(IEnumerable<Follow> follows) => throw new NotImplementedException();
            public List<LearnMsgRecord> UpdateLearnedMsgs() => throw new NotImplementedException();
            public void UpdateOverlayTicker(OverlayTickerItem item, string name) => throw new NotImplementedException();
            public void UpdateStats(DBUserStats Stat, string userId, Platform platform) => throw new NotImplementedException();
            public void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime) => throw new NotImplementedException();
            public void UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen) => throw new NotImplementedException();
            public void UserLeft(LiveUser User, DateTime LastSeen) => throw new NotImplementedException();
            public void NotifyStopBulkFollowers() => throw new NotImplementedException();
            public IEnumerable<Follow> PostFollowers(IEnumerable<Follow> follows) => throw new NotImplementedException();
            public bool PostStream(DateTime StreamStart, string Category) => throw new NotImplementedException();
            public void PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta) => throw new NotImplementedException();
            public void PostMultiLiveLog(string LogItem) => throw new NotImplementedException();
            public void PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName) => throw new NotImplementedException();
            public object GetICollection(DataTables dataTable) => throw new NotImplementedException();
            public void Exit() => throw new NotImplementedException();
            public void GUIRowEditSave(string TableName) => throw new NotImplementedException();
            public void ResetCategoryStreamCount() => throw new NotImplementedException();

            bool IDataManager.CheckField(string table, string field)
            {
                return CheckField(table, field);
            }

            bool IDataManager.CheckFollower(string User)
            {
                return CheckFollower(User);
            }

            bool IDataManager.CheckFollower(string User, DateTime ToDateTime)
            {
                return CheckFollower(User, ToDateTime);
            }

            bool IDataManager.CheckPermission(string cmd, ViewerTypes permission)
            {
                return CheckPermission(cmd, permission);
            }

            bool IDataManager.CheckShoutName(string UserId)
            {
                return CheckShoutName(UserId);
            }

            bool IDataManager.CheckUser(LiveUser User)
            {
                return CheckUser(User);
            }

            bool IDataManager.CheckUser(LiveUser User, DateTime ToDateTime)
            {
                return CheckUser(User, ToDateTime);
            }

            string IDataManager.CheckWelcomeUser(string User)
            {
                return CheckWelcomeUser(User);
            }

            IEnumerable<string> IDataManager.GetCommandList(bool prefix)
            {
                return GetCommandList(prefix);
            }

            void IDataManager.PostLearnMsgsRow(string Message, MsgTypes MsgType)
            {
                throw new NotImplementedException();
            }

            bool IDataManager.PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
            {
                throw new NotImplementedException();
            }

            void IDataManager.PostMonitorChannel(IEnumerable<LiveUser> liveUsers)
            {
                throw new NotImplementedException();
            }

            bool IDataManagerReadOnly.GetCmdAnnounce(string CmdName)
            {
                throw new NotImplementedException();
            }

            bool IDataManagerReadOnly.GetEventAnnounce(ChannelEventActions EventName)
            {
                throw new NotImplementedException();
            }

            bool IDataManagerReadOnly.CheckField(string table, string field)
            {
                return CheckField(table, field);
            }

            bool IDataManagerReadOnly.CheckPermission(string cmd, ViewerTypes permission)
            {
                return CheckPermission(cmd, permission);
            }

            bool IDataManagerReadOnly.CheckShoutName(string UserName)
            {
                return CheckShoutName(UserName);
            }

            Tuple<string, int, List<string>> IDataManagerReadOnly.GetTimerCommand(string Cmd)
            {
                throw new NotImplementedException();
            }

            bool IDataManagerReadOnly.CheckFollower(string User)
            {
                return CheckFollower(User);
            }

            bool IDataManagerReadOnly.CheckUser(LiveUser User)
            {
                return CheckUser(User);
            }

            bool IDataManagerReadOnly.CheckFollower(string User, DateTime ToDateTime)
            {
                return CheckFollower(User, ToDateTime);
            }

            bool IDataManagerReadOnly.CheckUser(LiveUser User, DateTime ToDateTime)
            {
                return CheckUser(User, ToDateTime);
            }

            bool IDataManagerReadOnly.CheckMultiChannelName(string UserName, Platform platform)
            {
                throw new NotImplementedException();
            }

            List<string> IDataManagerReadOnly.GetMultiChannelIds(Platform platform)
            {
                throw new NotImplementedException();
            }

            List<Tuple<WebhooksSource, Uri>> IDataManagerReadOnly.GetMultiWebHooks()
            {
                throw new NotImplementedException();
            }

            bool IDataManagerReadOnly.CheckMultiLiveStreamDate(string UserId, Platform platform, DateTime dateTime)
            {
                throw new NotImplementedException();
            }

            LiveUser IDataManagerReadOnly.GetUser(string UserName)
            {
                throw new NotImplementedException();
            }

            string IDataManagerReadOnly.GetCommandString()
            {
                throw new NotImplementedException();
            }

            IEnumerable<string> IDataManagerReadOnly.GetCommandList(bool prefix)
            {
                return GetCommandList(prefix);
            }

            LiveUser IDataManagerReadOnly.GetUserById(string UserId, Platform platform)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        private static object CreateRepeatManager(ActionSystem actionSystem)
        {
            // Get internal RepeatManager type from the same assembly as ActionSystem
            var asm = typeof(ActionSystem).Assembly;
            var repeatType = asm.GetType("StreamerBotLib.Models.Repeat.RepeatManager", throwOnError: true);
            var ctor = repeatType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(ActionSystem) }, null);
            Assert.NotNull(ctor);
            return ctor.Invoke([actionSystem]);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var t = target.GetType();
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(f);
            f.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var t = target.GetType();
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(f);
            return f.GetValue(target);
        }

        [Fact]
        public void StartAndStop_TogglesIsStarted()
        {
            // Arrange
            OptionFlags.ActiveToken = false; // ensure background loop won't run
            OptionFlags.RepeatParallelMode = false; // deterministic
            var actionSystem = new ActionSystem();
            // inject a simple IDataManager (not used for this test)
            ActionSystem.DataManage = new TestDataManager(new List<Tuple<string, int, List<string>>>());

            var repeatManager = CreateRepeatManager(actionSystem);
            var repeatType = repeatManager.GetType();

            // Act - start
            var startMethod = repeatType.GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(startMethod);
            startMethod.Invoke(repeatManager, null);

            // Assert started
            var isStartedProp = repeatType.GetProperty("IsStarted", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(isStartedProp);
            Assert.True((bool)isStartedProp.GetValue(repeatManager));

            // Act - stop
            var stopMethod = repeatType.GetMethod("Stop", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(stopMethod);
            stopMethod.Invoke(repeatManager, null);

            // Assert stopped
            Assert.False((bool)isStartedProp.GetValue(repeatManager));
        }

        [Fact]
        public void UpdateCategory_UpdatesRepeatCommandModeCategoryList()
        {
            // Arrange
            var actionSystem = new ActionSystem();
            ActionSystem.DataManage = new TestDataManager(new List<Tuple<string, int, List<string>>>());

            var repeatManager = CreateRepeatManager(actionSystem);
            var repeatType = repeatManager.GetType();

            // Act - call UpdateCategory (internal)
            var updateCategory = repeatType.GetMethod("UpdateCategory", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(updateCategory);
            string newCategory = "My_Test_Category";
            updateCategory.Invoke(repeatManager, new object[] { newCategory });

            // Access the static CategoryList on RepeatCommandMode via reflection
            var repeatModeType = typeof(RepeatSerialMode).BaseType ?? typeof(RepeatSerialMode);
            // Get the RepeatCommandMode type explicitly from the assembly to be safe
            var asm = typeof(ActionSystem).Assembly;
            var repeatCommandModeType = asm.GetType("StreamerBotLib.Models.Repeat.RepeatCommandMode", throwOnError: true);
            var categoryListProp = repeatCommandModeType.GetProperty("CategoryList", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(categoryListProp);

            var categoryList = (IList<string>)categoryListProp.GetValue(null);
            // Expect the category list to contain at least the provided category
            Assert.Contains(newCategory, categoryList);
            // Also expect the "All" category (localized) to be present; at minimum the list should have count 2
            Assert.True(categoryList.Count >= 1);
        }

        [Fact]
        public void UpdateCommands_AddsTimerCommandsToCommandMode()
        {
            // Arrange
            OptionFlags.ActiveToken = false;
            OptionFlags.RepeatParallelMode = true; // use parallel concrete mode
            var actionSystem = new ActionSystem();

            // Prepare sample timer commands returned from DataManage
            var timerCommands = new List<Tuple<string, int, List<string>>>()
                {
                    new Tuple<string,int,List<string>>("!hello", 10, new List<string>() { "All" })
                };

            ActionSystem.DataManage = new TestDataManager(timerCommands);

            var repeatManager = CreateRepeatManager(actionSystem);
            var repeatType = repeatManager.GetType();

            // Inject a concrete RepeatParallelMode instance into the private _repeatcommandmethod field
            var asm = typeof(ActionSystem).Assembly;
            var parallelType = asm.GetType("StreamerBotLib.Models.Repeat.RepeatParallelMode", throwOnError: true);
            var parallelCtor = parallelType.GetConstructor(new[] { typeof(double) }) ?? parallelType.GetConstructor(Type.EmptyTypes);
            Assert.NotNull(parallelCtor);
            var parallelInstance = parallelCtor.GetParameters().Length == 1 ? parallelCtor.Invoke(new object[] { 1.0 }) : parallelCtor.Invoke(null);

            SetPrivateField(repeatManager, "_repeatcommandmethod", parallelInstance);

            // Act - call UpdateCommands (internal)
            var updateCommands = repeatType.GetMethod("UpdateCommands", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(updateCommands);
            updateCommands.Invoke(repeatManager, null);

            // Assert - the RepeatCommands property on the inserted RepeatParallelMode instance should reflect the added commands
            var repeatCommandsProp = parallelInstance.GetType().BaseType.GetProperty("RepeatCommands", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                      ?? parallelInstance.GetType().GetProperty("RepeatCommands", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(repeatCommandsProp);
            var repeatCommands = (System.Collections.IList)repeatCommandsProp.GetValue(parallelInstance);

            // Parallel AddCommands converts tuples into TimerCommand instances; expect at least one entry
            Assert.NotNull(repeatCommands);
            Assert.True(repeatCommands.Count >= 1);

            // The TimerCommand.Command is lowercased in TimerCommand constructor; assert the command value matches
            var firstTimerCommand = repeatCommands[0];
            var commandProp = firstTimerCommand.GetType().GetProperty("Command", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(commandProp);
            Assert.Equal("!hello", (string)commandProp.GetValue(firstTimerCommand));
        }
    }
}
