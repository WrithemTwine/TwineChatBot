using StreamerBotLib.DataSQL.EFC9;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.GUI;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;
using StreamerBotLib.Systems.Overlay.Enums;
using StreamerBotLib.Systems.Overlay.Models;

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

        private List<string> MultiLiveStatusLog;
        private const int MaxList = 50;

        public List<ArchiveMultiStream> CleanupList { get; } = [];

        public event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;
        public event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;
        public event EventHandler UpdatedMonitoringChannels;
        public event EventHandler<EventArgs> OnLoadCompleted;

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
            _dataManager.OnLoadCompleted += _dataManager_OnLoadCompleted;
        }

        public async Task InitializeDataManager()
        {
            await _dataManager.InitializeDataBaseAsync();
        }

        private void _dataManager_OnLoadCompleted(object sender, EventArgs e)
        {
            OnLoadCompleted?.Invoke(sender, e);
        }

        private void _dataManager_UpdatedMonitoringChannels(object sender, EventArgs e)
        {
            LogWriter.DebugLog("_dataManager_UpdatedMonitoringChannels", DebugLogTypes.DataManager, "UpdatedMonitoringChannels event triggered.");

            UpdatedMonitoringChannels?.Invoke(this, e);
        }

        private void _dataManager_OnBulkFollowersAddFinished(object sender, OnBulkFollowersAddFinishedEventArgs e)
        {
            LogWriter.DebugLog("_dataManager_OnBulkFollowersAddFinished", DebugLogTypes.DataManager, "OnBulkFollowersAddFinished event triggered.");
            OnBulkFollowersAddFinished?.Invoke(this, e);
        }

        private void _dataManager_OnDataCollectionUpdated(object sender, OnDataCollectionUpdatedEventArgs e)
        {
            LogWriter.DebugLog("_dataManager_OnDataCollectionUpdated", DebugLogTypes.DataManager, "OnDataCollectionUpdated event triggered.");
            OnDataCollectionUpdated?.Invoke(this, e);
        }

        public bool CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            LogWriter.DebugLog("CheckCurrency", DebugLogTypes.DataManager, "Checking currency.");

            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckCurrency(User, value, CurrencyName).Result;
            }
        }

        public bool CheckField(string table, string field)
        {
            LogWriter.DebugLog("CheckField", DebugLogTypes.DataManager, "Checking field.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckField(table, field).Result;
            }
        }

        public bool CheckFollower(string User)
        {
            LogWriter.DebugLog("CheckFollower", DebugLogTypes.DataManager, "Checking follower.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckFollower(User).Result;
            }
        }

        public bool CheckFollower(string User, DateTime ToDateTime)
        {
            LogWriter.DebugLog("CheckFollower", DebugLogTypes.DataManager, "Checking follower.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckFollower(User, ToDateTime).Result;
            }
        }

        public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            LogWriter.DebugLog("CheckModApprovalRule", DebugLogTypes.DataManager, "Checking mod approval rule.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckModApprovalRule(modActionType, ModAction).Result;
            }
        }

        public bool CheckMultiChannelName(string UserName, Platform platform)
        {
            LogWriter.DebugLog("CheckMultiChannelName", DebugLogTypes.DataManager, "Checking multi-channel name.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckMultiChannelName(UserName, platform).Result;
            }
        }

        public bool CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime)
        {
            LogWriter.DebugLog("CheckMultiStreamDate", DebugLogTypes.DataManager, "Checking multi-stream date.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckMultiStreamDate(UserId, platform, dateTime).Result;
            }
        }

        public bool CheckMultiStreams(DateTime streamStart)
        {
            LogWriter.DebugLog("CheckMultiStreams", DebugLogTypes.DataManager, "Checking multi-streams.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckMultiStreams(streamStart).Result;
            }
        }

        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            LogWriter.DebugLog("CheckPermission", DebugLogTypes.DataManager, "Checking permission.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckPermission(cmd, permission).Result;
            }
        }

        public bool CheckShoutName(string UserId)
        {
            LogWriter.DebugLog("CheckShoutName", DebugLogTypes.DataManager, "Checking shout name.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckShoutName(UserId).Result;
            }
        }

        public bool CheckStreamTime(DateTime CurrTime)
        {
            LogWriter.DebugLog("CheckStreamTime", DebugLogTypes.DataManager, "Checking stream time.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckStreamTime(CurrTime).Result;
            }
        }

        public bool CheckUser(LiveUser User)
        {
            LogWriter.DebugLog("CheckUser", DebugLogTypes.DataManager, "Checking user.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckUser(User).Result;
            }
        }

        public bool CheckUser(LiveUser User, DateTime ToDateTime)
        {
            LogWriter.DebugLog("CheckUser", DebugLogTypes.DataManager, "Checking user.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckUser(User, ToDateTime).Result;
            }
        }

        public string CheckWelcomeUser(string User)
        {
            LogWriter.DebugLog("CheckWelcomeUser", DebugLogTypes.DataManager, "Checking welcome user.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.CheckWelcomeUser(User).Result;
            }
        }

        public void ClearAllCurrencyValues()
        {
            LogWriter.DebugLog("ClearAllCurrencyValues", DebugLogTypes.DataManager, "Clearing all currency values.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.ClearAllCurrencyValues();
                });
            }
        }

        public void ClearUsersNotFollowers()
        {
            LogWriter.DebugLog("ClearUsersNotFollowers", DebugLogTypes.DataManager, "Clearing users not followers.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.ClearUsersNotFollowers();
                });
            }
        }

        public void ClearWatchTime()
        {
            LogWriter.DebugLog("ClearWatchTime", DebugLogTypes.DataManager, "Clearing watch time.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.ClearWatchTime();
                });
            }
        }

        public void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            LogWriter.DebugLog("DeleteDataRows", DebugLogTypes.DataManager, "Deleting data rows - not available.");
            lock (GUIDataManagerLock.Lock) { }
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {
            LogWriter.DebugLog("EditCommand", DebugLogTypes.DataManager, "Editing command.");
            lock (GUIDataManagerLock.Lock)
            {
                RepeatTimerList.Clear(); // update may contain change to repeat timers, hence, reset the timer listing

                return _dataManager.EditCommand(cmd, Arglist).Result;
            }
        }

        public Tuple<ModActions, StreamerBotLib.Models.Enums.BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            LogWriter.DebugLog("FindRemedy", DebugLogTypes.DataManager, "Finding remedy.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.FindRemedy(viewerTypes, msgTypes).Result;
            }
        }

        public void SetCleanupList(ref List<ArchiveMultiStream> archiveMultiStreams)
        {
            LogWriter.DebugLog("GetCleanupList", DebugLogTypes.DataManager, "Getting cleanup list.");
            lock (GUIDataManagerLock.Lock)
            {
                _dataManager.SetCleanupList(ref archiveMultiStreams);
            }
        }

        public void SetMultiLiveStatusLog(ref List<string> log)
        {
            LogWriter.DebugLog("SetMultiLiveStatusLog", DebugLogTypes.DataManager, "Setting multi-live status log.");
            lock (GUIDataManagerLock.Lock)
            {
                MultiLiveStatusLog = log;
            }
        }

        public bool GetCmdAnnounce(string CmdName)
        {
            LogWriter.DebugLog("GetCmdAnnounce", DebugLogTypes.DataManager, $"Getting announcement check for command {CmdName}.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetCmdAnnounce(CmdName).Result;
            }
        }

        public bool GetEventAnnounce(ChannelEventActions EventName)
        {
            LogWriter.DebugLog("GetEventAnnounce", DebugLogTypes.DataManager, $"Getting announcement check for event {EventName}.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetEventAnnounce(EventName).Result;
            }
        }

        public object GetICollection(DataTables dataTable)
        {
            LogWriter.DebugLog("GetObservableCollection", DebugLogTypes.DataManager, "Getting observable collection.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetICollection(dataTable);
            }
        }

        public CommandData GetCommand(string cmd)
        {
            LogWriter.DebugLog("GetCommand", DebugLogTypes.DataManager, "Getting command.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetCommand(cmd).Result;
            }
        }

        public IEnumerable<string> GetCommandList(bool prefix)
        {
            LogWriter.DebugLog("GetCommandList", DebugLogTypes.DataManager, "Getting command list.");
            lock (GUIDataManagerLock.Lock)
            {
                var Commands = _dataManager.GetCommandList(prefix).Result;

                return Commands.Count == 0 ? [LocalizedMsgSystem.GetVar("MsgNoCommands")] : Commands;
            }
        }

        public string GetCommandString()
        {
            LogWriter.DebugLog("GetCommandString", DebugLogTypes.DataManager, "Getting command string.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetCommandString().Result;
            }
        }

        public List<string> GetCurrencyNames()
        {
            LogWriter.DebugLog("GetCurrencyNames", DebugLogTypes.DataManager, "Getting currency names.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetCurrencyNames().Result;
            }
        }

        public int GetDeathCounter(string currCategory)
        {
            LogWriter.DebugLog("GetDeathCounter", DebugLogTypes.DataManager, "Getting death counter.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetDeathCounter(currCategory).Result;
            }
        }

        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi)
        {
            LogWriter.DebugLog("GetEventRowData", DebugLogTypes.DataManager, "Getting event row data.");
            lock (GUIDataManagerLock.Lock)
            {
                Tuple<string, bool, short> data = _dataManager.GetEventRowData(rowcriteria).Result;
                Enabled = data?.Item2 ?? false;
                Multi = data?.Item3 ?? 0;
                return data?.Item1;
            }
        }

        public int GetFollowerCount()
        {
            LogWriter.DebugLog("GetFollowerCount", DebugLogTypes.DataManager, "Getting follower count.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetFollowerCount().Result;
            }
        }

        public List<CategoryData> GetGameCategories()
        {
            LogWriter.DebugLog("GetGameCategories", DebugLogTypes.DataManager, "Getting game categories.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetGameCategories().Result;
            }
        }

        public string GetKey(string Table)
        {
            LogWriter.DebugLog("GetKey", DebugLogTypes.DataManager, "Getting key.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetKey(Table).Result;
            }
        }

        public IEnumerable<string> GetKeys(string Table)
        {
            LogWriter.DebugLog("GetKeys", DebugLogTypes.DataManager, "Getting keys.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetKeys(Table).Result;
            }
        }

        public List<string> GetMultiChannelIds(Platform platform)
        {
            LogWriter.DebugLog("GetMultiChannelIds", DebugLogTypes.DataManager, "Getting multi-channel IDs.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetMultiChannelIds(platform).Result;
            }
        }

        public List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks()
        {
            LogWriter.DebugLog("GetMultiWebHooks", DebugLogTypes.DataManager, "Getting multi web hooks.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetMultiWebHooks().Result;
            }
        }

        public string GetNewestFollower()
        {
            LogWriter.DebugLog("GetNewestFollower", DebugLogTypes.DataManager, "Getting newest follower.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetNewestFollower().Result;
            }
        }
        public Dictionary<string, List<string>> GetOverlayActions()
        {
            LogWriter.DebugLog("GetOverlayActions", DebugLogTypes.DataManager, "Getting overlay actions.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetOverlayActions().Result;
            }
        }

        public List<OverlayActionType> GetOverlayActions(OverlayTypes overlayType, string overlayAction, string username)
        {
            LogWriter.DebugLog("GetOverlayActions", DebugLogTypes.DataManager, "Getting overlay actions.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetOverlayActions(overlayType, overlayAction, username).Result;
            }
        }

        public string GetQuote(int QuoteNum)
        {
            LogWriter.DebugLog("GetQuote", DebugLogTypes.DataManager, "Getting quote.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetQuote(QuoteNum).Result;
            }
        }

        public int GetQuoteCount()
        {
            LogWriter.DebugLog("GetQuoteCount", DebugLogTypes.DataManager, "Getting quote count.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetQuoteCount().Result;
            }
        }

        public List<string> GetSocialComs()
        {
            LogWriter.DebugLog("GetSocialComs", DebugLogTypes.DataManager, "Getting social commands.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetSocialComs().Result;
            }
        }

        public string GetSocials()
        {
            LogWriter.DebugLog("GetSocials", DebugLogTypes.DataManager, "Getting socials.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetSocials().Result;
            }
        }
        public StreamStat GetStreamData(DateTime dateTime)
        {
            LogWriter.DebugLog("GetStreamData", DebugLogTypes.DataManager, "Getting stream data.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetStreamData(dateTime).Result;
            }
        }

        public List<string> GetTableFields(string TableName)
        {
            LogWriter.DebugLog("GetTableFields", DebugLogTypes.DataManager, "Getting table fields.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetTableFields(TableName).Result;
            }
        }

        public List<string> GetTableNames()
        {
            LogWriter.DebugLog("GetTableNames", DebugLogTypes.DataManager, "Getting table names.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetTableNames().Result;
            }
        }

        public List<TickerItem> GetTickerItems()
        {
            LogWriter.DebugLog("GetTickerItems", DebugLogTypes.DataManager, "Getting ticker items.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetTickerItems().Result;
            }
        }

        public Tuple<string, int, List<string>> GetTimerCommand(string Cmd)
        {
            LogWriter.DebugLog("GetTimerCommand", DebugLogTypes.DataManager, "Getting timer command.");
            lock (GUIDataManagerLock.Lock)
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
            LogWriter.DebugLog("GetTimerCommands", DebugLogTypes.DataManager, "Getting timer commands.");
            lock (GUIDataManagerLock.Lock)
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
            LogWriter.DebugLog("GetTimerCommandTime", DebugLogTypes.DataManager, "Getting timer command time.");
            lock (GUIDataManagerLock.Lock)
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
            LogWriter.DebugLog("GetUsage", DebugLogTypes.DataManager, "Getting usage.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetUsage(command).Result;
            }
        }

        public LiveUser GetUser(string UserName)
        {
            LogWriter.DebugLog("GetUser", DebugLogTypes.DataManager, "Getting user.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetUser(UserName).Result;
            }
        }

        public string GetUserId(LiveUser User)
        {
            LogWriter.DebugLog("GetUserId", DebugLogTypes.DataManager, "Getting user ID.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetUserId(User).Result;
            }
        }

        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksSource webhooksSource, WebhooksKind webhooks)
        {
            LogWriter.DebugLog("GetWebhooks", DebugLogTypes.DataManager, "Getting webhooks.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.GetWebhooks(webhooksSource, webhooks).Result;
            }
        }

        public void GUIRowEditSave()
        {
            LogWriter.DebugLog("GUIRowEditSave", DebugLogTypes.DataManager, "GUI row edit save.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.GUIRowEditSave();
                });
            }
        }

        public void Initialize()
        {
            LogWriter.DebugLog("Initialize", DebugLogTypes.DataManager, "Initializing.");
            lock (GUIDataManagerLock.Lock)
            {
                Task.Run(async () =>
                {
                    await _dataManager.Initialize();
                });
            }
        }

        public void NotifyStopBulkFollowers()
        {
            LogWriter.DebugLog("NotifyStopBulkFollowers", DebugLogTypes.DataManager, "Notifying stop bulk followers.");
            lock (GUIDataManagerLock.Lock)
            {
                _dataManager.NotifyStopBulkFollowers();
            }
        }

        public object[] PerformQuery(CommandsBase row, int Top)
        {
            LogWriter.DebugLog("PerformQuery", DebugLogTypes.DataManager, "Performing query.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PerformQuery(row, Top).Result;
            }
        }

        public object PerformQuery(CommandsBase row, string ParamValue)
        {
            LogWriter.DebugLog("PerformQuery", DebugLogTypes.DataManager, "Performing query.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PerformQuery(row, ParamValue).Result;
            }
        }

        public bool PostCategory(CategoryData categoryData)
        {
            LogWriter.DebugLog("PostCategory", DebugLogTypes.DataManager, "Posting category.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostCategory(categoryData).Result;
            }
        }

        public void PostCategoryStream(CategoryData category, int StreamCount)
        {
            LogWriter.DebugLog("PostCategoryStream", DebugLogTypes.DataManager, "Posting category stream.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostCategoryStream(category, StreamCount);
                });
            }
        }

        public bool PostClip(string ClipId, DateTime CreatedAt, decimal Duration, string GameId, string Language, string Title, string Url, string fromUserId, string fromUserName)
        {
            LogWriter.DebugLog("PostClip", DebugLogTypes.DataManager, "Posting clip.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostClip(ClipId, CreatedAt, Duration, GameId, Language, Title, Url, fromUserId, fromUserName).Result;
            }
        }

        public string PostCommand(string cmd, CommandParams Params)
        {
            LogWriter.DebugLog("PostCommand", DebugLogTypes.DataManager, "Posting command.");
            lock (GUIDataManagerLock.Lock)
            {
                RepeatTimerList.Clear(); // new command may have a repeat timer, clear to reset the timer list

                return _dataManager.PostCommand(cmd, Params).Result;
            }
        }

        public void PostCurrencyType(Models.CurrencyType currencyType)
        {
            LogWriter.DebugLog("PostCurrencyType", DebugLogTypes.DataManager, "Posting currency type.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostCurrencyType(currencyType);
                });
            }
        }

        public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
            LogWriter.DebugLog("PostCurrencyUpdate", DebugLogTypes.DataManager, "Posting currency update.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostCurrencyUpdate(User, value, CurrencyName);
                });
            }
        }

        public void PostCurrencyUpdate(List<PlayGameUserWager<PlayingCardFrench, PlayingCardSuit>> Updates, string CurrencyName)
        {
            LogWriter.DebugLog("PostCurrencyUpdate", DebugLogTypes.DataManager, "Posting currency update.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostCurrencyUpdate(Updates, CurrencyName);
                });
            }
        }

        public void PostDataGridGUIAddRow(IDatabaseTableMeta tableMeta)
        {
            LogWriter.DebugLog("PostDataGridGUIAddRow", DebugLogTypes.DataManager, "Posting data grid GUI add row.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostDataGridGUIAddRow(tableMeta);

                    if (tableMeta.TableName is "Commands" or "CommandsUser")
                    { // some update to Commands or CommandsUser, reset the repeat timer command list - in case the user changed the timer value
                        RepeatTimerList.Clear();
                    }
                });
            }
        }

        public int PostDeathCounterUpdate(string currCategory, bool Reset, int updateValue)
        {
            LogWriter.DebugLog("PostDeathCounterUpdate", DebugLogTypes.DataManager, "Posting death counter update.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostDeathCounterUpdate(currCategory, Reset, updateValue).Result;
            }
        }

        public bool PostFollower(Follow follow)
        {
            LogWriter.DebugLog("PostFollower", DebugLogTypes.DataManager, "Posting follower.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostFollower(follow).Result;
            }
        }

        public IEnumerable<Follow> PostFollowers(IEnumerable<Follow> follows)
        {
            LogWriter.DebugLog("PostFollowers", DebugLogTypes.DataManager, "Posting followers.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostFollowers(follows).Result;
            }
        }

        public void PostGiveawayData(string UserId, DateTime dateTime)
        {
            LogWriter.DebugLog("PostGiveawayData", DebugLogTypes.DataManager, "Posting giveaway data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostGiveawayData(UserId, dateTime);
                });
            }
        }

        public void PostInRaidData(LiveUser user, DateTime time, int viewers, CategoryData gamename)
        {
            LogWriter.DebugLog("PostInRaidData", DebugLogTypes.DataManager, "Posting in-raid data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostInRaidData(user, time, viewers, gamename);
                });
            }
        }

        public void PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            LogWriter.DebugLog("PostLearnMsgsRow", DebugLogTypes.DataManager, "Posting learn messages row.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostLearnMsgsRow(Message, MsgType);
                });
            }
        }

        public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
        {
            LogWriter.DebugLog("PostMergeUserStats", DebugLogTypes.DataManager, "Posting merge user stats.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostMergeUserStats(CurrUser, SourceUser, platform).Result;
            }
        }

        public void PostMonitorChannel(IEnumerable<LiveUser> liveUsers)
        {
            LogWriter.DebugLog("PostMonitorChannel", DebugLogTypes.DataManager, "Posting monitor channel.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostMonitorChannel(liveUsers);
                });
            }
        }

        public void PostMultiLiveLog(string LogItem)
        {
            LogWriter.DebugLog("PostMultiLiveLog", DebugLogTypes.DataManager, "Posting multi-live log.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(() =>
                {
                    MultiLiveStatusLog.Insert(0, LogItem);

                    if (MultiLiveStatusLog.Count > MaxList)
                    { // limit the list to MaxList items
                        LogWriter.DebugLog("PostMultiLiveLog", DebugLogTypes.DataManager, $"Trimming MultiLiveStatusLog to {MaxList} items.");
                        MultiLiveStatusLog.RemoveRange(MaxList - 1, MultiLiveStatusLog.Count - MaxList);
                    }

                    _dataManager.NotifyDataCollectionUpdated(nameof(MultiLiveStatusLog));
                });
            }
        }

        public bool PostMultiStreamDate(LiveUser liveUser, DateTime onDate)
        {
            LogWriter.DebugLog("PostMultiStreamDate", DebugLogTypes.DataManager, "Posting multi-stream date.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostMultiStreamDate(liveUser, onDate).Result;
            }
        }

        public void PostNewAutoShoutUser(string UserId, Platform platform)
        {
            LogWriter.DebugLog("PostNewAutoShoutUser", DebugLogTypes.DataManager, "Posting new auto-shout user.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostNewAutoShoutUser(UserId, platform);
                });
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            LogWriter.DebugLog("PostOutgoingRaid", DebugLogTypes.DataManager, "Posting outgoing raid.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostOutgoingRaid(HostedChannel, dateTime);
                });
            }
        }

        public int PostQuote(string Text)
        {
            LogWriter.DebugLog("PostQuote", DebugLogTypes.DataManager, "Posting quote.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.PostQuote(Text).Result;
            }
        }

        public bool PostStream(DateTime StreamStart, string Category)
        {
            LogWriter.DebugLog("PostStream", DebugLogTypes.DataManager, "Posting stream.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostStream(StreamStart, Category);
                });
                return true;
            }
        }

        public void PostStreamStat(StreamStat streamStat)
        {
            LogWriter.DebugLog("PostStreamStat", DebugLogTypes.DataManager, "Posting stream stat.");
            lock (GUIDataManagerLock.Lock)
            {
                LogWriter.DebugLog("PostStreamStat", DebugLogTypes.DataManager, $"Posting stream stats for stream started {streamStat.StreamStart}.");

                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostStreamStat(streamStat);
                });
            }
        }

        public void PostUserCustomWelcome(LiveUser User, string WelcomeMsg)
        {
            LogWriter.DebugLog("PostUserCustomWelcome", DebugLogTypes.DataManager, "Posting user custom welcome.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.PostUserCustomWelcome(User, WelcomeMsg);
                });
            }
        }

        public void RemoveAllFollowers()
        {
            LogWriter.DebugLog("RemoveAllFollowers", DebugLogTypes.DataManager, "Removing all followers.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllFollowers();
                });
            }
        }

        public void RemoveAllGiveawayData()
        {
            LogWriter.DebugLog("RemoveAllGiveawayData", DebugLogTypes.DataManager, "Removing all giveaway data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllGiveawayData();
                });
            }
        }

        public void RemoveAllInRaidData()
        {
            LogWriter.DebugLog("RemoveAllInRaidData", DebugLogTypes.DataManager, "Removing all in-raid data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllInRaidData();
                });
            }
        }

        public void RemoveAllOutRaidData()
        {
            LogWriter.DebugLog("RemoveAllOutRaidData", DebugLogTypes.DataManager, "Removing all out-raid data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllOutRaidData();
                });
            }
        }

        public void RemoveAllOverlayTickerData()
        {
            LogWriter.DebugLog("RemoveAllOverlayTickerData", DebugLogTypes.DataManager, "Removing all overlay ticker data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllOverlayTickerData();
                });
            }
        }

        public void RemoveAllStreamStats()
        {
            LogWriter.DebugLog("RemoveAllStreamStats", DebugLogTypes.DataManager, "Removing all stream stats.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllStreamStats();
                });
            }
        }

        public void RemoveAllUsers()
        {
            LogWriter.DebugLog("RemoveAllUsers", DebugLogTypes.DataManager, "Removing all users.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.RemoveAllUsers();
                });
            }
        }

        public bool RemoveCommand(string command)
        {
            LogWriter.DebugLog("RemoveCommand", DebugLogTypes.DataManager, "Removing command.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.RemoveCommand(command).Result;
            }
        }

        public bool RemoveQuote(int QuoteNum)
        {
            LogWriter.DebugLog("RemoveQuote", DebugLogTypes.DataManager, "Removing quote.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.RemoveQuote(QuoteNum).Result;
            }
        }

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetBuiltInCommandsEnabled", DebugLogTypes.DataManager, "Setting built-in commands enabled.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.SetBuiltInCommandsEnabled(Enabled);
                });
            }
        }

        [Obsolete("No longer compatible after upgrade to Entity Framework Core")]
        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled)
        {
            LogWriter.DebugLog("SetIsEnabled", DebugLogTypes.DataManager, "Setting is enabled.");
            lock (GUIDataManagerLock.Lock) { }
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetSystemEventsEnabled", DebugLogTypes.DataManager, "Setting system events enabled.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.SetSystemEventsEnabled(Enabled);
                });
            }
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetUserDefinedCommandsEnabled", DebugLogTypes.DataManager, "Setting user-defined commands enabled.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    RepeatTimerList.Clear(); // clear the list for enabled change - the timers don't respond for disabled commands

                    await _dataManager.SetUserDefinedCommandsEnabled(Enabled);
                });
            }
        }

        public void SetWebhooksEnabled(bool Enabled)
        {
            LogWriter.DebugLog("SetWebhooksEnabled", DebugLogTypes.DataManager, "Setting webhooks enabled.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.SetWebhooksEnabled(Enabled);
                });
            }
        }

        public void StartBulkFollowers()
        {
            LogWriter.DebugLog("StartBulkFollowers", DebugLogTypes.DataManager, "Starting bulk followers.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.StartBulkFollowers();
                });
            }
        }

        public void SummarizeStreamData()
        {
            LogWriter.DebugLog("SummarizeStreamData", DebugLogTypes.DataManager, "Summarizing stream data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.SummarizeStreamData();
                });
            }
        }

        public void SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            LogWriter.DebugLog("SummarizeStreamData", DebugLogTypes.DataManager, "Summarizing stream data.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.SummarizeStreamData(archiveRecord);
                });
            }
        }

        public IEnumerable<LiveUser> TestGetRandomUsers(int count)
        {
            LogWriter.DebugLog("TestGetRandomUsers", DebugLogTypes.DataManager, "Testing get random users.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.TestGetRandomUsers(count).Result;
            }
        }

        public bool TestInRaidData(string user, DateTime time, int viewers, string gamename)
        {
            LogWriter.DebugLog("TestInRaidData", DebugLogTypes.DataManager, "Testing in-raid data.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.TestInRaidData(user, time, viewers, gamename).Result;
            }
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            LogWriter.DebugLog("TestOutRaidData", DebugLogTypes.DataManager, "Testing out-raid data.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.TestOutRaidData(HostedChannel, dateTime).Result;
            }
        }

        public void UpdateCurrency(List<LiveUser> Users, DateTime dateTime)
        {
            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.DataManager, "Updating currency.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UpdateCurrency(Users, dateTime);
                });
            }
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
            LogWriter.DebugLog("UpdateFollowers", DebugLogTypes.DataManager, "Updating followers.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UpdateFollowers(follows);
                });
            }
        }

        public List<LearnMsgRecord> UpdateLearnedMsgs()
        {
            LogWriter.DebugLog("UpdateLearnedMsgs", DebugLogTypes.DataManager, "Updating learned messages.");
            lock (GUIDataManagerLock.Lock)
            {
                return _dataManager.UpdateLearnedMsgs().Result;
            }
        }

        public void UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            LogWriter.DebugLog("UpdateOverlayTicker", DebugLogTypes.DataManager, "Updating overlay ticker.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UpdateOverlayTicker(item, name);
                });
            }
        }

        public void UpdateStats(DBUserStats Stat, string userId, Platform platform)
        {
            LogWriter.DebugLog("UpdateStats", DebugLogTypes.DataManager, "Updating stats.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UpdateStats(Stat, userId, platform);
                });
            }
        }

        public void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime)
        {
            LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.DataManager, "Updating watch time.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UpdateWatchTime(Users, CurrTime);
                });
            }
        }

        public void UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen)
        {
            LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager, "User joined.");
            lock (GUIDataManagerLock.Lock)
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                    $"Updating {Users.Count()} users now joined to the channel.");
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UserJoined(Users, NowSeen);
                });
            }
        }

        public void UserLeft(LiveUser User, DateTime LastSeen)
        {
            LogWriter.DebugLog("UserLeft", DebugLogTypes.DataManager, "User left.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.UserLeft(User, LastSeen);
                });
            }
        }

        public void Exit()
        {
            LogWriter.DebugLog("Exit", DebugLogTypes.DataManager, "Exiting.");
            lock (GUIDataManagerLock.Lock)
            {
                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    await _dataManager.Exit();
                });
            }
        }
    }
}
