using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Import;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace StreamerBotLib.DataSQL
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

        internal DataManagerSQLAsync()
        {
            GUIContext = dbContextFactory.CreateDbContext();

            if (!OptionFlags.EFCDataImportedDataGram)
            {
                bool LogStatus = OptionFlags.LogBotStatus;  // save current logging status

                OptionFlags.LogBotStatus = true; // force logging operations to status during import

                using var context = BuildDataContext();

                ImportDataSources importDataSources = new(); // load the primary database data
                importDataSources.ConvertData(context, this); // convert data loaded from main and multilive data files
                context.SaveChanges(true);

                OptionFlags.LogBotStatus = LogStatus; // restore preferred log status after import
                OptionFlags.EFCDataImportedDataGram = true;
            }
        }

        #region Process Queue

        private readonly ConcurrentDictionary<string, Task> queueDictionary = new();
        private bool StartedProcessingQueue = false;

        private void PostActionQueue(Task action, string key)
        {
            if (!queueDictionary.ContainsKey(key))
            {
                queueDictionary.Append(new(key, action));
            }

            if (!StartedProcessingQueue)
            {
                StartedProcessingQueue = true;
                ThreadManager.CreateThreadStart("PostActionQueue", () => ProcessQueuedActionsAsync() );
            }
        }

        private async Task ProcessQueuedActionsAsync()
        {
            try
            {
                await Task.Delay(5000); // wait 2.5 seconds before processing queue
                while(queueDictionary.Keys.Any())
                {
                    string key = queueDictionary.Keys.FirstOrDefault();
                    if (key != null)
                    {
                        queueDictionary.Remove(key, out var task);
                        task.RunSynchronously();
                    }
                    await Task.Delay(800);
                }

                StartedProcessingQueue = false;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex,"ProcessQueuedActionsAsync");
            }
        }

        #endregion

        internal void Exit()
        {
            GUIContext.SaveChanges(true);
            GUIContext.Dispose();
        }

        private SQLDBContext BuildDataContext()
        {
            return dbContextFactory.CreateDbContext();
        }

        private void ClearDataContext(SQLDBContext context)
        {
            context.Dispose();
        }

        internal void DeleteDataRows(IEnumerable<DataRow> dataRows)
        {
            throw new NotImplementedException();
        }

        internal Task<string> EditCommand(string cmd, List<string> Arglist)
        {
            return Task.Run(async () =>
            {
                string result = "";

                using var context = BuildDataContext();

                Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);
                CommandsBase EditCom = (from C in context.CommandsBase
                                        where C.CmdName == cmd
                                        select C).FirstOrDefault();

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
            });
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
                    nameof(Followers) => (from F in context.Followers.Include(user=>user.User) orderby F.User.UserName select F),
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
        internal Task SetBuiltInCommandsEnabled(bool Enabled)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.Commands.ExecuteUpdateAsync((c) => c.SetProperty((n) => n.IsEnabled, (e) => Enabled));
                await context.SaveChangesAsync();
                RefreshCommandsList();
            });
        }

        internal Task SetWebhooksEnabled(bool Enabled)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();

                await context.Webhooks.ExecuteUpdateAsync((w) => w.SetProperty((u) => u.IsEnabled, (h) => Enabled));
                await context.SaveChangesAsync();
                RefreshWebhooksList();
            });
        }

        [Obsolete("No longer compatible after upgrade to Entity Framework Core")]
        internal void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            throw new NotImplementedException();
        }

        internal Task SetSystemEventsEnabled(bool Enabled)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.ChannelEvents.ExecuteUpdateAsync((c) => c.SetProperty((e) => e.IsEnabled, (ce) => Enabled));
                await context.SaveChangesAsync();
                RefreshChannelEventsList();
            });
        }

        /// <summary>
        /// Sets the 'IsEnabled' column for all records of the Commands table, specifically the user created commands (not the default commands).
        /// </summary>
        /// <param name="Enabled">The value to set for 'IsEnabled'.</param>
        internal Task SetUserDefinedCommandsEnabled(bool Enabled)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.CommandsUser.ExecuteUpdateAsync((c) => c.SetProperty((n) => n.IsEnabled, (e) => Enabled));
                await context.SaveChangesAsync();
                RefreshCommandsUserList();
            });
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
                await GUIContext.SaveChangesAsync(); // tracked entities displayed in GUI DataGrid; user performed an edit, need to save any changes
            });
        }
    }
}
