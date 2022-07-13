#if DEBUG
#define noLogDataManager_Actions
#endif

using MediaOverlayServer.Models;

using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.MachineLearning;
using StreamerBotLib.Models;
using StreamerBotLib.Static;
using StreamerBotLib.Systems;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using static StreamerBotLib.Data.DataSource;

namespace StreamerBotLib.Data
{
    public partial class DataManager
    {
        #region DataSource
        /// <summary>
        /// Specifies the database xml save file name
        /// </summary>
        public static readonly string DataFileXML = "ChatDataStore.xml";

#if DEBUG
        /// <summary>
        /// Specifies the debug location of the database xml file name - user specific
        /// </summary>
        public static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\StreamerBot\bin\Debug\net6.0-windows", DataFileXML);
#else
        private static readonly string DataFileName = DataFileXML;
#endif

        internal readonly DataSource _DataSource;
        #endregion DataSource


        private bool LearnMsgChanged = true; // always true to begin one learning cycle

        public bool UpdatingFollowers { get; set; }

        public DataManager()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, "Build DataManager object.");
#endif

            BackupSaveToken = DateTime.Now.Minute / BackupSaveIntervalMins;

            _DataSource = new();
            _DataSource.BeginInit();
            LoadData();
            _DataSource.EndInit();
            OnSaveData += SaveData;

            _DataSource.LearnMsgs.TableNewRow += LearnMsgs_TableNewRow;
            //_DataSource.LearnMsgs.LearnMsgsRowChanged += LearnMsgs_LearnMsgsRowChanged;
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get event row data for {rowcriteria}.");
#endif

            string Msg = "";

            lock(GUIDataManagerLock.Lock)
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check permission for {cmd}.");
#endif

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if {UserName} is in the Shout list.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return GetRow(_DataSource.ShoutOuts, $"{_DataSource.ShoutOuts.UserNameColumn.ColumnName}='{UserName}'") != null;
            }
        }

        public string AddCommand(string cmd, CommandParams Params)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add a new command called {cmd}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                _DataSource.Commands.AddCommandsRow(cmd, Params.AddMe, Params.Permission.ToString(), Params.IsEnabled, Params.Message, Params.Timer, Params.RepeatMsg, Params.Category, Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, GetKey(Params.Table), Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort);
            }
            NotifySaveData();
            return string.Format(CultureInfo.CurrentCulture, LocalizedMsgSystem.GetDefaultComMsg(DefaultCommand.addcommand), cmd);
        }

        public string EditCommand(string cmd, List<string> Arglist)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Editing command {cmd}.");
#endif

            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow commandsRow = (CommandsRow)GetRow(_DataSource.Commands, cmd);
                lock (GUIDataManagerLock.Lock)
                {
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
            }
            NotifySaveData();
            return result;
        }

        /// <summary>
        /// Remove the specified command.
        /// </summary>
        /// <param name="command">The "CmdName" of the command to remove.</param>
        /// <returns><c>True</c> with successful removal, <c>False</c> with command not found.</returns>
        public bool RemoveCommand(string command)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Removing command {command}.");
#endif

            return DeleteDataRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{command}'");
        }

        public string GetSocials()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, "Getting the socials listing.");
#endif

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get the usage information for {command}.");
#endif

            return GetCommand(command)?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
        }

        public CommandData GetCommand(string cmd)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get the command row for {cmd}.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow comrow = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{cmd}'");

                return comrow != null ? new(comrow) : null;
            }
        }

        public string GetCommands()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get a list of all commands.");
#endif


            string result = "";

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow[] commandsRows = (CommandsRow[])GetRows(_DataSource.Commands, $"{_DataSource.Commands.MessageColumn.ColumnName} <>'{DefaulSocialMsg}' AND {_DataSource.Commands.IsEnabledColumn.ColumnName}=True");
                for (int i = 0; i < commandsRows.Length; i++)
                {
                    result += (i != 0 ? ", " : "") + "!" + commandsRows[i].CmdName;
                }
            }

            return result;
        }

        public object PerformQuery(CommandData row, string ParamValue)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Perform the query for command {row.CmdName}.");
#endif

            object output;
            //CommandParams query = CommandParams.Parse(row.Params);

            lock (GUIDataManagerLock.Lock)
            {
                DataRow result = GetRows(_DataSource.Tables[row.Table], $"{row.Key_field}='{ParamValue}'").FirstOrDefault();

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Perform the multi object query for command {row.CmdName}.");
#endif
            
            List<Tuple<object, object>> outlist = null;
            lock (GUIDataManagerLock.Lock)
            {
               outlist = new(from DataRow d in GetRows(_DataSource.Tables[row.Table], Sort: Top < 0 ? null : row.Key_field + " " + row.Sort)
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all the timer commands.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return new(from CommandsRow row in (CommandsRow[])GetRows(_DataSource.Commands, "RepeatTimer>0 AND IsEnabled=True") select new Tuple<string, int, string[]>(row.CmdName, row.RepeatTimer, row.Category?.Split(',', StringSplitOptions.TrimEntries) ?? Array.Empty<string>()));
            }
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get timer command {Cmd}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                CommandsRow row = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{Cmd}'");
                return (row == null) ? null : new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>());
            }
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Set the enable as {Enabled} for all the system events.");
#endif

            SetDataTableFieldRows(_DataSource.ChannelEvents, _DataSource.ChannelEvents.IsEnabledColumn, Enabled);
        }

        private static string ComFilter()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get the list of default commands and socials, to filter out user-defined commands.");
#endif

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Set the enable as {Enabled} for all the built-in commands.");
#endif

            SetDataTableFieldRows(_DataSource.Commands, _DataSource.Commands.IsEnabledColumn, Enabled, "CmdName IN (" + ComFilter() + ")");
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Set the enable as {Enabled} for all the user-defined commands.");
#endif

            SetDataTableFieldRows(_DataSource.Commands, _DataSource.Commands.IsEnabledColumn, Enabled, "CmdName NOT IN (" + ComFilter() + ")");
        }

        public void SetDiscordWebhooksEnabled(bool Enabled)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Set the enable as {Enabled} for all the Discord webhooks.");
#endif

            SetDataTableFieldRows(_DataSource.Discord, _DataSource.Discord.IsEnabledColumn, Enabled);
        }

        public void SetIsEnabled(IEnumerable<DataRow> dataRows, bool IsEnabled = false)
        {
            foreach(DataRow dr in dataRows)
            {
                if (CheckField(dr.Table.TableName, "IsEnabled"))
                {
                    SetDataRowFieldRow(dr, "IsEnabled", IsEnabled);
                }
            }
        }

        public List<string> GetCurrencyNames()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all of the currency names.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return GetRowsDataColumn(_DataSource.CurrencyType, _DataSource.CurrencyType.CurrencyNameColumn).ConvertAll((value) => value.ToString());
            }
        }

        #endregion

        #region Stream Statistics
        private StreamStatsRow CurrStreamStatRow;

        public StreamStatsRow[] GetAllStreamData()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all stream data.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return (StreamStatsRow[])GetRows(_DataSource.StreamStats);
            }
        }

        private StreamStatsRow GetAllStreamData(DateTime dateTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Find stream data for the {dateTime} date time.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                return (from StreamStatsRow streamStatsRow in GetAllStreamData()
                        where streamStatsRow.StreamStart == dateTime
                        select streamStatsRow).FirstOrDefault();
            }
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get the content stats for a particular stream dated {dateTime}.");
#endif

            lock (GUIDataManagerLock.Lock)
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
        }

        public bool CheckMultiStreams(DateTime dateTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if there are multiple streams for {dateTime}.");
#endif

            return (from StreamStatsRow row in GetAllStreamData()
                    where row.StreamStart.ToShortDateString() == dateTime.ToShortDateString()
                    select row).Count() > 1;
        }

        public bool AddStream(DateTime StreamStart)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add a new stream for {StreamStart}, checking if one already exists.");
#endif

            bool returnvalue;

            if (StreamStart == DateTime.MinValue.ToLocalTime() || CheckStreamTime(StreamStart))
            {
                returnvalue = false;
            }
            else
            {
                CurrStreamStart = StreamStart;

                lock(GUIDataManagerLock.Lock)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    NotifySaveData();
                    returnvalue = true;
                }
            }
            return returnvalue;
        }

        public void PostStreamStat(StreamStat streamStat)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Update stream stats for {streamStat.StreamStart} stream.");
#endif

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if there already is a stream for {CurrTime}.");
#endif

            return GetAllStreamData(CurrTime) != null;
        }

        /// <summary>
        /// Remove all stream stats, to satisfy a user option selection to not track stats
        /// </summary>
        public void RemoveAllStreamStats()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Remove all stream data.");
#endif

            DeleteDataRows(GetRows(_DataSource.StreamStats));
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Update for a user joined, user {User} at {NowSeen}.");
#endif

            static DateTime Max(DateTime A, DateTime B) => A <= B ? B : A;

            lock(GUIDataManagerLock.Lock)
            {
                UsersRow userrow = AddNewUser(User, NowSeen);
                userrow.CurrLoginDate = Max(userrow.CurrLoginDate, NowSeen);
                userrow.LastDateSeen = Max(userrow.LastDateSeen, NowSeen);
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check for a custom user welcome message for user {User}.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                return ((CustomWelcomeRow)GetRow(_DataSource.CustomWelcome, Filter: $"{_DataSource.CustomWelcome.UserNameColumn.ColumnName}='{User}'"))?.Message ?? "";
            }
        }

        public void UserLeft(LiveUser User, DateTime LastSeen)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Update the user {User} has left, at {LastSeen}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User}' AND {_DataSource.Users.PlatformColumn.ColumnName}='{User.Source}'");
                if (user != null)
                {
                    UpdateWatchTime(ref user, LastSeen); // will update the "LastDateSeen"
                    if (OptionFlags.TwitchCurrencyStart && (OptionFlags.TwitchCurrencyOnline && OptionFlags.IsStreamOnline))
                    {
                        UpdateCurrency(ref user, LastSeen);
                    } // will update the "CurrLoginDate"
                }
            }
        }

        private void UpdateWatchTime(ref UsersRow User, DateTime CurrTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Update the user's watchtime, for {User.UserName} at {CurrTime}.");
#endif

            if (User != null)
            {
                lock(GUIDataManagerLock.Lock)
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
                }
            }
        }

        /// <summary>
        /// Accepts the string version of a UserName and will look up the user in the database.
        /// </summary>
        /// <param name="UserName">String of the UserName to update the watchtime.</param>
        /// <param name="CurrTime">The Current Time to compare against for updating the watch time.</param>
        private void UpdateWatchTime(string UserName, DateTime CurrTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Specifically update watch time for user {UserName} and at {CurrTime}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)_DataSource.Users.Select($"{_DataSource.Users.UserNameColumn.ColumnName}='{UserName}'").FirstOrDefault();
                UpdateWatchTime(ref user, CurrTime);
            }
        }

        public void UpdateWatchTime(List<string> Users, DateTime CurrTime)
        {
            lock (GUIDataManagerLock.Lock)
            {
                foreach (string U in Users)
                {
                    UpdateWatchTime(U, CurrTime);
                }
                NotifySaveData();
            }
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><c>true</c> if the user has arrived prior to DateTime.MaxValue, <c>false</c> otherwise.</returns>
        public bool CheckUser(LiveUser User)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if user {User} has already been in the channel.");
#endif

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if user {User} has arrived before {ToDateTime}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                UsersRow user = (UsersRow)_DataSource.Users.Select($"{_DataSource.Users.UserNameColumn.ColumnName}='{User.UserName}' AND ({_DataSource.Users.PlatformColumn.ColumnName}='{User.Source.ToString()}' OR {_DataSource.Users.PlatformColumn.ColumnName} is NULL)").FirstOrDefault();

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if {User} is a current follower.");
#endif

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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Check if user {User} followed prior to {ToDateTime}, for the follower welcome back notice.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                FollowersRow datafollowers = (FollowersRow)_DataSource.Followers.Select($"{_DataSource.Followers.UserNameColumn.ColumnName}='{User}'").FirstOrDefault();

                return datafollowers != null
                    && datafollowers.IsFollower
                    && datafollowers.FollowedDate <= ToDateTime;
            }
        }

        /// <summary>
        /// Add a new follower to the data table.
        /// </summary>
        /// <param name="User">The Username of the new Follow</param>
        /// <param name="FollowedDate">The date of the Follow.</param>
        /// <returns>True if the follower is the first time. False if already followed.</returns>
        public bool AddFollower(LiveUser User, DateTime FollowedDate)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add user {User} as a new follower at {FollowedDate}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                bool newfollow;

                UsersRow users = AddNewUser(User, FollowedDate);
                FollowersRow followers = (FollowersRow)GetRow(_DataSource.Followers, $"{_DataSource.Followers.UserNameColumn.ColumnName}='{User.UserName}'");

                if (followers != null)
                {
                    // newfollow = !followers.IsFollower;
                    newfollow = false;
                    followers.IsFollower = true;
                    followers.FollowedDate = FollowedDate;


                    if (followers.UserId == null && User.UserId != string.Empty && User.UserId != null)
                    {
                        followers.UserId = User.UserId;
                    }
                    if (followers.Platform == string.Empty || followers.Platform == null)
                    {
                        followers.Platform = User.Source.ToString();
                    }
                }
                else
                {
                    newfollow = true;
                    _DataSource.Followers.AddFollowersRow(users, users.UserName, true, FollowedDate, User.UserId, User.Source.ToString());
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
        private UsersRow AddNewUser(LiveUser User, DateTime FirstSeen)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add a new user {User}, first seen at {FirstSeen}.");
#endif

            UsersRow usersRow = null;

            if (!CheckUser(User))
            {
                lock (GUIDataManagerLock.Lock)
                {
                    usersRow = _DataSource.Users.AddUsersRow(User.UserName, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero, User.UserId, User.Source.ToString());
                    //AddCurrencyRows(ref usersRow);
                }
            }

            // if the user is added to list before identified as follower, update first seen date to followed date
            lock(GUIDataManagerLock.Lock)
            {
                usersRow = (UsersRow)GetRow(_DataSource.Users,$"{_DataSource.Users.UserNameColumn.ColumnName}='{User.UserName}'");

                if (FirstSeen <= usersRow.FirstDateSeen)
                {
                    usersRow.FirstDateSeen = FirstSeen;
                }

                if(usersRow.UserId == null && User.UserId != null)
                {
                    usersRow.UserId = User.UserId;
                }
                if(usersRow.Platform == null)
                {
                    usersRow.Platform = User.Source.ToString();
                }
            }

            NotifySaveData();
            return usersRow;
        }

        public void StartFollowers()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Start updating followers in bulk, set all as false to then mark as a follower.");
#endif

            UpdatingFollowers = true;
            lock(GUIDataManagerLock.Lock)
            {
                List<FollowersRow> temp = new();
                temp.AddRange((FollowersRow[])GetRows(_DataSource.Followers));
                temp.ForEach((f) => f.IsFollower = false);
            }
            NotifySaveData();
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Update and add all followers in bulk.");
#endif

            if (follows.Any())
            {
                foreach (Follow f in follows)
                {
                    _ = AddFollower(f.FromUser, f.FollowedAt);
                }
            }

            NotifySaveData();
        }

        public void StopBulkFollows()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Stop bulk updating all followers.");
#endif

            if (OptionFlags.TwitchPruneNonFollowers)
            {
                lock(GUIDataManagerLock.Lock)
                {
                    List<FollowersRow> temp = new();
                    temp.AddRange((FollowersRow[])GetRows(_DataSource.Followers));
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Clear all watch time.");
#endif

            SetDataTableFieldRows(_DataSource.Users, _DataSource.Users.WatchTimeColumn, new TimeSpan(0));
        }

        public void AddNewAutoShoutUser(string UserName)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Adding user {UserName} to the auto shout-out listing.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                if (GetRow(_DataSource.ShoutOuts, $"{_DataSource.ShoutOuts.UserNameColumn.ColumnName}='{UserName}'") == null)
                {
                    _DataSource.ShoutOuts.AddShoutOutsRow(UserName);
                }
            }
        }

        #endregion Users and Followers

        #region Giveaways
        public void PostGiveawayData(string DisplayName, DateTime dateTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Posting the giveaway data.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                _ = _DataSource.GiveawayUserData.AddGiveawayUserDataRow(DisplayName, dateTime);
            }
            NotifySaveData();
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Update currency for user {User}.");
#endif
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Realize the currency accruals for user {User.UserName}.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                if (User != null)
                {
                    TimeSpan currencyclock = CurrTime - User.CurrLoginDate; // the amount of time changed for the currency accrual calculation

                    double ComputeCurrency(double Accrue, double Seconds)
                    {
                        return Accrue * (currencyclock.TotalSeconds / Seconds);
                    }

                    AddCurrencyRows(ref User);

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
        public void AddCurrencyRows(ref UsersRow usersRow)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add all currency rows for user {usersRow.UserName}.");
#endif
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

        /// <summary>
        /// For every user in the database, add currency rows for each currency type - add missing rows.
        /// </summary>
        public void AddCurrencyRows()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add currency for all users.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                UsersRow[] UserRows = (UsersRow[])GetRows(_DataSource.Users);

                for (int i = 0; i < UserRows.Length; i++)
                {
                    UsersRow users = UserRows[i];
                    AddCurrencyRows(ref users);
                }
                NotifySaveData();
            }
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        public void ClearUsersNotFollowers()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Clear users who are not followers.");
#endif
            List<string> RemoveIds = new();

            lock (GUIDataManagerLock.Lock)
            {
                foreach (UsersRow U in GetRows(_DataSource.Users))
                {
                    if (_DataSource.Followers.Select($"{_DataSource.Followers.IdColumn.ColumnName}='{U.Id}'").FirstOrDefault() == null)
                    {
                        RemoveIds.Add(U.Id.ToString());
                    }
                }

                foreach (string Id in RemoveIds)
                {
                    ((UsersRow)_DataSource.Users.Select($"{_DataSource.Users.IdColumn.ColumnName}='{Id}'").FirstOrDefault()).Delete();
                }
            }
            NotifySaveData();
        }

        /// <summary>
        /// Empty every currency to 0, for all users for all currencies.
        /// </summary>
        public void ClearAllCurrencyValues()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Clear all currency values.");
#endif

            SetDataTableFieldRows(_DataSource.Currency, _DataSource.Currency.ValueColumn, 0);
        }

        #endregion

        #region Raid Data
        public void PostInRaidData(string user, DateTime time, string viewers, string gamename)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Post incoming raid data.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                _ = _DataSource.InRaidData.AddInRaidDataRow(user, viewers, time, gamename);
            }
            NotifySaveData();
        }

        public bool TestInRaidData(string user, DateTime time, string viewers, string gamename)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Test the incoming raid data, code testing if raid data was added.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                // 2021-12-06T01:19:16.0248427-05:00
                //string.Format("UserName='{0}' and DateTime='{1}' and ViewerCount='{2}' and Category='{3}'", user, time, viewers, gamename)
                return (InRaidDataRow)GetRow(_DataSource.InRaidData, $"{_DataSource.InRaidData.UserNameColumn.ColumnName}='{user}' AND {_DataSource.InRaidData.DateTimeColumn.ColumnName}=#{time:O}# AND {_DataSource.InRaidData.ViewerCountColumn.ColumnName}='{viewers}' AND {_DataSource.InRaidData.CategoryColumn.ColumnName}='{gamename}'") != null;
            }
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Test the outgoing raid data, code testing if raid data was added.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                // string.Format("ChannelRaided='{0}' and DateTime='{1}'", HostedChannel, dateTime)
                return (OutRaidDataRow)GetRow(_DataSource.OutRaidData, $"{_DataSource.OutRaidData.ChannelRaidedColumn.ColumnName}='{HostedChannel}' AND {_DataSource.OutRaidData.DateTimeColumn.ColumnName}=#{dateTime:O}#") != null;
            }
        }

        public void PostOutgoingRaid(string HostedChannel, DateTime dateTime)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Post the outgoing raid data.");
#endif

            lock (GUIDataManagerLock.Lock)
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get all the available Discord webhooks, for the {webhooks} type.");
#endif

            return new(((DiscordRow[])GetRows(_DataSource.Discord, $"{_DataSource.Discord.KindColumn.ColumnName}='{webhooks}' AND {_DataSource.Discord.IsEnabledColumn.ColumnName}=True")).Select(d => new Tuple<bool, Uri>(d.AddEveryone, new Uri(d.Webhook))));
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add and update the {newCategory} category.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                CategoryListRow categoryList = (CategoryListRow)GetRow(_DataSource.CategoryList, $"{_DataSource.CategoryList.CategoryColumn.ColumnName}='{FormatData.AddEscapeFormat(newCategory)}' OR {_DataSource.CategoryList.CategoryIdColumn.ColumnName}='{CategoryId}'");

                if (categoryList == null)
                {
                    _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory, 1);
                    NotifySaveData();
                }
                else
                {
                    if (categoryList.CategoryId == null)
                    {
                        categoryList.CategoryId = CategoryId;
                    }
                    if (categoryList.Category == null)
                    {
                        categoryList.Category = newCategory;
                    }

                    if (OptionFlags.IsStreamOnline)
                    {
                        categoryList.StreamCount++;
                    }
                    NotifySaveData();

                }

                return categoryList != null;
            }
        }

        /// <summary>
        /// Retrieves all GameIds and GameCategories added to the database.
        /// </summary>
        /// <returns>Returns a list of <code>Tuple<string GameId, string GameName></code> objects.</returns>
        public List<Tuple<string, string>> GetGameCategories()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Get a list of all categories.");
#endif
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
        public bool AddClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Add a new clip.");
#endif

            bool result = false;
            lock (GUIDataManagerLock.Lock)
            {
                if (!((ClipsRow[])GetRows(_DataSource.Clips, $"{_DataSource.Clips.IdColumn.ColumnName}='{ClipId}'")).Any())
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime(), Title, GameId, Language, (decimal)Duration, Url);
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
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Machine learning, setting learned messages.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                if (!GetRows(_DataSource.LearnMsgs).Any())
                {
                    foreach (LearnedMessage M in LearnedMessagesPrimer.PrimerList)
                    {
                        _DataSource.LearnMsgs.AddLearnMsgsRow(M.MsgType.ToString(), M.Message);
                    }
                }

                if (!GetRows(_DataSource.BanReasons).Any())
                {
                    foreach (BanReason B in LearnedMessagesPrimer.BanReasonList)
                    {
                        _DataSource.BanReasons.AddBanReasonsRow(B.MsgType.ToString(), B.Reason.ToString());
                    }
                }

                if (!GetRows(_DataSource.BanRules).Any())
                {
                    foreach (BanViewerRule BVR in LearnedMessagesPrimer.BanViewerRulesList)
                    {
                        _DataSource.BanRules.AddBanRulesRow(BVR.ViewerType.ToString(), BVR.MsgType.ToString(), BVR.ModAction.ToString(), BVR.TimeoutSeconds);
                    }
                }
                NotifySaveData();
            }
        }

        private void LearnMsgs_LearnMsgsRowDeleted(object sender, LearnMsgsRowChangeEvent e)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Machine learning, whether learned message rows are deleted.");
#endif

            LearnMsgChanged = true;
        }

//        private void LearnMsgs_LearnMsgsRowChanged(object sender, LearnMsgsRowChangeEvent e)
//        {
//#if LogDataManager_Actions
//            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Machine learning, whether learned message rows are changed.");
//#endif

//            LearnMsgChanged = true;
//        }

        private void LearnMsgs_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Machine learning, whether adding a new learned message.");
#endif

            LearnMsgChanged = true;
        }

        public List<LearnMsgsRow> UpdateLearnedMsgs()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Machine learning, get all the learned messages to update training model.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                if (LearnMsgChanged)
                {
                    LearnMsgChanged = false;
                    return new((LearnMsgsRow[])GetRows(_DataSource.LearnMsgs));
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddLearnMsgsRow(string Message, MsgTypes MsgType)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Adding a new learned message row.");
#endif
            lock (GUIDataManagerLock.Lock)
            {
                bool found = (from LearnMsgsRow learnMsgsRow in GetRows(_DataSource.LearnMsgs, $"{_DataSource.LearnMsgs.TeachingMsgColumn.ColumnName}='{FormatData.AddEscapeFormat(Message)}'")
                              select new { }).Any();

                if (!found)
                {
                    _DataSource.LearnMsgs.AddLearnMsgsRow(MsgType.ToString(), Message);

                    NotifySaveData();
                }
            }
        }

        public Tuple<ModActions, BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Machine learning, find the remedy to the found message determination.");
#endif

            lock (GUIDataManagerLock.Lock)
            {
                BanReasons banReason;

                BanReasonsRow banrow = (BanReasonsRow)GetRow(_DataSource.BanReasons, $"{_DataSource.BanReasons.MsgTypeColumn.ColumnName}='{msgTypes}'");

                banReason = banrow != null ? (BanReasons)Enum.Parse(typeof(BanReasons), banrow.BanReason) : BanReasons.None;

                BanRulesRow banRulesRow = (BanRulesRow)GetRow(_DataSource.BanRules, $"{_DataSource.BanRules.ViewerTypesColumn.ColumnName}='{viewerTypes}' and {_DataSource.BanRules.MsgTypeColumn.ColumnName}='{msgTypes}'");

                int Timeout = banRulesRow == null ? 0 : int.Parse(banRulesRow.TimeoutSeconds);
                ModActions action = banRulesRow == null ? ModActions.Allow : (ModActions)Enum.Parse(typeof(ModActions), banRulesRow.ModAction);

                return new(action, banReason, Timeout);
            }
        }



        #endregion

        #region Media Overlay Service

        public List<OverlayActionType> GetOverlayActions(string overlayType, string overlayAction, string username)
        {
            lock (GUIDataManagerLock.Lock)
            {
                List<OverlayActionType> found = new(from OverlayServicesRow overlayServicesRow in GetRows(_DataSource.OverlayServices, Filter: $"{_DataSource.OverlayServices.IsEnabledColumn.ColumnName}=true AND {_DataSource.OverlayServices.OverlayTypeColumn.ColumnName}='{overlayType}' AND ({_DataSource.OverlayServices.UserNameColumn.ColumnName}='' OR {_DataSource.OverlayServices.UserNameColumn.ColumnName}='{username}')") select new OverlayActionType() { ActionValue = overlayServicesRow.OverlayAction, Duration = overlayServicesRow.Duration, MediaFile = overlayServicesRow.MediaFile, ImageFile = overlayServicesRow.ImageFile, Message = overlayServicesRow.Message, OverlayType = (MediaOverlayServer.Enums.OverlayTypes)Enum.Parse(typeof(MediaOverlayServer.Enums.OverlayTypes), overlayServicesRow.OverlayType), UserName = overlayServicesRow.UserName, UseChatMsg = overlayServicesRow.UseChatMsg });

                List<OverlayActionType> result = new();

                foreach(OverlayActionType OAT in found)
                {
                    if(OAT.ActionValue == overlayAction)
                    {
                        result.Add(OAT);
                    }
                }

                return result;
            }
        }


        #endregion

        #region Remove Data
        /// <summary>
        /// Remove all Users from the database.
        /// </summary>
        public void RemoveAllUsers()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Removing all users.");
#endif

            DeleteDataRows(GetRows(_DataSource.Users));
        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        public void RemoveAllFollowers()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Removing all followers.");
#endif

            DeleteDataRows(GetRows(_DataSource.Followers));
        }

        /// <summary>
        /// Removes all incoming raid data from database.
        /// </summary>
        public void RemoveAllInRaidData()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Removing all incoming raid data.");
#endif

            DeleteDataRows(GetRows(_DataSource.InRaidData));
        }

        /// <summary>
        /// Removes all outgoing raid data from database.
        /// </summary>
        public void RemoveAllOutRaidData()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Removing all outgoing raid data.");
#endif

            DeleteDataRows(GetRows(_DataSource.OutRaidData));
        }

        /// <summary>
        /// Removes all Giveaway table data from the database.
        /// </summary>
        public void RemoveAllGiveawayData()
        {
#if LogDataManager_Actions
            LogWriter.DataActionLog(MethodBase.GetCurrentMethod().Name, $"Removing all giveaway data.");
#endif

            DeleteDataRows(GetRows(_DataSource.GiveawayUserData));
        }

        #endregion

    }
}
