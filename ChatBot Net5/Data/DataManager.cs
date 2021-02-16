using ChatBot_Net5.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Xml;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Data
{
    public class DataManager
    {
        #region DataSource

        private static readonly string DataFileName = Path.Combine(Directory.GetCurrentDirectory(), "ChatDataStore.xml");
        private DataSource _DataSource;

        public List<string> KindsWebhooks { get; private set; } = new List<string>(Enum.GetNames(typeof(WebhooksKind)));
        public DataView ChannelEvents { get; private set; } // DataSource.ChannelEventsDataTable
        public DataView Users { get; private set; }  // DataSource.UsersDataTable
        public DataView Followers { get; private set; } // DataSource.FollowersDataTable
        public DataView Discord { get; private set; } // DataSource.DiscordDataTable
        public DataView Currency { get; private set; }  // DataSource.CurrencyDataTable
        public DataView CurrencyAccrued { get; private set; }  // DataSource.CurrencyAccruedDataTable
        public DataView Commands { get; private set; }  // DataSource.CommandsDataTable


        internal DateTime defaultDate = DateTime.Parse("01/01/1900");

        private Thread followerThread;

        #endregion DataSource

        public DataManager()
        {
            LoadData();

            ChannelEvents = _DataSource.ChannelEvents.DefaultView;
            Users = _DataSource.Users.DefaultView;
            Followers = _DataSource.Followers.DefaultView;
            Discord = _DataSource.Discord.DefaultView;
            Currency = _DataSource.Currency.DefaultView;
            CurrencyAccrued = _DataSource.CurrencyAccrued.DefaultView;
            Commands = _DataSource.Commands.DefaultView;
        }

        #region Load and Exit Ops
        /// <summary>
        /// Load the data source and populate with default data
        /// </summary>
        private void LoadData()
        {
            _DataSource = new DataSource();

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


            if (CheckName(CommandAction.Follow.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.Follow.ToString(), true, "Thanks #user for the follow!", "#user");
            }
            if (CheckName(CommandAction.Subscribe.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.Subscribe.ToString(), true, "Thanks #user for subscribing!", "#user, #submonths, #subplan, #subplanname");
            }
            if (CheckName(CommandAction.Resubscribe.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.Resubscribe.ToString(), true, "Thanks #user for re-subscribing!", "#user, #months, #submonths, #subplan, #subplanname, #streak");
            }
            if (CheckName(CommandAction.GiftSub.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.GiftSub.ToString(), true, "Thanks #user for gifting a #subplan subscription to #receiveuser !", "#user, #months, #receiveuser, #subplan, #subplanname");
            }
            if (CheckName(CommandAction.BeingHosted.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.BeingHosted.ToString(), true, "Thanks #user for #autohost this channel!", "#user, #autohost, #viewers");
            }
            if (CheckName(CommandAction.Raid.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.Raid.ToString(), true, "Thanks #user for bringing #viewers and raiding the channel!", "#user, #viewers");
            }
            if (CheckName(CommandAction.Bits.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.Bits.ToString(), true, "Thanks #user for donating #bits !", "#user, #bits");
            }
            if (CheckName(CommandAction.Live.ToString()))
            {
                _DataSource.ChannelEvents.AddChannelEventsRow(CommandAction.Live.ToString(), true, "@everyone, #user is now live! #title and playing #category at #url", "#user, #category, #title, #url");
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

            List<object> list = new List<object>();
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
            DataSource.UsersRow user = AddNewUser(User, NowSeen);
            user.CurrLoginDate = NowSeen;
        }

        internal void UserLeft(string User, DateTime LastSeen)
        {
            DataSource.UsersRow user = _DataSource.Users.FindByUserName(User);
            user.LastDateSeen = LastSeen;
            user.WatchTime.Add(user.LastDateSeen - user.CurrLoginDate);
        }

        internal bool CheckFollower(string User)
        {
            DataRow[] datafollowers = _DataSource.Followers.Select("UserName='" + User + "'");
            DataSource.FollowersRow followers = datafollowers.Length > 0 ? (DataSource.FollowersRow)datafollowers[0] : null;

            if (followers == null)
            {
                return false;
            } else
            {
                return followers.IsFollower;
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
            bool newfollow = false;
            DataSource.UsersRow users = AddNewUser(User, FollowedDate);

            lock (_DataSource)
            {
                DataRow[] datafollowers = _DataSource.Followers.Select("UserName='" + User + "'");
                DataSource.FollowersRow followers = datafollowers.Length>0 ? (DataSource.FollowersRow) datafollowers[0] : null;
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
            }

            return newfollow;
        }

        /// <summary>
        /// Add a new User to the User Table
        /// </summary>
        /// <param name="User">The user name.</param>
        /// <param name="FirstSeen">The first time the user is seen.</param>
        /// <returns>True if the user is added, else false if the user already existed.</returns>
        private DataSource.UsersRow AddNewUser(string User, DateTime FirstSeen)
        {
            lock (_DataSource) {
                if (_DataSource.Users.FindByUserName(User) == null)
                {
                    return _DataSource.Users.AddUsersRow(User, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero);
                }
            }
            return _DataSource.Users.FindByUserName(User);
        }


        //private void AddUserData(string User, DateTime FirstSeen, DateTime CurrLoginDate, DateTime LastDateSeen, bool IsFollower, DateTime FollowedDate)
        //{
        //    lock (_DataSource)
        //    {
        //        if (_DataSource.Users.Select("UserName = '" + User + "'").Length == 0)
        //        {
        //            _DataSource.Users.AddUsersRow(User, FirstSeen, CurrLoginDate, LastDateSeen, LastDateSeen - CurrLoginDate, IsFollower, FollowedDate);
        //        }
        //        else
        //        {
        //            UpdateUserData(User, CurrLoginDate, LastDateSeen);
        //        }
        //    }
        //}

        //private void UpdateUserData(string User, DateTime? CurrLoginDate = null, DateTime? LastDateSeen = null)
        //{
        //    lock (_DataSource)
        //    {
        //        DataSource.UsersRow[] usersRow = ((DataSource.UsersRow[])_DataSource.Users.Select("Name='" + User + "'"));
        //        if (usersRow.Length != 0)
        //        {
        //            DataSource.UsersRow user = usersRow[0];
        //            if (CurrLoginDate != null) user.CurrLoginDate = CurrLoginDate.Value;
        //            if (LastDateSeen != null) user.LastDateSeen = LastDateSeen.Value;

        //            if (DateTime.Compare(user.CurrLoginDate, user.LastDateSeen) < 0)
        //            {
        //                user.WatchTime.Add(user.LastDateSeen - user.CurrLoginDate);
        //            }
        //        }
        //    }
        //}

        //private DateTime GetUserFollowedDate(string User)
        //{
        //    lock (_DataSource)
        //    {
        //        DataSource.UsersRow[] usersRow;
        //        if (ContainsUserFollower(User))
        //        {
        //            usersRow = ((DataSource.UsersRow[])_DataSource.Users.Select("Name='" + User + "'"));
        //            return usersRow[0].FollowedDate;
        //        }
        //    }
        //    return defaultDate;
        //}

        ///// <summary>
        ///// Checks if the user is already a follower.
        ///// </summary>
        ///// <param name="User">The username to check.</param>
        ///// <returns>Returns true if the provided username is also a follower.</returns>
        //internal bool ContainsUserFollower(string User)
        //{
        //    lock (_DataSource)
        //    {
        //        return _DataSource.Users.Select("UserName = '" + User + "' and IsFollower=true").Length != 0;
        //    }
        //}

        //private void AddFollower(string User, bool IsFollower, DateTime FollowedDate)
        //{
        //    if (!ContainsUserFollower(User))
        //    {
        //        lock (_DataSource)
        //        {
        //            AddUserData(User, defaultDate, defaultDate, defaultDate, IsFollower, FollowedDate);
        //        }
        //    }
        //}

        internal void UpdateFollowers(string ChannelName, Dictionary<string, List<Follow>> follows)
        {
            followerThread = new Thread(new ThreadStart(() =>
                {
                    if (follows.Count > 1)
                    {
                        foreach (Follow f in follows[ChannelName])
                        {
                            AddFollower(f.FromUserName, f.FollowedAt);
                        }
                    }
                })
            );

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

            List<Uri> uris = new List<Uri>();

            foreach ( DataRow d in dataRows)
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
    }
}
