using ChatBot_Net5.Enum;
using ChatBot_Net5.Models;
using ChatBot_Net5.Static;
using ChatBot_Net5.Systems;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Data
{
    /*
        - DataManager should not be converted to "local culture" strings, because the data file would have problems if the system language changed
        - Having mixed identifiers within the data file would create problems
        - Can convert to localized GUI identifiers to aid users not comfortable with English (and their language is provided to the app)
        - Also, the 'add command' parameters should remain
        - A GUI to add new commands would provide more localized help, apart from names of data tables <= unless there's somehow a converter between the name they choose and the database name => could be a dictionary with keys of the localized language and the values to the data manager data table values
    */

    public class DataManager : INotifyPropertyChanged
    {

        #region DataSource
#if DEBUG
        private static readonly string DataFileName = Path.Combine(@"D:\Source\Chat Bot Apps\ChatBotNet5\bin\Debug\net5.0-windows", "ChatDataStore.xml");
#else
        private static readonly string DataFileName = Path.Combine(Directory.GetCurrentDirectory(), "ChatDataStore.xml");
#endif


        private readonly DataSource _DataSource;
        private Thread followerThread;

        private readonly Queue<Task> SaveTasks = new();
        private bool SaveThreadStarted = false;
        private const int SaveThreadWait = 1500;

        public bool UpdatingFollowers { get; set; } = false;

        public List<string> KindsWebhooks { get; private set; } = new(System.Enum.GetNames(typeof(WebhooksKind)));
        public DataView ChannelEvents { get; private set; } // DataSource.ChannelEventsDataTable
        public DataView Users { get; private set; }  // DataSource.UsersDataTable
        public DataView Followers { get; private set; } // DataSource.FollowersDataTable
        public DataView Discord { get; private set; } // DataSource.DiscordDataTable
        public DataView Currency { get; private set; }  // DataSource.CurrencyDataTable
        public DataView CurrencyType { get; private set; }  // DataSource.CurrencyTypeDataTable
        public DataView BuiltInCommands { get; private set; } // DataSource.CommandsDataTable
        public DataView Commands { get; private set; }  // DataSource.CommandsDataTable
        public DataView StreamStats { get; private set; } // DataSource.StreamStatsTable
        public DataView ShoutOuts { get; private set; } // DataSource.ShoutOutsTable
        public DataView Category { get; private set; } // DataSource.CategoryTable
        public DataView Clips { get; private set; }  // DataSource.ClipsDataTable

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new(PropName));
        }

        #endregion DataSource

        public DataManager()
        {
            static string ComFilter()
            {
                string filter = string.Empty;

                foreach (DefaultCommand d in System.Enum.GetValues(typeof(DefaultCommand)))
                {
                    filter += "'" + d.ToString() + "',";
                }

                foreach (DefaultSocials s in System.Enum.GetValues(typeof(DefaultSocials)))
                {
                    filter += "'" + s.ToString() + "',";
                }

                return filter == string.Empty ? "" : filter[0..^1];
            }

            _DataSource = new();
            LocalizedMsgSystem.SetDataManager(this);
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
            ShoutOuts = new(_DataSource.ShoutOuts, null, "UserName", DataViewRowState.CurrentRows);
            Category = new(_DataSource.CategoryList, null, "Id", DataViewRowState.CurrentRows);
            Clips = new(_DataSource.Clips, null, "Id", DataViewRowState.CurrentRows);
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
                _ = _DataSource.ReadXml(xmlreader, XmlReadMode.DiffGram);
            }

            SetDefaultChannelEventsTable();  // check all default ChannelEvents names
            SetDefaultCommandsTable(); // check all default Commands

            SaveData();
        }

        /// <summary>
        /// Save data to file upon exit and after data changes. Pauses for 15 seconds (unless exiting) to slow down multiple saves in a short time.
        /// </summary>
        public void SaveData()
        {
            if (!UpdatingFollowers) // block saving data until the follower updating is completed
            {
                lock (SaveTasks) // lock the Queue, block thread if currently save task has started
                {
                    if (!SaveThreadStarted) // only start the thread once per save cycle, flag is an object lock
                    {
                        SaveThreadStarted = true;
                        new Thread(new ThreadStart(PerformSaveOp)).Start();
                    }

                    SaveTasks.Enqueue(new(() =>
                    {
                        string result = Path.GetRandomFileName();

                        lock (_DataSource)
                        {
                            _DataSource.AcceptChanges();

                            try
                            {
                                _DataSource.WriteXml(result, XmlWriteMode.DiffGram);

                                DataSource testinput = new();
                                using (XmlReader xmlReader = new XmlTextReader(result))
                                {
                                    // test load
                                    _ = testinput.ReadXml(xmlReader, XmlReadMode.DiffGram);
                                }

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

        private void PerformSaveOp()
        {
            if (OptionFlags.ProcessOps) // don't sleep if exiting app
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
                SaveThreadStarted = false; // indicate start another thread to save data
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

            foreach (ChannelEventActions command in System.Enum.GetValues(typeof(ChannelEventActions)))
            {
                // consider only the values in the dictionary, check if data is already defined in the data table
                if (dictionary.ContainsKey(command) && CheckName(command.ToString()))
                {   // extract the default data from the dictionary and add to the data table
                    Tuple<string, string> values = dictionary[command];
                    lock (_DataSource)
                    {
                        _DataSource.ChannelEvents.AddChannelEventsRow(command.ToString(), false, true, values.Item1, values.Item2);
                    }
                }
            }

            _DataSource.ChannelEvents.AcceptChanges();
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
                UpdateWatchTime(user, DateTime.Now);
                SaveData();
                OnPropertyChanged(nameof(Users));
            }
        }

        internal void UserLeft(string User, DateTime LastSeen)
        {
            lock (_DataSource.Users)
            {
                DataSource.UsersRow user = _DataSource.Users.FindByUserName(User);
                if (user != null)
                {
                    UpdateWatchTime(user, LastSeen);
                    SaveData();
                    OnPropertyChanged(nameof(Users));
                }
            }
        }

        internal void UpdateWatchTime(string User, DateTime CurrTime)
        {
            lock (_DataSource.Users)
            {
                UpdateWatchTime(_DataSource.Users.FindByUserName(User), CurrTime);
            }
        }

        internal void UpdateWatchTime(DataSource.UsersRow User, DateTime CurrTime)
        {
            lock (_DataSource.Users)
            {
                if (User != null)
                {
                    User.WatchTime = User.WatchTime.Add(CurrTime - User.LastDateSeen);
                    User.LastDateSeen = CurrTime;
                    SaveData();
                    OnPropertyChanged(nameof(Users));
                }
            }
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.Now.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><c>true</c> if the user has arrived prior to DateTime.Now, <c>false</c> otherwise.</returns>
        internal bool CheckUser(string User)
        {
            return CheckUser(User, DateTime.Now);
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        internal bool CheckUser(string User, DateTime ToDateTime)
        {
            DataSource.UsersRow user = _DataSource.Users.FindByUserName(User);

            return !(user == null) || user?.FirstDateSeen <= ToDateTime;
        }

        /// <summary>
        /// Check if the User is already a follower prior to now.
        /// </summary>
        /// <param name="User">The name of the user to check.</param>
        /// <returns>Returns <c>true</c> if the <paramref name="User"/> is a follower prior to DateTime.Now.</returns>
        internal bool CheckFollower(string User)
        {
            return CheckFollower(User, DateTime.Now);
        }

        /// <summary>
        /// Check if a user is a follower at or before the current date.
        /// </summary>
        /// <param name="User">The user to query.</param>
        /// <param name="ToDateTime">The date to check FollowedDate <= <c>ToDateTime</c></param>
        /// <returns></returns>
        internal bool CheckFollower(string User, DateTime ToDateTime)
        {
            lock (_DataSource.Followers)
            {
                DataRow[] datafollowers = _DataSource.Followers.Select("UserName='" + User + "' and FollowedDate<='" + ToDateTime.ToLocalTime().ToString(CultureInfo.CurrentCulture) + "'");
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
        /// Remove all Users from the database.
        /// </summary>
        internal void RemoveAllUsers()
        {
            lock (_DataSource.Users)
            {
                _DataSource.Users.Clear();
            }
            OnPropertyChanged(nameof(Users));

        }

        /// <summary>
        /// Remove all Followers from the database.
        /// </summary>
        internal void RemoveAllFollowers()
        {
            lock (_DataSource.Followers)
            {
                _DataSource.Followers.Clear();
            }
            OnPropertyChanged(nameof(Followers));
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
                SaveData();
                OnPropertyChanged(nameof(Followers));
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
            DataSource.UsersRow usersRow = null;

            lock (_DataSource.Users)
            {
                if (!CheckUser(User))
                {
                    DataSource.UsersRow output = _DataSource.Users.AddUsersRow(User, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero);
                    SaveData();
                    return output;
                }
            }

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
                UpdatingFollowers = true;
                lock (_DataSource.Followers)
                {
                    List<DataSource.FollowersRow> temp = new();
                    temp.AddRange((DataSource.FollowersRow[])_DataSource.Followers.Select());
                    temp.ForEach((f) => f.IsFollower = false);
                }
                if (follows[ChannelName].Count > 1)
                {
                    foreach (Follow f in follows[ChannelName])
                    {
                        AddFollower(f.FromUserName, f.FollowedAt);
                    }
                }

                if (OptionFlags.TwitchPruneNonFollowers)
                {
                    lock (_DataSource.Followers)
                    {
                        List<DataSource.FollowersRow> temp = new();
                        temp.AddRange((DataSource.FollowersRow[])_DataSource.Followers.Select());
                        foreach (DataSource.FollowersRow f in from DataSource.FollowersRow f in temp
                                                              where !f.IsFollower
                                                              select f)
                        {
                            _DataSource.Followers.RemoveFollowersRow(f);
                        }
                    }
                }

                UpdatingFollowers = false;
                SaveData();
            }));

            followerThread.Start();
        }

        #endregion Users and Followers

        #region Discord and Webhooks
        /// <summary>
        /// Retrieve all the webhooks from the Discord table
        /// </summary>
        /// <returns></returns>
        internal List<Tuple<bool, Uri>> GetWebhooks(WebhooksKind webhooks)
        {
            DataRow[] dataRows = _DataSource.Discord.Select();

            List<Tuple<bool, Uri>> uris = new();

            foreach (DataRow d in dataRows)
            {
                DataSource.DiscordRow row = d as DataSource.DiscordRow;

                if (row.Kind == webhooks.ToString())
                {
                    uris.Add(new Tuple<bool, Uri>(row.AddEveryone, new Uri(row.Webhook)));
                }
            }
            return uris;
        }
        #endregion Discord and Webhooks

        #region Stream Statistics
        private DataSource.StreamStatsRow CurrStreamStatRow;

        internal DataSource.StreamStatsRow[] GetAllStreamData()
        {
            lock (_DataSource.StreamStats)
            {
                return (DataSource.StreamStatsRow[])_DataSource.StreamStats.Select();
            }
        }

        internal DataSource.StreamStatsRow GetAllStreamData(DateTime dateTime)
        {
            foreach (DataSource.StreamStatsRow streamStatsRow in GetAllStreamData())
            {
                if (streamStatsRow.StreamStart == dateTime)
                {
                    return streamStatsRow;
                }
            }

            return null;
        }

        internal bool CheckMultiStreams(DateTime dateTime)
        {
            int x = 0;
            foreach (DataSource.StreamStatsRow row in GetAllStreamData())
            {
                if (row.StreamStart.ToShortDateString() == dateTime.ToShortDateString())
                {
                    x++;
                }
            }

            return x > 1;
        }

        internal bool AddStream(DateTime StreamStart)
        {
            if (CheckStreamTime(StreamStart))
            {
                return false;
            }
            lock (_DataSource.StreamStats)
            {
                _DataSource.StreamStats.AddStreamStatsRow(StreamStart, StreamStart, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                SaveData();

                //CurrStreamStatRow = GetAllStreamData(StreamStart);
                OnPropertyChanged(nameof(StreamStats));

                return true;
            }
        }

        internal void PostStreamStat(StreamStat streamStat)
        {
            // TODO: consider regularly posting stream stats in case bot crashes and loses current stream stats up to the crash
            lock (_DataSource.StreamStats)
            {
                CurrStreamStatRow = GetAllStreamData(streamStat.StreamStart);

                if (CurrStreamStatRow == null)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(streamStat.StreamStart, streamStat.StreamEnd, streamStat.NewFollows, streamStat.NewSubs, streamStat.GiftSubs, streamStat.Bits, streamStat.Raids, streamStat.Hosted, streamStat.UsersBanned, streamStat.UsersTimedOut, streamStat.ModsPresent, streamStat.SubsPresent, streamStat.VIPsPresent, streamStat.TotalChats, streamStat.Commands, streamStat.AutoEvents, streamStat.AutoCommands, streamStat.DiscordMsgs, streamStat.ClipsMade, streamStat.ChannelPtCount, streamStat.ChannelChallenge, streamStat.MaxUsers);
                }
                else
                {
                    CurrStreamStatRow.StreamStart = streamStat.StreamStart;
                    CurrStreamStatRow.StreamEnd = streamStat.StreamEnd;
                    CurrStreamStatRow.NewFollows = streamStat.NewFollows;
                    CurrStreamStatRow.NewSubscribers = streamStat.NewSubs;
                    CurrStreamStatRow.GiftSubs = streamStat.GiftSubs;
                    CurrStreamStatRow.Bits = streamStat.Bits;
                    CurrStreamStatRow.Raids = streamStat.Raids;
                    CurrStreamStatRow.Hosted = streamStat.Hosted;
                    CurrStreamStatRow.UsersBanned = streamStat.UsersBanned;
                    CurrStreamStatRow.UsersTimedOut = streamStat.UsersTimedOut;
                    CurrStreamStatRow.ModeratorsPresent = streamStat.ModsPresent;
                    CurrStreamStatRow.SubsPresent = streamStat.SubsPresent;
                    CurrStreamStatRow.VIPsPresent = streamStat.VIPsPresent;
                    CurrStreamStatRow.TotalChats = streamStat.TotalChats;
                    CurrStreamStatRow.Commands = streamStat.Commands;
                    CurrStreamStatRow.AutomatedEvents = streamStat.AutoEvents;
                    CurrStreamStatRow.AutomatedCommands = streamStat.AutoCommands;
                    CurrStreamStatRow.DiscordMsgs = streamStat.DiscordMsgs;
                    CurrStreamStatRow.ClipsMade = streamStat.ClipsMade;
                    CurrStreamStatRow.ChannelPtCount = streamStat.ChannelPtCount;
                    CurrStreamStatRow.ChannelChallenge = streamStat.ChannelChallenge;
                    CurrStreamStatRow.MaxUsers = streamStat.MaxUsers;
                }
                SaveData();
            }
            OnPropertyChanged(nameof(StreamStats));
        }

        internal bool CheckStreamTime(DateTime CurrTime)
        {
            return GetAllStreamData(CurrTime) != null;
        }

        internal void RemoveAllStreamStats()
        {
            lock (_DataSource.StreamStats)
            {
                _DataSource.StreamStats.Clear();
            }
            OnPropertyChanged(nameof(StreamStats));

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
            lock (_DataSource.Commands)
            {
                if (_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'").Length == 0)
                {
                    _DataSource.CategoryList.AddCategoryListRow(null, LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry));
                    _DataSource.CategoryList.AcceptChanges();
                }

                DataSource.CategoryListRow categoryListRow = (DataSource.CategoryListRow)_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'")[0];

                bool CheckName(string criteria)
                {
                    DataSource.CommandsRow[] datarow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + criteria + "'");
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
        internal bool CheckShoutName(string UserName)
        {
            lock (_DataSource.ShoutOuts)
            {
                return _DataSource.ShoutOuts.Select("UserName='" + UserName + "'").Length > 0;
            }
        }

        internal string GetKey(string Table)
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

        internal string AddCommand(string cmd, CommandParams Params)
        {
            //string strParams = Params.DBParamsString();
            DataSource.CategoryListRow categoryListRow = (DataSource.CategoryListRow)_DataSource.CategoryList.Select("Category='" + LocalizedMsgSystem.GetVar(Msg.MsgAllCateogry) + "'")[0];

            lock (_DataSource.Commands)
            {
                _DataSource.Commands.AddCommandsRow(cmd, Params.AddMe, Params.Permission.ToString(), Params.Message, Params.Timer, categoryListRow, Params.AllowParam, Params.Usage, Params.LookupData, Params.Table, GetKey(Params.Table), Params.Field, Params.Currency, Params.Unit, Params.Action, Params.Top, Params.Sort);
                SaveData();
                OnPropertyChanged(nameof(Commands));
            }
            return string.Format(CultureInfo.CurrentCulture, "Command {0} added!", cmd);
        }

        internal string GetSocials()
        {
            string filter = "";

            foreach (DefaultSocials s in System.Enum.GetValues(typeof(DefaultSocials)))
            {
                filter += "'" + s.ToString() + "',";
            }

            DataSource.CommandsRow[] socialrows = null;
            lock (_DataSource.Commands)
            {
                socialrows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + LocalizedMsgSystem.GetVar(DefaultCommand.socials) + "'");
            }

            string socials = socialrows[0].Message;

            if (OptionFlags.MsgPerComMe && socialrows[0].AddMe == true)
            {
                socials = "/me " + socialrows[0].Message;
            }

            lock (_DataSource.Commands)
            {
                socialrows = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName IN (" + filter[0..^1] + ")");
            }

            foreach (DataSource.CommandsRow com in socialrows)
            {
                if (com.Message != DefaulSocialMsg && com.Message != string.Empty)
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

                return usagerows[0]?.Usage ?? LocalizedMsgSystem.GetVar(Msg.MsgNoUsage);
            }
        }

        // older code
        //internal string PerformCommand(string cmd, string InvokedUser, string ParamUser, List<string> ParamList=null)
        //{
        //    DataSource.CommandsRow[] comrow = null;

        //    lock (_DataSource.Commands)
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

        internal DataSource.CommandsRow GetCommand(string cmd)
        {
            DataSource.CommandsRow[] comrow = null;

            lock (_DataSource.Commands)
            {
                comrow = (DataSource.CommandsRow[])_DataSource.Commands.Select("CmdName='" + cmd + "'");
            }

            //if (comrow == null || comrow.Length == 0)
            //{
            //    throw new KeyNotFoundException(LocalizedMsgSystem.GetVar(ChatBotExceptions.ExceptionKeyNotFound));
            //}

            return comrow?[0];
        }

        internal object PerformQuery(DataSource.CommandsRow row, string ParamValue)
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
                if (resulttype == typeof(DataSource.UsersRow))
                {
                    DataSource.UsersRow usersRow = (DataSource.UsersRow)result;
                    UpdateWatchTime(ParamValue, DateTime.Now);
                    return usersRow[row.data_field];
                }
                else if (resulttype == typeof(DataSource.FollowersRow))
                {
                    DataSource.FollowersRow follower = (DataSource.FollowersRow)result;

                    return follower.IsFollower ? follower.FollowedDate : LocalizedMsgSystem.GetVar(Msg.MsgNotFollower);
                }
                else if (resulttype == typeof(DataSource.CurrencyRow))
                {

                }
                else if (resulttype == typeof(DataSource.CurrencyTypeRow))
                {

                }
                else if (resulttype == typeof(DataSource.CommandsRow))
                {

                }
            }

            return result;
        }

        internal object[] PerformQuery(DataSource.CommandsRow row, int Top = 0)
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
        internal List<Tuple<string, int, string[]>> GetTimerCommands()
        {
            lock (_DataSource.Commands)
            {
                List<Tuple<string, int, string[]>> TimerList = new();
                foreach (DataSource.CommandsRow row in (DataSource.CommandsRow[])_DataSource.Commands.Select("RepeatTimer>0"))
                {
                    TimerList.Add(new(row.CmdName, row.RepeatTimer, row.Category?.Split(',') ?? Array.Empty<string>()));
                }
                return TimerList;
            }
        }

        #endregion

        #region Category

        /// <summary>
        /// Checks for the supplied category in the category list, adds if it isn't already saved.
        /// </summary>
        /// <param name="newCategory">The category to add to the list if it's not available.</param>
        internal void UpdateCategory(string CategoryId, string newCategory)
        {
            DataSource.CategoryListRow[] categoryList = (DataSource.CategoryListRow[])_DataSource.CategoryList.Select("Category='" + newCategory.Replace("'", "''") + "'");

            if (categoryList.Length == 0)
            {
                _DataSource.CategoryList.AddCategoryListRow(CategoryId, newCategory);
            }
            else if (categoryList[0].CategoryId == null)
            {
                categoryList[0].CategoryId = CategoryId;
            }

            SaveData();
        }
        #endregion

        #region Clips

        internal bool AddClip(string ClipId, string CreatedAt, float Duration, string GameId, string Language, string Title, string Url)
        {
            DataSource.ClipsRow[] clipsRows = (DataSource.ClipsRow[])_DataSource.Clips.Select("Id='" + ClipId + "'");

            if (clipsRows.Length == 0)
            {
                //DataSource.CategoryListRow categoryListRow = ((DataSource.CategoryListRow[]) _DataSource.CategoryList.Select("CategoryId='" + GameId + "'")).First();
                _ = _DataSource.Clips.AddClipsRow(ClipId, CreatedAt, Title, GameId, Language, (decimal)Duration, Url);
                SaveData();
                return true;
            }

            return false;
        }

        #endregion
    }
}
