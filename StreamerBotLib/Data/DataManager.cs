
using StreamerBotLib.Data.DataSetCommonMethods;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.MLearning;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Overlay.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System.Data;
using System.Globalization;
using System.Reflection;

using static StreamerBotLib.Data.DataSetCommonMethods.DataSetStatic;
using static StreamerBotLib.Data.DataSource;

namespace StreamerBotLib.Data
{

    /*
            lock (GUIDataManagerLock.Lock)
            {
                using var context = dbContextFactory.CreateDbContext();

            }

    */
    public partial class DataManager //: IDataManager
    {
        #region DataSource
        /// <summary>
        /// Specifies the database xml save file name
        /// </summary>
        private static readonly string DataFileXML = "ChatDataStore.xml";

        /// <summary>
        /// The database structured data, with supporting data typing code calls.
        /// </summary>
        internal readonly DataSource _DataSource;
        #endregion DataSource

        /// <summary>
        /// always true to begin one learning cycle
        /// </summary>
        private bool LearnMsgChanged = true;

        /// <summary>
        /// When the follower bot begins a bulk follower update, this flag 'locks' the database Follower table from changes until bulk update concludes.
        /// </summary>
        public bool UpdatingFollowers { get; set; }

        public DataManager() : base(DataFileXML)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, "Build DataManager object.");
            _DataSource = new();
            _DataSource.BeginInit();
            LoadData();
            _DataSource.EndInit();
            OnSaveData += SaveData;

            _DataSource.LearnMsgs.TableNewRow += LearnMsgs_TableNewRow;
            _DataSource.LearnMsgs.LearnMsgsRowDeleted += LearnMsgs_LearnMsgsRowDeleted;
        }

        #region Channel Events
        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>Null for no value or the first row found using the <i>rowcriteria</i></returns>
        public string GetEventRowData(ChannelEventActions rowcriteria, out bool Enabled, out short Multi)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get event row data for {rowcriteria}.");

            string Msg = "";

            lock (GUIDataManagerLock.Lock)
            {
                ChannelEventsRow channelEventsRow = (ChannelEventsRow)GetRow(_DataSource.ChannelEvents, $"{_DataSource.ChannelEvents.NameColumn.ColumnName}='{rowcriteria}'");

                if (channelEventsRow != null)
                {
                    Multi = channelEventsRow.RepeatMsg;
                    Enabled = channelEventsRow.IsEnabled;
                    Msg = channelEventsRow.Message;
                }
                else
                {
                    Msg = null;
                    Multi = 0;
                    Enabled = false;
                }
            }
            return Msg;
        }

        #endregion

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

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><i>true</i> - the permission is allowed to the command. <i>false</i> - the command permission is not allowed.</returns>
        /// <exception cref="InvalidOperationException">The command is not found.</exception>
        public bool CheckPermission(string cmd, ViewerTypes permission)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check permission for {cmd}.");

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow row = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{cmd}'");

                if (row != null)
                {
                    ViewerTypes cmdpermission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), row.Permission);

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

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if {UserName} is in the Shout list.");

            lock (GUIDataManagerLock.Lock)
            {
                return GetRow(_DataSource.ShoutOuts, $"{_DataSource.ShoutOuts.UserNameColumn.ColumnName}='{UserName}'") != null;
            }
        }

        public string PostCommand(string cmd, CommandParams Params)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add a new command called {cmd}.");


            lock (GUIDataManagerLock.Lock)
            {
                _DataSource.Commands.AddCommandsRow(cmd, Params.AddMe, Params.Permission.ToString(), Params.IsEnabled, Params.Message, Params.Timer, Params.RepeatMsg, Params.Category, Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, DataSetStatic.GetKey(_DataSource.Tables[Params.Table], Params.Table), Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort);

                _DataSource.Commands.AcceptChanges();
            }
            NotifySaveData();
            return string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Editing command {cmd}.");


            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow commandsRow = (CommandsRow)GetRow(_DataSource.Commands, cmd);
                if (commandsRow != null)
                {
                    Dictionary<string, string> EditParamsDict = CommandParams.ParseEditCommandParams(Arglist);

                    foreach (string k in EditParamsDict.Keys)
                    {
                        commandsRow[k] = EditParamsDict[k];
                    }
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.editcommand), cmd);

                    _DataSource.Commands.AcceptChanges();
                    NotifySaveData();
                }
                else
                {
                    result = string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetVar("Msgcommandnotfound"), cmd);
                }
            }
            return result;
        }

        /// <summary>
        /// Remove the specified command.
        /// </summary>
        /// <param name="command">The "CmdName" of the command to remove.</param>
        /// <returns><c>True</c> with successful removal, <c>False</c> with command not found.</returns>
        public bool RemoveCommand(string command)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Removing command {command}.");


            return DeleteDataRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{command}'");
        }

        public List<string> GetSocialComs()
        {
            List<string> Coms = [];
            string filter = "";

            System.Collections.IList list = Enum.GetValues(typeof(DefaultSocials));
            for (int i = 0; i < list.Count; i++)
            {
                DefaultSocials s = (DefaultSocials)list[i];
                filter += $"{(i != 0 ? ", " : "")}'{s}'";
            }

            lock (GUIDataManagerLock.Lock)
            {
                foreach (string Command in from CommandsRow com in GetRows(_DataSource.Commands, $"CmdName IN ({filter})")
                                           where com.Message != DefaulSocialMsg && com.Message != string.Empty
                                           select com.CmdName)
                {
                    Coms.Add(Command);
                }
            }

            return Coms;
        }

        /// <summary>
        /// Retrieves all of the non-default social messages.
        /// </summary>
        /// <returns>Searches each social message and combines each social message which isn't the default message.</returns>
        public string GetSocials()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, "Getting the socials listing.");


            string filter = "";

            System.Collections.IList list = Enum.GetValues(typeof(DefaultSocials));
            for (int i = 0; i < list.Count; i++)
            {
                DefaultSocials s = (DefaultSocials)list[i];
                filter += $"{(i != 0 ? ", " : "")}'{s}'";
            }

            string socials = "";

            lock (GUIDataManagerLock.Lock)
            {
                foreach (CommandsRow com in from CommandsRow com in GetRows(_DataSource.Commands, $"CmdName IN ({filter})")
                                            where com.Message != DefaulSocialMsg && com.Message != string.Empty
                                            select com)
                {
                    socials += com.Message + " ";
                }
            }

            return socials.Trim();
        }

        public string GetUsage(string command)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get the usage information for {command}.");


            return GetCommand(command)?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
        }

        public CommandData GetCommand(string cmd)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get the command row for {cmd}.");

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow comrow = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{cmd}'");

                return comrow != null ? new(comrow) : null;
            }
        }

        public string GetCommands()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get a list of all commands.");

            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow[] commandsRows = (CommandsRow[])GetRows(_DataSource.Commands, $"{_DataSource.Commands.MessageColumn.ColumnName} <>'{DefaulSocialMsg}' AND {_DataSource.Commands.IsEnabledColumn.ColumnName}=True", $"{_DataSource.Commands.CmdNameColumn.ColumnName} ASC");

                for (int i = 0; i < commandsRows.Length; i++)
                {
                    result += (i != 0 ? ", " : "") + "!" + commandsRows[i].CmdName;
                }
            }

            return result;
        }

        public IEnumerable<string> GetCommandList()
        {
            return GetCommands().Split(", ");
        }

        public object PerformQuery(CommandData row, string ParamValue)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Perform the query for command {row.CmdName}.");


            object output;
            //CommandParams query = CommandParams.Parse(row.Params);

            lock (GUIDataManagerLock.Lock)
            {
                string Currency = "";
                if (row.Table == _DataSource.Currency.TableName)
                {
                    Currency = $" AND {_DataSource.Currency.CurrencyNameColumn.ColumnName}='{row.Currency_field}'";
                }

                DataRow result = GetRows(_DataSource.Tables[row.Table], $"{row.Key_field}='{ParamValue}'{Currency}").FirstOrDefault();

                if (result == null)
                {
                    output = LocalizedMsgSystem.GetVar(Msg.MsgDataNotFound);
                }
                else
                {
                    if (result.GetType() == typeof(FollowersRow))
                    {
                        FollowersRow follower = (FollowersRow)result;
                        output = follower.IsFollower ? follower.FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                    }
                    else
                    {
                        output = result[row.Data_field];
                    }
                }
            }

            return output;
        }

        public object[] PerformQuery(CommandData row, int Top = 0)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Perform the multi object query for command {row.CmdName}.");

            string Currency = "";
            if (row.Table == _DataSource.Currency.TableName)
            {
                Currency = $" {_DataSource.Currency.CurrencyNameColumn.ColumnName}='{row.Currency_field}'";
            }

            List<Tuple<object, object>> outlist = null;
            lock (GUIDataManagerLock.Lock)
            {
                outlist = new(from DataRow d in GetRows(_DataSource.Tables[row.Table], Filter: Currency, Sort: Top < 0 ? null : row.Key_field + " " + row.Sort)
                              select new Tuple<object, object>(d[row.Key_field], d[row.Data_field]));
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

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get all the timer commands.");

            lock (GUIDataManagerLock.Lock)
            {
                return new(from CommandsRow row in (CommandsRow[])GetRows(_DataSource.Commands, "RepeatTimer>0 AND IsEnabled=True")
                           select new Tuple<string, int, string[]>(row.CmdName,
                                                                                                                                                                            row.RepeatTimer,
                                                                                                                                                                            row.Category?.Split(',', StringSplitOptions.TrimEntries) ?? []));
            }
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get timer command {Cmd}.");


            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow row = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{Cmd}'");
                return (row == null) ? null : new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>());
            }
        }

        public int? GetTimerCommandTime(string Cmd)
        {
            return GetTimerCommand(Cmd)?.Item2;
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Set the enable as {Enabled} for all the system events.");


            SetDataTableFieldRows(_DataSource.ChannelEvents, _DataSource.ChannelEvents.IsEnabledColumn, Enabled);
            NotifySaveData();
        }

        private static string ComFilter()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get the list of default commands and socials, to filter out user-defined commands.");


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

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Set the enable as {Enabled} for all the built-in commands.");


            SetDataTableFieldRows(_DataSource.Commands, _DataSource.Commands.IsEnabledColumn, Enabled, "CmdName IN (" + ComFilter() + ")");
            NotifySaveData();
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Set the enable as {Enabled} for all the user-defined commands.");


            SetDataTableFieldRows(_DataSource.Commands, _DataSource.Commands.IsEnabledColumn, Enabled, "CmdName NOT IN (" + ComFilter() + ")");
            NotifySaveData();
        }

        public void SetDiscordWebhooksEnabled(bool Enabled)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Set the enable as {Enabled} for all the Discord webhooks.");


            SetDataTableFieldRows(_DataSource.Discord, _DataSource.Discord.IsEnabledColumn, Enabled);
            NotifySaveData();
        }

        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            lock (GUIDataManagerLock.Lock)
            {
                List<DataTable> updated = [];
                foreach (DataRow dr in dataRows)
                {
                    if (CheckField(dr.Table.TableName, "IsEnabled"))
                    {
                        updated.UniqueAdd(dr.Table);
                        SetDataRowFieldRow(dr, "IsEnabled", IsEnabled);
                    }
                }
                updated.ForEach((T) => T.AcceptChanges());
            }
        }

        public List<string> GetCurrencyNames()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get all of the currency names.");

            lock (GUIDataManagerLock.Lock)
            {
                return DataSetStatic.GetRowsDataColumn(_DataSource.CurrencyType, _DataSource.CurrencyType.CurrencyNameColumn).ConvertAll((value) => value.ToString());
            }
        }

        #endregion

        #region Stream Statistics
        private StreamStatsRow CurrStreamStatRow;

        private StreamStatsRow[] GetAllStreamData()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get all stream data.");

            lock (GUIDataManagerLock.Lock)
            {
                return (StreamStatsRow[])GetRows(_DataSource.StreamStats);
            }
        }

        private StreamStatsRow GetAllStreamData(DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Find stream data for the {dateTime} date time.");


            lock (GUIDataManagerLock.Lock)
            {
                return (from StreamStatsRow streamStatsRow in GetAllStreamData()
                        where streamStatsRow.StreamStart == dateTime
                        select streamStatsRow).FirstOrDefault();
            }
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get the content stats for a particular stream dated {dateTime}.");


            lock (GUIDataManagerLock.Lock)
            {
                StreamStatsRow streamStatsRow = GetAllStreamData(dateTime);
                StreamStat streamStat = new();

                if (streamStatsRow != null)
                {
                    // can't use a simple method to duplicate this because "ref" can't be used with boxing
                    foreach (PropertyInfo property in streamStat.GetType().GetProperties())
                    {
                        if (streamStatsRow.GetType().GetProperties().Contains(property))
                        {
                            // use properties from 'StreamStat' since StreamStatRow has additional properties
                            property.SetValue(streamStat, streamStatsRow.GetType().GetProperty(property.Name).GetValue(streamStatsRow));
                        }
                    }
                }

                return streamStat;
            }
        }

        public bool CheckMultiStreams(DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if there are multiple streams for {dateTime}.");


            return (from StreamStatsRow row in GetAllStreamData()
                    where row.StreamStart.ToShortDateString() == dateTime.ToShortDateString()
                    select row).Count() > 1;
        }

        public bool PostStream(DateTime StreamStart)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add a new stream for {StreamStart}, checking if one already exists.");

            bool returnvalue;

            if (StreamStart == DateTime.MinValue.ToLocalTime() || CheckStreamTime(StreamStart))
            {
                returnvalue = false;
            }
            else
            {
                CurrStreamStart = StreamStart;

                lock (GUIDataManagerLock.Lock)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    _DataSource.StreamStats.AcceptChanges();
                    NotifySaveData();
                    returnvalue = true;
                }
            }
            return returnvalue;
        }

        public void PostStreamStat(StreamStat streamStat)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Update stream stats for {streamStat.StreamStart} stream.");

            if (streamStat.StreamStart != streamStat.StreamEnd)
            {
                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Livestream ended at {streamStat.StreamEnd}.");
            }

            lock (GUIDataManagerLock.Lock)
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
                        bool found = (from PropertyInfo trgtprop in typeof(StreamStat).GetProperties()
                                      where trgtprop.Name == srcprop.Name
                                      select new { }).Any();

                        if (found)
                        {
                            // use properties from 'StreamStat' since StreamStatRow has additional properties
                            srcprop.SetValue(CurrStreamStatRow, streamStat.GetType().GetProperty(srcprop.Name).GetValue(streamStat));
                        }
                    }
                }
                _DataSource.StreamStats.AcceptChanges();
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

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if there already is a stream for {CurrTime}.");


            return GetAllStreamData(CurrTime) != null;
        }

        /// <summary>
        /// Remove all stream stats, to satisfy a user option selection to not track stats
        /// </summary>
        public void RemoveAllStreamStats()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Remove all stream data.");


            DataSetStatic.DeleteDataRows(GetRows(_DataSource.StreamStats));
            NotifySaveData();
        }

        #endregion

        #region Users and Followers

        private static DateTime CurrStreamStart { get; set; }

        public string GetUserId(LiveUser User)
        {
            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User.UserName}' AND {_DataSource.Users.PlatformColumn.ColumnName}='{User.Source}'");

                return user?.UserId ?? string.Empty;
            }
        }

        public void UserJoined(LiveUser User, DateTime NowSeen)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Update for a user joined, user {User} at {NowSeen}.");


            static DateTime Max(DateTime A, DateTime B) => A <= B ? B : A;

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow userrow = PostNewUser(User, NowSeen);
                userrow.CurrLoginDate = Max(userrow.CurrLoginDate, NowSeen);
                userrow.LastDateSeen = Max(userrow.LastDateSeen, NowSeen);
                _DataSource.Users.AcceptChanges();
                NotifySaveData();
            }
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        public string CheckWelcomeUser(string User)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check for a custom user welcome message for user {User}.");

            lock (GUIDataManagerLock.Lock)
            {
                return ((CustomWelcomeRow)GetRow(_DataSource.CustomWelcome, Filter: $"{_DataSource.CustomWelcome.UserNameColumn.ColumnName}='{User}'"))?.Message ?? "";
            }
        }

        public void PostUserCustomWelcome(string User, string WelcomeMsg)
        {
            lock (GUIDataManagerLock.Lock)
            {
                _DataSource.CustomWelcome.AddCustomWelcomeRow(User, WelcomeMsg);
                _DataSource.CustomWelcome.AcceptChanges();
            }
            NotifySaveData();
        }

        public void UserLeft(LiveUser User, DateTime LastSeen)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Update the user {User} has left, at {LastSeen}.");

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User}' AND {_DataSource.Users.PlatformColumn.ColumnName}='{User.Source}'");
                if (user != null)
                {
                    UpdateWatchTime(User.UserName, LastSeen); // will update the "LastDateSeen"
                    if (OptionFlags.CurrencyStart && (OptionFlags.CurrencyOnline && OptionFlags.IsStreamOnline))
                    {
                        UpdateCurrency(ref user, LastSeen);
                        _DataSource.Currency.AcceptChanges();
                    } // will update the "CurrLoginDate"
                    _DataSource.Users.AcceptChanges();
                }
                NotifySaveData();
            }
        }

        public void UpdateWatchTime(string User, DateTime CurrTime)
        {
            UpdateWatchTime(new List<string>() { User }, CurrTime);
        }

        public void UpdateWatchTime(List<string> Users, DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                foreach (UsersRow U in (UsersRow[])GetRows(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName} in ('{string.Join("', '", [.. Users])}')"))
                {
                    if (U.LastDateSeen < CurrStreamStart)
                    {
                        U.LastDateSeen = CurrStreamStart;
                    }

                    if (CurrTime > U.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        U.WatchTime = U.WatchTime.Add(CurrTime - U.LastDateSeen);
                    }

                    U.LastDateSeen = CurrTime;
                }
                _DataSource.Users.AcceptChanges();
            }
            NotifySaveData();
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><c>true</c> if the user has arrived prior to DateTime.MaxValue, <c>false</c> otherwise.</returns>
        public bool CheckUser(LiveUser User)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if user {User} has already been in the channel.");


            return CheckUser(User, DateTime.MaxValue);
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        public bool CheckUser(LiveUser User, DateTime ToDateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if user {User} has arrived before {ToDateTime}.");


            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User.UserName}' AND ({_DataSource.Users.PlatformColumn.ColumnName}='{User.Source}' OR {_DataSource.Users.PlatformColumn.ColumnName} is NULL)");

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

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if {User} is a current follower.");


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

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Check if user {User} followed prior to {ToDateTime}, for the follower welcome back notice.");


            lock (GUIDataManagerLock.Lock)
            {
                FollowersRow datafollowers = (FollowersRow)GetRow(_DataSource.Followers, $"{_DataSource.Followers.UserNameColumn.ColumnName}='{User}'");

                return datafollowers != null
                    && datafollowers.IsFollower
                    && datafollowers.FollowedDate <= ToDateTime;
            }
        }

        // TODO: validate adding follower category, carry forward category when follower changes viewer name

        /// <summary>
        /// Add a new follower to the data table.
        /// </summary>
        /// <param name="User">The Username of the new Follow</param>
        /// <param name="FollowedDate">The date of the Follow.</param>
        /// <param name="Category">Stream category-shows streamer under which category the viewer followed</param>
        /// <returns>True if the follower is the first time. False if already followed.</returns>
        public bool PostFollower(LiveUser User, DateTime FollowedDate, string Category)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add user {User} as a new follower at {FollowedDate}.");


            lock (GUIDataManagerLock.Lock)
            {
                bool newfollow;
                bool update = false;

                UsersRow users = PostNewUser(User, FollowedDate);
                FollowersRow followers = (FollowersRow)GetRow(_DataSource.Followers, $"{_DataSource.Followers.UserNameColumn.ColumnName}='{User.UserName}'");

                if (followers != null)
                {
                    // newfollow = !followers.IsFollower;
                    newfollow = false;
                    followers.IsFollower = true;

                    if (DBNull.Value.Equals(followers["FollowedDate"]) || followers.FollowedDate != FollowedDate)
                    {
                        followers.FollowedDate = FollowedDate;
                    }

                    if (DBNull.Value.Equals(followers["StatusChangeDate"]))
                    {
                        followers.StatusChangeDate = FollowedDate;
                    }

                    if (DBNull.Value.Equals(followers["UserId"]) && User.UserId != string.Empty && User.UserId != null)
                    {
                        followers.UserId = User.UserId;
                        update = true;
                    }
                    if (DBNull.Value.Equals(followers["Platform"]) || followers.Platform == string.Empty)
                    {
                        followers.Platform = User.Source.ToString();
                        update = true;
                    }
                    if (followers.IsCategoryNull() || followers.Category == string.Empty)
                    {
                        followers.Category = Category;
                    }
                }
                else
                {
                    newfollow = true;

                    string GameCategory = Category;

                    FollowersRow ExistingUserFollow = (FollowersRow)GetRow(_DataSource.Followers, $"{_DataSource.Followers.UserIdColumn.ColumnName}='{User.UserId}' AND {_DataSource.Followers.CategoryColumn.ColumnName} is NOT NULL");

                    if (ExistingUserFollow != null)
                    {
                        GameCategory = ExistingUserFollow.Category;
                    }

                    _DataSource.Followers.AddFollowersRow(users, users.UserName, true, FollowedDate, User.UserId, User.Source.ToString(), FollowedDate, Category);
                    update = true;
                }
                if (update)
                {
                    _DataSource.Followers.AcceptChanges();
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
        private UsersRow PostNewUser(LiveUser User, DateTime FirstSeen)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add a new user {User}, first seen at {FirstSeen}.");


            UsersRow usersRow = null;
            lock (GUIDataManagerLock.Lock)
            {
                if (!CheckUser(User))
                {

                    usersRow = _DataSource.Users.AddUsersRow(User.UserName, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero, User.UserId, User.Source.ToString());
                    //AddCurrencyRows(ref usersRow);
                }


                // if the user is added to list before identified as follower, update first seen date to followed date

                usersRow = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User.UserName}'");

                if (FirstSeen <= usersRow.FirstDateSeen)
                {
                    usersRow.FirstDateSeen = FirstSeen;
                }

                if (usersRow.UserId == null && User.UserId != null)
                {
                    usersRow.UserId = User.UserId;
                }
                usersRow.Platform ??= User.Source.ToString();
            }

            NotifySaveData();
            return usersRow;
        }

        /// <summary>
        /// Call to prepare database for follower update operation, blocks new follower operations until the bulk operation completes.
        /// </summary>
        public void StartBulkFollowers()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Start updating followers in bulk, set all as false to then mark as a follower.");


            UpdatingFollowers = true;
            lock (GUIDataManagerLock.Lock)
            {
                SetDataTableFieldRows(_DataSource.Followers, _DataSource.Followers.IsFollowerColumn, false);
            }
            //NotifySaveData();
        }

        /// <summary>
        /// Performs the bulk follower update with the list of current followers. 
        /// Because the streaming service doesn't/didn't support a user unfollowing the streamer, we need to regularly update the follower list in its entirety-retrieve all followers.
        /// </summary>
        /// <param name="follows">A list of current followers to add into the database.</param>
        public void UpdateFollowers(IEnumerable<Follow> follows, string Category)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Update and add all followers in bulk.");

            if (follows.Any())
            {
                foreach (Follow f in follows)
                {
                    _ = PostFollower(f.FromUser, f.FollowedAt, Category);
                }
            }

            //NotifySaveData();
        }

        /// <summary>
        /// Call to conclude the bulk follower update and release the database.
        /// </summary>
        public void StopBulkFollows()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Stop bulk updating all followers.");

            if (OptionFlags.TwitchPruneNonFollowers)
            {
                lock (GUIDataManagerLock.Lock)
                {
                    foreach (FollowersRow f in from FollowersRow f in new List<FollowersRow>((FollowersRow[])GetRows(_DataSource.Followers))
                                               where !f.IsFollower
                                               select f)
                    {
                        _DataSource.Followers.RemoveFollowersRow(f);
                    }
                }
            }
            else
            {
                lock (GUIDataManagerLock.Lock)
                {
                    DateTime datenow = DateTime.Now;
                    List<FollowersRow> AllFollowers = new((FollowersRow[])GetRows(_DataSource.Followers));
                    foreach (var FR in from FollowersRow FR in
                                           from FollowersRow f in AllFollowers
                                           where AllFollowers.FindAll(user => user.UserId == f.UserId).Count > 1
                                           select f
                                       where DBNull.Value.Equals(FR["StatusChangeDate"]) || FR.StatusChangeDate <= FR.FollowedDate
                                       select FR)
                    {
                        FR.StatusChangeDate = datenow;
                    }

                    // TODO: verify bulk follower updates for unfollows - updating the dates
                    foreach (var nonfollowers in
                            from string UId in new List<string>
                            (
                                (
                                    from FollowersRow U in new List<FollowersRow>
                                    (
                                        from FollowersRow f in AllFollowers
                                        where f.IsFollower == false
                                        orderby f.Id descending
                                        select f
                                    )
                                    select U.UserId).ToList().Distinct())
                            let nonfollowers = new List<FollowersRow>(from FollowersRow f in AllFollowers
                                                                      where f.IsFollower == false
                                                                      orderby f.Id descending
                                                                      select f
                                                                    ).FindAll((user) => user.UserId == UId
                            )
                            select nonfollowers
                    )
                    {
                        if (nonfollowers.Count > 1)
                        {
                            if (nonfollowers[0].StatusChangeDate == nonfollowers[1].StatusChangeDate)
                            {
                                nonfollowers[0].StatusChangeDate = datenow;
                            }
                        }
                        else
                        {
                            if (nonfollowers[0].StatusChangeDate == nonfollowers[0].FollowedDate)
                            {
                                nonfollowers[0].StatusChangeDate = datenow;
                            }
                        }
                    }
                }
            }
            lock (GUIDataManagerLock.Lock)
            {
                _DataSource.Followers.AcceptChanges();
            }
            NotifySaveData();
            UpdatingFollowers = false;
        }

        /// <summary>
        /// Queries the Follower table for the most recent follower.
        /// </summary>
        /// <returns>Returns the most recent follower UserName from the Follower table.</returns>
        public string GetNewestFollower()
        {
            return ((FollowersRow)GetRows(_DataSource.Followers, null, $"{_DataSource.Followers.FollowedDateColumn.ColumnName} DESC").FirstOrDefault()).UserName;
        }

        /// <summary>
        /// Query the Follower table to count the current active channel followers.
        /// </summary>
        /// <returns>The count of the current follower list.</returns>
        public int? GetFollowerCount()
        {
            return (GetRows(_DataSource.Followers, $"{_DataSource.Followers.IsFollowerColumn}='true'"))?.Length ?? null;
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Clear all watch time.");


            SetDataTableFieldRows(_DataSource.Users, _DataSource.Users.WatchTimeColumn, new TimeSpan(0));
            NotifySaveData();
        }

        /// <summary>
        /// Adds a user to the auto shout table.
        /// </summary>
        /// <param name="UserName">The Username to add, duplicates are not added.</param>
        public void PostNewAutoShoutUser(string UserName, string UserId, string platform)
        {
            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Adding user {UserName} to the auto shout-out listing.");

            lock (GUIDataManagerLock.Lock)
            {
                if (GetRow(_DataSource.ShoutOuts, $"{_DataSource.ShoutOuts.UserNameColumn.ColumnName}='{UserName}'") == null)
                {
                    _DataSource.ShoutOuts.AddShoutOutsRow(UserId);
                    _DataSource.ShoutOuts.AcceptChanges();
                    NotifySaveData();
                }
            }
        }

        /// <summary>
        /// Combine two user names, in the case of a user visits the channel for awhile, changes their name, and they want to update their progress across user names.
        /// </summary>
        /// <param name="CurrUser">The user receiving all of the user data.</param>
        /// <param name="SourceUser">The user containing all of the stats to migrate.</param>
        /// <param name="platform">The source platform for these users, to distinguish unique user names per streaming platform.</param>
        /// <returns></returns>
        public bool PostMergeUserStats(string CurrUser, string SourceUser, Platform platform)
        {
            bool success = false;

            lock (GUIDataManagerLock.Lock)
            {
                // do currency updates first

                CurrencyRow[] CurrencyCurrUserRow = (CurrencyRow[])GetRows(_DataSource.Currency, $"{_DataSource.Currency.UserNameColumn.ColumnName}='{CurrUser}'");
                CurrencyRow[] CurrencySourceUserRow = (CurrencyRow[])GetRows(_DataSource.Currency, $"{_DataSource.Currency.UserNameColumn.ColumnName}='{SourceUser}'");

                foreach (var (SCR, CCR) in from CurrencyRow SCR in CurrencySourceUserRow
                                           from CurrencyRow CCR in CurrencyCurrUserRow
                                           where SCR.CurrencyName == CCR.CurrencyName
                                           select (SCR, CCR))
                {
                    CCR.Value += SCR.Value;
                    success = true;
                }

                if (success)
                {
                    foreach (CurrencyRow cr in CurrencySourceUserRow)
                    {
                        cr.Delete();
                    }
                }

                _DataSource.Currency.AcceptChanges();

                // do user table updates last
                UsersRow CurrUserRow = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{CurrUser}' AND {_DataSource.Users.PlatformColumn.ColumnName}='{platform}'");
                UsersRow SourceUserRow = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{SourceUser}' AND {_DataSource.Users.PlatformColumn.ColumnName}='{platform}'");

                if (CurrUserRow != null && SourceUserRow != null)
                {
                    CurrUserRow.WatchTime += SourceUserRow.WatchTime;
                    SourceUserRow.Delete();
                    _DataSource.Users.AcceptChanges();
                    _DataSource.Followers.AcceptChanges();
                    success = success && true;
                }

                if (success)
                {
                    NotifySaveData();
                }
            }

            return success;
        }

        #endregion Users and Followers

        #region Giveaways
        public void PostGiveawayData(string DisplayName, DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Posting the giveaway data.");


            lock (GUIDataManagerLock.Lock)
            {
                _ = _DataSource.GiveawayUserData.AddGiveawayUserDataRow(DisplayName, dateTime);
                _DataSource.GiveawayUserData.AcceptChanges();
            }
            NotifySaveData();
        }

        #endregion

        #region Quotes

        /// <summary>
        /// Add a new quote to the 'Quotes' Table. First, this finds the minimum unused quote number-to recycle the numbers- or uses the 
        /// next available number. Then, with this number adds the new quote to the table.
        /// </summary>
        /// <param name="Text">The quote text to add to the table.</param>
        /// <returns>The quote number for the newly added quote.</returns>
        public int PostQuote(string Text)
        {
            // get the quotes, sorted by ascending quote number
            List<QuotesRow> quotes = new((QuotesRow[])GetRows(_DataSource.Quotes, Sort: $"{_DataSource.Quotes.NumberColumn.ColumnName} ASC"));

            int newNumber = -1; // set a check number.

            if (quotes.Count > 0 && quotes.Last().Number != quotes.Count) // if list is full, there are no empty numbers
            {
                // find the missing quote number in the existing quote listing
                for (int x = 1; x <= quotes.Count; x++)
                {
                    if (quotes.FindIndex((m) => m.Number == x) == -1) // -1 is the number is 'not found' 
                    {
                        newNumber = x;
                    }
                }
            }

            if (newNumber == -1) // should the quote list lookup have all the numbers
            {
                newNumber = quotes.Count + 1; // pick next number
            }

            lock (GUIDataManagerLock.Lock)
            {// add the quote
                _DataSource.Quotes.AddQuotesRow(newNumber, Text);
                _DataSource.Quotes.AcceptChanges();
            }

            // return the quote number
            return newNumber;
        }

        /// <summary>
        /// Get a specific quote from the data table per the <paramref name="QuoteNum"/>
        /// </summary>
        /// <param name="QuoteNum">The quote number to provide.</param>
        /// <returns>The quote number and quote from the quote table -or- null if there is no quote.</returns>
        public string GetQuote(int QuoteNum)
        {
            string quotedata;

            QuotesRow FindQuote = ((QuotesRow)GetRow(_DataSource.Quotes, $"{_DataSource.Quotes.NumberColumn.ColumnName}='{QuoteNum}'"));

            if (FindQuote != null)
            {
                quotedata = $"{QuoteNum}: \"{FindQuote.Quote}\"";
            }
            else
            {
                quotedata = null;
            }

            return quotedata;
        }

        /// <summary>
        /// Find the highest quote number from the quote table.
        /// </summary>
        /// <returns>The maximum quote number (where deleted quotes leave empty numbers somewhere in the middle)</returns>
        public int GetQuoteCount()
        {
            return ((QuotesRow[])GetRows(_DataSource.Quotes, Sort: $"{_DataSource.Quotes.NumberColumn.ColumnName} ASC")).Last().Number;
        }

        /// <summary>
        /// Delete a quote from the quote table, per the specified <paramref name="QuoteNum"/>
        /// </summary>
        /// <param name="QuoteNum">The quote number to delete.</param>
        /// <returns><c>true</c> if the quote successfully delete; otherwise <c>false</c>.</returns>
        public bool RemoveQuote(int QuoteNum)
        {
            bool task = false;

            // get the quote row, test if it's found
            QuotesRow row = (QuotesRow)GetRow(_DataSource.Quotes, $"{_DataSource.Quotes.NumberColumn.ColumnName}='{QuoteNum}'");

            lock (GUIDataManagerLock.Lock)
            {
                if (row != null)
                {
                    row.Delete(); // row exists, delete it
                    _DataSource.Quotes.AcceptChanges(); // accept the change
                    task = true;
                }
            }

            return task;
        }

        #endregion

        #region Currency

        public void UpdateCurrency(List<string> Users, DateTime dateTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                foreach (string U in Users)
                {
                    UpdateCurrency(U, dateTime);
                }
                _DataSource.Currency.AcceptChanges();
                _DataSource.Users.AcceptChanges();
                NotifySaveData();
            }
        }

        /// <summary>
        /// For the supplied user string, update the currency based on the supplied time to the currency accrual rates the streamer specified for the currency.
        /// </summary>
        /// <param name="User">The name of the user to find in the database.</param>
        /// <param name="dateTime">The time to base the currency calculation.</param>
        private void UpdateCurrency(string User, DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Update currency for user {User}.");

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User}'");
                UpdateCurrency(ref user, dateTime);
            }
        }

        /// <summary>
        /// Process currency accruals per user, if currency type is defined, otherwise currency accruals are ignored. Afterward, the 'CurrLoginDate' is updated.
        /// </summary>
        /// <param name="User">The user to evaluate.</param>
        /// <param name="CurrTime">The time to update and accrue the currency.</param>
        private void UpdateCurrency(ref UsersRow User, DateTime CurrTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Realize the currency accruals for user {User.UserName}.");


            lock (GUIDataManagerLock.Lock)
            {
                if (User != null)
                {
                    TimeSpan currencyclock = CurrTime - User.CurrLoginDate; // the amount of time changed for the currency accrual calculation

                    double ComputeCurrency(double Accrue, double Seconds)
                    {
                        return Accrue * (currencyclock.TotalSeconds / Seconds);
                    }

                    PostCurrencyRows(ref User);

                    CurrencyTypeRow[] currencyType = (CurrencyTypeRow[])GetRows(_DataSource.CurrencyType);
                    CurrencyRow[] userCurrency = (CurrencyRow[])GetRows(_DataSource.Currency, $"{_DataSource.Currency.IdColumn.ColumnName}='{User.Id}'");

                    foreach ((CurrencyTypeRow typeRow, CurrencyRow currencyRow) in currencyType.SelectMany(typeRow => userCurrency.Where(currencyRow => currencyRow.CurrencyName == typeRow.CurrencyName).Select(currencyRow => (typeRow, currencyRow))))
                    {
                        currencyRow.Value = Math.Min(Math.Round(currencyRow.Value + ComputeCurrency(typeRow.AccrueAmt, typeRow.Seconds), 2), typeRow.MaxValue);
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
        public void PostCurrencyRows(ref UsersRow usersRow)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add all currency rows for user {usersRow.UserName}.");

            lock (GUIDataManagerLock.Lock)
            {
                CurrencyTypeRow[] currencyTypeRows = (CurrencyTypeRow[])GetRows(_DataSource.CurrencyType);
                if (usersRow != null)
                {
                    CurrencyRow[] currencyRows = (CurrencyRow[])GetRows(_DataSource.Currency, $"{_DataSource.Currency.UserNameColumn.ColumnName}='{usersRow.UserName}'");
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
        }

        public bool CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            CurrencyRow currencyRow = (CurrencyRow)GetRow(_DataSource.Currency, $"{_DataSource.Currency.UserNameColumn.ColumnName}='{User.UserName}' AND {_DataSource.Currency.CurrencyNameColumn.ColumnName}='{CurrencyName}'");
            _DataSource.Currency.AcceptChanges();
            return currencyRow.Value >= value;
        }

        public void PostCurrencyUpdate(LiveUser User, double value, string CurrencyName)
        {
            CurrencyRow currencyRow = (CurrencyRow)GetRow(_DataSource.Currency, $"{_DataSource.Currency.UserNameColumn.ColumnName}='{User.UserName}' AND {_DataSource.Currency.CurrencyNameColumn.ColumnName}='{CurrencyName}'");
            CurrencyTypeRow currencyTypeRow = (CurrencyTypeRow)GetRow(_DataSource.CurrencyType, $"{_DataSource.CurrencyType.CurrencyNameColumn.ColumnName}='{CurrencyName}'");
            if (currencyRow != null && currencyTypeRow != null)
            {
                lock (_DataSource)
                {
                    currencyRow.Value = Math.Min(Math.Round(currencyRow.Value + value, 2), currencyTypeRow.MaxValue);
                    _DataSource.Currency.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// For every user in the database, add currency rows for each currency type - add missing rows.
        /// </summary>
        public void PostCurrencyRows()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add currency for all users.");

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow[] UserRows = (UsersRow[])GetRows(_DataSource.Users);

                for (int i = 0; i < UserRows.Length; i++)
                {
                    UsersRow users = UserRows[i];
                    PostCurrencyRows(ref users);
                }
                _DataSource.Currency.AcceptChanges();
                NotifySaveData();
            }
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        public void ClearUsersNotFollowers()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Clear users who are not followers.");

            List<string> RemoveIds = [];

            lock (GUIDataManagerLock.Lock)
            {
                foreach (UsersRow U in GetRows(_DataSource.Users).Cast<UsersRow>())
                {
                    if (GetRow(_DataSource.Followers, $"{_DataSource.Followers.IdColumn.ColumnName}='{U.Id}'") == null
                        || GetRow(_DataSource.Followers, $"{_DataSource.Followers.IdColumn.ColumnName}='{U.Id}' AND {_DataSource.Followers.IsFollowerColumn.ColumnName}=false") != null)
                    {
                        RemoveIds.Add(U.Id.ToString());
                    }
                }

                DataSetStatic.DeleteDataRows(GetRows(_DataSource.Users, $"{_DataSource.Users.IdColumn.ColumnName} in ('{string.Join("', '", RemoveIds)}')"));
            }
            NotifySaveData();
        }

        /// <summary>
        /// Empty every currency to 0, for all users for all currencies.
        /// </summary>
        public void ClearAllCurrencyValues()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Clear all currency values.");


            SetDataTableFieldRows(_DataSource.Currency, _DataSource.Currency.ValueColumn, 0);
            NotifySaveData();
        }

        #endregion

        #region Raid Data
        public void PostInRaidData(string user, DateTime time, string viewers, string gamename)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Post incoming raid data.");


            lock (GUIDataManagerLock.Lock)
            {
                _ = _DataSource.InRaidData.AddInRaidDataRow(user, viewers, time, gamename);
                _DataSource.InRaidData.AcceptChanges();
            }
            NotifySaveData();
        }

        public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Test the incoming raid data, code testing if raid data was added.");


            lock (GUIDataManagerLock.Lock)
            {
                // 2021-12-06T01:19:16.0248427-05:00
                //string.Format("UserName='{0}' and DateTime='{1}' and ViewerCount='{2}' and Category='{3}'", user, time, viewers, gamename)
                return (InRaidDataRow)GetRow(_DataSource.InRaidData, $"{_DataSource.InRaidData.UserNameColumn.ColumnName}='{user}' AND {_DataSource.InRaidData.DateTimeColumn.ColumnName}=#{time:O}# AND {_DataSource.InRaidData.ViewerCountColumn.ColumnName}='{viewers}' AND {_DataSource.InRaidData.CategoryColumn.ColumnName}='{gamename}'") != null;
            }
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Test the outgoing raid data, code testing if raid data was added.");


            lock (GUIDataManagerLock.Lock)
            {
                // string.Format("ChannelRaided='{0}' and DateTime='{1}'", HostedChannel, dateTime)
                return (OutRaidDataRow)GetRow(_DataSource.OutRaidData, $"{_DataSource.OutRaidData.ChannelRaidedColumn.ColumnName}='{HostedChannel}' AND {_DataSource.OutRaidData.DateTimeColumn.ColumnName}=#{dateTime:O}#") != null;
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Post the outgoing raid data.");


            lock (GUIDataManagerLock.Lock)
            {
                _ = _DataSource.OutRaidData.AddOutRaidDataRow(HostedChannel, dateTime);
                _DataSource.OutRaidData.AcceptChanges();
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

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get all the available Discord webhooks, for the {webhooks} type.");


            return new(((DiscordRow[])GetRows(_DataSource.Discord, $"{_DataSource.Discord.KindColumn.ColumnName}='{webhooks}' AND {_DataSource.Discord.IsEnabledColumn.ColumnName}=True")).Select(d => new Tuple<bool, Uri>(d.AddEveryone, new Uri(d.Webhook))));
        }

        #endregion Discord and Webhooks

        #region Category

        #region Death Counter Data

        /// <summary>
        /// Updates the death counter for the specified category.
        /// </summary>
        /// <param name="currCategory">The category to update.</param>
        /// <param name="Reset"><code>true</code>: changes value to specified value, <code>false</code>: increments the counter by <paramref name="Value"/></param>
        /// <param name="Value">The value to reset, when <paramref name="Reset"/> is <code>true</code>. Otherwise, default increment by 1.</param>
        /// <returns>The updated value of the current category death counter.</returns>
        public int PostDeathCounterUpdate(string currCategory, bool Reset = false, int updateValue = 1)
        {
            int returnValue = 0;
            lock (GUIDataManagerLock.Lock)
            {
                GameDeadCounterRow deadCounterRow = (GameDeadCounterRow)GetRow(_DataSource.GameDeadCounter, $"{_DataSource.GameDeadCounter.CategoryColumn.ColumnName}='{currCategory}'");

                if (deadCounterRow != null) // check if row exists
                {
                    if (Reset)
                    {
                        deadCounterRow.Counter = updateValue;
                    }
                    else
                    {
                        deadCounterRow.Counter += updateValue;
                    }
                    returnValue = deadCounterRow.Counter;
                }
                else // create row, only if category is found
                {
                    // we should find category, because it should be added when bot starts and stream starts
                    CategoryListRow currCategoryRow = (CategoryListRow)GetRow(_DataSource.CategoryList, $"{_DataSource.CategoryList.CategoryColumn.ColumnName}='{currCategory}'");

                    if (currCategoryRow != null) // category is already added into database, in case it might not be...
                    {
                        _DataSource.GameDeadCounter.AddGameDeadCounterRow(currCategoryRow, updateValue);
                        returnValue = updateValue;
                    }
                }

                _DataSource.GameDeadCounter.AcceptChanges();
            }

            return returnValue;
        }

        /// <summary>
        /// Retrieve the death counter for a specific category.
        /// </summary>
        /// <param name="currCategory">Category to retrieve the current death counter.</param>
        /// <returns>The death counter for the category, or <code>-1</code>: if counter doesn't exist.</returns>
        public int GetDeathCounter(string currCategory)
        {
            int result = -1;

            lock (GUIDataManagerLock.Lock)
            {
                GameDeadCounterRow deadCounterRow = (GameDeadCounterRow)GetRow(_DataSource.GameDeadCounter, $"{_DataSource.GameDeadCounter.CategoryColumn.ColumnName}='{currCategory}'");
                if (deadCounterRow != null)
                {
                    result = deadCounterRow.Counter;
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Checks for the supplied category in the category list, adds if it isn't already saved.
        /// </summary>
        /// <param name="CategoryId">The ID of the stream category.</param>
        /// <param name="newCategory">The category to add to the list if it's not available.</param>
        /// <returns>True if category OR game ID are found; False if no category nor game ID is found.</returns>
        public bool PostCategory(string CategoryId, string newCategory)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add and update the {newCategory} category.");

            if (CategoryId != "" && newCategory != "")
            {
                lock (GUIDataManagerLock.Lock)
                {
                    CategoryListRow categoryList = (CategoryListRow)GetRow(_DataSource.CategoryList, 
                        $"{_DataSource.CategoryList.CategoryColumn.ColumnName}='{FormatData.AddEscapeFormat(newCategory)}' " +
                        $"OR {_DataSource.CategoryList.CategoryIdColumn.ColumnName}='{CategoryId}'");
                    bool found = false;
                    if (categoryList == null)
                    {
                        _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory, 1);
                        found = true;
                    }
                    else
                    {
                        if (categoryList.CategoryId == null)
                        {
                            categoryList.CategoryId = CategoryId;
                            found = true;
                        }
                        if (categoryList.Category == null)
                        {
                            categoryList.Category = newCategory;
                            found = true;
                        }

                        if (OptionFlags.IsStreamOnline)
                        {
                            categoryList.StreamCount++;
                            found = true;
                        }
                    }

                    if (found)
                    {
                        _DataSource.CategoryList.AcceptChanges();
                        NotifySaveData();
                    }

                    return categoryList != null;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves all GameIds and GameCategories added to the database.
        /// </summary>
        /// <returns>Returns a list of <code>Tuple<string GameId, string GameName></code> objects.</returns>
        public List<Tuple<string, string>> GetGameCategories()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Get a list of all categories.");

            lock (GUIDataManagerLock.Lock)
            {
                return new(from CategoryListRow c in GetRows(_DataSource.CategoryList)
                           orderby c.Category
                           let item = new Tuple<string, string>(c.CategoryId, c.Category)
                           select item);
            }
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
        public bool PostClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Add a new clip.");


            bool result = false;
            lock (GUIDataManagerLock.Lock)
            {
                if (((ClipsRow[])GetRows(_DataSource.Clips, $"{_DataSource.Clips.IdColumn.ColumnName}='{ClipId}'")).Length == 0)
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime(), Title, GameId, Language, (decimal)Duration, Url);
                    _DataSource.Clips.AcceptChanges();
                    NotifySaveData();
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        #endregion

        #region Machine Learning Moderation

        private void SetLearnedMessages()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, setting learned messages.");


            lock (GUIDataManagerLock.Lock)
            {
                bool found = false;

                if (GetRows(_DataSource.LearnMsgs).Length == 0)
                {
                    foreach (LearnedMessage M in LearnedMessagesPrimer.PrimerList)
                    {
                        _DataSource.LearnMsgs.AddLearnMsgsRow(M.MsgType.ToString(), M.Message);
                        found = true;
                    }
                    if (found)
                    {
                        _DataSource.LearnMsgs.AcceptChanges();
                    }
                }

                if (GetRows(_DataSource.BanReasons).Length == 0)
                {
                    found = false;
                    foreach (BanReason B in LearnedMessagesPrimer.BanReasonList)
                    {
                        _DataSource.BanReasons.AddBanReasonsRow(B.MsgType.ToString(), B.Reason.ToString());
                        found = true;
                    }
                    if (found)
                    {
                        _DataSource.BanReasons.AcceptChanges();
                    }
                }

                if (GetRows(_DataSource.BanRules).Length == 0)
                {
                    found = false;
                    foreach (BanViewerRule BVR in LearnedMessagesPrimer.BanViewerRulesList)
                    {
                        _DataSource.BanRules.AddBanRulesRow(BVR.ViewerType.ToString(), BVR.MsgType.ToString(), BVR.ModAction.ToString(), BVR.TimeoutSeconds);
                        found = true;
                    }
                    if (found)
                    {
                        _DataSource.BanRules.AcceptChanges();
                    }
                }
                NotifySaveData();
            }
        }

        private void LearnMsgs_LearnMsgsRowDeleted(object sender, LearnMsgsRowChangeEvent e)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether learned message rows are deleted.");


            LearnMsgChanged = true;
        }

        //        private void LearnMsgs_LearnMsgsRowChanged(object sender, LearnMsgsRowChangeEvent e)
        //        {
        //
        //            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether learned message rows are changed.");
        //

        //            LearnMsgChanged = true;
        //        }

        private void LearnMsgs_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, whether adding a new learned message.");


            LearnMsgChanged = true;
        }

        public List<LearnMsgRecord> UpdateLearnedMsgs()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, get all the learned messages to update training model.");


            lock (GUIDataManagerLock.Lock)
            {
                if (LearnMsgChanged)
                {
                    LearnMsgChanged = false;
                    return new List<LearnMsgsRow>(
                        (LearnMsgsRow[])GetRows(_DataSource.LearnMsgs)).ConvertAll(
                        (L) => new LearnMsgRecord(L.Id, L.MsgType, L.TeachingMsg));
                }
                else
                {
                    return null;
                }
            }
        }

        public void PostLearnMsgsRow(string Message, MsgTypes MsgType)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Adding a new learned message row.");

            lock (GUIDataManagerLock.Lock)
            {
                bool found = (from LearnMsgsRow learnMsgsRow in GetRows(_DataSource.LearnMsgs, $"{_DataSource.LearnMsgs.TeachingMsgColumn.ColumnName}='{FormatData.AddEscapeFormat(Message)}'")
                              select new { }).Any();

                if (!found)
                {
                    _DataSource.LearnMsgs.AddLearnMsgsRow(MsgType.ToString(), Message);
                    _DataSource.LearnMsgs.AcceptChanges();
                    NotifySaveData();
                }
            }
        }

        public Tuple<ModActions, BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Machine learning, find the remedy to the found message determination.");


            lock (GUIDataManagerLock.Lock)
            {
                BanReasons banReason;

                BanReasonsRow banrow = (BanReasonsRow)GetRow(_DataSource.BanReasons, $"{_DataSource.BanReasons.MsgTypeColumn.ColumnName}='{msgTypes}'");

                banReason = banrow != null ? (BanReasons)Enum.Parse(typeof(BanReasons), banrow.BanReason) : BanReasons.None;

                BanRulesRow banRulesRow = (BanRulesRow)GetRow(_DataSource.BanRules, 
                    $"{_DataSource.BanRules.ViewerTypesColumn.ColumnName}='{viewerTypes}' and {_DataSource.BanRules.MsgTypeColumn.ColumnName}='{msgTypes}'");

                int Timeout = banRulesRow == null ? 0 : int.Parse(banRulesRow.TimeoutSeconds);
                ModActions action = banRulesRow == null ? ModActions.Allow : (ModActions)Enum.Parse(typeof(ModActions), banRulesRow.ModAction);

                return new(action, banReason, Timeout);
            }
        }

        #endregion

        #region Moderator Approval
        public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            lock (GUIDataManagerLock.Lock)
            {
                ModeratorApproveRow moderatorApproveRow = (ModeratorApproveRow)GetRow(
                    _DataSource.ModeratorApprove,
                    $"{_DataSource.ModeratorApprove.ModActionTypeColumn.ColumnName}='{modActionType}' AND {_DataSource.ModeratorApprove.ModActionNameColumn.ColumnName}='{ModAction}'");

                return moderatorApproveRow == null ? null :
                    new(
                    DBNull.Value.Equals(moderatorApproveRow.ModPerformType) || moderatorApproveRow.ModPerformType == "" ? moderatorApproveRow.ModActionType : moderatorApproveRow.ModPerformType,
                    DBNull.Value.Equals(moderatorApproveRow.ModPerformAction) || moderatorApproveRow.ModPerformAction == "" ? moderatorApproveRow.ModActionName : moderatorApproveRow.ModPerformAction
                    );
            }
        }

        #endregion

        #region Media Overlay Service

        /// <summary>
        /// Retrieve any overlay actions saved to the database - as the streamer setup for use during a stream.
        /// </summary>
        /// <param name="overlayType">The type of the overlay source. e.g. channel points, commands - StreamerBotLib.Enums.Overlay.Enums.OverlayTickerItem</param>
        /// <param name="overlayAction">The name of the overlay type action for the specific overlay to invoke.</param>
        /// <param name="username">A Username if an overlay action is based on a certain user.</param>
        /// <returns>A collection of found OverlayActions matching the parameter criteria.</returns>
        public List<OverlayActionType> GetOverlayActions(string overlayType, string overlayAction, string username)
        {
            lock (GUIDataManagerLock.Lock)
            {
                List<OverlayActionType> found = new(
                    from OverlayServicesRow overlayServicesRow in GetRows(_DataSource.OverlayServices, 
                    Filter: $"{_DataSource.OverlayServices.IsEnabledColumn.ColumnName}=true " +
                    $"AND {_DataSource.OverlayServices.OverlayTypeColumn.ColumnName}='{overlayType}' " +
                    $"AND ({_DataSource.OverlayServices.UserNameColumn.ColumnName}='' " +
                    $"OR {_DataSource.OverlayServices.UserNameColumn.ColumnName}='{username}')") select new OverlayActionType() 
                    { ActionValue = overlayServicesRow.OverlayAction, 
                        Duration = overlayServicesRow.Duration, 
                        MediaFile = overlayServicesRow.MediaFile, 
                        ImageFile = overlayServicesRow.ImageFile, 
                        Message = overlayServicesRow.Message, 
                        OverlayType = (OverlayTypes)Enum.Parse(typeof(OverlayTypes), overlayServicesRow.OverlayType), 
                        UserName = overlayServicesRow.UserName, 
                        UseChatMsg = overlayServicesRow.UseChatMsg });

                List<OverlayActionType> result = [];

                foreach (OverlayActionType OAT in found)
                {
                    if (OAT.ActionValue == overlayAction)
                    {
                        result.Add(OAT);
                    }
                }

                LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.OverlayBot, $"Found {result.Count} Overlay actions in the database matching the Type {overlayType} and Action {overlayAction}.");


                return result;
            }
        }

        /// <summary>
        /// Retrieve all Ticker Items from the OverlayTicker table
        /// </summary>
        /// <returns>A list collection of ticker items.</returns>
        public List<TickerItem> GetTickerItems()
        {
            lock (GUIDataManagerLock.Lock)
            {
                return new(from OverlayTickerRow row in GetRows(_DataSource.OverlayTicker)
                           let ticker = new TickerItem() { OverlayTickerItem = (OverlayTickerItem)Enum.Parse(typeof(OverlayTickerItem), row.TickerName), UserName = row.UserName }
                           select ticker);
            }
        }

        /// <summary>
        /// Post an update to an OverlayTicker item, either updates an existing data row or adds a new row if ticker data doesn't exist.
        /// </summary>
        /// <param name="item">The OverlayTickerItem enum name for the ticker to add or replace.</param>
        /// <param name="name">The Username to update for the ticker item.</param>
        public void UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            lock (GUIDataManagerLock.Lock)
            {
                if (_DataSource.OverlayTicker.Select($"{_DataSource.OverlayTicker.TickerNameColumn.ColumnName}='{item}'").Length != 0)
                {
                    SetDataTableFieldRows(_DataSource.OverlayTicker, _DataSource.OverlayTicker.UserNameColumn, name, $"{_DataSource.OverlayTicker.TickerNameColumn.ColumnName}='{item}'");
                }
                else
                {
                    _DataSource.OverlayTicker.AddOverlayTickerRow(item.ToString(), name);
                    _DataSource.OverlayTicker.AcceptChanges();
                }

                NotifySaveData();
            }
        }

        #endregion

        #region Remove Data
        /// <summary>
        /// Remove all Users from the database.
        /// </summary>
        public void RemoveAllUsers()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Removing all users.");


            DataSetStatic.DeleteDataRows(GetRows(_DataSource.Users));
            NotifySaveData();
        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        public void RemoveAllFollowers()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Removing all followers.");


            DataSetStatic.DeleteDataRows(GetRows(_DataSource.Followers));
            NotifySaveData();
        }

        /// <summary>
        /// Removes all incoming raid data from database.
        /// </summary>
        public void RemoveAllInRaidData()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Removing all incoming raid data.");


            DataSetStatic.DeleteDataRows(GetRows(_DataSource.InRaidData));
            NotifySaveData();
        }

        /// <summary>
        /// Removes all outgoing raid data from database.
        /// </summary>
        public void RemoveAllOutRaidData()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Removing all outgoing raid data.");


            DataSetStatic.DeleteDataRows(GetRows(_DataSource.OutRaidData));
            NotifySaveData();
        }

        /// <summary>
        /// Removes all Giveaway table data from the database.
        /// </summary>
        public void RemoveAllGiveawayData()
        {

            LogWriter.DebugLog(MethodBase.GetCurrentMethod().Name, DebugLogTypes.DataManager, $"Removing all giveaway data.");


            DataSetStatic.DeleteDataRows(GetRows(_DataSource.GiveawayUserData));
            NotifySaveData();
        }

        /// <summary>
        /// Removes all OverlayTicker table data from the database.
        /// </summary>
        public void RemoveAllOverlayTickerData()
        {
            DataSetStatic.DeleteDataRows(GetRows(_DataSource.OverlayTicker));
            NotifySaveData();
        }

        #endregion

    }
}
