using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Xml;
using System.Linq;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Data
{
    public class DataManager
    {
        #region DataSource

        private static readonly string DataFileName = "ChatDataStore.xml"; // Path.Combine(Directory.GetCurrentDirectory(), "ChatDataStore.xml");
        private DataSource _DataSource;
        private Thread followerThread;

        public bool UpdatingFollowers { get; set; } = false;

        public List<string> KindsWebhooks { get; private set; } = new(Enum.GetNames(typeof(WebhooksKind)));
        public DataView ChannelEvents { get; private set; } // DataSource.ChannelEventsDataTable
        public DataView Users { get; private set; }  // DataSource.UsersDataTable
        public DataView Followers { get; private set; } // DataSource.FollowersDataTable
        public DataView Discord { get; private set; } // DataSource.DiscordDataTable
        public DataView Currency { get; private set; }  // DataSource.CurrencyDataTable
        public DataView CurrencyType { get; private set; }  // DataSource.CurrencyTypeDataTable
        public DataView BuiltInCommands { get; private set; } // DataSource.CommandsDataTable
        public DataView Commands { get; private set; }  // DataSource.CommandsDataTable
        public DataView StreamStats { get; private set; } // DataSource.StreamStatsTable

        #endregion DataSource

        public DataManager()
        {
            string ComFilter()
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

                return filter == string.Empty ? "" : filter.Substring(0, filter.Length - 1);
            }


            _DataSource = new();
            LoadData();

            ChannelEvents = _DataSource.ChannelEvents.DefaultView;
            Users = new(_DataSource.Users, null, "UserName", DataViewRowState.CurrentRows);
            Followers = new(_DataSource.Followers, null, "FollowedDate", DataViewRowState.CurrentRows);
            Discord = _DataSource.Discord.DefaultView;
            CurrencyType = new(_DataSource.CurrencyType, null, "Id", DataViewRowState.CurrentRows);
            Currency = new(_DataSource.Currency, null, "UserName", DataViewRowState.CurrentRows);
            BuiltInCommands = new(_DataSource.Commands, "CmdName IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            Commands = new(_DataSource.Commands, "CmdName NOT IN (" + ComFilter() + ")", "CmdName", DataViewRowState.CurrentRows);
            StreamStats = new(_DataSource.StreamStats, null, "StreamStart", DataViewRowState.CurrentRows);
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        private void LoadData()
        {
            if (!File.Exists(DataFileName))
            {
                _DataSource.WriteXml(DataFileName);
            }

            using (XmlReader xmlreader = new XmlTextReader(DataFileName))
            {
                _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);
            }

            // check all default ChannelEvents names
            SetDefaultChannelEventsTable();

            // check all default Commands
            SetDefaultCommandsTable();

            _DataSource.AcceptChanges();
        }

        /// <summary>
        /// Save data to file upon exit
        /// </summary>
        public void ExitSave()
        {
            _DataSource.AcceptChanges();

            _DataSource.WriteXml(DataFileName, XmlWriteMode.DiffGram);
        }
        #endregion

        #region Regular Channel Events
        /// <summary>
        /// Add default data to Channel Events table, to ensure the data is available to use in event messages.
        /// </summary>
        private void SetDefaultChannelEventsTable()
        {
            bool CheckName(string criteria) => _DataSource.ChannelEvents.FindByName(criteria) == null;

            Dictionary<ChannelEventActions, Tuple<string, string>> dictionary = new()
            {
                { ChannelEventActions.BeingHosted, new("Thanks #user for #autohost this channel!", "#user, #autohost, #viewers") },
                { ChannelEventActions.Bits, new("Thanks #user for giving #bits!", "#user, #bits") },
                { ChannelEventActions.CommunitySubs, new("Thanks #user for giving #count to the community!", "#user, #count, #subplan") },
                { ChannelEventActions.Follow, new("Thanks #user for the follow!", "#user") },
                { ChannelEventActions.GiftSub, new("Thanks #user for gifting a #subplan subscription to #receiveuser!", "#user, #months, #receiveuser, #subplan, #subplanname") },
                { ChannelEventActions.Live, new("@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url", "#user, #category, #title, #url") },
                { ChannelEventActions.Raid, new("Thanks #user for bringing #viewers and raiding the channel!", "#user, #viewers") },
                { ChannelEventActions.Resubscribe, new("Thanks #user for re-subscribing!", "#user, #months, #submonths, #subplan, #subplanname, #streak") },
                { ChannelEventActions.Subscribe, new("Thanks #user for subscribing!", "#user, #submonths, #subplan, #subplanname") },
                { ChannelEventActions.UserJoined, new("Welcome #user! Glad you could make it to the stream. How are you?", "#user") }
            };

            foreach (ChannelEventActions command in Enum.GetValues(typeof(ChannelEventActions)))
            {
                // consider only the values in the dictionary, check if data is already defined in the data table
                if (dictionary.ContainsKey(command) && CheckName(command.ToString()))
                {   // extract the default data from the dictionary and add to the data table
                    Tuple<string, string> values = dictionary[command];
                    lock (_DataSource)
                    {
                        _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), true, values.Item1, values.Item2);
                    }
                }

            }

        }
        #endregion Regular Channel Events

        #region Helpers
        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>Null for no value or the first row found using the <i>rowcriteria</i></returns>
        internal object GetRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
        {
            return GetAllRowData(dataRetrieve, rowcriteria)?[0];
        }

        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>All data found using the <i>rowcriteria</i></returns>
        internal object[] GetAllRowData(DataRetrieve dataRetrieve, ChannelEventActions rowcriteria)
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

        #region Users and Followers

        internal void UserJoined(string User, DateTime NowSeen)
        {
            lock (_DataSource.Users)
            {
                DataSource.UsersRow user = AddNewUser(User, NowSeen);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;
                _DataSource.AcceptChanges();
            }
        }

        internal void UserLeft(string User, DateTime LastSeen)
        {
            lock (_DataSource.Users)
            {
                DataSource.UsersRow user = _DataSource.Users.FindByUserName(User);
                user.LastDateSeen = LastSeen;
                _DataSource.AcceptChanges();
            }
        }

        internal void UpdateWatchTime(string User, DateTime CurrTime)
        {
            lock (_DataSource.Users)
            {
                DataSource.UsersRow user = _DataSource.Users.FindByUserName(User);
                user.WatchTime = user.WatchTime.Add(CurrTime - user.LastDateSeen);
                user.LastDateSeen = CurrTime;
                _DataSource.AcceptChanges();
            }
        }

        internal bool CheckFollower(string User)
        {
            lock (_DataSource.Followers)
            {
                DataRow[] datafollowers = _DataSource.Followers.Select("UserName='" + User + "'");
                DataSource.FollowersRow followers = datafollowers.Length > 0 ? (DataSource.FollowersRow)datafollowers[0] : null;

                if (followers == null)
                {
                    return false;
                }
                else
                {
                    return followers.IsFollower;
                }
            }
        }

        /// <summary>
        /// Add a new follower to the data table.
        /// </summary>
        /// <param name="User">The Username of the new Follow</param>
        /// <param name="FollowedDate">The date of the Follow.</param>
        /// <returns>True if the follower is the first time. False if already followed.</returns>
        internal bool AddFollower(string User, DateTime FollowedDate)
        {
            lock (_DataSource.Followers)
            {
                bool newfollow = false;

                DataSource.UsersRow users = AddNewUser(User, FollowedDate);

                DataRow[] datafollowers = _DataSource.Followers.Select("UserName='" + User + "'");
                DataSource.FollowersRow followers = datafollowers.Length > 0 ? (DataSource.FollowersRow)datafollowers[0] : null;
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
                _DataSource.AcceptChanges();

                return newfollow;
            }
        }

        /// <summary>
        /// Add a new User to the User Table
        /// </summary>
        /// <param name="User">The user name.</param>
        /// <param name="FirstSeen">The first time the user is seen.</param>
        /// <returns>True if the user is added, else false if the user already existed.</returns>
        private DataSource.UsersRow AddNewUser(string User, DateTime FirstSeen)
        {
            lock (_DataSource.Users)
            {
                if (_DataSource.Users.FindByUserName(User) == null)
                {
                    DataSource.UsersRow output = _DataSource.Users.AddUsersRow(User, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero);

                    return output;
                }
            }

            DataSource.UsersRow usersRow = null;
            // if the user is added to list before identified as follower, update first seen date to followed date
            lock (_DataSource.Users)
            {
                usersRow = _DataSource.Users.FindByUserName(User);

                if (DateTime.Compare(usersRow.FirstDateSeen, FirstSeen) > 0)
                {
                    usersRow.FirstDateSeen = FirstSeen;
                }
            }
            return usersRow;
        }

        internal void UpdateFollowers(string ChannelName, Dictionary<string, List<Follow>> follows)
        {
            followerThread = new(new ThreadStart(() =>
            {
                UpdatingFollowers = true; lock (_DataSource.Followers) { List<DataSource.FollowersRow> temp = new(); temp.AddRange((DataSource.FollowersRow[])_DataSource.Followers.Select()); temp.ForEach((f) => f.IsFollower = false); }
                if (follows[ChannelName].Count > 1) { foreach (Follow f in follows[ChannelName]) { AddFollower(f.FromUserName, f.FollowedAt); } }
                _DataSource.AcceptChanges(); UpdatingFollowers = false;
            }));

            followerThread.Start();
        }

        #endregion Users and Followers

        #region Discord and Webhooks
        /// <summary>
        /// Retrieve all the webhooks from the Discord table
        /// </summary>
        /// <returns></returns>
        internal List<Uri> GetWebhooks(WebhooksKind webhooks)
        {
            DataRow[] dataRows = _DataSource.Discord.Select();

            List<Uri> uris = new();

            foreach (DataRow d in dataRows)
            {
                DataSource.DiscordRow row = (d as DataSource.DiscordRow);

                if (row.Kind == webhooks.ToString())
                {
                    uris.Add(new Uri(row.Webhook));
                }
            }
            return uris;
        }
        #endregion Discord and Webhooks

        #region Stream Statistics
        internal void AddStream(DateTime StreamStart)
        {
            lock (_DataSource.StreamStats)
            {
                _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                _DataSource.StreamStats.AcceptChanges();
            }
        }

        internal void PostStreamStat(StreamStat streamStat)
        {
            lock (_DataSource.StreamStats)
            {
                DataSource.StreamStatsRow[] statsRowAll = (DataSource.StreamStatsRow[])_DataSource.StreamStats.Select();

                DataSource.StreamStatsRow statsRow = null;

                foreach (DataSource.StreamStatsRow s in statsRowAll) // loop through each data item because a date string causes a data format exception in .Select( ...DateTime.ToString() );
                {
                    if (s.StreamStart == streamStat.StreamStart)
                    {
                        statsRow = s;
                        break;
                    }
                }

                if (statsRow == null)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(streamStat.StreamStart, streamStat.StreamEnd, streamStat.NewFollows, streamStat.NewSubs, streamStat.GiftSubs, streamStat.Bits, streamStat.Raids, streamStat.Hosted, streamStat.UsersBanned, streamStat.UsersTimedOut, streamStat.ModsPresent, streamStat.SubsPresent, streamStat.VIPsPresent, streamStat.TotalChats, streamStat.Commands, streamStat.AutoEvents, streamStat.AutoCommands, streamStat.DiscordMsgs, streamStat.ClipsMade, streamStat.ChannelPtCount, streamStat.ChannelChallenge, streamStat.MaxUsers);
                }
                else
                {
                    statsRow.StreamStart = streamStat.StreamStart;
                    statsRow.StreamEnd = streamStat.StreamEnd;
                    statsRow.NewFollows = streamStat.NewFollows;
                    statsRow.NewSubscribers = streamStat.NewSubs;
                    statsRow.GiftSubs = streamStat.GiftSubs;
                    statsRow.Bits = streamStat.Bits;
                    statsRow.Raids = streamStat.Raids;
                    statsRow.Hosted = streamStat.Hosted;
                    statsRow.UsersBanned = streamStat.UsersBanned;
                    statsRow.UsersTimedOut = streamStat.UsersTimedOut;
                    statsRow.ModeratorsPresent = streamStat.ModsPresent;
                    statsRow.SubsPresent = streamStat.SubsPresent;
                    statsRow.VIPsPresent = streamStat.VIPsPresent;
                    statsRow.TotalChats = streamStat.TotalChats;
                    statsRow.Commands = streamStat.Commands;
                    statsRow.AutomatedEvents = streamStat.AutoEvents;
                    statsRow.AutomatedCommands = streamStat.AutoCommands;
                    statsRow.DiscordMsgs = streamStat.DiscordMsgs;
                    statsRow.ClipsMade = streamStat.ClipsMade;
                    statsRow.ChannelPtCount = streamStat.ChannelPtCount;
                    statsRow.ChannelChallenge = streamStat.ChannelChallenge;
                    statsRow.MaxUsers = streamStat.MaxUsers;
                }

                _DataSource.StreamStats.AcceptChanges();
            }
        }

        internal bool GetTodayStream(DateTime CurrTime)
        {
            DataSource.StreamStatsRow[] streamStatsRows = null;
            lock (_DataSource.StreamStats)
            {
                streamStatsRows = (DataSource.StreamStatsRow[])_DataSource.StreamStats.Select();
            }

            foreach (DataSource.StreamStatsRow s in streamStatsRows)
            {
                if (s.StreamStart.Date == CurrTime.Date)
                {
                    return true;
                }
            }

            return false;
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
-u:<allow other user>
-timer:<seconds>
-use:<usage message>


<message> -> The message to display, may include parameters (e.g. #user, #field).
         */

        private readonly string DefaulSocialMsg = "Social media url here";

        /// <summary>
        /// Add all of the default commands to the table, ensure they are available
        /// </summary>
        private void SetDefaultCommandsTable()
        {
            bool CheckName(string criteria) => _DataSource.Commands.Select("CmdName='" + criteria + "'").Length == 0;

            // command name     // msg   // params  
            Dictionary<string, Tuple<string, string>> DefCommandsDictionary = new()
            {
                { DefaultCommand.addcommand.ToString(), new("Command added", "-p:Mod -use:!addcommand !command <switches-optional> <message>. See documentation for <switches>.") },
                { DefaultCommand.commands.ToString(), new("", "-t:Commands -f:CmdName -s:ASC -use:!commands") },
                { DefaultCommand.bot.ToString(), new("Twine ChatBot written by WrithemTwine, https://github.com/WrithemTwine/TwineChatBot/", "-use:!bot") },
                { DefaultCommand.lurk.ToString(), new("#user is now lurking. See you soon!", "-use:!lurk") },
                { DefaultCommand.worklurk.ToString(), new("#user is lurking while making some moohla! See you soon!", "-use:!worklurk") },
                { DefaultCommand.unlurk.ToString(), new("#user has returned. Welcome back!", "-use:!unlurk") },
                { DefaultCommand.socials.ToString(), new("Here are all of my social media connections: ", "-use:!socials") },
                { DefaultCommand.so.ToString(), new("", "-p:Mod -u:true -use:!so user, only mods can use !so.") },
                { DefaultCommand.join.ToString(), new("The message isn't used in response.","") },
                { DefaultCommand.leave.ToString(), new("The message isn't used in response.", "") },
                { DefaultCommand.queue.ToString(), new("The message isn't used in response.", "-p:Mod") }
            };

            foreach (DefaultSocials social in Enum.GetValues(typeof(DefaultSocials)))
            {
                DefCommandsDictionary.Add(social.ToString(), new(DefaulSocialMsg, "-use:!<social name>, use !socials for all available."));
            }

            foreach (string key in DefCommandsDictionary.Keys)
            {
                if (CheckName(key))
                {
                    CommandParams param = CommandParams.Parse(DefCommandsDictionary[key].Item2);
                    _DataSource.Commands.AddCommandsRow(key, param.Permission.ToString(), DefCommandsDictionary[key].Item1, param.Timer, param.DBParamsString(), param.AllowUser, param.Usage);
                }
            }
        }

        /// <summary>
        /// Check if the provided table exists within the database system.
        /// </summary>
        /// <param name="table">The table name to check.</param>
        /// <returns><i>true</i> - if database contains the supplied table, <i>false</i> - if database doesn't contain the supplied table.</returns>
        internal bool CheckTable(string table)
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
        internal bool CheckField(string table, string field)
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
        internal bool CheckPermission(string cmd, ViewerTypes permission)
        {
            lock (_DataSource.Commands)
            {
                DataSource.CommandsRow[] rows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");

                if (rows != null)
                {
                    ViewerTypes cmdpermission = (ViewerTypes)Enum.Parse(typeof(ViewerTypes), rows[0].Permission);

                    return cmdpermission <= permission;
                }
                else
                    throw new InvalidOperationException("Command not found.");
            }
        }

        internal string AddCommand(string cmd, CommandParams Params)
        {
            string strParams = Params.DBParamsString();

            lock (_DataSource.Commands)
            {
                _DataSource.Commands.AddCommandsRow(cmd, Params.Permission.ToString(), Params.Message, Params.Timer, strParams, Params.AllowUser, Params.Usage);
            }
            return "Command added!";
        }

        internal string GetSocials()
        {
            string filter = "";

            foreach (DefaultSocials s in Enum.GetValues(typeof(DefaultSocials)))
            {
                filter += "'" + s.ToString() + "',";
            }

            DataSource.CommandsRow[] socialrows = null;

            lock (_DataSource.Commands)
            {
                socialrows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName IN (" + filter.Substring(0, filter.Length - 1) + ")");
            }

            lock (_DataSource.Commands)
            {
                socialrows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='socials'");
            }

            string socials = socialrows[0].Message;

            foreach (DataSource.CommandsRow com in socialrows)
            {
                if (com.Message != DefaulSocialMsg || com.Message != string.Empty)
                {
                    socials += com.Message + " ";
                }
            }

            return socials.Trim();
        }

        internal string GetUsage(string command)
        {
            lock (_DataSource.Commands)
            {
                DataSource.CommandsRow[] usagerows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + command + "'");

                return usagerows[0]?.Usage ?? "No usage available.";
            }
        }

        internal string PerformCommand(string cmd, string InvokedUser, string ParamUser, List<string> ParamList)
        {
            DataSource.CommandsRow[] comrow = null;

            lock (_DataSource.Commands)
            {
                comrow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");
            }

            if (comrow == null || comrow.Length == 0)
            {
                return "Command not found.";
            }

            object[] value = comrow[0].Params != string.Empty ? PerformQuery(comrow[0], InvokedUser, ParamUser) : null;


            Dictionary<string, string> datavalues = new()
            {
                { "#user", comrow[0].AllowUser ? ParamUser : InvokedUser },
                { "#url", "http://www.twitch.tv/" + (comrow[0].AllowUser ? ParamUser : InvokedUser) }
            };

            return BotIOController.BotController.ParseReplace(comrow[0].Message, datavalues);
        }

        private object[] PerformQuery(DataSource.CommandsRow row, string InvokedUser, string ParamUser)
        {
            CommandParams query = CommandParams.Parse(row.Params);
            DataRow[] result = null;

            lock (_DataSource)
            {
                result = _DataSource.Tables[query.Table].Select("UserName='" + (row.AllowUser ? ParamUser : InvokedUser) + "'");

                if (query.Currency == string.Empty)
                {

                }
            }

            return null;
        }


        #endregion

    }
}
