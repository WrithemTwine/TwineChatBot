using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.DataSQL.SingleContext.Import;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Models.Events;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;
using System.Data;
using System.Globalization;

namespace StreamerBotLib.DataSQL.SingleContext
{
    /*

  !command: <switches-optional> <message>

  switches:
  -t:<table>   (requires -f)
  -f:<field>    (requires -t)
  -c:<currency> (requires -f, optional switch)
  -unit:<field units>   (optional with -f, but recommended)

  -p:<permission>
  -top:<number>
  -s:<sort>
  -a:<action>
  -e:<true|false> // IsEnabled
  -param:<allow params to command>
  -timer:<seconds>
  -use:<usage message>
  -category:<All-defaul>

  -m:<message> -> The message to display, may include parameters (e.g. #user, #field).
   */

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

        private readonly SQLDBContext context;

        internal DataManagerSQLAsync()
        {
            if (!OptionFlags.EFCDataImportedDataGram)
            {
                bool LogStatus = OptionFlags.LogBotStatus;  // save current logging status

                OptionFlags.LogBotStatus = true; // force logging operations to status during import

                using var importcontext = dbContextFactory.CreateDbContext();

                using var transaction = importcontext.Database.BeginTransaction();

                ImportDataSources importDataSources = new(); // load the primary database data
                importDataSources.ConvertData(importcontext, this); // convert data loaded from main and multilive data files

                transaction.Commit();
                importcontext.SaveChanges(true);

                OptionFlags.LogBotStatus = LogStatus; // restore preferred log status after import
                OptionFlags.EFCDataImportedDataGram = true;
            }

            var initialcontext = dbContextFactory.CreateDbContext();
            initialcontext.Database.EnsureCreated();
            initialcontext.SaveChanges(true);
            initialcontext.Dispose();

            try
            {
                using var context1 = dbContextFactory.CreateDbContext();
                context1.Database.Migrate();
                context1.SaveChanges();
            }
            catch { /* ignore */ }

            context = dbContextFactory.CreateDbContext();
        }

        #region Process Queue

        //private Thread processQueueTaskThread;
        //private readonly ConcurrentQueue<ManagedAction> queueTasks = new();
        //private bool StartedProcessingQueue = false;

        //private void PostActionQueue(Action action, string key)
        //{
        //    ManagedAction Action = new(key, action);

        //    if (!queueTasks.Contains(Action))
        //    {
        //        queueTasks.Enqueue(Action);
        //    }

        //    if (!StartedProcessingQueue)
        //    {
        //        StartedProcessingQueue = true;
        //        processQueueTaskThread = ThreadManager.CreateThread("PostActionQueue", async () => await ProcessQueuedActionsAsync());
        //        processQueueTaskThread.Start();
        //    }
        //}

        //private async Task ProcessQueuedActionsAsync()
        //{
        //    while (OptionFlags.ActiveToken)
        //    {
        //        await Task.Delay(5000); // wait 5 seconds before processing queue
        //        try
        //        {
        //            while (queueTasks.TryDequeue(out ManagedAction result))
        //            {
        //                result.Action.Invoke();
        //                await Task.Delay(800);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            LogWriter.LogException(ex, "ProcessQueuedActionsAsync");
        //        }
        //    }
        //}

        #endregion

        internal async Task Exit()
        {
            //processQueueTaskThread?.Join();
            await context.SaveChangesAsync(true);
            context.Dispose();
        }

        private SQLDBContext BuildDataContext()
        {
            return context;
        }

        private void ClearDataContext(SQLDBContext context)
        {
            context.Dispose();
        }

        internal void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            throw new NotImplementedException();
        }

        internal async Task<string> EditCommand(string cmd, List<string> Arglist, IDbContextTransaction contextTransaction = null)
        {
            string result = "";

            // // using var context = BuildDataContext();

            Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);
            CommandsBase EditCom = (from C in context.CommandsBase
                                    where C.CmdName == cmd
                                    select C).FirstOrDefault();

            if (EditCom != default)
            {
                using var transaction = context.Database.BeginTransaction();

                foreach (string k in EditParamsDict.Keys)
                {
                    EditCom.GetType().GetProperty(k).SetValue(EditCom, EditParamsDict[k]);
                }
                result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);

                //await transaction.CommitAsync();
                await context.SaveChangesAsync(true);

                if (Enum.GetNames<DefaultCommand>().Contains(cmd))
                {
                    RefreshCommandsList();
                }
                else
                {
                    RefreshCommandsUserList();
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

                // // using var context = BuildDataContext();

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

                // // using var context = BuildDataContext();

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
        internal async Task SetBuiltInCommandsEnabled(bool Enabled, IDbContextTransaction contextTransaction = null)
        {
            // // using var context = BuildDataContext();
            using var transaction = context.Database.BeginTransaction();

            await context.Commands.ExecuteUpdateAsync((c) => c.SetProperty((n) => n.IsEnabled, (e) => Enabled));

            //await transaction.CommitAsync();
            await context.SaveChangesAsync();
            RefreshCommandsList();
        }

        internal async Task SetWebhooksEnabled(bool Enabled, IDbContextTransaction contextTransaction = null)
        {
            // // using var context = BuildDataContext();
            using var transaction = context.Database.BeginTransaction();

            await context.Webhooks.ExecuteUpdateAsync((w) => w.SetProperty((u) => u.IsEnabled, (h) => Enabled));
            //await transaction.CommitAsync();
            await context.SaveChangesAsync();
            RefreshWebhooksList();
        }

        [Obsolete("No longer compatible after upgrade to Entity Framework Core")]
        internal void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            throw new NotImplementedException();
        }

        internal async Task SetSystemEventsEnabled(bool Enabled, IDbContextTransaction contextTransaction = null)
        {
            // // using var context = BuildDataContext();
            using var transaction = context.Database.BeginTransaction();

            await context.ChannelEvents.ExecuteUpdateAsync((c) => c.SetProperty((e) => e.IsEnabled, (ce) => Enabled));
            //await transaction.CommitAsync();
            await context.SaveChangesAsync();
            RefreshChannelEventsList();
        }

        /// <summary>
        /// Sets the 'IsEnabled' column for all records of the Commands table, specifically the user created commands (not the default commands).
        /// </summary>
        /// <param name="Enabled">The value to set for 'IsEnabled'.</param>
        internal async Task SetUserDefinedCommandsEnabled(bool Enabled, IDbContextTransaction contextTransaction = null)
        {
            // // using var context = BuildDataContext();
            using var transaction = context.Database.BeginTransaction();

            await context.CommandsUser.ExecuteUpdateAsync((c) => c.SetProperty((n) => n.IsEnabled, (e) => Enabled));
            //await transaction.CommitAsync();
            await context.SaveChangesAsync();
            RefreshCommandsUserList();
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

        internal Task GUIRowEditSave()
        {
            return Task.Run(async () =>
            {
                await context.SaveChangesAsync(); // tracked entities displayed in GUI DataGrid; user performed an edit, need to save any changes
            });
        }
    }
}
