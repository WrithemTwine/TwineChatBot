using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;
using System.Data;
using System.Globalization;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        private readonly DataManagerFactory dbContextFactory = new();

        private bool BulkFollowerUpdate;

        /// <summary>
        /// always true to begin one learning cycle
        /// </summary>
        private bool LearnMsgChanged = true;

        private readonly ConcurrentQueue<IEnumerable<Follow>> followsQueue = new();

        private readonly string DefaulSocialMsg = LocalizedMsgSystem.GetVar(Msg.MsgDefaultSocialMsg);
        private DateTime CurrStreamStart { get; set; } = default;

        internal event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;
        internal event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;

        internal event EventHandler<EventArgs> OnLoadCompleted;
        //private event EventHandler<EventArgs> OnInitialize;

        internal DataManagerSQLAsync()
        {
            GUIContext = BuildDataContext();
            GUIContext.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;

            //OnInitialize += OnInitializeHandler;

            //ThreadManager.CreateThreadStart(".ctor_DataManagerSQLAsync", () => { OnInitialize?.Invoke(this, EventArgs.Empty); });
        }

        private SQLDBContext BuildDataContext()
        {
            return dbContextFactory.CreateDbContext();
        }

        //private void OnInitializeHandler(object sender, EventArgs e)
        //{
        //    _ = InitializeDataBaseAsync();
        //}

        public async Task InitializeDataBaseAsync()
        {
            var initialcontext = BuildDataContext();
            initialcontext.Database.EnsureCreated();
            await initialcontext.SaveChangesAsync(true);
            initialcontext.Dispose();

            await Initialize();

            OnLoadCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void ClearDataContext(SQLDBContext context)
        {
            context.Dispose();
        }

        internal async Task Exit()
        {
            await Task.Delay(100); // Allow any pending operations to complete
            GUIContext?.Dispose();
        }

        #region Process Queue

        private Thread processQueueTaskThread;
        private readonly ConcurrentQueue<ManagedAction> queueTasks = new();
        private bool StartedProcessingQueue = false;

        private void PostActionQueue(Task action, string key)
        {
            ManagedAction Action = new(key, action);

            if (!queueTasks.Contains(Action))
            {
                queueTasks.Enqueue(Action);
            }

            if (!StartedProcessingQueue)
            {
                StartedProcessingQueue = true;
                processQueueTaskThread = ThreadManager.CreateThread("PostActionQueue", async () => await ProcessQueuedActionsAsync());
                processQueueTaskThread.Start();
            }
        }

        private async Task ProcessQueuedActionsAsync()
        {
            while (OptionFlags.ActiveToken)
            {
                try
                {
                    while (queueTasks.TryDequeue(out ManagedAction result))
                    {
                        if (result.Action.Status is TaskStatus.WaitingToRun or TaskStatus.Created or TaskStatus.WaitingForActivation)
                        {
                            result.Action.Start();
                        }

                        if (result.Action.Status == TaskStatus.Running)
                        {
                            await result.Action.WaitAsync(CancellationToken.None);
                        }
                        else if (result.Action.Status == TaskStatus.Faulted)
                        {
                            LogWriter.DebugLog("ProcessQueuedActionsAsync", DebugLogTypes.DataManager, $"Task {result.TaskName} failed with exception: {result.Action.Exception}");
                        }
                        else if (result.Action.Status == TaskStatus.Canceled)
                        {
                            LogWriter.DebugLog("ProcessQueuedActionsAsync", DebugLogTypes.DataManager, $"Task {result.TaskName} was canceled.");
                        }
                        else if (result.Action.Status == TaskStatus.RanToCompletion)
                        {
                            // Do nothing, the task has already completed
                        }

                        await Task.Delay(150); // Prevents tight loop, allowing other tasks to run
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex, "ProcessQueuedActionsAsync");
                }
            }
        }

        #endregion

        internal async Task<string> EditCommand(string cmd, List<string> Arglist)
        {
            string result = "";

            using var context = BuildDataContext();

            Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);

            CommandsBase EditCom = context.CommandsBase
                .FirstOrDefault(C => C.CmdName == cmd);

            if (EditCom != default)
            {
                foreach (string k in EditParamsDict.Keys)
                {
                    EditCom.GetType().GetProperty(k).SetValue(EditCom, EditParamsDict[k]);
                }
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);
                await context.SaveChangesAsync(true);

                if (Enum.GetNames<DefaultCommand>().Contains(cmd))
                {
                    await RefreshCommandsList(true);
                }
                else
                {
                    await RefreshCommandsUserList(true);
                }
            }
            else
            {
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);
            }

            return result;
        }

        internal Task<object[]> PerformQuery(CommandsBase row, int Top = 0)
        {
            return Task.Run(() =>
            {
                IEnumerable<object> output;

                using var context = BuildDataContext();

                output = row.Table switch
                {
                    nameof(Currency) => (from C in context.Currency.Include(user => user.User) where C.CurrencyName == row.CurrencyField orderby C.User.UserName select new Tuple<object, object>(C[row.KeyField], C[row.DataField])),
                    nameof(Followers) => (from F in context.Followers.Include(user => user.User) orderby F.User.UserName select F),
                    nameof(UserStats) => (from US in context.UserStats.Include(user => user.User) orderby US.User.UserName select new Tuple<object, object>(US[row.KeyField], US[row.DataField])),
                    _ => [""]
                };


                if (row.Sort == CommandSort.DESC)
                {
                    output = output.OrderByDescending((o) => (o as Tuple<object, object>).Item1);
                }

                if (row.Top > 0)
                {
                    output = output.Take(row.Top);
                }

                return output.ToArray();
            });
        }

        internal Task<object> PerformQuery(CommandsBase row, string ParamValue)
        {
            return Task.Run(() =>
            {
                object output;

                using var context = BuildDataContext();

                output = row.Table switch
                {
                    nameof(Currency) => (from C in context.Currency.Include(user => user.User) where (C.User.UserName == ParamValue && C.CurrencyName == row.CurrencyField) select C[row.DataField ?? "Value"]).FirstOrDefault(),
                    nameof(CustomWelcome) => (from W in context.CustomWelcome.Include(user => user.User) where W.User.UserName == ParamValue select W[row.DataField]).FirstOrDefault(),
                    nameof(Followers) => (from F in context.Followers.Include(user => user.User) where F.User.UserName == ParamValue select F).FirstOrDefault(),
                    nameof(UserStats) => (from US in context.UserStats.Include(user => user.User) where US.User.UserName == ParamValue select US[row.DataField]).FirstOrDefault(),
                    nameof(CommandsBase) => (from C in context.CommandsBase where C.CmdName == ParamValue select C[row.DataField]).FirstOrDefault(),
                    _ => ""
                };

                if (output != null && row.Table == nameof(Followers))
                {
                    output = ((Followers)output).IsFollower ? ((Followers)output).FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                }


                return output;
            });
        }

        #region Set_IsEnabled Methods
        internal async Task SetBuiltInCommandsEnabled(bool Enabled)
        {
            using var context = BuildDataContext();
            await context.Commands.ExecuteUpdateAsync((c) => c.SetProperty((n) => n.IsEnabled, (e) => Enabled));
            await context.SaveChangesAsync(true);
            await RefreshCommandsList();
        }

        internal async Task SetWebhooksEnabled(bool Enabled)
        {
            using var context = BuildDataContext();

            await context.Webhooks.ExecuteUpdateAsync((w) => w.SetProperty((u) => u.IsEnabled, (h) => Enabled));
            await context.SaveChangesAsync(true);
            await RefreshWebhooksList();
        }

        [Obsolete("No longer compatible after upgrade to Entity Framework Core")]
        internal void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            throw new NotImplementedException();
        }

        internal async Task SetSystemEventsEnabled(bool Enabled)
        {
            using var context = BuildDataContext();
            await context.ChannelEvents.ExecuteUpdateAsync((c) => c.SetProperty((e) => e.IsEnabled, (ce) => Enabled));
            await context.SaveChangesAsync(true);
            await RefreshChannelEventsList();
        }

        /// <summary>
        /// Sets the 'IsEnabled' column for all records of the Commands table, specifically the user created commands (not the default commands).
        /// </summary>
        /// <param name="Enabled">The value to set for 'IsEnabled'.</param>
        internal async Task SetUserDefinedCommandsEnabled(bool Enabled)
        {
            using var context = BuildDataContext();
            await context.CommandsUser.ExecuteUpdateAsync((c) => c.SetProperty((n) => n.IsEnabled, (e) => Enabled));
            await context.SaveChangesAsync(true);
            await RefreshCommandsUserList();
        }

        #endregion

        private void LearnMsgs_LearnMsgsRowDeleted(object sender, EventArgs e)
        {
            LogWriter.DebugLog("LearnMsgs_LearnMsgsRowDeleted", DebugLogTypes.DataManager, $"Machine learning, whether learned message rows are deleted.");

            LearnMsgChanged = true;
        }

        private void LearnMsgs_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            LogWriter.DebugLog("LearnMsgs_TableNewRow", DebugLogTypes.DataManager, $"Machine learning, whether adding a new learned message.");

            LearnMsgChanged = true;
        }

        internal async Task GUIRowEditSave(string TableName)
        {
            await GUIContext.SaveChangesAsync(); // tracked entities displayed in GUI DataGrid; user performed an edit, need to save any changes

            switch (TableName)
            {
                case "BanReasons":
                    await RefreshBanReasonsList();
                    break;
                case "BanRules":
                    await RefreshBanRulesList();
                    break;
                case "CategoryList":
                    await RefreshCategoryListList();
                    break;
                case "ChannelEvents":
                    await RefreshChannelEventsList();
                    break;
                case "Clips":
                    await RefreshClipsList();
                    break;
                case "Commands":
                    await RefreshCommandsList();
                    break;
                case "CommandsUser":
                    await RefreshCommandsUserList();
                    break;
                case "Currency":
                    await RefreshCurrencyList();
                    break;
                case "CurrencyType":
                    await RefreshCurrencyTypeList();
                    break;
                case "CustomWelcome":
                    await RefreshCustomWelcomeList();
                    break;
                case "Followers":
                    await RefreshFollowersList();
                    break;
                case "GameDeadCounter":
                    await RefreshGameDeadCounterList();
                    break;
                case "GiveawayUserData":
                    await RefreshGiveawayUserDataList();
                    break;
                case "InRaidData":
                    await RefreshInRaidDataList();
                    break;
                case "LearnMsgs":
                    await RefreshLearnMsgsList();
                    break;
                case "ModeratorApprove":
                    await RefreshModeratorApproveList();
                    break;
                case "OutRaidData":
                    await RefreshOutRaidDataList();
                    break;
                case "OverlayServices":
                    await RefreshOverlayServicesList();
                    break;
                case "OverlayTicker":
                    await RefreshOverlayTickerList();
                    break;
                case "Quotes":
                    await RefreshQuotesList();
                    break;
                case "ShoutOuts":
                    await RefreshShoutOutsList();
                    break;
                case "StreamStats":
                    await RefreshStreamStatsList();
                    break;
                case "Users":
                    await RefreshUsersList();
                    break;
                case "Webhooks":
                    await RefreshWebhooksList();
                    break;
                case "MultiChannels":
                    await RefreshMultiChannelsList();
                    break;
                case "MultiLiveStreams":
                    await RefreshMultiLiveStreamsList();
                    break;
                case "MultiWebhooks":
                    await RefreshMultiWebhooksList();
                    break;
                case "MultiSummaryLiveStreams":
                    await RefreshMultiSummaryLiveStreamsList();
                    break;
                default:
                    // Optionally handle unknown table names
                    break;
            }
        }


    }
}
