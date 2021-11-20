using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {
        #region Users and Followers

        private static DateTime CurrStreamStart { get; set; }

        public void UserJoined(string User, DateTime NowSeen)
        {
            lock (_DataSource.Users)
            {
                DataSource.UsersRow user = AddNewUser(User, NowSeen);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;
                NotifySaveData();
            }
        }

        public void UserLeft(string User, DateTime LastSeen)
        {
            lock (_DataSource.Users)
            {
                DataSource.UsersRow[] user = (DataSource.UsersRow[])_DataSource.Users.Select("UserName='" + User + "'");
                if (user != null)
                {
                    UpdateWatchTime(ref user[0], LastSeen); // will update the "LastDateSeen"
                    UpdateCurrency(ref user[0], LastSeen); // will update the "CurrLoginDate"

                    NotifySaveData();
                }
            }
        }

        public void UpdateWatchTime(ref DataSource.UsersRow User, DateTime CurrTime)
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
            lock (_DataSource.Users)
            {
                DataSource.UsersRow[] user = (DataSource.UsersRow[])_DataSource.Users.Select("UserName='" + UserName + "'");
                UpdateWatchTime(ref user[0], CurrTime);
            }
        }

        //public void UpdateWatchTime(DateTime dateTime)
        //{
        //    // LastDateSeen ==> watchtime clock time
        //    // CurrLoginDate ==> currency clock time

        //    lock (_DataSource.Users)
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
            lock (_DataSource.Users)
            {
                DataSource.UsersRow[] user = (DataSource.UsersRow[])_DataSource.Users.Select("UserName='" + User + "'");

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
            lock (_DataSource.Followers)
            {
                DataSource.FollowersRow datafollowers = (DataSource.FollowersRow)_DataSource.Followers.Select("UserName='" + User + "'").FirstOrDefault();

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
        private DataSource.UsersRow AddNewUser(string User, DateTime FirstSeen)
        {
            DataSource.UsersRow usersRow = null;

            lock (_DataSource.Users)
            {
                if (!CheckUser(User))
                {
                    usersRow = _DataSource.Users.AddUsersRow(User, FirstSeen, FirstSeen, FirstSeen, TimeSpan.Zero);
                    //AddCurrencyRows(ref usersRow);
                    NotifySaveData();
                }
            }

            // if the user is added to list before identified as follower, update first seen date to followed date
            lock (_DataSource.Users)
            {
                usersRow = (DataSource.UsersRow)_DataSource.Users.Select("UserName='" + User + "'")[0];

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

        public void UpdateFollowers(string ChannelName, Dictionary<string, IEnumerable<Follow>> follows)
        {
            //new Thread(new ThreadStart(() =>
            //{
            UpdatingFollowers = true;
            lock (_DataSource.Followers)
            {
                List<DataSource.FollowersRow> temp = new();
                temp.AddRange((DataSource.FollowersRow[])_DataSource.Followers.Select());
                temp.ForEach((f) => f.IsFollower = false);
            }
            if (follows[ChannelName].Count() > 1)
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
            NotifySaveData();
            //})).Start();
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime()
        {
            lock (_DataSource.Users)
            {
                foreach (DataSource.UsersRow users in _DataSource.Users.Select())
                {
                    users.WatchTime = new(0);
                }
            }
        }

        #endregion Users and Followers

    }
}
