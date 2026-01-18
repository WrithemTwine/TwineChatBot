using StreamerBotLib.DataSQL;
using StreamerBotLib.GUI.Windows;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Models.Interfaces;
using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

using System.Collections.Concurrent;
using System.Data;

namespace StreamerBotLib.Systems
{
    public class DataBot : IActionSystem
    {
        #region Debug and Test Methods

#if DEBUG

        public DataManagerSQL GetDataManager()
        {
            return ActionSystem.DataManage as DataManagerSQL;
        }

        public void SetPostChannelMessageHandler(EventHandler<PostChannelMessageEventArgs> handler)
        {
            SystemAction.PostChannelMessage += handler;
        }

        public void TestAddUsers()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("TestAddUsers", DebugLogTypes.DataBot, "Testing adding users.");
                SystemAction.TestAddUsers();
            }));
        }

        public void DebugAddNewMultiLiveData()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("DebugAddNewMultiLiveData", DebugLogTypes.DataBot, "Debugging adding new multi live data.");
                SystemAction.DebugAddNewMultiLiveData();
            }));
        }
#endif
        #endregion

        #region Constructor - NotifyBot - Exit
        private ActionSystem SystemAction { get; set; }

        public DataBot()
        {
            SystemAction = new ActionSystem();
            ThreadManager.CreateThreadStart("DataBotActionThread", ActionThread);

            ManageWindows.DataGridUpdatedRowHandler = DataGridUpdatedRow;
        }

        public void NotifyBotStart()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("NotifyBotStart", DebugLogTypes.DataBot, "Notifying bot start.");
                SystemAction.NotifyBotStart();
            }));
        }

        public void NotifyBotStop()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("NotifyBotStop", DebugLogTypes.DataBot, "Notifying bot stop.");
                SystemAction.NotifyBotStop();
            }));
        }

        public void Exit()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("Exit", DebugLogTypes.DataBot, "Exiting the DataBot.");
                SystemAction.Exit();
            }));
        }

        #endregion

        #region DataBot Action Manager

        private ConcurrentQueue<Task> ActionQueue { get; set; } = new();

        private void ActionThread()
        {
            while (OptionFlags.ActiveToken)
            {
                if (ActionQueue.TryDequeue(out Task action))
                {
                    try
                    {
                        action?.Start();
                        action?.Wait();
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogException(ex, "ActionThread");
                    }
                }
                else
                {
                    Thread.Sleep(100); // Sleep to prevent busy waiting
                }
            }
        }

        #endregion

        #region Initialize DataBot

        public void SetLoadCompletedHandler(EventHandler<EventArgs> handler)
        {
            ActionSystem.DataManage.OnLoadCompleted += handler;
        }

        public void SetGetClipsHandler(EventHandler<GetChannelClipsEventArgs> handler)
        {
            SystemAction.GetChannelClipsEvent += handler;
        }

        public void Initialize()
        {
            ActionQueue.Enqueue(new Task(async () => await SystemAction.InitializeDataManager()));
        }

        public void InitializeDataManagerViews(IGUIDataManagerViews GUIDataManagerViews)
        {
            GUIDataManagerViews.SetDataManagerViews(this, GetCommandList, GetCommandListNoParams);
            GUIDataManagerViews.SetSystemCollections(SystemAction);
        }

        public void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> overlayHandler, EventHandler<UpdatedTickerItemsEventArgs> tickerHandler)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetNewOverlayEventHandler", DebugLogTypes.DataBot, "Setting new overlay event handler.");
                SystemAction.SetNewOverlayEventHandler(overlayHandler, tickerHandler);
            }));
        }

        public void SetCleanupList(ref List<ArchiveMultiStream> archiveMultiStreams)
        {
            SystemAction.SetCleanupList(ref archiveMultiStreams);
        }

        public void SetMultiStatusLog(ref List<string> log)
        {
            SystemAction.SetMultiStatusLog(ref log);
        }

        internal void InitializeBotControllerHandlers(
            EventHandler<PostChannelMessageEventArgs> PostChannelMessage,
            EventHandler<BanUserRequestEventArgs> BanUserRequest,
            EventHandler<TwitchShoutOutUsersEventArgs> TwitchShoutOutUser)
        {
            SystemAction.PostChannelMessage += PostChannelMessage;
            SystemAction.BanUserRequest += BanUserRequest;
            SystemAction.TwitchShoutOutUser += TwitchShoutOutUser;
        }

        internal EventHandler<PostChannelMessageEventArgs> GetPostChannelMessageHandler()
        {
            return SystemAction.OutputSentToBotsHandler;
        }

        public void InitializeDataManagerCollectionUpdateEvent(EventHandler<OnDataCollectionUpdatedEventArgs> eventHandler)
        {
            SystemAction.InitializeDataManagerCollectionUpdateEvent(eventHandler);
        }

        public void InitializeLiveMonitorUpdateChannels(EventHandler eventHandler)
        {
            SystemAction.InitializeUpdatedMonitoringChannelsEvent(eventHandler);
        }


        #endregion

        #region Helpers
        public void ClipHelper(bool AllClips, List<Clip> clips)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ClipHelper", DebugLogTypes.DataBot, $"Processing clips count: {clips?.Count ?? 0}.");
                SystemAction.ClipHelper(AllClips, clips);
            }));
        }
        public void UpdatedStat(params StreamStatType[] statTypes)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UpdatedStat", DebugLogTypes.DataBot, $"Updating stats: {string.Join(", ", statTypes)}.");
                SystemAction.UpdatedStat(statTypes);
            }));
        }
        public void UpdatedStat(StreamStatType statType, int value)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UpdatedStat", DebugLogTypes.DataBot, $"Updating stat: {statType} with value: {value}.");
                SystemAction.UpdatedStat(statType, value);
            }));
        }

        #endregion

        #region Stream Status

        public void StreamOnline(DateTime startedAt, CategoryData category, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("StreamOnline", DebugLogTypes.DataBot, "Setting stream online.");
                SystemAction.SetCategory(category);
                callback?.Invoke(SystemAction.StreamOnline(startedAt));
            }));
        }

        public void StreamOffline(DateTime currTime)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("StreamOffline", DebugLogTypes.DataBot, "Setting stream offline.");
                SystemAction.StreamOffline(currTime);
            }));
        }

        #region MultiLive Stream Status

        public void PostMultiLiveLog(string message)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostMultiLiveLog", DebugLogTypes.DataBot, $"Posting multi live log: {message}.");
                SystemAction.PostMultiLiveLog(message);
            }));
        }
        public void CheckMultiLiveStreamDate(string userId, Platform platform, DateTime currTime, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("CheckMultiStreamDate", DebugLogTypes.DataBot, $"Checking multi stream date for userId: {userId}, platform: {platform}, current time: {currTime}.");
                callback?.Invoke(SystemAction.CheckMultiLiveStreamDate(userId, platform, currTime));
            }));
        }
        public void PostMultiStreamDate(LiveUser User, DateTime currTime, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostMultiStreamDate", DebugLogTypes.DataBot, $"Posting multi stream date for user: {User?.UserName}, current time: {currTime}.");
                callback?.Invoke(SystemAction.PostMultiStreamDate(User, currTime));
            }));
        }
        public void MultiSummarize(MultiLiveSummarizeEventArgs multiLiveSummarizeEventArgs)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("MultiSummarize", DebugLogTypes.DataBot, $"Summarizing multi live data for users.");
                SystemAction.MultiSummarize(multiLiveSummarizeEventArgs);
            }));
        }
        public void AddNewMonitorChannel(List<LiveUser> users)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("AddNewMonitorChannel", DebugLogTypes.DataBot, $"Adding new monitor channel for users: {string.Join(", ", users.Select(u => u.UserName))}.");
                SystemAction.AddNewMonitorChannel(users);
            }));
        }
        public void GetMonitorChannels(Platform platform, Action<IEnumerable<string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetMonitorChannels", DebugLogTypes.DataBot, $"Getting monitor channels for platform: {platform}.");
                callback?.Invoke(SystemAction.GetMonitorChannels(platform));
            }));
        }

        #endregion

        #endregion

        #region Get Data

        public void GetICollection(DataTables dataTables, Action<object> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                var collections = SystemAction.GetICollection(dataTables);
                callback?.Invoke(collections);
            }));
        }
        public void GetDiscordWebhooks(WebhooksKind webhooksKind, Action<IEnumerable<Tuple<bool, Uri>>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetDiscordWebhooks", DebugLogTypes.DataBot, $"Getting Discord webhooks for {webhooksKind}.");
                callback?.Invoke(SystemAction.GetDiscordWebhooks(webhooksKind));
            }));
        }
        public void GetEventAnnounce(ChannelEventActions channelEventActions, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetEventAnnounce", DebugLogTypes.DataBot, $"Getting event announce for {channelEventActions}.");
                callback?.Invoke(SystemAction.GetEventAnnounce(channelEventActions));
            }));
        }
        public void GetOverlayActions(Action<Dictionary<string, List<string>>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetOverlayActions", DebugLogTypes.DataBot, "Getting overlay actions.");
                callback?.Invoke(SystemAction.GetOverlayActions());
            }));
        }
        public void GetCommandList(bool prefix, Action<IEnumerable<string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetCommandList", DebugLogTypes.DataBot, $"Getting command list with prefix: {prefix}.");
                callback?.Invoke(SystemAction.GetCommandList(prefix));
            }));
        }
        public void GetCommandListNoParams(bool prefix, Action<IEnumerable<string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetCommandList", DebugLogTypes.DataBot, $"Getting command list with prefix: {prefix}.");
                callback?.Invoke(SystemAction.GetCommandListNoParams(prefix));
            }));
        }
        public void GetMultiWebHooks(Action<IEnumerable<Tuple<WebhooksSource, Uri>>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetMultiWebHooks", DebugLogTypes.DataBot, "Getting multi webhooks.");
                var result = SystemAction.GetMultiWebHooks();
                callback?.Invoke(result);
            }));
        }
        public void GetApprovalRule(ModActionType type, string rewardTitle, Action<Tuple<string, string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetApprovalRule", DebugLogTypes.DataBot, $"Getting approval rule for type: {type} with reward title: {rewardTitle}.");
                callback?.Invoke(SystemAction.GetApprovalRule(type, rewardTitle));
            }));
        }
        public void GetGameCategories(Action<IEnumerable<CategoryData>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetGameCategories", DebugLogTypes.DataBot, "Getting game categories.");
                callback?.Invoke(SystemAction.GetGameCategories());
            }));
        }
        public void GetUserId(LiveUser liveUser, Action<string> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GetUserId", DebugLogTypes.DataBot, $"Getting user ID for user: {liveUser?.UserName}.");
                callback?.Invoke(SystemAction.GetUserId(liveUser));
            }));
        }


        #endregion

        #region Set Data
        public void GUISaveDataGridEdits(bool CommandUpdate, string TableName)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("GUISaveDataGridEdits", DebugLogTypes.DataBot, $"Saving data grid edits with command update: {CommandUpdate}.");
                SystemAction.GUISaveDataGridEdits(CommandUpdate, TableName);
            }));
        }

        public void ManageDatabase()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ManageDatabase", DebugLogTypes.DataBot, "Managing database.");
                SystemAction.ManageDatabase();
            }));
        }

        internal void DataGridUpdatedRow(object sender, AddNewRowEventArgs e)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("DataGridUpdatedRow", DebugLogTypes.DataBot, $"Updating data grid row: {e.NewRow?.TableName}.");
                SystemAction.PostDataGridGUIAddRow(e.NewRow);
            }));
        }

        public void SetChannelRewardList(List<string> channelPointNames)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetChannelRewardList", DebugLogTypes.DataBot, "Setting channel reward list.");
                SystemAction.SetChannelRewardList(channelPointNames);
            }));
        }

        public void SetCategory(CategoryData categoryData)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetCategory", DebugLogTypes.DataBot, $"Setting category: {categoryData.CategoryName}.");
                SystemAction.SetCategory(categoryData);
            }));
        }

        /// <summary>
        /// Used only for stream online updates to increment category stream count
        /// </summary>
        /// <param name="categoryData">The category to update.</param>
        public void PostCategoryStream(CategoryData categoryData)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostCategoryStreamCount", DebugLogTypes.DataBot, $"Posting category stream count for: {categoryData.CategoryName}.");
                SystemAction.PostCategoryStream(categoryData);
            }));
        }

        public void PostViewerCategory(CategoryData CategoryData)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostViewerCategory", DebugLogTypes.DataBot, $"Posting viewer category: {CategoryData.CategoryName}.");
                SystemAction.PostViewerCategory(CategoryData);
            }));
        }

        public void SetSystemEventsEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetSystemEventsEnabled", DebugLogTypes.DataBot, $"Setting system events enabled: {enabled}.");
                SystemAction.SetSystemEventsEnabled(enabled);
            }));
        }
        public void SetBuiltInCommandsEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetBuiltInCommandsEnabled", DebugLogTypes.DataBot, $"Setting built-in commands enabled: {enabled}.");
                SystemAction.SetBuiltInCommandsEnabled(enabled);
            }));
        }

        public void SetUserDefinedCommandsEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetUserDefinedCommandsEnabled", DebugLogTypes.DataBot, $"Setting user-defined commands enabled: {enabled}.");
                SystemAction.SetUserDefinedCommandsEnabled(enabled);
            }));
        }

        public void SetDiscordWebhooksEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SetDiscordWebhooksEnabled", DebugLogTypes.DataBot, $"Setting Discord webhooks enabled: {enabled}.");
                SystemAction.SetDiscordWebhooksEnabled(enabled);
            }));
        }

        #endregion

        #region User Operations
        public void UserJoined(List<LiveUser> users)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.DataBot, $"Users joined: {string.Join(", ", users.Select(u => u.UserName))}.");
                SystemAction.UserJoined(users);
            }));
        }
        public void UserLeft(LiveUser user)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UserLeft", DebugLogTypes.DataBot, $"User left: {user.UserName}.");
                SystemAction.UserLeft(user);
            }));
        }
        public void AddNewAutoShoutUser(string userId, Platform platform)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("AddNewAutoShoutUser", DebugLogTypes.DataBot, $"Adding new auto shout user: {userId} on platform: {platform}.");
                SystemAction.AddNewAutoShoutUser(userId, platform);
            }));
        }


        #region Giveaway
        public void ManageGiveaway(LiveUser user)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ManageGiveaway", DebugLogTypes.DataBot, $"Managing giveaway for user: {user.UserName}.");
                SystemAction.ManageGiveaway(user);
            }));
        }

        public void BeginGiveaway()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("BeginGiveaway", DebugLogTypes.DataBot, "Beginning giveaway.");
                SystemAction.BeginGiveaway();
            }));
        }

        public void EndGiveaway()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("EndGiveaway", DebugLogTypes.DataBot, "Ending giveaway.");
                SystemAction.EndGiveaway();
            }));
        }

        public void PostGiveawayResult()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostGiveawayResult", DebugLogTypes.DataBot, "Posting giveaway result.");
                SystemAction.PostGiveawayResult();
            }));
        }

        #endregion

        #region Followers

        public void StartBulkFollowers()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("StartBulkFollowers", DebugLogTypes.DataBot, "Starting bulk followers process.");
                SystemAction.StartBulkFollowers();
            }));
        }
        public void UpdateFollowers(List<Follow> follows)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UpdateFollowers", DebugLogTypes.DataBot, $"Updating followers count: {follows?.Count ?? 0}.");
                SystemAction.UpdateFollowers(follows);
            }));
        }
        public void StopBulkFollowers()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("StopBulkFollowers", DebugLogTypes.DataBot, "Stopping bulk followers process.");
                SystemAction.StopBulkFollowers();
            }));
        }
        public void AddNewFollowers(List<Follow> follows)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("AddNewFollowers", DebugLogTypes.DataBot, $"Adding new followers count: {follows?.Count ?? 0}.");
                SystemAction.AddNewFollowers(follows);
            }));
        }

        #endregion

        #endregion

        #region Commands & Repeater

        public void ProcessCommand(CmdMessage commandmsg, Platform source)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ProcessCommand", DebugLogTypes.DataBot, $"Processing command: {commandmsg.CommandText} from source: {source}.");
                SystemAction.ProcessCommand(commandmsg, source);
            }));
        }
        public void ActivateRepeatTimers()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ActivateRepeatTimers", DebugLogTypes.DataBot, "Activating repeat timers.");
                SystemAction.ActivateRepeatTimers();
            }));
        }
        public void ResetRepeatTimerMode()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ActivateRepeatTimers", DebugLogTypes.DataBot, "Activating repeat timers.");
                SystemAction.ResetRepeatTimerMode();
            }));
        }
        public void UpdateRepeatCommands()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UpdateRepeatCommands", DebugLogTypes.DataBot, "Updating repeat commands.");
                SystemAction.UpdateRepeatCommands();
            }));
        }

        #endregion

        #region Channel Events
        public void MessageReceived(CmdMessage msg, LiveUser user)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("MessageReceived", DebugLogTypes.DataBot, $"Message received from user: {user.UserName}, message: {msg.Message}.");
                SystemAction.MessageReceived(msg, user);
            }));
        }

        public void UserCheered(LiveUser user, int bits)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("UserCheered", DebugLogTypes.DataBot, $"User cheered: {user.UserName} with bits: {bits}.");
                SystemAction.UserCheered(user, bits);
            }));
        }
        public void PostOutgoingRaid(string hostedChannel, DateTime currTime)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostOutgoingRaid", DebugLogTypes.DataBot, $"Posting outgoing raid to channel: {hostedChannel} at time: {currTime}.");
                SystemAction.PostOutgoingRaid(hostedChannel, currTime);
            }));
        }

        public void PostApproval(string message, Task action)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostApproval", DebugLogTypes.DataBot, $"Posting approval message: {message} with action: {action?.Id}.");
                SystemAction.PostApproval(message, action);
            }));
        }

        public void PostIncomingRaid(LiveUser user, DateTime raidTime, int viewerCount, CategoryData category)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("PostIncomingRaid", DebugLogTypes.DataBot, $"Posting incoming raid from user: {user?.UserName} at time: {raidTime}, viewer count: {viewerCount}, category: {category?.CategoryName}.");
                SystemAction.PostIncomingRaid(user, raidTime, viewerCount, category);
            }));
        }


        #endregion

        #region Overlay Actions

        public void CheckForOverlayEvent(OverlayTypes overlayType, ChannelEventActions eventAction, LiveUser user, string UserMsg = null)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.DataBot, $"Checking for overlay event: {overlayType} with action: {eventAction} for user: {user?.UserName}.");
                SystemAction.CheckForOverlayEvent(overlayType, eventAction, user, UserMsg);
            }));
        }
        public void CheckForOverlayEvent(OverlayTypes overlayType, string eventAction, LiveUser user, string UserMsg = null)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("CheckForOverlayEvent", DebugLogTypes.DataBot, $"Checking for overlay event: {overlayType} with action: {eventAction} for user: {user?.UserName}.");
                SystemAction.CheckForOverlayEvent(overlayType, eventAction, user, UserMsg);
            }));
        }
        public void SendInitialTickerItems()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("SendInitialTickerItems", DebugLogTypes.DataBot, "Sending initial ticker items.");
                SystemAction.SendInitialTickerItems();
            }));
        }
        public void AddNewOverlayTickerItem(OverlayTickerItem item, string value)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("AddNewOverlayTickerItem", DebugLogTypes.DataBot, $"Adding new overlay ticker item: {item} with value: {value}.");
                SystemAction.AddNewOverlayTickerItem(item, value);
            }));
        }

        #endregion

        #region Clear & Reset Data

        public void ClearWatchTime()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ClearWatchTime", DebugLogTypes.DataBot, "Clearing watch time.");
                SystemAction.ClearWatchTime();
            }));
        }

        public void ClearAllCurrenciesValues()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ClearAllCurrenciesValues", DebugLogTypes.DataBot, "Clearing all currencies values.");
                SystemAction.ClearAllCurrenciesValues();
            }));
        }

        public void ClearUsersNonFollowers()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ClearUsersNonFollowers", DebugLogTypes.DataBot, "Clearing users who are not followers.");
                SystemAction.ClearUsersNonFollowers();
            }));
        }

        public void ResetCategoryStreamCount()
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("ResetCategoryStreamCount", DebugLogTypes.DataBot, "Resetting stream counts for all categories.");
                SystemAction.ResetCategoryStreamCount();
            }));
        }

        public void DeleteDataRows(IEnumerable<object> entities, string TableName)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                LogWriter.DebugLog("DeleteDataRows", DebugLogTypes.DataBot, "Deleting data rows.");
                SystemAction.DeleteRows(entities, TableName);
            }));
        }

        #endregion
    }
}
