using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        public LiveUser GetUser(string UserName, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                LiveUser result = (from U in context.Users
                                   where U.UserName == UserName
                                   select new LiveUser(U.UserName, U.Platform, U.UserId)).FirstOrDefault();

                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetUserId(LiveUser User, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = null;
                List<LiveUser> UsersList = new(from U in context.Users
                                               select new LiveUser(U.UserName, U.Platform, U.UserId));

                foreach (var s in from LiveUser s in UsersList
                                  where s.UserName.Equals(User.UserName, StringComparison.OrdinalIgnoreCase)
                                  select s)
                {
                    result = s.UserId;
                }

                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public string GetNewestFollower(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = (from F in context.Followers orderby F.FollowedDate descending select F).FirstOrDefault()?.User?.UserName;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Add a new follower to the database.
        /// </summary>
        /// <param name="follow">The follower information to add to the database.</param>
        /// <returns>
        ///     <code>true</code>: first time follower; 
        ///     <code>false</code>: user previously followed.
        /// </returns>
        public bool PostFollower(Follow follow, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                currtime = DateTime.Now.ToLocalTime();
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = !(from F in context.Followers
                               where F.UserId == follow.FromUser.UserId && F.Platform == follow.FromUser.Platform
                               select F).Any();
                followsQueue.Enqueue([follow]);
                PostFollowsQueue();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public IEnumerable<Follow> PostFollowers(IEnumerable<Follow> follows, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                currtime = DateTime.Now.ToLocalTime();
                SQLDBContext context = Refcontext ?? BuildDataContext();

                List<Follow> ReturnList = new(from F in follows
                                              join DF in context.Followers on F.FromUserId equals DF.UserId
                                              where DF.UserId is null
                                              select F);

                followsQueue.Enqueue(follows);
                PostFollowsQueue();
                if (Refcontext == null) { ClearDataContext(context); }

                return ReturnList;
            }
        }

        private static bool ProcessFollowQueuestarted = false;

        /// <summary>
        /// Threaded database update to add followers.
        /// </summary>
        private void PostFollowsQueue(SQLDBContext Refcontext = null)
        {
            if (!ProcessFollowQueuestarted)
            {
                ProcessFollowQueuestarted = true;

                ThreadManager.CreateThreadStart("PostFollowsQueue", () =>
                {
                    SQLDBContext context = Refcontext ?? BuildDataContext();

                    while (followsQueue.TryDequeue(out IEnumerable<Follow> currUser))
                    {
                        lock (GUIDataManagerLock.Lock)
                        {
                            List<Followers> tempfollow = [];
                            foreach (Follow f in currUser)
                            {
                                var user = PostNewUser(f.FromUser, f.FollowedAt, context);

                                Followers currFollow = (from UF in context.Followers
                                                        where UF.UserId == f.FromUserId && UF.Platform == f.FromUser.Platform
                                                        select UF).FirstOrDefault();

                                if (currFollow != null)
                                {
                                    currFollow.IsFollower = true;
                                    currFollow.FollowedDate = f.FollowedAt;
                                    currFollow.Category ??= f.Category;
                                }
                                else
                                {
                                    tempfollow.Add(new Followers(userId: f.FromUser.UserId,
                                                                        platform: f.FromUser.Platform,
                                                                        isFollower: true, followedDate: f.FollowedAt,
                                                                        statusChangeDate: f.FollowedAt, addDate: currtime,
                                                                        category: f.Category));
                                }
                            }
                            context.Followers.AddRange(tempfollow);
                        }
                    }

                    lock (GUIDataManagerLock.Lock)
                    {
                        context.SaveChanges(true);
                        RefreshFollowersObservableCollection();
                        RefreshUsersObservableCollection();
                        NotifyDataCollectionUpdated("CurrFollowers");
                    }
                    if (Refcontext == null) { ClearDataContext(context); }
                    ProcessFollowQueuestarted = false;

                    if (!BulkFollowerUpdate)
                    {
                        StopBulkFollows();
                    }
                });
            }
        }

        private Users PostNewUser(LiveUser User, DateTime FirstSeen, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                Users newuser = (from U in context.Users where (U.UserId == User.UserId && U.Platform == User.Platform) select U).FirstOrDefault();
                if (newuser == default)
                {
                    newuser = context.Users.Add(new(userId: User.UserId, userName: User.UserName,
                                                    platform: User.Platform, firstDateSeen: FirstSeen,
                                                    currLoginDate: FirstSeen, lastDateSeen: FirstSeen)).Entity;
                }
                else
                {
                    if (newuser.Platform == default) { newuser.Platform = User.Platform; }
                    if (newuser.UserName != User.UserName && newuser.UserId == User.UserId) { newuser.UserName = User.UserName; }
                }

                if (!(from US in context.UserStats
                      where (US.UserId == User.UserId && US.Platform == User.Platform)
                      select US).Any())
                {
                    context.UserStats.Add(new(userId: User.UserId, platform: User.Platform, watchTime: new(0, 0, 0)));
                }

                context.SaveChanges(true);

                foreach (Models.CurrencyType t in context.CurrencyType)
                {
                    Currency curr = (from UC in context.Currency
                                     where (UC.UserId == newuser.UserId && UC.CurrencyName == t.CurrencyName)
                                     select UC).FirstOrDefault();

                    if (curr == null)
                    {
                        context.Currency.Add(new(userId: newuser.UserId, platform: newuser.Platform, value: 0, currencyName: t.CurrencyName));
                    }
                }

                context.SaveChanges(true);
                if (Refcontext == null) { ClearDataContext(context); }
                return newuser;
            }
        }


        private DateTime currtime;
        public void StartBulkFollowers(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                BulkFollowerUpdate = true;
                currtime = DateTime.Now.ToLocalTime();
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (Followers F in context.Followers)
                {
                    F.IsFollower = false; // reset all followers to not following, add existing followers back as followers
                }
                context.SaveChanges(true);
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void NotifyStopBulkFollowers(SQLDBContext Refcontext = null)
        {
            BulkFollowerUpdate = false;
        }

        private void StopBulkFollows(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                if (OptionFlags.TwitchPruneNonFollowers)
                {
                    context.Followers.RemoveRange(from R in context.Followers where !R.IsFollower select R);
                }
                else // if pruning followers, there won't be multiple 'UserId' records
                {
                    //foreach (Followers F in (from OF in context.Followers where !OF.IsFollower select OF))
                    //{
                    //    if (!(from OFU in context.OldFollowUsers where OFU.UserId == F.UserId select OFU).Any())
                    //    {
                    //        context.OldFollowUsers.Add(new OldFollowUsers(F, F.User.UserName, currtime));
                    //    }
                    //}
                    //context.SaveChanges(true);

                    //foreach (OldFollowUsers OF in context.OldFollowUsers)
                    //{
                    //    Followers currFollow = (from F in context.Followers where F.UserId == OF.UserId && F.Platform == OF.Platform select F).FirstOrDefault();
                    //    if (currFollow != null)
                    //    {
                    //        currFollow.StatusChangeDate = currtime;
                    //    }
                    //}

                    foreach (Followers OldFollowlist in

                                        from NonFollows in (from F in context.Followers
                                                            where !F.IsFollower
                                                            select F)
                                        join OF in context.OldFollowUsers
                                        on NonFollows.UserId equals OF.UserId
                                        where NonFollows.User.UserName != OF.UserName && NonFollows.Platform == OF.Platform
                                        select NonFollows)
                    {
                        context.OldFollowUsers.Add(new OldFollowUsers(OldFollowlist, OldFollowlist.User.UserName, currtime));
                    }
                    context.Followers.RemoveRange(from F in context.Followers where !F.IsFollower select F);
                    context.SaveChanges(true);

                    foreach (Followers UpdatedFollower in (from F in context.Followers
                                                           join Match in context.OldFollowUsers
                                                           on F.UserId equals Match.UserId
                                                           where F.Platform == Match.Platform && F.FollowedDate == F.StatusChangeDate
                                                           select F)
                        )
                    {
                        UpdatedFollower.StatusChangeDate = currtime;
                    }
                    context.SaveChanges(true);
                }
                OnBulkFollowersAddFinished?.Invoke(this, new(GetNewestFollower()));
                RefreshUsersObservableCollection();
                RefreshFollowersObservableCollection();
                RefreshOldFollowUsersObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Adds a new user to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="User">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        public void UserJoined(LiveUser User, DateTime NowSeen, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                Users user = PostNewUser(User, NowSeen, context);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;

                if (Refcontext == null)
                {
                    context.SaveChanges(true);
                    RefreshUsersObservableCollection();
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            }
        }

        /// <summary>
        /// Adds a collection of new users to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="Users">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        public void UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (var L in Users)
                {
                    UserJoined(L, NowSeen, context);
                }
                context.SaveChanges(true);
                RefreshUsersObservableCollection();
                RefreshUserStatsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void UserLeft(LiveUser User, DateTime LastSeen, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                Users user = (from U in context.Users where (U.UserId == User.UserId && U.Platform == User.Platform) select U).FirstOrDefault();
                if (user != default)
                {
                    UpdateWatchTime(User, LastSeen, context);
                    if (OptionFlags.CurrencyStart && (OptionFlags.CurrencyOnline && OptionFlags.IsStreamOnline))
                    {
                        UpdateCurrency(ref user, LastSeen);
                    }
                }
                if (Refcontext == null)
                {
                    context.SaveChanges(true);
                    RefreshUsersObservableCollection();
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            }
        }

        public void UpdateFollowers(IEnumerable<Follow> follows, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                if (follows.Any())
                {
                    lock (followsQueue)
                    {
                        followsQueue.Enqueue(follows);
                        PostFollowsQueue();
                    }
                }
            }
        }


    }
}
