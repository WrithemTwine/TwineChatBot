using StreamerBotLib.DataSQL.Import;
using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Events;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Reflection;

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

    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        private readonly DataManagerFactory dbContextFactory = new();

        private bool constructingModel_Context;
        private bool BulkFollowerUpdate;

        /// <summary>
        /// always true to begin one learning cycle
        /// </summary>
        private bool LearnMsgChanged = true;

        private readonly ConcurrentQueue<IEnumerable<Follow>> followsQueue = new();

        private readonly string DefaulSocialMsg = LocalizedMsgSystem.GetVar(Msg.MsgDefaultSocialMsg);
        private DateTime CurrStreamStart { get; set; } = default;

        public event EventHandler<OnBulkFollowersAddFinishedEventArgs> OnBulkFollowersAddFinished;
        public event EventHandler<OnDataCollectionUpdatedEventArgs> OnDataCollectionUpdated;

        public DataManagerSQL()
        {
            GUIContext = dbContextFactory.CreateDbContext();

            if (!OptionFlags.EFCDataImportedDataGram)
            {
                bool LogStatus = OptionFlags.LogBotStatus;  // save current logging status

                OptionFlags.LogBotStatus = true; // force logging operations to status during import

                var context = BuildDataContext();

                ImportDataSources importDataSources = new(); // load the primary database data
                importDataSources.ConvertData(context, this); // convert data loaded from main and multilive data files
                context.SaveChanges(true);

                OptionFlags.LogBotStatus = LogStatus; // restore preferred log status after import
                OptionFlags.EFCDataImportedDataGram = true;
            }
        }

        public void Exit()
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

        public void NotifyDataCollectionUpdated(string TableName)
        {
            OnDataCollectionUpdated?.Invoke(this, new(TableName));
        }

        public void DeleteDataRows(IEnumerable<DataRow> dataRows, SQLDBContext Refcontext = null)
        {
            throw new NotImplementedException();
        }

        public string EditCommand(string cmd, List<string> Arglist, SQLDBContext Refcontext = null)
        {
            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
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
                    context.SaveChanges(true);

                    if (Enum.GetNames<DefaultCommand>().Contains(cmd))
                    {
                        RefreshCommandsObservableCollection();
                    }
                    else
                    {
                        RefreshCommandsUserObservableCollection();
                    }
                }
                else
                {
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);

                }
                if (Refcontext == null) { ClearDataContext(context); }
            }
            return result;
        }

        public object[] PerformQuery(CommandsBase row, int Top = 0, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                IEnumerable<object> output = row.Table switch
                {
                    nameof(Currency) => (from C in context.Currency where C.CurrencyName == row.CurrencyField orderby C.User.UserName select new Tuple<object, object>(C[row.KeyField], C[row.DataField])),
                    nameof(Followers) => (from F in context.Followers orderby F.User.UserName select F),
                    nameof(UserStats) => (from US in context.UserStats orderby US.User.UserName select new Tuple<object, object>(US[row.KeyField], US[row.DataField])),
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

                if (Refcontext == null) { ClearDataContext(context); }
                return output.ToArray();
            }
        }

        public object PerformQuery(CommandsBase row, string ParamValue, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                object output = row.Table switch
                {
                    nameof(Currency) => (from C in context.Currency where (C.User.UserName == ParamValue && C.CurrencyName == row.CurrencyField) select C[row.DataField ?? "Value"]).FirstOrDefault(),
                    nameof(CustomWelcome) => (from W in context.CustomWelcome where W.User.UserName == ParamValue select W[row.DataField]).FirstOrDefault(),
                    nameof(Followers) => (from F in context.Followers where F.User.UserName == ParamValue select F).FirstOrDefault(),
                    nameof(UserStats) => (from US in context.UserStats where US.User.UserName == ParamValue select US[row.DataField]).FirstOrDefault(),
                    nameof(CommandsBase) => (from C in context.CommandsBase where C.CmdName == ParamValue select C[row.DataField]).FirstOrDefault(),
                    _ => ""
                };

                if (output != null && row.Table == nameof(Followers))
                {
                    output = ((Followers)output).IsFollower ? ((Followers)output).FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                }

                if (Refcontext == null) { ClearDataContext(context); }
                return output;
            }
        }

        public bool RemoveCommand(string command, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                bool found = false;

                CommandsUser cmd = (from C in context.CommandsUser where C.CmdName == command select C).FirstOrDefault();
                if (cmd != default)
                {
                    context.CommandsUser.Remove(cmd);
                    found = true;
                }
                context.SaveChanges(true);
                RefreshCommandsUserObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return found;
            }
        }

        public bool RemoveQuote(int QuoteNum, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                bool found = false;

                Quotes quotes = (from Q in context.Quotes where Q.Number == QuoteNum select Q).FirstOrDefault();
                if (quotes != default)
                {
                    context.Quotes.Remove(quotes);
                    found = true;
                }
                context.SaveChanges(true);
                RefreshQuotesObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return found;
            }
        }

        #region Set_IsEnabled Methods
        public void SetBuiltInCommandsEnabled(bool Enabled, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (var Command in (from C in context.Commands
                                         join D in
                                             (from def in Enum.GetNames<DefaultCommand>().Union(Enum.GetNames<DefaultSocials>().ToList())
                                              select def) on C.CmdName equals D into DefCmds
                                         from DC in DefCmds.DefaultIfEmpty()
                                         where DC != null
                                         orderby C.CmdName
                                         select C))
                {
                    Command.IsEnabled = Enabled;
                }
                context.SaveChanges(true);
                RefreshCommandsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void SetWebhooksEnabled(bool Enabled, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                foreach (var webhook in context.Webhooks)
                {
                    webhook.IsEnabled = Enabled;
                }
                context.SaveChanges(true);
                RefreshWebhooksObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        [Obsolete("No longer compatible after upgrade to Entity Framework Core")]
        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false, SQLDBContext Refcontext = null)
        {
            throw new NotImplementedException();
        }

        public void SetSystemEventsEnabled(bool Enabled, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (var Sys in context.ChannelEvents)
                {
                    Sys.IsEnabled = Enabled;
                }
                context.SaveChanges(true);
                RefreshChannelEventsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Sets the 'IsEnabled' column for all records of the Commands table, specifically the user created commands (not the default commands).
        /// </summary>
        /// <param name="Enabled">The value to set for 'IsEnabled'.</param>
        public void SetUserDefinedCommandsEnabled(bool Enabled, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (var Command in (from C in context.Commands
                                         join D in (from def in Enum.GetNames<DefaultCommand>().Union([.. Enum.GetNames<DefaultSocials>()])
                                                    select def) on C.CmdName equals D into UsrCmds
                                         from UC in UsrCmds.DefaultIfEmpty()
                                         where UC == null
                                         orderby C.CmdName
                                         select C))
                {
                    Command.IsEnabled = Enabled;
                }
                context.SaveChanges(true);
                RefreshCommandsUserObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        #endregion

        private void LearnMsgs_LearnMsgsRowDeleted(object sender, EventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether learned message rows are deleted.");

            LearnMsgChanged = true;
        }

        private void LearnMsgs_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether adding a new learned message.");

            LearnMsgChanged = true;
        }

        public void GUIRowEditSave(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                context.SaveChanges(true); // tracked entities displayed in GUI DataGrid; user performed an edit, need to save any changes

                if (Refcontext == null) { ClearDataContext(context); }
            }
        }
    }
}
