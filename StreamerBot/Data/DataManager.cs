using StreamerBot.Enums;
using StreamerBot.Interfaces;
using StreamerBot.Models;
using StreamerBot.Static;
using StreamerBot.Systems;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using static StreamerBot.Data.DataSource;

namespace StreamerBot.Data
{
    public partial class DataManager : IDataManageReadOnly
    {
        #region DataSource
        private static readonly string DataFileXML = "ChatDataStore.xml";

#if DEBUG
        private static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\StreamerBot\bin\Debug\net5.0-windows", DataFileXML);
#else
        private static readonly string DataFileName = DataFileXML;
#endif

        internal readonly DataSource _DataSource;
        #endregion DataSource

        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 10000;

        /// <summary>
        /// Record count to distinguish using Parallel For, ForEach loops
        /// </summary>
        private const int UseParallelThreashold = 5000;

        public bool UpdatingFollowers { get; set; }

        public DataManager()
        {
            _DataSource = new();
            LoadData();
            OnSaveData += SaveData;
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        private void LoadData()
        {
            lock (_DataSource)
            {
                if (!File.Exists(DataFileName))
                {
                    _DataSource.WriteXml(DataFileName);
                }

                using XmlReader xmlreader = new XmlTextReader(DataFileName);
                _ = _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);


                // see if clip dates are correctly formatted

                foreach (ClipsRow c in _DataSource.Clips.Select())
                {
                    c.CreatedAt = DateTime.Parse(c.CreatedAt).ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
                }

                foreach (CommandsRow c in _DataSource.Commands.Select())
                {
                    if (DBNull.Value.Equals(c["IsEnabled"]))
                    {
                        c["IsEnabled"] = true;
                    }
                }
            }

            SaveData(this, new());
        }

        public void Initialize()
        {
            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands
            NotifySaveData();
        }

        /// <summary>
        /// Provide an internal notification event to save the data outside of any multi-threading mechanisms.
        /// </summary>
        public event EventHandler OnSaveData;
        public void NotifySaveData()
        {
            OnSaveData?.Invoke(this, new());
        }

        /// <summary>
        /// Save data to file upon exit and after data changes. Pauses for 15 seconds (unless exiting) to slow down multiple saves in a short time.
        /// </summary>
        public void SaveData(object sender, EventArgs e)
        {
            if (!UpdatingFollowers) // block saving data until the follower updating is completed
            {
                if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
                {
                    SaveThreadStarted = true;
                    new Thread(new ThreadStart(PerformSaveOp)).Start();
                }

                if (_DataSource.HasChanges())
                {
                    lock (_DataSource)
                    {
                        _DataSource.AcceptChanges();
                    }

                    lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                    {
                        SaveTasks.Enqueue(new(() =>
                        {
                            lock (_DataSource)
                            {
                                try
                                {
                                    MemoryStream SaveData = new();  // new memory stream

                                    _DataSource.WriteXml(SaveData, XmlWriteMode.DiffGram); // save the database to the memory stream

                                    DataSource testinput = new();   // start a new database
                                    SaveData.Position = 0;          // reset the reader
                                    testinput.ReadXml(SaveData);    // try to read the database, when in valid state this doesn't cause an exception (try/catch)

                                    _DataSource.WriteXml(DataFileName, XmlWriteMode.DiffGram); // write the valid data to file
                                }
                                catch (Exception ex)
                                {
                                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                                }
                            }
                        }));
                    }
                }
            }
        }

        private void PerformSaveOp()
        {
            if (OptionFlags.ActiveToken) // don't sleep if exiting app
            {
                Thread.Sleep(SaveThreadWait);
            }

            lock (SaveTasks) // in case save actions arrive during save try
            {
                if (SaveTasks.Count >= 1)
                {
                    SaveTasks.Dequeue().Start(); // only run 1 of the save tasks
                }
                SaveTasks.Clear();
            }
            SaveThreadStarted = false; // indicate start another thread to save data
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>Null for no value or the first row found using the <i>rowcriteria</i></returns>
        public object GetRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
        {
            return GetAllRowData(dataRetrieve, rowcriteria).FirstOrDefault();
        }

        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>All data found using the <i>rowcriteria</i></returns>
        public object[] GetAllRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
        {
            string criteriacolumn = "";
            string datacolumn = "";
            string table = "";

            switch (dataRetrieve)
            {
                case DataRetrieve.EventMessage:
                    table = DataSourceTableName.ChannelEvents.ToString();
                    criteriacolumn = "Name";
                    datacolumn = "MsgStr";
                    break;
                case DataRetrieve.EventEnabled:
                    table = DataSourceTableName.ChannelEvents.ToString();
                    criteriacolumn = "Name";
                    datacolumn = "IsEnabled";
                    break;
                case DataRetrieve.EventRepeat:
                    table = DataSourceTableName.ChannelEvents.ToString();
                    criteriacolumn = "Name";
                    datacolumn = "RepeatMsg";
                    break;
            }

            DataRow[] row = null;

            lock (_DataSource)
            {
                row = _DataSource.Tables[table].Select($"{criteriacolumn}='{rowcriteria}'");
            }

            List<object> list = new();
            foreach (DataRow d in row)
            {
                list.Add(d.Field<object>(datacolumn));
            }

            return list.ToArray();
        }
        #endregion Helpers

        #region CommandSystem

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

        private readonly string DefaulSocialMsg = "Social media url here";

        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private void SetDefaultCommandsTable()
        {
            lock (_DataSource)
            {
                if (_DataSource.CategoryList.Select($"Category='{LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry)}'").Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(null, LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                    _DataSource.CategoryList.AcceptChanges();
                }

                bool CheckName(string criteria)
                {
                    CommandsRow datarow = (CommandsRow)_DataSource.Commands.Select($"CmdName='{criteria}'").FirstOrDefault();
                    if (datarow != null)
                    {
                        if (datarow.Category == string.Empty)
                        {
                            datarow.Category = LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry);
                        }
                        if (DBNull.Value.Equals(datarow["SendMsgCount"]))
                        {
                            datarow.SendMsgCount = 0;
                        }
                    }
                    return datarow == null;
                }

                // dictionary with commands, messages, and parameters
                // command name     // msg   // params
                Dictionary<string, Tuple<string, string>> DefCommandsDictionary = new();

                // add each of the default commands with localized strings
                foreach (DefaultCommand com in Enum.GetValues(typeof(DefaultCommand)))
                {
                    DefCommandsDictionary.Add(com.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(com), LocalizedMsgSystem.GetDefaultComParam(com)));
                }

                // add each of the social commands
                foreach (DefaultSocials social in Enum.GetValues(typeof(DefaultSocials)))
                {
                    DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, LocalizedMsgSystem.GetDefaultComParam("eachsocial")));
                }

                foreach (var (key, param) in from string key in DefCommandsDictionary.Keys
                                             where CheckName(key)
                                             let param = CommandParams.Parse(DefCommandsDictionary[key].Item2)
                                             select (key, param))
                {
                    _DataSource.Commands.AddCommandsRow(key, false, param.Permission.ToString(), param.IsEnabled, DefCommandsDictionary[key].Item1, param.Timer, param.RepeatMsg, param.Category, param.AllowParam, param.Usage, param.LookupData, param.Table, GetKey(param.Table), param.Field, param.Currency, param.Unit, param.Action, param.Top, param.Sort);
                }
            }
        }

        /// <summary>
        /// Check if the provided table exists within the database system.
        /// </summary>
        /// <param name="table">The table name to check.</param>
        /// <returns><i>true</i> - if database contains the supplied table, <i>false</i> - if database doesn't contain the supplied table.</returns>
        public bool CheckTable(string table)
        {
            lock (_DataSource)
            {
                return _DataSource.Tables.Contains(table);
            }
        }

        /// <summary>
        /// Check if the provided field is part of the supplied table.
        /// </summary>
        /// <param name="table">The table to check.</param>
        /// <param name="field">The field within the table to see if it exists.</param>
        /// <returns><i>true</i> - if table contains the supplied field, <i>false</i> - if table doesn't contain the supplied field.</returns>
        public bool CheckField(string table, string field)
        {
            lock (_DataSource)
            {
                return _DataSource.Tables[table].Columns.Contains(field);
            }
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><i>true</i> - the permission is allowed to the command. <i>false</i> - the command permission is not allowed.</returns>
        /// <exception cref="InvalidOperationException">The command is not found.</exception>
        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            lock (_DataSource)
            {
                CommandsRow[] rows = (CommandsRow[])_DataSource.Commands.Select($"CmdName='{cmd}'");

                if (rows != null && rows.Length > 0)
                {
                    ViewerTypes cmdpermission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), rows[0].Permission);

                    return cmdpermission >= permission;
                }
                else
                    throw new InvalidOperationException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidOpCommand));
            }
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        /// <remarks>Thread-safe</remarks>
        public bool CheckShoutName(string UserName)
        {
            lock (_DataSource)
            {
                return _DataSource.ShoutOuts.Select($"UserName='{UserName}'").Length > 0;
            }
        }

        public string GetKey(string Table)
        {
            lock (_DataSource)
            {
                string key = "";

                if (Table != "")
                {
                    DataColumn[] k = _DataSource?.Tables[Table]?.PrimaryKey;
                    if (k?.Length > 1)
                    {
                        foreach (var d in from DataColumn d in k
                                          where d.ColumnName != "Id"
                                          select d)
                        {
                            key = d.ColumnName;
                        }
                    }
                    else
                    {
                        key = k?[0].ColumnName;
                    }
                }
                return key;
            }
        }

        public string AddCommand(string cmd, CommandParams Params)
        {
            lock (_DataSource)
            {
                _DataSource.Commands.AddCommandsRow(cmd, Params.AddMe, Params.Permission.ToString(), Params.IsEnabled, Params.Message, Params.Timer, Params.RepeatMsg, Params.Category, Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, GetKey(Params.Table), Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort);
            }
            NotifySaveData();
            return string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {
            string result = "";
            lock (_DataSource)
            {
                CommandsRow commandsRow = _DataSource.Commands.FindByCmdName(cmd);
                if (commandsRow != null)
                {
                    Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);

                    foreach (string k in EditParamsDict.Keys)
                    {
                        commandsRow[k] = EditParamsDict[k];
                    }
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);
                }
                else
                {
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);
                }
            }
            NotifySaveData();
            return result;
        }

        public bool RemoveCommand(string command)
        {
            bool removed = false;

            lock (_DataSource)
            {
                if (_DataSource.Commands.FindByCmdName(command) != null)
                {
                    _DataSource.Commands.RemoveCommandsRow(_DataSource.Commands.FindByCmdName(command));
                    removed = true;
                }
            }
            NotifySaveData();
            return removed;
        }

        public string GetSocials()
        {
            string filter = "";

            System.Collections.IList list = Enum.GetValues(typeof(DefaultSocials));
            for (int i = 0; i < list.Count; i++)
            {
                DefaultSocials s = (DefaultSocials)list[i];
                filter += (i != 0 ? ", " : "") + "'" + s.ToString() + "'";
            }

            CommandsRow[] socialrows = null;
            string socials = "";

            lock (_DataSource)
            {
                socialrows = (CommandsRow[])_DataSource.Commands.Select($"CmdName IN ({filter})");
            }

            foreach (CommandsRow com in from CommandsRow com in socialrows
                                where com.Message != DefaulSocialMsg && com.Message != string.Empty
                                select com)
            {
                socials += com.Message + " ";
            }

            return socials.Trim();
        }

        public string GetUsage(string command)
        {
            lock (_DataSource)
            {
                return ((CommandsRow[])_DataSource.Commands.Select($"CmdName='{command}'"))[0]?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
            }
        }

        public CommandsRow GetCommand(string cmd)
        {
            lock (_DataSource)
            {
                return (CommandsRow)_DataSource.Commands.Select($"CmdName='{cmd}'").FirstOrDefault();
            }
        }

        public string GetCommands()
        {
            CommandsRow[] commandsRows = null;

            lock (_DataSource)
            {
                commandsRows = (CommandsRow[])_DataSource.Commands.Select($"Message <>'{DefaulSocialMsg}' AND IsEnabled=True");
            }

            string result = "";

            for (int i = 0; i < commandsRows.Length; i++)
            {
                result += (i != 0 ? ", " : "") + "!" + commandsRows[i].CmdName;
            }

            return result;
        }

        public object PerformQuery(CommandsRow row, string ParamValue)
        {
            //CommandParams query = CommandParams.Parse(row.Params);
            DataRow result = null;

            lock (_DataSource)
            {
                DataRow[] temp = _DataSource.Tables[row.table].Select($"{row.key_field}='{ParamValue}'");

                result = temp.Length > 0 ? temp[0] : null;

                if (result == null)
                {
                    return LocalizedMsgSystem.GetVar(Msg.MsgDataNotFound);
                }

                Type resulttype = result.GetType();

                // certain tables have certain outputs - still deciphering how to optimize the data query portion of commands
                if (resulttype == typeof(UsersRow))
                {
                    UsersRow usersRow = (UsersRow)result;
                    return usersRow[row.data_field];
                }
                else if (resulttype == typeof(FollowersRow))
                {
                    FollowersRow follower = (FollowersRow)result;

                    return follower.IsFollower ? follower.FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                }
                else if (resulttype == typeof(CurrencyRow))
                {

                }
                else if (resulttype == typeof(CurrencyTypeRow))
                {

                }
                else if (resulttype == typeof(CommandsRow))
                {
                    CommandsRow commandsRow = (CommandsRow)result;
                    return commandsRow[row.data_field];
                }
            }

            return result;
        }

        public object[] PerformQuery(CommandsRow row, int Top = 0)
        {
            DataTable tabledata = _DataSource.Tables[row.table]; // the table to query
            List<Tuple<object, object>> outlist = new();

            lock (_DataSource)
            {
                outlist.AddRange(from DataRow d in Top < 0 ? tabledata.Select() : tabledata.Select(null, row.key_field + " " + row.sort)
                                 select new Tuple<object, object>(d[row.key_field], d[row.data_field]));
            }

            if (Top > 0)
            {
                outlist.RemoveRange(Top, outlist.Count - Top);
            }

            outlist.Sort();

            return outlist.ToArray();
        }

        /// <summary>
        /// Retrieves the commands with a timer setting > 0 seconds.
        /// </summary>
        /// <returns>The list of commands and the seconds to repeat the command.</returns>
        public List<Tuple<string, int, string[]>> GetTimerCommands()
        {
            lock (_DataSource)
            {
                return new(from CommandsRow row in (CommandsRow[])_DataSource.Commands.Select("RepeatTimer>0 AND IsEnabled=True")
                                   select new Tuple<string, int, string[]>(row.CmdName, row.RepeatTimer, row.Category?.Split(',', StringSplitOptions.TrimEntries) ?? Array.Empty<string>()));
            }
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {
            lock (_DataSource)
            {
                CommandsRow row = (CommandsRow)_DataSource.Commands.Select($"CmdName='{Cmd}'").FirstOrDefault();

                return (row == null) ? null : new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>());
            }
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            ChannelEventsRow[] channelEventsRows = (ChannelEventsRow[])_DataSource.ChannelEvents.Select();

            foreach (ChannelEventsRow channelEventsRow in channelEventsRows)
            {
                channelEventsRow.IsEnabled = Enabled;
            }
        }

        private static string ComFilter()
        {
            string filter = string.Empty;

            foreach (DefaultCommand d in Enum.GetValues(typeof(DefaultCommand)))
            {
                filter += "'" + d.ToString() + "',";
            }

            foreach (DefaultSocials s in Enum.GetValues(typeof(DefaultSocials)))
            {
                filter += "'" + s.ToString() + "',";
            }

            return filter == string.Empty ? "" : filter[0..^1];
        }

        private void SetCommandsEnabledHelper(bool Enabled, CommandsRow[] commandsRows)
        {
            if (commandsRows.Length <= UseParallelThreashold)
            {
                foreach (CommandsRow c in commandsRows)
                {
                    c.IsEnabled = Enabled;
                }
            }
            else
            {
                _ = Parallel.ForEach(commandsRows, (commandsRow) => { commandsRow.IsEnabled = Enabled; });
            }
        }

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            SetCommandsEnabledHelper(Enabled, (CommandsRow[])_DataSource.Commands.Select("CmdName IN (" + ComFilter() + ")"));
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            SetCommandsEnabledHelper(Enabled, (CommandsRow[])_DataSource.Commands.Select("CmdName NOT IN (" + ComFilter() + ")"));
        }

        #endregion

        #region Regular Channel Events
        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private void SetDefaultChannelEventsTable()
        {
            bool CheckName(string criteria)
            {
                ChannelEventsRow channelEventsRow = _DataSource.ChannelEvents.FindByName(criteria);

                if (channelEventsRow != null && DBNull.Value.Equals(channelEventsRow["RepeatMsg"]))
                {
                    channelEventsRow.RepeatMsg = 0;
                }

                return channelEventsRow == null;
            }

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
            {
                {
                    ChannelEventActions.BeingHosted,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.autohost, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Bits,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.bits }))
                },
                {
                    ChannelEventActions.CommunitySubs,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.count, MsgVars.subplan }))
                },
                {
                    ChannelEventActions.NewFollow,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.GiftSub,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.Live,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone }))
                },
                {
                    ChannelEventActions.Raid,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Resubscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak }))
                },
                {
                    ChannelEventActions.Subscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.UserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.ReturnUserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.SupporterJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                }
            };
            lock (_DataSource)
            {
                foreach (var (command, values) in from ChannelEventActions command in Enum.GetValues(typeof(ChannelEventActions))// consider only the values in the dictionary, check if data is already defined in the data table
                                                  where dictionary.ContainsKey(command) && CheckName(command.ToString())// extract the default data from the dictionary and add to the data table
                                                  let values = dictionary[command]
                                                  select (command, values))
                {
                    _ = _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), 0, false, true, values.Item1, values.Item2);
                }
            }
        }
        #endregion Regular Channel Events

        #region Stream Statistics
        private StreamStatsRow CurrStreamStatRow;

        public StreamStatsRow[] GetAllStreamData()
        {
            return (StreamStatsRow[])_DataSource.StreamStats.Select();
        }

        private StreamStatsRow GetAllStreamData(DateTime dateTime)
        {
            lock (_DataSource)
            {
                foreach (StreamStatsRow streamStatsRow in from StreamStatsRow streamStatsRow in GetAllStreamData()
                                                          where streamStatsRow.StreamStart == dateTime
                                                          select streamStatsRow)
                {
                    return streamStatsRow;
                }
            }
            return null;
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {
            StreamStatsRow streamStatsRow = GetAllStreamData(dateTime);
            StreamStat streamStat = new();

            if (streamStatsRow != null)
            {
                // can't use a simple method to duplicate this because "ref" can't be used with boxing
                foreach (PropertyInfo property in streamStat.GetType().GetProperties())
                {
                    // use properties from 'StreamStat' since StreamStatRow has additional properties
                    property.SetValue(streamStat, streamStatsRow.GetType().GetProperty(property.Name).GetValue(streamStatsRow));
                }
            }

            return streamStat;
        }

        public bool CheckMultiStreams(DateTime dateTime)
        {
            int x = 0;
            foreach (StreamStatsRow row in GetAllStreamData())
            {
                if (row.StreamStart.ToShortDateString() == dateTime.ToShortDateString())
                {
                    x++;
                }
            }

            return x > 1;
        }

        public bool AddStream(DateTime StreamStart)
        {
            lock (_DataSource)
            {
                bool returnvalue;

                if (CheckStreamTime(StreamStart))
                {
                    returnvalue = false;
                }
                else
                {
                    if (StreamStart != DateTime.MinValue.ToLocalTime())
                    {
                        CurrStreamStart = StreamStart;

                        lock (_DataSource)
                        {
                            _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                            NotifySaveData();
                            returnvalue = true;
                        }
                    }
                    else
                    {
                        returnvalue = false;
                    }
                }
                return returnvalue;
            }
        }

        public void PostStreamStat(StreamStat streamStat)
        {
            lock (_DataSource)
            {
                CurrStreamStatRow = GetAllStreamData(streamStat.StreamStart);

                if (CurrStreamStatRow == null)
                {
                    _ = _DataSource.StreamStats.AddStreamStatsRow(StreamStart: streamStat.StreamStart, StreamEnd: streamStat.StreamEnd, NewFollows: streamStat.NewFollows, NewSubscribers: streamStat.NewSubscribers, GiftSubs: streamStat.GiftSubs, Bits: streamStat.Bits, Raids: streamStat.Raids, Hosted: streamStat.Hosted, UsersBanned: streamStat.UsersBanned, UsersTimedOut: streamStat.UsersTimedOut, ModeratorsPresent: streamStat.ModeratorsPresent, SubsPresent: streamStat.SubsPresent, VIPsPresent: streamStat.VIPsPresent, TotalChats: streamStat.TotalChats, Commands: streamStat.Commands, AutomatedEvents: streamStat.AutomatedEvents, AutomatedCommands: streamStat.AutomatedCommands, DiscordMsgs: streamStat.DiscordMsgs, ClipsMade: streamStat.ClipsMade, ChannelPtCount: streamStat.ChannelPtCount, ChannelChallenge: streamStat.ChannelChallenge, MaxUsers: streamStat.MaxUsers);
                }
                else
                {
                    // can't use a simple method to duplicate this because "ref" can't be used with boxing

                    foreach (PropertyInfo srcprop in CurrStreamStatRow.GetType().GetProperties())
                    {
                        bool found = false;
                        foreach (var _ in from PropertyInfo trgtprop in typeof(StreamStat).GetProperties()
                                          where trgtprop.Name == srcprop.Name
                                          select new { })
                        {
                            found = true;
                        }

                        if (found)
                        {
                            // use properties from 'StreamStat' since StreamStatRow has additional properties
                            srcprop.SetValue(CurrStreamStatRow, streamStat.GetType().GetProperty(srcprop.Name).GetValue(streamStat));
                        }
                    }
                }
            }
            NotifySaveData();
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        public bool CheckStreamTime(DateTime CurrTime)
        {
            return GetAllStreamData(CurrTime) != null;
        }

        /// <summary>
        /// Remove all stream stats, to satisfy a user option selection to not track stats
        /// </summary>
        public void RemoveAllStreamStats()
        {
            lock (_DataSource)
            {
                _DataSource.StreamStats.Clear();
            }
        }

        #endregion

        #region Users and Followers

        private static DateTime CurrStreamStart { get; set; }

        public void UserJoined(string User, DateTime NowSeen)
        {
            static DateTime Max(DateTime A, DateTime B)
            {
                return A <= B ? B : A;
            }

            lock (_DataSource)
            {
                UsersRow user = AddNewUser(User, NowSeen);
                user.CurrLoginDate = Max(user.CurrLoginDate, NowSeen);
                user.LastDateSeen = Max(user.LastDateSeen, NowSeen);
                NotifySaveData();
            }
        }

        public void UserLeft(string User, DateTime LastSeen)
        {
            lock (_DataSource)
            {
                UsersRow[] user = (UsersRow[])_DataSource.Users.Select($"UserName='{User}'");
                if (user != null)
                {
                    UpdateWatchTime(ref user[0], LastSeen); // will update the "LastDateSeen"
                    UpdateCurrency(ref user[0], LastSeen); // will update the "CurrLoginDate"

                    NotifySaveData();
                }
            }
        }

        public void UpdateWatchTime(ref UsersRow User, DateTime CurrTime)
        {
            if (User != null)
            {
                if (User.LastDateSeen <= CurrStreamStart)
                {
                    User.LastDateSeen = CurrStreamStart;
                }

                if (CurrTime >= User.LastDateSeen && CurrTime >= CurrStreamStart)
                {
                    User.WatchTime = User.WatchTime.Add(CurrTime - User.LastDateSeen);
                }

                User.LastDateSeen = CurrTime;

                NotifySaveData();
            }
        }

        /// <summary>
        /// Accepts the string version of a UserName and will look up the user in the database.
        /// </summary>
        /// <param name="UserName">String of the UserName to update the watchtime.</param>
        /// <param name="CurrTime">The Current Time to compare against for updating the watch time.</param>
        public void UpdateWatchTime(string UserName, DateTime CurrTime)
        {
            lock (_DataSource)
            {
                UsersRow user = (UsersRow)_DataSource.Users.Select($"UserName='{UserName}'").FirstOrDefault();
                UpdateWatchTime(ref user, CurrTime);
            }
        }

        //public void UpdateWatchTime(DateTime dateTime)
        //{
        //    // LastDateSeen ==> watchtime clock time
        //    // CurrLoginDate ==> currency clock time

        //    lock (_DataSource)
        //    {
        //        foreach (DataSource.UsersRow d in (DataSource.UsersRow[])_DataSource.Users.Select())
        //        {
        //            if (d.LastDateSeen >= CurrStreamStart)
        //            {
        //                UpdateWatchTime(d, dateTime);
        //            }
        //        }
        //    }

        //    SaveData();
        //    OnPropertyChanged(nameof(Users));
        //}

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><c>true</c> if the user has arrived prior to DateTime.MaxValue, <c>false</c> otherwise.</returns>
        public bool CheckUser(string User)
        {
            return CheckUser(User, DateTime.MaxValue);
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        public bool CheckUser(string User, DateTime ToDateTime)
        {
            lock (_DataSource)
            {
                UsersRow user = (UsersRow)_DataSource.Users.Select($"UserName='{User}'").FirstOrDefault();

                return user != null && user.FirstDateSeen <= ToDateTime;
            }
        }

        /// <summary>
        /// Check if the User is already a follower prior to now.
        /// </summary>
        /// <param name="User">The name of the user to check.</param>
        /// <returns>Returns <c>true</c> if the <paramref name="User"/> is a follower prior to DateTime.MaxValue.</returns>
        public bool CheckFollower(string User)
        {
            return CheckFollower(User, DateTime.MaxValue);
        }

        /// <summary>
        /// Check if a user is a follower at or before the current date.
        /// </summary>
        /// <param name="User">The user to query.</param>
        /// <param name="ToDateTime">The date to check FollowedDate <= <c>ToDateTime</c></param>
        /// <returns></returns>
        public bool CheckFollower(string User, DateTime ToDateTime)
        {
            lock (_DataSource)
            {
                FollowersRow datafollowers = (FollowersRow)_DataSource.Followers.Select($"UserName='{User}'").FirstOrDefault();

                return datafollowers != null && datafollowers.IsFollower && datafollowers.FollowedDate <= ToDateTime;
            }
        }

        /// <summary>
        /// Add a new follower to the data table.
        /// </summary>
        /// <param name="User">The Username of the new Follow</param>
        /// <param name="FollowedDate">The date of the Follow.</param>
        /// <returns>True if the follower is the first time. False if already followed.</returns>
        public bool AddFollower(string User, DateTime FollowedDate)
        {
            lock (_DataSource)
            {
                bool newfollow;

                UsersRow users = AddNewUser(User, FollowedDate);
                FollowersRow followers = (FollowersRow)_DataSource.Followers.Select($"UserName='{User}'").FirstOrDefault();

                if (followers != null)
                {
                    // newfollow = !followers.IsFollower;
                    newfollow = false;
                    followers.IsFollower = true;
                    followers.FollowedDate = FollowedDate;
                }
                else
                {
                    newfollow = true;
                    _DataSource.Followers.AddFollowersRow(users, users.UserName, true, FollowedDate);
                }
                NotifySaveData();
                return newfollow;
            }
        }

        /// <summary>
        /// Add a new User to the User Table
        /// </summary>
        /// <param name="User">The user name.</param>
        /// <param name="FirstSeen">The first time the user is seen.</param>
        /// <returns>True if the user is added, else false if the user already existed.</returns>
        private UsersRow AddNewUser(string User, DateTime FirstSeen)
        {
            UsersRow usersRow = null;

            lock (_DataSource)
            {
                if (!CheckUser(User))
                {
                    usersRow = _DataSource.Users.AddUsersRow(User, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero);
                    //AddCurrencyRows(ref usersRow);
                    NotifySaveData();
                }
            }

            // if the user is added to list before identified as follower, update first seen date to followed date
            lock (_DataSource)
            {
                usersRow = (UsersRow)_DataSource.Users.Select($"UserName='{User}'").First();

                if (FirstSeen <= usersRow.FirstDateSeen)
                {
                    usersRow.FirstDateSeen = FirstSeen;
                }
            }

            return usersRow;
        }


        //public event EventHandler<OnFoundNewFollowerEventArgs> OnFoundNewFollower;

        //private void InvokeFoundNewFollower(string fromUserName, DateTime followedAt)
        //{
        //    OnFoundNewFollower?.Invoke(this, new() { FromUserName = fromUserName, FollowedAt = followedAt });
        //}

        public void StartFollowers()
        {
            UpdatingFollowers = true;
            lock (_DataSource)
            {
                List<FollowersRow> temp = new();
                temp.AddRange((FollowersRow[])_DataSource.Followers.Select());
                temp.ForEach((f) => f.IsFollower = false);
            }
            NotifySaveData();
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
            if (follows.Any())
            {
                if (follows.Count() <= UseParallelThreashold)
                {
                    foreach (Follow f in follows)
                    {
                        _ = AddFollower(f.FromUserName, f.FollowedAt);
                    }
                }
                else
                {
                    _ = Parallel.ForEach(follows, (f) =>
                      {
                          _ = AddFollower(f.FromUserName, f.FollowedAt);
                      });
                }
            }

            NotifySaveData();
        }

        public void StopBulkFollows()
        {
            if (OptionFlags.TwitchPruneNonFollowers)
            {
                lock (_DataSource)
                {
                    List<FollowersRow> temp = new();
                    temp.AddRange((FollowersRow[])_DataSource.Followers.Select());
                    foreach (FollowersRow f in from FollowersRow f in temp
                                               where !f.IsFollower
                                               select f)
                    {
                        _DataSource.Followers.RemoveFollowersRow(f);
                    }
                }
            }

            NotifySaveData();
            UpdatingFollowers = false;
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            lock (_DataSource)
            {
                if (_DataSource.Users.Count <= UseParallelThreashold)
                {
                    foreach (UsersRow users in (UsersRow[])_DataSource.Users.Select())
                    {
                        users.WatchTime = new(0);
                    }
                }
                else
                {
                    _ = Parallel.ForEach((UsersRow[])_DataSource.Users.Select(), (users) => { users.WatchTime = new(0); });
                }
            }
        }

        #endregion Users and Followers

        #region Giveaways
        public void PostGiveawayData(string DisplayName, DateTime dateTime)
        {
            lock (_DataSource)
            {
                _ = _DataSource.GiveawayUserData.AddGiveawayUserDataRow(DisplayName, dateTime);
            }
            NotifySaveData();
        }

        #endregion

        #region Currency
        /// <summary>
        /// For the supplied user string, update the currency based on the supplied time to the currency accrual rates the streamer specified for the currency.
        /// </summary>
        /// <param name="User">The name of the user to find in the database.</param>
        /// <param name="dateTime">The time to base the currency calculation.</param>
        public void UpdateCurrency(string User, DateTime dateTime)
        {
            UsersRow user = _DataSource.Users.FindByUserName(User);
            UpdateCurrency(ref user, dateTime);
            NotifySaveData();
        }

        /// <summary>
        /// Process currency accruals per user, if currency type is defined, otherwise currency accruals are ignored. Afterward, the 'CurrLoginDate' is updated.
        /// </summary>
        /// <param name="User">The user to evaluate.</param>
        /// <param name="CurrTime">The time to update and accrue the currency.</param>
        public void UpdateCurrency(ref UsersRow User, DateTime CurrTime)
        {
            lock (_DataSource)
            {
                if (User != null)
                {
                    TimeSpan currencyclock = CurrTime - User.CurrLoginDate; // the amount of time changed for the currency accrual calculation

                    double ComputeCurrency(double Accrue, double Seconds)
                    {
                        return Math.Round(Accrue * (currencyclock.TotalSeconds / Seconds), 2);
                    }

                    AddCurrencyRows(ref User);

                    CurrencyTypeRow[] currencyType = (CurrencyTypeRow[])_DataSource.CurrencyType.Select();
                    CurrencyRow[] userCurrency = (CurrencyRow[])_DataSource.Currency.Select($"Id='{User.Id}'");

                    foreach (var (typeRow, currencyRow) in currencyType.SelectMany(typeRow => userCurrency.Where(currencyRow => currencyRow.CurrencyName == typeRow.CurrencyName).Select(currencyRow => (typeRow, currencyRow))))
                    {
                        currencyRow.Value = Math.Min(currencyRow.Value + ComputeCurrency(typeRow.AccrueAmt, typeRow.Seconds), typeRow.MaxValue);
                    }

                    // set the current login date, always set regardless if currency accrual is started
                    User.CurrLoginDate = CurrTime;
                }
            }
            return;
        }

        /// <summary>
        /// Update the currency accrual for the specified user, add all currency rows per the user.
        /// </summary>
        /// <param name="usersRow">The user row containing data for creating new rows depending if the currency doesn't have a row for each currency type.</param>
        public void AddCurrencyRows(ref UsersRow usersRow)
        {
            lock (_DataSource)
            {
                CurrencyTypeRow[] currencyTypeRows = (CurrencyTypeRow[])_DataSource.CurrencyType.Select();
                if (usersRow != null)
                {
                    CurrencyRow[] currencyRows = (CurrencyRow[])_DataSource.Currency.Select($"UserName='{usersRow.UserName}'");
                    foreach (CurrencyTypeRow typeRow in currencyTypeRows)
                    {
                        bool found = false;
                        foreach (CurrencyRow CR in currencyRows)
                        {
                            if (CR.CurrencyName == typeRow.CurrencyName)
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            _DataSource.Currency.AddCurrencyRow(usersRow.Id, usersRow, typeRow, 0);
                        }
                    }
                }
            }
            NotifySaveData();
        }

        /// <summary>
        /// For every user in the database, add currency rows for each currency type - add missing rows.
        /// </summary>
        public void AddCurrencyRows()
        {
            lock (_DataSource)
            {
                UsersRow[] UserRows = (UsersRow[])_DataSource.Users.Select();

                if (UserRows.Length <= UseParallelThreashold)
                {
                    for (int i = 0; i < UserRows.Length; i++)
                    {
                        UsersRow users = UserRows[i];
                        AddCurrencyRows(ref users);
                    }
                }
                else
                {
                    _ = Parallel.For(0, UserRows.Length, (i) =>
                       {
                           AddCurrencyRows(ref UserRows[i]);
                       });
                }
            }
            NotifySaveData();
        }

        /// <summary>
        /// Empty every currency to 0, for all users for all currencies.
        /// </summary>
        public void ClearAllCurrencyValues()
        {
            lock (_DataSource)
            {
                if (_DataSource.Currency.Count <= UseParallelThreashold)
                {
                    foreach (CurrencyRow row in (CurrencyRow[])_DataSource.Currency.Select())
                    {
                        row.Value = 0;
                    }
                }
                else
                {
                    _ = Parallel.ForEach((CurrencyRow[])_DataSource.Currency.Select(), (row) =>
                      {
                          row.Value = 0;
                      });
                }
            }
        }

        #endregion

        #region Raid Data
        public void PostInRaidData(string user, DateTime time, string viewers, string gamename)
        {
            lock (_DataSource)
            {
                _ = _DataSource.InRaidData.AddInRaidDataRow(user, viewers, time, gamename);
            }
            NotifySaveData();
        }

        public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
        {
            lock (_DataSource)
            {
                // 2021-12-06T01:19:16.0248427-05:00
                //string.Format("UserName='{0}' and DateTime='{1}' and ViewerCount='{2}' and Category='{3}'", user, time, viewers, gamename)
                return (InRaidDataRow)_DataSource.InRaidData.Select($"UserName='{user}' AND DateTime=#{time:O}# AND ViewerCount='{viewers}' AND Category='{gamename}'").FirstOrDefault() != null;
            }
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            lock (_DataSource)
            {
                // string.Format("ChannelRaided='{0}' and DateTime='{1}'", HostedChannel, dateTime)
                return (OutRaidDataRow)_DataSource.OutRaidData.Select($"ChannelRaided='{HostedChannel}' AND DateTime=#{dateTime:O}#").FirstOrDefault() != null;
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
            lock (_DataSource)
            {
                _ = _DataSource.OutRaidData.AddOutRaidDataRow(HostedChannel, dateTime);
            }
            NotifySaveData();
        }

        #endregion

        #region Discord and Webhooks
        /// <summary>
        /// Retrieve all the webhooks from the Discord table
        /// </summary>
        /// <returns></returns>
        public List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks)
        {
            lock (_DataSource)
            {
                return new(((DiscordRow[])_DataSource.Discord.Select()).Where(d => d.Kind == webhooks.ToString() && d.IsEnabled == true).Select(d => new Tuple<bool, Uri>(d.AddEveryone, new Uri(d.Webhook))));
            }
        }

        #endregion Discord and Webhooks

        #region Category

        /// <summary>
        /// Checks for the supplied category in the category list, adds if it isn't already saved.
        /// </summary>
        /// <param name="CategoryId">The ID of the stream category.</param>
        /// <param name="newCategory">The category to add to the list if it's not available.</param>
        /// <returns>True if category OR game ID are found; False if no category nor game ID is found.</returns>
        public bool AddCategory(string CategoryId, string newCategory)
        {
            bool found = true;
            lock (_DataSource)
            {
                CategoryListRow categoryList = (CategoryListRow)_DataSource.CategoryList.Select($"Category='{newCategory.Replace("'", "''")}' OR CategoryId='{CategoryId}'").FirstOrDefault();

                if (categoryList == null)
                {
                    _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory);
                    found = false;
                }
                else if (categoryList.CategoryId == null)
                {
                    categoryList.CategoryId = CategoryId;
                }
                else if (categoryList.Category == null)
                {
                    categoryList.Category = newCategory;
                }
            }

            NotifySaveData();
            return found;
        }

        /// <summary>
        /// Retrieves all GameIds and GameCategories added to the database.
        /// </summary>
        /// <returns>Returns a list of <code>Tuple<string GameId, string GameName></code> objects.</returns>
        public List<Tuple<string, string>> GetGameCategories()
        {
            return new(from CategoryListRow c in _DataSource.CategoryList.Select()
                       let item = new Tuple<string, string>(c.CategoryId, c.Category)
                       select item);
        }

        #endregion

        #region Clips

        /// <summary>
        /// Add a clip to the database.
        /// </summary>
        /// <param name="ClipId">The already assigned ID number</param>
        /// <param name="CreatedAt">Time clip created</param>
        /// <param name="Duration">The duration of the clip</param>
        /// <param name="GameId">The ID of the Game in the clip</param>
        /// <param name="Language">The channel language for the clip</param>
        /// <param name="Title">The clip title a viewer assigned the clip</param>
        /// <param name="Url">The URL to reach the clip</param>
        /// <returns><c>true</c> when clip added to database, <c>false</c> when clip is already added.</returns>
        public bool AddClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {
            bool result;

            lock (_DataSource)
            {
                ClipsRow[] clipsRows = (ClipsRow[])_DataSource.Clips.Select($"Id='{ClipId}'");

                if (clipsRows.Length == 0)
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime().ToString(), Title, GameId, Language, (decimal)Duration, Url);
                    NotifySaveData();
                    result = true;
                }
                result = false;
            }

            return result;
        }

        #endregion

        #region Remove Data
        /// <summary>
        /// Remove all Users from the database.
        /// </summary>
        public void RemoveAllUsers()
        {
            lock (_DataSource)
            {
                _DataSource.Users.Clear();
            }
            NotifySaveData();
        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        public void RemoveAllFollowers()
        {
            lock (_DataSource)
            {
                _DataSource.Followers.Clear();
            }
            NotifySaveData();
        }

        /// <summary>
        /// Removes all incoming raid data from database.
        /// </summary>
        public void RemoveAllInRaidData()
        {
            lock (_DataSource)
            {
                _DataSource.InRaidData.Clear();
            }
            NotifySaveData();
        }

        /// <summary>
        /// Removes all outgoing raid data from database.
        /// </summary>
        public void RemoveAllOutRaidData()
        {
            lock (_DataSource)
            {
                _DataSource.OutRaidData.Clear();
            }
            NotifySaveData();
        }

        /// <summary>
        /// Removes all Giveaway table data from the database.
        /// </summary>
        public void RemoveAllGiveawayData()
        {
            lock (_DataSource)
            {
                _DataSource.GiveawayUserData.Clear();
            }
            NotifySaveData();
        }

        #endregion

    }
}
