using StreamerBot.Enum;
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

                using (XmlReader xmlreader = new XmlTextReader(DataFileName))
                {
                    _ = _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);
                }
            }

            SaveData(this, new());
        }

        public void Initialize()
        {
            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands
        }

        /// <summary>
        /// Provide an internal notification event to save the data outside of any multi-threading mechanisms.
        /// </summary>
        public event EventHandler OnSaveData;
        private void NotifySaveData()
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
                    _DataSource.AcceptChanges();

                    lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                    {
                        SaveTasks.Enqueue(new(() =>
                        {
                            lock (_DataSource)
                            {
                                string result = Path.GetRandomFileName();
                                try
                                {
                                    _DataSource.WriteXml(result, XmlWriteMode.DiffGram);

                                    DataSource testinput = new();

                                    XmlReader xmlReader = new XmlTextReader(result);
                                    // test load
                                    _ = testinput.ReadXml(xmlReader, XmlReadMode.DiffGram);
                                    xmlReader.Close();

                                    File.Move(result, DataFileName, true);
                                    File.Delete(result);
                                }
                                catch (Exception ex)
                                {
                                    LogWriter.LogException(ex, MethodBase.GetCurrentMethod().Name);
                                    File.Delete(result);
                                }
                            }
                        }));
                    }
                }
            }
        }

        private void PerformSaveOp()
        {
            if (OptionFlags.BotStarted) // don't sleep if exiting app
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
            }

            DataRow[] row = null;

            lock (_DataSource)
            {
                row = _DataSource.Tables[table].Select(criteriacolumn + "='" + rowcriteria.ToString() + "'");
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
-param:<allow params to command>
-timer:<seconds>
-use:<usage message>

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
                if (_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'").Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(null, LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                    _DataSource.CategoryList.AcceptChanges();
                }

                CategoryListRow categoryListRow = (CategoryListRow)_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'")[0];

                bool CheckName(string criteria)
                {
                    CommandsRow[] datarow = (CommandsRow[])_DataSource.Commands.Select("CmdName='" + criteria + "'");
                    if (datarow.Length > 0 && datarow[0].Category == string.Empty)
                    {
                        datarow[0].Category = LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry);
                    }
                    return datarow.Length == 0;
                }

                // TODO: convert commands table to localized strings, except the query parameters should stay in English

                // command name     // msg   // params  
                Dictionary<string, Tuple<string, string>> DefCommandsDictionary = new()
                {
                    { DefaultCommand.addcommand.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), "-p:Mod -use:!addcommand command <switches-optional> <message>. See documentation for <switches>.") },
                    // '-top:-1' means all items
                    { DefaultCommand.commands.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.commands), "-t:Commands -f:CmdName -top:-1 -s:ASC -use:!commands") },
                    { DefaultCommand.bot.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.bot), "-use:!bot") },
                    { DefaultCommand.lurk.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.lurk), "-use:!lurk") },
                    { DefaultCommand.worklurk.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.worklurk), "-use:!worklurk") },
                    { DefaultCommand.unlurk.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.unlurk), "-use:!unlurk") },
                    { DefaultCommand.socials.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.socials), "-use:!socials") },
                    { DefaultCommand.so.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.so), "-p:Mod -param:true -use:!so username - only mods can use !so.") },
                    { DefaultCommand.join.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.join), "-use:!join") },
                    { DefaultCommand.leave.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.leave), "-use:!leave") },
                    { DefaultCommand.queue.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.queue), "-p:Mod -use:!queue mods only") },
                    { DefaultCommand.qinfo.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qinfo), "-use:!qinfo") },
                    { DefaultCommand.qstart.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstart), "-p:Mod -use:!qstart mod only") },
                    { DefaultCommand.qstop.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.qstop), "-p:Mod -use:!qstop mod only") },
                    { DefaultCommand.follow.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.follow), "-use:!follow") },
                    { DefaultCommand.watchtime.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.watchtime), "-t:Users -f:WatchTime -param:true -use:!watchtime or !watchtime <user>") },
                    { DefaultCommand.uptime.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.uptime), "-use:!uptime") },
                    { DefaultCommand.followage.ToString(), new(LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.followage), "-t:Followers -f:FollowedDate -param:true -use:!followage or !followage <user>") }
                };

                foreach (DefaultSocials social in System.Enum.GetValues(typeof(DefaultSocials)))
                {
                    DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, "-use:!<social_name> -top:0"));
                }

                foreach (string key in DefCommandsDictionary.Keys)
                {
                    if (CheckName(key))
                    {
                        CommandParams param = CommandParams.Parse(DefCommandsDictionary[key].Item2);
                        _DataSource.Commands.AddCommandsRow(key, false, param.Permission.ToString(), DefCommandsDictionary[key].Item1, param.Timer, categoryListRow, param.AllowParam, param.Usage, param.LookupData, param.Table, GetKey(param.Table), param.Field, param.Currency, param.Unit, param.Action, param.Top, param.Sort);
                    }
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
                CommandsRow[] rows = (CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");

                if (rows != null && rows.Length > 0)
                {
                    ViewerTypes cmdpermission = (ViewerTypes)System.Enum.Parse(typeof(ViewerTypes), rows[0].Permission);

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
                return _DataSource.ShoutOuts.Select("UserName='" + UserName + "'").Length > 0;
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
                        foreach (DataColumn d in k)
                        {
                            if (d.ColumnName != "Id")
                            {
                                key = d.ColumnName;
                            }
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
                //string strParams = Params.DBParamsString();
                CategoryListRow categoryListRow = (CategoryListRow)_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'")[0];


                _DataSource.Commands.AddCommandsRow(cmd, Params.AddMe, Params.Permission.ToString(), Params.Message, Params.Timer, categoryListRow, Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, GetKey(Params.Table), Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort);
                NotifySaveData();
            }
            return string.Format(CultureInfo.CurrentCulture, "Command {0} added!", cmd);
        }

        public string GetSocials()
        {
            string filter = "";

            foreach (DefaultSocials s in System.Enum.GetValues(typeof(DefaultSocials)))
            {
                filter += "'" + s.ToString() + "',";
            }

            CommandsRow[] socialrows = null;
            lock (_DataSource)
            {
                socialrows = (CommandsRow[])_DataSource.Commands.Select("CmdName='" + LocalizedMsgSystem.GetVar(DefaultCommand.socials) + "'");
            }

            string socials = socialrows[0].Message;

            if (OptionFlags.MsgPerComMe && socialrows[0].AddMe == true)
            {
                socials = "/me " + socialrows[0].Message;
            }

            lock (_DataSource)
            {
                socialrows = (CommandsRow[])_DataSource.Commands.Select("CmdName IN (" + filter[0..^1] + ")");
            }

            foreach (CommandsRow com in socialrows)
            {
                if (com.Message != DefaulSocialMsg && com.Message != string.Empty)
                {
                    socials += com.Message + " ";
                }
            }

            return socials.Trim();
        }

        public string GetUsage(string command)
        {
            lock (_DataSource)
            {
                CommandsRow[] usagerows = (CommandsRow[])_DataSource.Commands.Select("CmdName='" + command + "'");

                return usagerows[0]?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
            }
        }

        // older code
        //public string PerformCommand(string cmd, string InvokedUser, string ParamUser, List<string> ParamList=null)
        //{
        //    DataSource.CommandsRow[] comrow = null;

        //    lock (_DataSource)
        //    {
        //        comrow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");
        //    }

        //    if (comrow == null || comrow.Length == 0)
        //    {
        //        throw new KeyNotFoundException( "Command not found." );
        //    }

        //    //object[] value = comrow[0].Params != string.Empty ? PerformQuery(comrow[0], InvokedUser, ParamUser) : null;

        //    string user = (comrow[0].AllowParam ? ParamUser : InvokedUser);
        //    if (user.Contains('@'))
        //    {
        //        user = user.Remove(0,1);
        //    }

        //    Dictionary<string, string> datavalues = new()
        //    {
        //        { "#user", user },
        //        { "#url", "http://www.twitch.tv/" + user }
        //    };

        //    return BotController.ParseReplace(comrow[0].Message, datavalues);
        //}

        public CommandsRow GetCommand(string cmd)
        {
            CommandsRow[] comrow = null;

            lock (_DataSource)
            {
                comrow = (CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");
            }

            //if (comrow == null || comrow.Length == 0)
            //{
            //    throw new KeyNotFoundException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound));
            //}

            return comrow?[0];
        }

        public object PerformQuery(CommandsRow row, string ParamValue)
        {
            //CommandParams query = CommandParams.Parse(row.Params);
            DataRow result = null;

            lock (_DataSource)
            {
                DataRow[] temp = _DataSource.Tables[row.table].Select(row.key_field + "='" + ParamValue + "'");

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

                }
            }

            return result;
        }

        public object[] PerformQuery(CommandsRow row, int Top = 0)
        {
            DataTable tabledata = _DataSource.Tables[row.table]; // the table to query
            DataRow[] output;
            List<Tuple<object, object>> outlist = new();

            lock (_DataSource)
            {
                output = Top < 0 ? tabledata.Select() : tabledata.Select(null, row.key_field + " " + row.sort);

                foreach (DataRow d in output)
                {
                    outlist.Add(new(d[row.key_field], d[row.data_field]));
                }
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
                List<Tuple<string, int, string[]>> TimerList = new();
                foreach (CommandsRow row in (CommandsRow[])_DataSource.Commands.Select("RepeatTimer>0"))
                {
                    TimerList.Add(new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>()));
                }
                return TimerList;
            }
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {
            lock (_DataSource)
            {
                CommandsRow[] row = (CommandsRow[])_DataSource.Commands.Select("CmdName='" + Cmd + "'");

                return row == null ? null : new(row[0].CmdName, row[0].RepeatTimer, row[0].Category?.Split(',') ?? Array.Empty<string>());
            }
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
                return _DataSource.ChannelEvents.FindByName(criteria) == null;
            }

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
            {
                {
                    ChannelEventActions.BeingHosted,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.BeingHosted, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.autohost, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Bits,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Bits, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.bits }))
                },
                {
                    ChannelEventActions.CommunitySubs,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.CommunitySubs, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.count, MsgVars.subplan }))
                },
                {
                    ChannelEventActions.NewFollow,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.NewFollow, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.GiftSub,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.GiftSub, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.receiveuser, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.Live,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Live, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.category, MsgVars.title, MsgVars.url, MsgVars.everyone }))
                },
                {
                    ChannelEventActions.Raid,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Raid, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.viewers }))
                },
                {
                    ChannelEventActions.Resubscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Resubscribe, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.months, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname, MsgVars.streak }))
                },
                {
                    ChannelEventActions.Subscribe,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.Subscribe, out _), VariableParser.ConvertVars(new[] { MsgVars.user, MsgVars.submonths, MsgVars.subplan, MsgVars.subplanname }))
                },
                {
                    ChannelEventActions.UserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.UserJoined, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.ReturnUserJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.ReturnUserJoined, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                },
                {
                    ChannelEventActions.SupporterJoined,
                    new(LocalizedMsgSystem.GetEventMsg(ChannelEventActions.SupporterJoined, out _), VariableParser.ConvertVars(new[] { MsgVars.user }))
                }
            };
            lock (_DataSource)
            {
                foreach (ChannelEventActions command in System.Enum.GetValues(typeof(ChannelEventActions)))
                {
                    // consider only the values in the dictionary, check if data is already defined in the data table
                    if (dictionary.ContainsKey(command) && CheckName(command.ToString()))
                    {   // extract the default data from the dictionary and add to the data table
                        Tuple<string, string> values = dictionary[command];

                        _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), false, true, values.Item1, values.Item2);

                    }
                }

                _DataSource.ChannelEvents.AcceptChanges();
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

        public void PostStreamStat(ref StreamStat streamStat)
        {
            lock (_DataSource)
            {
                CurrStreamStatRow = GetAllStreamData(streamStat.StreamStart);

                if (CurrStreamStatRow == null)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(streamStat.StreamStart, streamStat.StreamEnd, streamStat.NewFollows, streamStat.NewSubscribers, streamStat.GiftSubs, streamStat.Bits, streamStat.Raids, streamStat.Hosted, streamStat.UsersBanned, streamStat.UsersTimedOut, streamStat.ModeratorsPresent, streamStat.SubsPresent, streamStat.VIPsPresent, streamStat.TotalChats, streamStat.Commands, streamStat.AutomatedEvents, streamStat.AutomatedCommands, streamStat.DiscordMsgs, streamStat.ClipsMade, streamStat.ChannelPtCount, streamStat.ChannelChallenge, streamStat.MaxUsers);
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
            lock (_DataSource)
            {
                UsersRow user = AddNewUser(User, NowSeen);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;
                NotifySaveData();
            }
        }

        public void UserLeft(string User, DateTime LastSeen)
        {
            lock (_DataSource)
            {
                UsersRow[] user = (UsersRow[])_DataSource.Users.Select("UserName='" + User + "'");
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
                UsersRow[] user = (UsersRow[])_DataSource.Users.Select("UserName='" + UserName + "'");
                UpdateWatchTime(ref user[0], CurrTime);
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
                UsersRow[] user = (UsersRow[])_DataSource.Users.Select("UserName='" + User + "'");

                return (user.Length > 0) ? user[0]?.FirstDateSeen <= ToDateTime : false;
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
                FollowersRow datafollowers = (FollowersRow)_DataSource.Followers.Select("UserName='" + User + "'").FirstOrDefault();

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
                bool newfollow = false;

                UsersRow users = AddNewUser(User, FollowedDate);

                DataRow[] datafollowers = _DataSource.Followers.Select("UserName='" + User + "'");
                FollowersRow followers = datafollowers.Length > 0 ? (FollowersRow)datafollowers[0] : null;
                if (followers != null)
                {
                    newfollow = !followers.IsFollower;
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
                usersRow = (UsersRow)_DataSource.Users.Select("UserName='" + User + "'")[0];

                if (DateTime.Compare(usersRow.FirstDateSeen, FirstSeen) > 0)
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

        public void UpdateFollowers( IEnumerable<Follow> follows)
        {
            //new Thread(new ThreadStart(() =>
            //{
            UpdatingFollowers = true;
            lock (_DataSource)
            {
                List<FollowersRow> temp = new();
                temp.AddRange((FollowersRow[])_DataSource.Followers.Select());
                temp.ForEach((f) => f.IsFollower = false);
            }
            if (follows.Any())
            {
                foreach (Follow f in follows)
                {
                    AddFollower(f.FromUserName, f.FollowedAt);
                }
            }

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

            UpdatingFollowers = false;
            NotifySaveData();
            //})).Start();
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            lock (_DataSource)
            {
                foreach (UsersRow users in _DataSource.Users.Select())
                {
                    users.WatchTime = new(0);
                }
            }
        }

        #endregion Users and Followers

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
                        return Accrue * (currencyclock.TotalSeconds / Seconds);
                    }

                    AddCurrencyRows(ref User);

                    CurrencyTypeRow[] currencyType = (CurrencyTypeRow[])_DataSource.CurrencyType.Select();
                    CurrencyRow[] userCurrency = (CurrencyRow[])_DataSource.Currency.Select("Id='" + User.Id + "'");

                    foreach (var (typeRow, currencyRow) in currencyType.SelectMany(typeRow => userCurrency.Where(currencyRow => currencyRow.CurrencyName == typeRow.CurrencyName).Select(currencyRow => (typeRow, currencyRow))))
                    {
                        currencyRow.Value += ComputeCurrency(typeRow.AccrueAmt, typeRow.Seconds);
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
                    CurrencyRow[] currencyRows = (CurrencyRow[])_DataSource.Currency.Select("UserName='" + usersRow.UserName + "'");
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
                System.Data.DataRow[] UserRows = _DataSource.Users.Select();
                for (int i = 0; i < UserRows.Length; i++)
                {
                    UsersRow users = (UsersRow)UserRows[i];
                    AddCurrencyRows(ref users);
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
                foreach (CurrencyRow row in _DataSource.Currency.Select())
                {
                    row.Value = 0;
                }
            }
        }

        #endregion

        #region Raid Data
        public void AddRaidData(string user, DateTime time, string viewers, string gamename)
        {
            _ = _DataSource.RaidData.AddRaidDataRow(user, viewers, time, gamename);
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
                DataRow[] dataRows = _DataSource.Discord.Select();

                List<Tuple<bool, Uri>> uris = new();

                foreach (DataRow d in dataRows)
                {
                    DiscordRow row = d as DiscordRow;

                    if (row.Kind == webhooks.ToString())
                    {
                        uris.Add(new Tuple<bool, Uri>(row.AddEveryone, new Uri(row.Webhook)));
                    }
                }
                return uris;
            }
        }
        #endregion Discord and Webhooks

        #region Category

        /// <summary>
        /// Checks for the supplied category in the category list, adds if it isn't already saved.
        /// </summary>
        /// <param name="newCategory">The category to add to the list if it's not available.</param>
        public void UpdateCategory(string CategoryId, string newCategory)
        {
            lock (_DataSource)
            {
                CategoryListRow[] categoryList = (CategoryListRow[])_DataSource.CategoryList.Select("Category='" + newCategory.Replace("'", "''") + "'");

                if (categoryList.Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory);
                }
                else if (categoryList[0].CategoryId == null)
                {
                    categoryList[0].CategoryId = CategoryId;
                }
            }

            NotifySaveData();
        }
        #endregion

        #region Clips

        public bool AddClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {
            lock (_DataSource)
            {
                ClipsRow[] clipsRows = (ClipsRow[])_DataSource.Clips.Select("Id='" + ClipId + "'");

                if (clipsRows.Length == 0)
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime().ToString(), Title, GameId, Language, (decimal)Duration, Url);
                    NotifySaveData();
                    return true;
                }
                return false;
            }
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
        }

        public void RemoveAllRaidData()
        {
            _DataSource.RaidData.Clear();
        }

        #endregion
    }
}
