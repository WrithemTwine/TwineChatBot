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
        private ActionSystem SystemAction { get; set; }

        private ConcurrentQueue<Task> ActionQueue { get; set; } = new();

        public DataBot()
        {
            SystemAction = new ActionSystem();
            ThreadManager.CreateThreadStart("DataBotActionThread", ActionThread);

            ManageWindows.DataGridUpdatedRowHandler = DataGridUpdatedRow;
        }

        public void SetLoadCompletedHandler(EventHandler<EventArgs> handler)
        {
            ActionSystem.DataManage.OnLoadCompleted += handler;
        }

        public void Initialize()
        {
            ActionQueue.Enqueue(new Task(async () => await SystemAction.InitializeDataManager()));
        }

        private void ActionThread()
        {
            while (OptionFlags.ActiveToken)
            {
                if (ActionQueue.TryDequeue(out Task action))
                {
                    try
                    {
                        action.Start();
                        action.Wait();
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

        public void InitializeDataManagerViews(IGUIDataManagerViews GUIDataManagerViews)
        {
            GUIDataManagerViews.SetDataManagerViews(this, GetCommandList);
            GUIDataManagerViews.SetSystemCollections(SystemAction);
        }

        public void GetICollection(DataTables dataTables, Action<object> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                var collections = SystemAction.GetICollection(dataTables);
                callback?.Invoke(collections);
            }));
        }

        public void SetCleanupList(ref List<ArchiveMultiStream> archiveMultiStreams)
        {
            SystemAction.SetCleanupList(ref archiveMultiStreams);
        }

        public void SetMultiStatusLog(List<string> log)
        {
            SystemAction.SetMultiStatusLog(log);
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

        public void NotifyBotStart()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.NotifyBotStart()));
        }

        public void NotifyBotStop()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.NotifyBotStop()));
        }

        public void Exit()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.Exit()));
        }

        public void ActivateRepeatTimers()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ActivateRepeatTimers()));
        }

        public void StreamOnline(DateTime startedAt, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.StreamOnline(startedAt));
            }));
        }

        public void GetDiscordWebhooks(WebhooksKind webhooksKind, Action<IEnumerable<Tuple<bool, Uri>>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetDiscordWebhooks(webhooksKind));
            }));
        }

        public void GetEventAnnounce(ChannelEventActions channelEventActions, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetEventAnnounce(channelEventActions));
            }));
        }

        public void GetOverlayActions(Action<Dictionary<string, List<string>>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetOverlayActions());
            }));
        }

        public void GetCommandList(bool prefix, Action<IEnumerable<string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetCommandList(prefix));
            }));
        }

        public void SetChannelRewardList(List<string> channelPointNames)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetChannelRewardList(channelPointNames)));
        }

        public void SetCategory(CategoryData categoryData)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetCategory(categoryData)));
        }

        public void StreamOffline(DateTime currTime)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.StreamOffline(currTime)));
        }

        public void SetNewOverlayEventHandler(EventHandler<NewOverlayEventArgs> overlayHandler, EventHandler<UpdatedTickerItemsEventArgs> tickerHandler)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetNewOverlayEventHandler(overlayHandler, tickerHandler)));
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, ChannelEventActions eventAction, LiveUser user, string UserMsg = null)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.CheckForOverlayEvent(overlayType, eventAction, user, UserMsg)));
        }

        public void CheckForOverlayEvent(OverlayTypes overlayType, string eventAction, LiveUser user, string UserMsg = null)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.CheckForOverlayEvent(overlayType, eventAction, user, UserMsg)));
        }

        public void SendInitialTickerItems()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SendInitialTickerItems()));
        }

        public void ClipHelper(List<Clip> clips)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ClipHelper(clips)));
        }

        public void UpdatedStat(params StreamStatType[] statTypes)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UpdatedStat(statTypes)));
        }

        public void UpdatedStat(StreamStatType statType, int value)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UpdatedStat(statType, value)));
        }

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
            ActionQueue.Enqueue(new Task(() => SystemAction.TestAddUsers()));
        }
#endif
        #endregion

        public void UserJoined(List<LiveUser> users)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UserJoined(users)));
        }

        public void UserLeft(LiveUser user)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UserLeft(user)));
        }

        public void MessageReceived(CmdMessage msg, LiveUser user)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.MessageReceived(msg, user)));
        }

        public void ManageGiveaway(LiveUser user)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ManageGiveaway(user)));
        }

        public void BeginGiveaway()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.BeginGiveaway()));
        }

        public void EndGiveaway()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.EndGiveaway()));
        }

        public void PostGiveawayResult()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.PostGiveawayResult()));
        }

        public void UserCheered(LiveUser user, int bits)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UserCheered(user, bits)));
        }

        public void AddNewAutoShoutUser(string userId, Platform platform)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.AddNewAutoShoutUser(userId, platform)));
        }

        public void StartBulkFollowers()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.StartBulkFollowers()));
        }

        public void UpdateFollowers(List<Follow> follows)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UpdateFollowers(follows)));
        }

        public void StopBulkFollowers()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.StopBulkFollowers()));
        }

        public void PostOutgoingRaid(string hostedChannel, DateTime currTime)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.PostOutgoingRaid(hostedChannel, currTime)));
        }

        public void ProcessCommand(CmdMessage commandmsg, Platform source)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ProcessCommand(commandmsg, source)));
        }

        public void AddNewFollowers(List<Follow> follows)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.AddNewFollowers(follows)));
        }

        public void PostApproval(string message, Task action)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.PostApproval(message, action)));
        }

        public void AddNewOverlayTickerItem(OverlayTickerItem item, string value)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.AddNewOverlayTickerItem(item, value)));
        }

        public void PostIncomingRaid(LiveUser user, DateTime raidTime, int viewerCount, CategoryData category)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.PostIncomingRaid(user, raidTime, viewerCount, category)));
        }

        public void SetSystemEventsEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetSystemEventsEnabled(enabled)));
        }

        public void GetMultiWebHooks(Action<IEnumerable<Tuple<WebhooksSource, Uri>>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                var result = SystemAction.GetMultiWebHooks();
                callback?.Invoke(result);
            }));
        }

        public void GetApprovalRule(ModActionType type, string rewardTitle, Action<Tuple<string, string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetApprovalRule(type, rewardTitle));
            }));
        }

        public void GUISaveDataGridEdits(bool CommandUpdate)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.GUISaveDataGridEdits(CommandUpdate)));
        }

        public void ManageDatabase()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ManageDatabase()));
        }

        internal void DataGridUpdatedRow(object sender, AddNewRowEventArgs e)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.PostDataGridGUIAddRow(e.NewRow)));
        }

        public void ClearWatchTime()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ClearWatchTime()));
        }

        public void ClearAllCurrenciesValues()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ClearAllCurrenciesValues()));
        }

        public void ClearUsersNonFollowers()
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.ClearUsersNonFollowers()));
        }

        public void SetBuiltInCommandsEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetBuiltInCommandsEnabled(enabled)));
        }

        public void SetUserDefinedCommandsEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetUserDefinedCommandsEnabled(enabled)));
        }

        public void SetDiscordWebhooksEnabled(bool enabled)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.SetDiscordWebhooksEnabled(enabled)));
        }

        public void UpdatedIsEnabledRows(IEnumerable<DataRow> dataRows, bool IsEnabled)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.UpdatedIsEnabledRows(dataRows, IsEnabled)));
        }

        public void GetUserId(LiveUser liveUser, Action<string> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetUserId(liveUser));
            }));
        }

        public void GetGameCategories(Action<IEnumerable<CategoryData>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetGameCategories());
            }));
        }

        public void PostMultiLiveLog(string message)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.PostMultiLiveLog(message)));
        }

        public void CheckMultiStreamDate(string userId, Platform platform, DateTime currTime, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.CheckMultiStreamDate(userId, platform, currTime));
            }));
        }

        public void PostMultiStreamDate(LiveUser User, DateTime currTime, Action<bool> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.PostMultiStreamDate(User, currTime));
            }));
        }

        public void MultiSummarize(MultiLiveSummarizeEventArgs multiLiveSummarizeEventArgs)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.MultiSummarize(multiLiveSummarizeEventArgs)));
        }

        public void AddNewMonitorChannel(List<LiveUser> users)
        {
            ActionQueue.Enqueue(new Task(() => SystemAction.AddNewMonitorChannel(users)));
        }

        public void GetMonitorChannels(Platform platform, Action<IEnumerable<string>> callback)
        {
            ActionQueue.Enqueue(new Task(() =>
            {
                callback?.Invoke(SystemAction.GetMonitorChannels(platform));
            }));
        }

    }
}
