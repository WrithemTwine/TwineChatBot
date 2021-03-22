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
        public DataView CurrencyAccrued { get; private set; }  // DataSource.CurrencyAccruedDataTable
        public DataView Commands { get; private set; }  // DataSource.CommandsDataTable
        public DataView StreamStats { get; private set; } // DataSource.StreamStatsTable

        #endregion DataSource

        public DataManager()
        {
            _DataSource = new();
            LoadData();

            ChannelEvents = _DataSource.ChannelEvents.DefaultView;
            Users = new(_DataSource.Users, null, "UserName", DataViewRowState.CurrentRows);
            Followers = new(_DataSource.Followers, null, "UserName", DataViewRowState.CurrentRows);
            Discord = _DataSource.Discord.DefaultView;
            Currency = new (_DataSource.Currency, null, "Id", DataViewRowState.CurrentRows);
            CurrencyAccrued = new(_DataSource.CurrencyAccrued, null, "UserName", DataViewRowState.CurrentRows);
            Commands = new(_DataSource.Commands, null, "CmdName", DataViewRowState.CurrentRows);
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

            // check all default names
            SetChannelEventsTableDefault();

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
        private void SetChannelEventsTableDefault()
        {
            bool CheckName(string criteria) => _DataSource.ChannelEvents.FindByName(criteria) == null;

            Dictionary<CommandAction, Tuple<string, string>> dictionary = new()
            {
                {CommandAction.BeingHosted, new("Thanks #user for #autohost this channel!", "#user, #autohost, #viewers") },
                {CommandAction.Bits, new("Thanks #user for giving #bits!", "#user, #bits") },
                {CommandAction.CommunitySubs, new("Thanks #user for giving #count to the community!", "#user, #count, #subplan") },
                {CommandAction.Follow, new("Thanks #user for the follow!", "#user") },
                {CommandAction.GiftSub, new("Thanks #user for gifting a #subplan subscription to #receiveuser!", "#user, #months, #receiveuser, #subplan, #subplanname") },
                {CommandAction.Live, new("@everyone, #user is now live streaming #category - #title! Come join and say hi at: #url", "#user, #category, #title, #url") },
                {CommandAction.Raid, new("Thanks #user for bringing #viewers and raiding the channel!", "#user, #viewers") },
                {CommandAction.Resubscribe, new("Thanks #user for re-subscribing!", "#user, #months, #submonths, #subplan, #subplanname, #streak") },
                {CommandAction.Subscribe, new("Thanks #user for subscribing!", "#user, #submonths, #subplan, #subplanname") }
            };

            foreach (CommandAction command in Enum.GetValues(typeof(CommandAction)))
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
        internal object GetRowData(DataRetrieve dataRetrieve, CommandAction rowcriteria)
        {
            return GetAllRowData(dataRetrieve, rowcriteria)?[0];
        }

        /// <summary>
        /// Access the DataSource to retrieve the first row matching the search criteria.
        /// </summary>
        /// <param name="dataRetrieve">The name of the table and column to retrieve.</param>
        /// <param name="rowcriteria">The search string for a particular row.</param>
        /// <returns>All data found using the <i>rowcriteria</i></returns>
        internal object[] GetAllRowData(DataRetrieve dataRetrieve, CommandAction rowcriteria)
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
                DataSource.StreamStatsRow statsRow = (DataSource.StreamStatsRow)_DataSource.StreamStats.Select("StreamStart=" + streamStat.StreamStart.ToString())[0];

                if (statsRow == null)
                {
                    _DataSource.StreamStats.AddStreamStatsRow(streamStat.StreamStart, streamStat.StreamEnd, streamStat.NewFollows, streamStat.NewSubs, streamStat.GiftSubs, streamStat.Bits, streamStat.Raids, streamStat.Hosted, streamStat.UsersBanned, streamStat.UsersTimedOut, streamStat.ModsPresent, streamStat.SubsPresent, streamStat.VIPsPresent, streamStat.TotalChats, streamStat.Commands, streamStat.AutoEvents, streamStat.AutoCommands, streamStat.DiscordMsgs, streamStat.ClipsMade, streamStat.ChannelPtCount, streamStat.ChannelChallenge, streamStat.MaxUsers);
                }
                else
                {
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
    }
}
