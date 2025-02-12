using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;
using System.Data;

namespace StreamerBotLib.DataSQL
{
    /// <summary>
    /// A wrapper class to manage sequential DbContext access, which is not thread-safe. 
    /// https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
    /// </summary>
    public class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        private readonly DataManagerSQLAsync _dataManager;

        public string MultiLiveStatusLog { get; private set; }
        private readonly List<string> MultiLiveStatusList = [];
        private const int MaxList = 50;

        public ObservableCollection<ArchiveMultiStream> CleanupList { get; } = [];

        public event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;

        public event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;

        public event EventHandler UpdatedMonitoringChannels;

        /// <summary>
        /// Cache a list to maintain until user adjusts commands, they remain unchanged
        /// </summary>
        private List<Tuple<string, int, List<string>>> RepeatTimerList = [];

        public DataManagerSQL()
        {
            _dataManager = new DataManagerSQLAsync();

            _dataManager.OnDataCollectionUpdated += _dataManager_OnDataCollectionUpdated;
            _dataManager.OnBulkFollowersAddFinished += _dataManager_OnBulkFollowersAddFinished;
            _dataManager.UpdatedMonitoringChannels += _dataManager_UpdatedMonitoringChannels;
        }

        private void _dataManager_UpdatedMonitoringChannels(object sender, EventArgs e)
        {
            UpdatedMonitoringChannels?.Invoke(this, new());
        }

        private void _dataManager_OnBulkFollowersAddFinished(object sender, OnBulkFollowersAddFinishedEventArgs e)
        {
            OnBulkFollowersAddFinished?.Invoke(this, e);
        }

        private void _dataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            OnDataCollectionUpdated?.Invoke(this, e);
        }

        public bool CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckCurrency(User, value, CurrencyName).Result;
            }
        }

        public bool CheckField(string table, string field)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckField(table, field).Result;
            }
        }

        public bool CheckFollower(string User)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckFollower(User).Result;
            }
        }

        public bool CheckFollower(string User, DateTime ToDateTime)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckFollower(User, ToDateTime).Result;
            }
        }

        public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckModApprovalRule(modActionType, ModAction).Result;
            }
        }

        public bool CheckMultiChannelName(string UserName, Platform platform)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckMultiChannelName(UserName, platform).Result;
            }
        }

        public bool CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckMultiStreamDate(UserId, platform, dateTime).Result;
            }
        }

        public bool CheckMultiStreams(DateTime streamStart)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckMultiStreams(streamStart).Result;
            }
        }

        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckPermission(cmd, permission).Result;
            }
        }

        public bool CheckShoutName(string UserId)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckShoutName(UserId).Result;
            }
        }

        public bool CheckStreamTime(DateTime CurrTime)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckStreamTime(CurrTime).Result;
            }
        }

        public bool CheckUser(LiveUser User)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckUser(User).Result;
            }
        }

        public bool CheckUser(LiveUser User, DateTime ToDateTime)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckUser(User, ToDateTime).Result;
            }
        }

        public string CheckWelcomeUser(string User)
        {
            lock (_dataManager)
            {
                return _dataManager.CheckWelcomeUser(User).Result;
            }
        }

        public void ClearAllCurrencyValues()
        {
            lock (_dataManager)
            {
                _dataManager.ClearAllCurrencyValues();
            }
        }

        public void ClearUsersNotFollowers()
        {
            lock (_dataManager)
            {
                _dataManager.ClearUsersNotFollowers();
            }
        }

        public void ClearWatchTime()
        {
            lock (_dataManager)
            {
                _dataManager.ClearWatchTime();
            }
        }

        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            lock (_dataManager) { }
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {
            lock (_dataManager)
            {
                RepeatTimerList.Clear(); // update may contain change to repeat timers, hence, reset the timer listing

                return _dataManager.EditCommand(cmd, Arglist).Result;
            }
        }

        public Tuple<ModActions, Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            lock (_dataManager)
            {
                return _dataManager.FindRemedy(viewerTypes, msgTypes).Result;
            }
        }

        public ObservableCollection<ArchiveMultiStream> GetCleanupList()
        {
            lock (_dataManager)
            {
                return _dataManager.GetCleanupList();
            }
        }

        public CommandData GetCommand(string cmd)
        {
            lock (_dataManager)
            {
                return _dataManager.GetCommand(cmd).Result;
            }
        }

        public IEnumerable<string> GetCommandList(bool prefix)
        {
            lock (_dataManager)
            {
                return _dataManager.GetCommandList(prefix).Result;
            }
        }

        public string GetCommandString()
        {
            lock (_dataManager)
            {
                return _dataManager.GetCommandString().Result;
            }
        }

        public List<string> GetCurrencyNames()
        {
            lock (_dataManager)
            {
                return _dataManager.GetCurrencyNames().Result;
            }
        }

        public int GetDeathCounter(string currCategory)
        {
            lock (_dataManager)
            {
                return _dataManager.GetDeathCounter(currCategory).Result;
            }
        }

        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi)
        {
            lock (_dataManager)
            {
                Tuple<string, bool, short> data = _dataManager.GetEventRowData(rowcriteria).Result;
                Enabled = data.Item2;
                Multi = data.Item3;
                return data.Item1;
            }
        }

        public int GetFollowerCount()
        {
            lock (_dataManager)
            {
                return _dataManager.GetFollowerCount().Result;
            }
        }

        public List<CategoryData> GetGameCategories()
        {
            lock (_dataManager)
            {
                return _dataManager.GetGameCategories().Result;
            }
        }

        public string GetKey(string Table)
        {
            lock (_dataManager)
            {
                return _dataManager.GetKey(Table).Result;
            }
        }

        public IEnumerable<string> GetKeys(string Table)
        {
            lock (_dataManager)
            {
                return _dataManager.GetKeys(Table).Result;
            }
        }

        public List<string> GetMultiChannelIds(Platform platform)
        {
            lock (_dataManager)
            {
                return _dataManager.GetMultiChannelIds(platform).Result;
            }
        }

        public List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks()
        {
            lock (_dataManager)
            {
                return _dataManager.GetMultiWebHooks().Result;
            }
        }

        public string GetNewestFollower()
        {
            lock (_dataManager)
            {
                return _dataManager.GetNewestFollower().Result;
            }
        }

        public object GetObservableCollection(DataTables dataTable)
        {
            lock (_dataManager)
            {
                return _dataManager.GetObservableCollection(dataTable);
            }
        }

        public Dictionary<string, List<string>> GetOverlayActions()
        {
            lock (_dataManager)
            {
                return _dataManager.GetOverlayActions().Result;
            }
        }

        public List<OverlayActionType> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username)
        {
            lock (_dataManager)
            {
                return _dataManager.GetOverlayActions(overlayType, overlayAction, username).Result;
            }
        }

        public string GetQuote(int QuoteNum)
        {
            lock (_dataManager)
            {
                return _dataManager.GetQuote(QuoteNum).Result;
            }
        }

        public int GetQuoteCount()
        {
            lock (_dataManager)
            {
                return _dataManager.GetQuoteCount().Result;
            }
        }

        public List<string> GetSocialComs()
        {
            lock (_dataManager)
            {
                return _dataManager.GetSocialComs().Result;
            }
        }

        public string GetSocials()
        {
            lock (_dataManager)
            {
                return _dataManager.GetSocials().Result;
            }
        }
        public StreamStat GetStreamData(DateTime dateTime)
        {
            lock (_dataManager)
            {
                return _dataManager.GetStreamData(dateTime).Result;
            }
        }

        public List<string> GetTableFields(string TableName)
        {
            lock (_dataManager)
            {
                return _dataManager.GetTableFields(TableName).Result;
            }
        }

        public List<string> GetTableNames()
        {
            lock (_dataManager)
            {
                return _dataManager.GetTableNames().Result;
            }
        }

        public List<TickerItem> GetTickerItems()
        {
            lock (_dataManager)
            {
                return _dataManager.GetTickerItems().Result;
            }
        }

        public Tuple<string, int, List<string>> GetTimerCommand(string Cmd)
        {
            lock (_dataManager)
            {
                if (RepeatTimerList.Count == 0)
                {
                    RepeatTimerList = _dataManager.GetTimerCommands().Result;
                }

                return RepeatTimerList.Find((r) => r.Item1 == Cmd);
            }
        }

        public List<Tuple<string, int, List<string>>> GetTimerCommands()
        {
            lock (_dataManager)
            {
                if (RepeatTimerList.Count == 0)
                {
                    RepeatTimerList = _dataManager.GetTimerCommands().Result;
                }

                return RepeatTimerList;
            }
        }

        public int GetTimerCommandTime(string Cmd)
        {
            lock (_dataManager)
            {
                if (RepeatTimerList.Count == 0)
                {
                    RepeatTimerList = _dataManager.GetTimerCommands().Result;
                }

                return RepeatTimerList.Find((r) => r.Item1 == Cmd).Item2;
            }
        }

        public string GetUsage(string command)
        {
            lock (_dataManager)
            {
                return _dataManager.GetUsage(command).Result;
            }
        }

        public LiveUser GetUser(string UserName)
        {
            lock (_dataManager)
            {
                return _dataManager.GetUser(UserName).Result;
            }
        }

        public string GetUserId(LiveUser User)
        {
            lock (_dataManager)
            {
                return _dataManager.GetUserId(User).Result;
            }
        }

        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks)
        {
            lock (_dataManager)
            {
                return _dataManager.GetWebhooks(webhooksSource, webhooks).Result;
            }
        }

        public void GUIRowEditSave()
        {
            lock (_dataManager)
            {
                _dataManager.GUIRowEditSave();
            }
        }

        public void Initialize()
        {
            lock (_dataManager)
            {
                Task.Run(async () =>
                {
                    await _dataManager.Initialize();
                });
            }
        }

        public void NotifyStopBulkFollowers()
        {
            lock (_dataManager)
            {
                _dataManager.NotifyStopBulkFollowers();
            }
        }

        public object[] PerformQuery(CommandsBase row, int Top)
        {
            lock (_dataManager)
            {
                return _dataManager.PerformQuery(row, Top).Result;
            }
        }

        public object PerformQuery(CommandsBase row, string ParamValue)
        {
            lock (_dataManager)
            {
                return _dataManager.PerformQuery(row, ParamValue).Result;
            }
        }

        public bool PostCategory(CategoryData categoryData)
        {
            lock (_dataManager)
            {
                return _dataManager.PostCategory(categoryData).Result;
            }
        }

        public void PostCategoryStream(CategoryData category, int StreamCount)
        {
            lock (_dataManager)
            {
                _dataManager.PostCategoryStream(category, StreamCount);
            }
        }

        public bool PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName)
        {
            lock (_dataManager)
            {
                return _dataManager.PostClip(ClipId, CreatedAt, Duration, GameId, Language, Title, Url, fromUserId, fromUserName).Result;
            }
        }

        public string PostCommand(string cmd, CommandParams Params)
        {
            lock (_dataManager)
            {
                RepeatTimerList.Clear(); // new command may have a repeat timer, clear to reset the timer list

                return _dataManager.PostCommand(cmd, Params).Result;
            }
        }

        public void PostCurrencyType(Models.CurrencyType currencyType)
        {
            lock (_dataManager)
            {
                _dataManager.PostCurrencyType(currencyType);
            }
        }

        public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
            lock (_dataManager)
            {
                _dataManager.PostCurrencyUpdate(User, value, CurrencyName);
            }
        }

        public void PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName)
        {
            lock (_dataManager)
            {
                _dataManager.PostCurrencyUpdate(Updates, CurrencyName);
            }
        }

        public void PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta)
        {
            lock (_dataManager)
            {
                _dataManager.PostDataGridGUIAddRow(tableMeta);

                if (tableMeta.TableName is "Commands" or "CommandsUser")
                { // some update to Commands or CommandsUser, reset the repeat timer command list - in case the user changed the timer value
                    RepeatTimerList.Clear();
                }
            }
        }

        public int PostDeathCounterUpdate(string currCategory, bool Reset, int updateValue)
        {
            lock (_dataManager)
            {
                return _dataManager.PostDeathCounterUpdate(currCategory, Reset, updateValue).Result;
            }
        }

        public bool PostFollower(Follow follow)
        {
            lock (_dataManager)
            {
                return _dataManager.PostFollower(follow).Result;
            }
        }

        public IEnumerable<Follow> PostFollowers(IEnumerable<Follow> follows)
        {
            lock (_dataManager)
            {
                return _dataManager.PostFollowers(follows).Result;
            }
        }

        public void PostGiveawayData(string UserId, DateTime dateTime)
        {
            lock (_dataManager)
            {
                _dataManager.PostGiveawayData(UserId, dateTime);
            }
        }

        public void PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename)
        {
            lock (_dataManager)
            {
                _dataManager.PostInRaidData(user, time, viewers, gamename);
            }
        }

        public void PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            lock (_dataManager)
            {
                _dataManager.PostLearnMsgsRow(Message, MsgType);
            }
        }

        public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
        {
            lock (_dataManager)
            {
                return _dataManager.PostMergeUserStats(CurrUser, SourceUser, platform).Result;
            }
        }

        public void PostMonitorChannel(IEnumerable<LiveUser> liveUsers)
        {
            lock (_dataManager)
            {
                _dataManager.PostMonitorChannel(liveUsers);
            }
        }

        public void PostMultiLiveLog(string LogItem)
        {
            lock (_dataManager)
            {
                MultiLiveStatusList.Insert(0, LogItem);

                if (MultiLiveStatusList.Count > MaxList)
                {
                    MultiLiveStatusList.RemoveRange(MaxList - 1, MultiLiveStatusList.Count - MaxList);
                }
                MultiLiveStatusLog = string.Join("\r\n", MultiLiveStatusList);
                _dataManager.NotifyDataCollectionUpdated(nameof(MultiLiveStatusLog));
            }
        }

        public bool PostMultiStreamDate(LiveUser liveUser, DateTime onDate)
        {
            lock (_dataManager)
            {
                return _dataManager.PostMultiStreamDate(liveUser, onDate).Result;
            }
        }

        public void PostNewAutoShoutUser(string UserId, Platform platform)
        {
            lock (_dataManager)
            {
                _dataManager.PostNewAutoShoutUser(UserId, platform);
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            lock (_dataManager)
            {
                _dataManager.PostOutgoingRaid(HostedChannel, dateTime);
            }
        }

        public int PostQuote(string Text)
        {
            lock (_dataManager)
            {
                return _dataManager.PostQuote(Text).Result;
            }
        }

        public bool PostStream(DateTime StreamStart, string Category)
        {
            lock (_dataManager)
            {
                return _dataManager.PostStream(StreamStart, Category).Result;
            }
        }

        public void PostStreamStat(StreamStat streamStat)
        {
            lock (_dataManager)
            {
                LogWriter.DebugLog("PostStreamStat", DebugLogTypes.DataManager, $"Posting stream stats for stream started {streamStat.StreamStart}.");

                _dataManager.PostStreamStat(streamStat);
            }
        }

        public void PostUserCustomWelcome(LiveUser User, string WelcomeMsg)
        {
            lock (_dataManager)
            {
                _dataManager.PostUserCustomWelcome(User, WelcomeMsg);
            }
        }

        public void RemoveAllFollowers()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllFollowers();
            }
        }

        public void RemoveAllGiveawayData()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllGiveawayData();
            }
        }

        public void RemoveAllInRaidData()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllInRaidData();
            }
        }

        public void RemoveAllOutRaidData()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllOutRaidData();
            }
        }

        public void RemoveAllOverlayTickerData()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllOverlayTickerData();
            }
        }

        public void RemoveAllStreamStats()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllStreamStats();
            }
        }

        public void RemoveAllUsers()
        {
            lock (_dataManager)
            {
                _dataManager.RemoveAllUsers();
            }
        }

        public bool RemoveCommand(string command)
        {
            lock (_dataManager)
            {
                return _dataManager.RemoveCommand(command).Result;
            }
        }

        public bool RemoveQuote(int QuoteNum)
        {
            lock (_dataManager)
            {
                return _dataManager.RemoveQuote(QuoteNum).Result;
            }
        }

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            lock (_dataManager)
            {
                _dataManager.SetBuiltInCommandsEnabled(Enabled);
            }
        }

        [Obsolete("No longer compatible after upgrade to Entity Framework Core")]
        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled)
        {
            lock (_dataManager) { }
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            lock (_dataManager)
            {
                _dataManager.SetSystemEventsEnabled(Enabled);
            }
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            lock (_dataManager)
            {
                RepeatTimerList.Clear(); // clear the list for enabled change - the timers don't respond for disabled commands

                _dataManager.SetUserDefinedCommandsEnabled(Enabled);
            }
        }

        public void SetWebhooksEnabled(bool Enabled)
        {
            lock (_dataManager)
            {
                _dataManager.SetWebhooksEnabled(Enabled);
            }
        }

        public void StartBulkFollowers()
        {
            lock (_dataManager)
            {
                _dataManager.StartBulkFollowers();
            }
        }

        public void SummarizeStreamData()
        {
            lock (_dataManager)
            {
                _dataManager.SummarizeStreamData();
            }
        }

        public void SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            lock (_dataManager)
            {
                _dataManager.SummarizeStreamData(archiveRecord);
            }
        }

        public IEnumerable<LiveUser> TestGetRandomUsers(int count)
        {
            lock (_dataManager)
            {
                return _dataManager.TestGetRandomUsers(count).Result;
            }
        }

        public bool TestInRaidData(string user, DateTime time, int viewers, string gamename)
        {
            lock (_dataManager)
            {
                return _dataManager.TestInRaidData(user, time, viewers, gamename).Result;
            }
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            lock (_dataManager)
            {
                return _dataManager.TestOutRaidData(HostedChannel, dateTime).Result;
            }
        }

        public void UpdateCurrency(List<string> Users, DateTime dateTime)
        {
            lock (_dataManager)
            {
                _dataManager.UpdateCurrency(Users, dateTime);
            }
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
            lock (_dataManager)
            {
                _dataManager.UpdateFollowers(follows);
            }
        }

        public List<LearnMsgRecord> UpdateLearnedMsgs()
        {
            lock (_dataManager)
            {
                return _dataManager.UpdateLearnedMsgs().Result;
            }
        }

        public void UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            lock (_dataManager)
            {
                _dataManager.UpdateOverlayTicker(item, name);
            }
        }

        public void UpdateStats(DBUserStats Stat, string userId, Platform platform)
        {
            lock (_dataManager)
            {
                _dataManager.UpdateStats(Stat, userId, platform);
            }
        }

        public void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime)
        {
            lock (_dataManager)
            {
                _dataManager.UpdateWatchTime(Users, CurrTime);
            }
        }

        public void UpdateWatchTime(LiveUser User, DateTime CurrTime)
        {
            lock (_dataManager)
            {
                UpdateWatchTime([User], CurrTime);
            }
        }

        public void UserJoined(LiveUser User, DateTime NowSeen)
        {
            lock (_dataManager)
            {
                _dataManager.UserJoined(User, NowSeen);
            }
        }

        public void UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen)
        {
            lock (_dataManager)
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                    $"Updating {Users.Count()} users now joined to the channel.");
                _dataManager.UserJoined(Users, NowSeen);
            }
        }

        public void UserLeft(LiveUser User, DateTime LastSeen)
        {
            lock (_dataManager)
            {
                _dataManager.UserLeft(User, LastSeen);
            }
        }

        public void Exit()
        {
            lock (_dataManager)
            {
                _dataManager.Exit();
            }
        }
    }
}
