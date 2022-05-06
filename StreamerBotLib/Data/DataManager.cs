using StreamerBotLib.Enums;
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
        public static readonly string DataFileXML = "ChatDataStore.xml";

#if DEBUG
        public static readonly string DataFileName = Path.Combine(@"C:\Source\ChatBotApp\StreamerBot\bin\Debug\net5.0-windows7.0", DataFileXML);
#else
        private static readonly string DataFileName = DataFileXML;
#endif

        internal readonly DataSource _DataSource;
        #endregion DataSource


        private bool LearnMsgChanged = true; // always true to begin one learning cycle

        public bool UpdatingFollowers { get; set; }

        public DataManager()
        {
            BackupSaveToken = DateTime.Now.Minute / BackupSaveIntervalMins;

            _DataSource = new();
            _DataSource.BeginInit();
            LoadData();
            _DataSource.EndInit();
            OnSaveData += SaveData;

            _DataSource.LearnMsgs.TableNewRow += LearnMsgs_TableNewRow;
            _DataSource.LearnMsgs.LearnMsgsRowChanged += LearnMsgs_LearnMsgsRowChanged;
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
            string Msg = "";

            lock (_DataSource)
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
            CommandsRow row = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{cmd}'");

            if (row != null)
            {
                ViewerTypes cmdpermission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), row.Permission);

                return cmdpermission >= permission;
            }
            else
                throw new InvalidOperationException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionInvalidOpCommand));
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        /// <remarks>Thread-safe</remarks>
        public bool CheckShoutName(string UserName)
        {
            return GetRow(_DataSource.ShoutOuts, $"{_DataSource.ShoutOuts.UserNameColumn.ColumnName}='{UserName}'") != null;
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

            CommandsRow commandsRow = (CommandsRow)GetRow(_DataSource.Commands, cmd);
            lock (_DataSource)
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
            return DeleteDataRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{command}'");
        }

        public string GetSocials()
        {
            string filter = "";

            System.Collections.IList list = Enum.GetValues(typeof(DefaultSocials));
            for (int i = 0; i < list.Count; i++)
            {
                DefaultSocials s = (DefaultSocials)list[i];
                filter += $"{(i != 0 ? ", " : "")}'{s}'";
            }

            string socials = "";

            foreach (CommandsRow com in from CommandsRow com in GetRows(_DataSource.Commands, $"CmdName IN ({filter})")
                                        where com.Message != DefaulSocialMsg && com.Message != string.Empty
                                        select com)
            {
                socials += com.Message + " ";
            }

            return socials.Trim();
        }

        public string GetUsage(string command)
        {
            return GetCommand(command)?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
        }

        public CommandsRow GetCommand(string cmd)
        {
            return (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{cmd}'");
        }

        public string GetCommands()
        {
            CommandsRow[] commandsRows = (CommandsRow[])GetRows(_DataSource.Commands, $"{_DataSource.Commands.MessageColumn.ColumnName} <>'{DefaulSocialMsg}' AND {_DataSource.Commands.IsEnabledColumn.ColumnName}=True");

            string result = "";

            lock (_DataSource)
            {
                for (int i = 0; i < commandsRows.Length; i++)
                {
                    result += (i != 0 ? ", " : "") + "!" + commandsRows[i].CmdName;
                }
            }

            return result;
        }

        public object PerformQuery(CommandsRow row, string ParamValue)
        {
            object output;
            //CommandParams query = CommandParams.Parse(row.Params);

            DataRow result = GetRows(_DataSource.Tables[row.table], $"{row.key_field}='{ParamValue}'").FirstOrDefault();

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
                    output = result[row.data_field];
                }
            }

            return output;
        }

        public object[] PerformQuery(CommandsRow row, int Top = 0)
        {
            List<Tuple<object, object>> outlist = new(from DataRow d in GetRows(_DataSource.Tables[row.table], Sort: Top < 0 ? null : row.key_field + " " + row.sort)
                                                      select new Tuple<object, object>(d[row.key_field], d[row.data_field]));

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
            return new(from CommandsRow row in (CommandsRow[])GetRows(_DataSource.Commands, "RepeatTimer>0 AND IsEnabled=True") select new Tuple<string, int, string[]>(row.CmdName, row.RepeatTimer, row.Category?.Split(',', StringSplitOptions.TrimEntries) ?? Array.Empty<string>()));
        }

        public Tuple<string, int, string[]> GetTimerCommand(string Cmd)
        {
            CommandsRow row = (CommandsRow)GetRow(_DataSource.Commands, $"{_DataSource.Commands.CmdNameColumn.ColumnName}='{Cmd}'");
            lock (_DataSource)
            {
                return (row == null) ? null : new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>());
            }
        }

        public void SetSystemEventsEnabled(bool Enabled)
        {
            SetDataTableFieldRows(_DataSource.ChannelEvents, _DataSource.ChannelEvents.IsEnabledColumn, Enabled);
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

        public void SetBuiltInCommandsEnabled(bool Enabled)
        {
            SetDataTableFieldRows(_DataSource.Commands, _DataSource.Commands.IsEnabledColumn, Enabled, "CmdName IN (" + ComFilter() + ")");
        }

        public void SetUserDefinedCommandsEnabled(bool Enabled)
        {
            SetDataTableFieldRows(_DataSource.Commands, _DataSource.Commands.IsEnabledColumn, Enabled, "CmdName NOT IN (" + ComFilter() + ")");
        }

        public void SetDiscordWebhooksEnabled(bool Enabled)
        {
            SetDataTableFieldRows(_DataSource.Discord, _DataSource.Discord.IsEnabledColumn, Enabled);
        }

        public List<string> GetCurrencyNames()
        {
            return GetRowsDataColumn(_DataSource.CurrencyType, _DataSource.CurrencyType.CurrencyNameColumn).ConvertAll((value) => value.ToString());
        }

        #endregion



        #region Stream Statistics
        private StreamStatsRow CurrStreamStatRow;

        public StreamStatsRow[] GetAllStreamData()
        {
            return (StreamStatsRow[])GetRows(_DataSource.StreamStats);
        }

        private StreamStatsRow GetAllStreamData(DateTime dateTime)
        {
            lock (_DataSource)
            {
                return (from StreamStatsRow streamStatsRow in GetAllStreamData()
                        where streamStatsRow.StreamStart == dateTime
                        select streamStatsRow).FirstOrDefault();
            }
        }

        public StreamStat GetStreamData(DateTime dateTime)
        {
            StreamStatsRow streamStatsRow = GetAllStreamData(dateTime);
            lock (_DataSource)
            {
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
            return (from StreamStatsRow row in GetAllStreamData()
                    where row.StreamStart.ToShortDateString() == dateTime.ToShortDateString()
                    select row).Count() > 1;
        }

        public bool AddStream(DateTime StreamStart)
        {
            bool returnvalue;

            if (StreamStart == DateTime.MinValue.ToLocalTime() || CheckStreamTime(StreamStart))
            {
                returnvalue = false;
            }
            else
            {
                CurrStreamStart = StreamStart;

                lock (_DataSource)
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
            return GetAllStreamData(CurrTime) != null;
        }

        /// <summary>
        /// Remove all stream stats, to satisfy a user option selection to not track stats
        /// </summary>
        public void RemoveAllStreamStats()
        {
            DeleteDataRows(GetRows(_DataSource.StreamStats));
        }

        #endregion

        #region Users and Followers

        private static DateTime CurrStreamStart { get; set; }

        public void UserJoined(string User, DateTime NowSeen)
        {
            static DateTime Max(DateTime A, DateTime B) => A <= B ? B : A;

            lock (_DataSource)
            {
                UsersRow user = AddNewUser(User, NowSeen);
                user.CurrLoginDate = Max(user.CurrLoginDate, NowSeen);
                user.LastDateSeen = Max(user.LastDateSeen, NowSeen);
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
            return ((CustomWelcomeRow)GetRow(_DataSource.CustomWelcome, Filter: $"{_DataSource.CustomWelcome.UserNameColumn.ColumnName}='{User}'"))?.Message ?? "";
        }

        public void UserLeft(string User, DateTime LastSeen)
        {
            UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User}'");
            if (user != null)
            {
                UpdateWatchTime(ref user, LastSeen); // will update the "LastDateSeen"
                UpdateCurrency(ref user, LastSeen); // will update the "CurrLoginDate"
            }
        }

        public void UpdateWatchTime(ref UsersRow User, DateTime CurrTime)
        {
            if (User != null)
            {
                lock (_DataSource)
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
                UsersRow user = (UsersRow)_DataSource.Users.Select($"{_DataSource.Users.UserNameColumn.ColumnName}='{UserName}'").FirstOrDefault();
                UpdateWatchTime(ref user, CurrTime);
            }
        }

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
                UsersRow user = (UsersRow)_DataSource.Users.Select($"{_DataSource.Users.UserNameColumn.ColumnName}='{User}'").FirstOrDefault();

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
        public bool AddFollower(string User, DateTime FollowedDate)
        {
            lock (_DataSource)
            {
                bool newfollow;

                UsersRow users = AddNewUser(User, FollowedDate);
                FollowersRow followers = (FollowersRow)_DataSource.Followers.Select($"{_DataSource.Followers.UserNameColumn.ColumnName}='{User}'").FirstOrDefault();

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
                }
            }

            // if the user is added to list before identified as follower, update first seen date to followed date
            lock (_DataSource)
            {
                usersRow = (UsersRow)_DataSource.Users.Select($"{_DataSource.Users.UserNameColumn.ColumnName}='{User}'").First();

                if (FirstSeen <= usersRow.FirstDateSeen)
                {
                    usersRow.FirstDateSeen = FirstSeen;
                }
            }

            NotifySaveData();
            return usersRow;
        }

        public void StartFollowers()
        {
            UpdatingFollowers = true;
            lock (_DataSource)
            {
                List<FollowersRow> temp = new();
                temp.AddRange((FollowersRow[])GetRows(_DataSource.Followers));
                temp.ForEach((f) => f.IsFollower = false);
            }
            NotifySaveData();
        }

        public void UpdateFollowers(IEnumerable<Follow> follows)
        {
            if (follows.Any())
            {
                foreach (Follow f in follows)
                {
                    _ = AddFollower(f.FromUserName, f.FollowedAt);
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
            SetDataTableFieldRows(_DataSource.Users, _DataSource.Users.WatchTimeColumn, new TimeSpan(0));
        }

        public void AddNewAutoShoutUser(string UserName)
        {
            if(GetRow(_DataSource.ShoutOuts,$"{_DataSource.ShoutOuts.UserNameColumn.ColumnName}='{UserName}'") == null)
            {
                _DataSource.ShoutOuts.AddShoutOutsRow(UserName);
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
            UsersRow user = (UsersRow)GetRow(_DataSource.Users, $"{_DataSource.Users.UserNameColumn.ColumnName}='{User}'");
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

                    CurrencyTypeRow[] currencyType = (CurrencyTypeRow[])GetRows(_DataSource.CurrencyType);
                    CurrencyRow[] userCurrency = (CurrencyRow[])GetRows(_DataSource.Currency, $"{_DataSource.Currency.IdColumn.ColumnName}='{User.Id}'");

                    foreach ((CurrencyTypeRow typeRow, CurrencyRow currencyRow) in currencyType.SelectMany(typeRow => userCurrency.Where(currencyRow => currencyRow.CurrencyName == typeRow.CurrencyName).Select(currencyRow => (typeRow, currencyRow))))
                    {
                        currencyRow.Value = Math.Min(Math.Round(currencyRow.Value + ComputeCurrency(typeRow.AccrueAmt, typeRow.Seconds), 2), typeRow.MaxValue);

                    }

                    // set the current login date, always set regardless if currency accrual is started
                    User.CurrLoginDate = CurrTime;
                    NotifySaveData();
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
            NotifySaveData();
        }

        /// <summary>
        /// For every user in the database, add currency rows for each currency type - add missing rows.
        /// </summary>
        public void AddCurrencyRows()
        {
            UsersRow[] UserRows = (UsersRow[])GetRows(_DataSource.Users);

            for (int i = 0; i < UserRows.Length; i++)
            {
                UsersRow users = UserRows[i];
                AddCurrencyRows(ref users);
            }
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        public void ClearUsersNotFollowers()
        {
            List<string> RemoveIds = new();

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
            NotifySaveData();
        }

        /// <summary>
        /// Empty every currency to 0, for all users for all currencies.
        /// </summary>
        public void ClearAllCurrencyValues()
        {
            SetDataTableFieldRows(_DataSource.Currency, _DataSource.Currency.ValueColumn, 0);
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
                return (InRaidDataRow)GetRow(_DataSource.InRaidData, $"{_DataSource.InRaidData.UserNameColumn.ColumnName}='{user}' AND {_DataSource.InRaidData.DateTimeColumn.ColumnName}=#{time:O}# AND {_DataSource.InRaidData.ViewerCountColumn.ColumnName}='{viewers}' AND {_DataSource.InRaidData.CategoryColumn.ColumnName}='{gamename}'") != null;
            }
        }

        public bool TestOutRaidData(string HostedChannel, DateTime dateTime)
        {
            lock (_DataSource)
            {
                // string.Format("ChannelRaided='{0}' and DateTime='{1}'", HostedChannel, dateTime)
                return (OutRaidDataRow)GetRow(_DataSource.OutRaidData, $"{_DataSource.OutRaidData.ChannelRaidedColumn.ColumnName}='{HostedChannel}' AND {_DataSource.OutRaidData.DateTimeColumn.ColumnName}=#{dateTime:O}#") != null;
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
            CategoryListRow categoryList = (CategoryListRow)GetRow(_DataSource.CategoryList, $"{_DataSource.CategoryList.CategoryColumn.ColumnName}='{FormatData.AddEscapeFormat(newCategory)}' OR {_DataSource.CategoryList.CategoryIdColumn.ColumnName}='{CategoryId}'");

            if (categoryList == null)
            {
                lock (_DataSource)
                {
                    _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory, 1);
                    NotifySaveData();
                }
            }
            else
            {
                lock (_DataSource)
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
            }

            return categoryList != null;
        }

        /// <summary>
        /// Retrieves all GameIds and GameCategories added to the database.
        /// </summary>
        /// <returns>Returns a list of <code>Tuple<string GameId, string GameName></code> objects.</returns>
        public List<Tuple<string, string>> GetGameCategories()
        {
            return new(from CategoryListRow c in GetRows(_DataSource.CategoryList)
                       orderby c.Category
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

            if (!((ClipsRow[])GetRows(_DataSource.Clips, $"{_DataSource.Clips.IdColumn.ColumnName}='{ClipId}'")).Any())
            {
                lock (_DataSource)
                {
                    _ = _DataSource.Clips.AddClipsRow(ClipId, DateTime.Parse(CreatedAt).ToLocalTime(), Title, GameId, Language, (decimal)Duration, Url);
                    NotifySaveData();
                    result = true;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        #endregion

        #region Machine Learning Moderation

        private void SetLearnedMessages()
        {
            lock (_DataSource)
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
            LearnMsgChanged = true;
        }

        private void LearnMsgs_LearnMsgsRowChanged(object sender, LearnMsgsRowChangeEvent e)
        {
            LearnMsgChanged = true;
        }

        private void LearnMsgs_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            LearnMsgChanged = true;
        }

        public List<LearnMsgsRow> UpdateLearnedMsgs()
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

        public void AddLearnMsgsRow(string Message, MsgTypes MsgType)
        {
            bool found = (from LearnMsgsRow learnMsgsRow in GetRows(_DataSource.LearnMsgs, $"{_DataSource.LearnMsgs.TeachingMsgColumn.ColumnName}='{FormatData.AddEscapeFormat(Message)}'")
                          select new { }).Any();

            if (!found)
            {
                lock (_DataSource)
                {
                    _DataSource.LearnMsgs.AddLearnMsgsRow(MsgType.ToString(), Message);
                }
                NotifySaveData();
            }
        }

        public Tuple<ModActions, BanReasons, int> FindRemedy(ViewerTypes viewerTypes, MsgTypes msgTypes)
        {
            BanReasons banReason;

            BanReasonsRow banrow = (BanReasonsRow)GetRow(_DataSource.BanReasons, $"{_DataSource.BanReasons.MsgTypeColumn.ColumnName}='{msgTypes}'");

            banReason = banrow != null ? (BanReasons)Enum.Parse(typeof(BanReasons), banrow.BanReason) : BanReasons.None;

            BanRulesRow banRulesRow = (BanRulesRow)GetRow(_DataSource.BanRules, $"{_DataSource.BanRules.ViewerTypesColumn.ColumnName}='{viewerTypes}' and {_DataSource.BanRules.MsgTypeColumn.ColumnName}='{msgTypes}'");

            int Timeout = banRulesRow == null ? 0 : int.Parse(banRulesRow.TimeoutSeconds);
            ModActions action = banRulesRow == null ? ModActions.Allow : (ModActions)Enum.Parse(typeof(ModActions), banRulesRow.ModAction);

            return new(action, banReason, Timeout);
        }



        #endregion

        #region Remove Data
        /// <summary>
        /// Remove all Users from the database.
        /// </summary>
        public void RemoveAllUsers()
        {
            DeleteDataRows(GetRows(_DataSource.Users));
        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        public void RemoveAllFollowers()
        {
            DeleteDataRows(GetRows(_DataSource.Followers));
        }

        /// <summary>
        /// Removes all incoming raid data from database.
        /// </summary>
        public void RemoveAllInRaidData()
        {
            DeleteDataRows(GetRows(_DataSource.InRaidData));
        }

        /// <summary>
        /// Removes all outgoing raid data from database.
        /// </summary>
        public void RemoveAllOutRaidData()
        {
            DeleteDataRows(GetRows(_DataSource.OutRaidData));
        }

        /// <summary>
        /// Removes all Giveaway table data from the database.
        /// </summary>
        public void RemoveAllGiveawayData()
        {
            DeleteDataRows(GetRows(_DataSource.GiveawayUserData));
        }

        #endregion

    }
}
