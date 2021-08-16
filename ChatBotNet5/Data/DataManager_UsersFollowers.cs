using ChatBot_Net5.Static;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;

using TwitchLib.Api.Helix.Models.Users.GetUserFollows;

namespace ChatBot_Net5.Data
{
    public partial class DataManager
    {

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
                DataSource.FollowersRow datafollowers = (DataSource.FollowersRow)_DataSource.Followers.Select("UserName='" + User + "' and FollowedDate<='" + ToDateTime.ToLocalTime().ToString(CultureInfo.CurrentCulture) + "'").FirstOrDefault();

                return datafollowers == null ? false : datafollowers.IsFollower;
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

    }
}
