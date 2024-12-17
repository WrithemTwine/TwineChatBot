using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        internal Task<LiveUser> GetUser(string UserName, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                LiveUser result = (from U in context.Users
                                   where U.UserName == UserName
                                   select new LiveUser(U.UserName, U.Platform, U.UserId)).FirstOrDefault();

                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<string> GetUserId(LiveUser User, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
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
            });
        }

        internal Task<string> GetNewestFollower(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                string result = (from F in context.Followers orderby F.FollowedDate descending select F).FirstOrDefault()?.User?.UserName;
                if (Refcontext == null) { ClearDataContext(context); }
                return result ?? "";
            });
        }

        /// <summary>
        /// Add a new follower to the database.
        /// </summary>
        /// <param name="follow">The follower information to add to the database.</param>
        /// <returns>
        ///     <code>true</code>: first time follower; 
        ///     <code>false</code>: user previously followed.
        /// </returns>
        internal Task<bool> PostFollower(Follow follow, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
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
            });
        }

        internal Task<List<Follow>> PostFollowers(IEnumerable<Follow> follows, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
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
            });
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

                ThreadManager.CreateThreadStart("PostFollowsQueue", async () =>
                {
                    SQLDBContext context = Refcontext ?? BuildDataContext();

                    while (followsQueue.TryDequeue(out IEnumerable<Follow> currUser))
                    {
                        List<Followers> tempfollow = [];
                        foreach (Follow f in currUser)
                        {
                            var user = await PostNewUser(f.FromUser, f.FollowedAt, context);

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
                        await context.Followers.AddRangeAsync(tempfollow);
                    }
                    await context.SaveChangesAsync();
                    RefreshFollowersObservableCollection();
                    RefreshUsersObservableCollection();
                    NotifyDataCollectionUpdated("CurrFollowers");

                    if (Refcontext == null) { ClearDataContext(context); }
                    ProcessFollowQueuestarted = false;

                    if (!BulkFollowerUpdate)
                    {
                        await StopBulkFollows();
                    }
                });
            }
        }

        private Task<Users> PostNewUser(LiveUser User, DateTime FirstSeen, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
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
                    await context.UserStats.AddAsync(new(userId: User.UserId, platform: User.Platform, watchTime: new(0, 0, 0)));
                }

                await context.SaveChangesAsync();

                foreach (Models.CurrencyType t in context.CurrencyType)
                {
                    Currency curr = (from UC in context.Currency
                                     where (UC.UserId == newuser.UserId && UC.CurrencyName == t.CurrencyName)
                                     select UC).FirstOrDefault();

                    if (curr == null)
                    {
                        await context.Currency.AddAsync(new(userId: newuser.UserId, platform: newuser.Platform, value: 0, currencyName: t.CurrencyName));
                    }
                }

                await context.SaveChangesAsync();
                if (Refcontext == null) { ClearDataContext(context); }
                return newuser;
            });
        }

        private DateTime currtime;
        internal Task StartBulkFollowers(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                BulkFollowerUpdate = true;
                currtime = DateTime.Now.ToLocalTime();
                SQLDBContext context = Refcontext ?? BuildDataContext();

                await context.Followers.ExecuteUpdateAsync((f) => f.SetProperty((u) => u.IsFollower, (c) => false));
                await context.SaveChangesAsync();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal void NotifyStopBulkFollowers(SQLDBContext Refcontext = null)
        {
            BulkFollowerUpdate = false;
        }

        private Task StopBulkFollows(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                if (OptionFlags.TwitchPruneNonFollowers)
                {
                    await context.Followers.Where((f) => !f.IsFollower).ExecuteDeleteAsync();
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
                    //context.SaveChangesAsync();

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
                        await context.OldFollowUsers.AddAsync(new OldFollowUsers(OldFollowlist, OldFollowlist.User.UserName, currtime));
                    }
                    context.Followers.RemoveRange(from F in context.Followers where !F.IsFollower select F);
                    await context.SaveChangesAsync();

                    foreach (Followers UpdatedFollower in (from F in context.Followers
                                                           join Match in context.OldFollowUsers
                                                           on F.UserId equals Match.UserId
                                                           where F.Platform == Match.Platform && F.FollowedDate == F.StatusChangeDate
                                                           select F)
                        )
                    {
                        UpdatedFollower.StatusChangeDate = currtime;
                    }
                    await context.SaveChangesAsync();
                }
                OnBulkFollowersAddFinished?.Invoke(this, new(GetNewestFollower().Result));
                RefreshUsersObservableCollection();
                RefreshFollowersObservableCollection();
                RefreshOldFollowUsersObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Adds a new user to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="User">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal Task UserJoined(LiveUser User, DateTime NowSeen, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                Users user = await PostNewUser(User, NowSeen, context);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;

                if (Refcontext == null)
                {
                    await context.SaveChangesAsync();
                    RefreshUsersObservableCollection();
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            });
        }

        /// <summary>
        /// Adds a collection of new users to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="Users">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal Task UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (var L in Users)
                {
                    LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                                    $"Updating {L.UserName} now joined to the channel.");
                    await UserJoined(L, NowSeen, context);
                }
                await context.SaveChangesAsync();
                //RefreshUsersObservableCollection();
                //RefreshUserStatsObservableCollection();
                await GUIContext.Users.LoadAsync();
                NotifyDataCollectionUpdated(nameof(Models.Users));
                await GUIContext.UserStats.LoadAsync();
                NotifyDataCollectionUpdated(nameof(Models.UserStats));

                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task UserLeft(LiveUser User, DateTime LastSeen, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.Users.Where((u) => u.UserId == User.UserId).ForEachAsync(async (u) =>
                {
                    if (u.UserStats == default)
                    {
                        await context.UserStats.AddAsync(new(userId: u.UserId, platform: u.Platform));
                    }

                    if (LastSeen > u.LastDateSeen && LastSeen > CurrStreamStart)
                    {
                        u.UserStats.WatchTime = u.UserStats.WatchTime.Add(LastSeen - u.LastDateSeen);
                    }

                    if (OptionFlags.CurrencyStart && (OptionFlags.CurrencyOnline && OptionFlags.IsStreamOnline))
                    {
                        TimeSpan clock = LastSeen - u.LastDateSeen;
                        foreach (Currency currency in u.Currency)
                        {
                            currency.Value =
                                    Math.Min(
                                        currency.CurrencyType.MaxValue,
                                        Math.Round((currency.Value + currency.CurrencyType.AccrueAmt) * (clock.TotalSeconds / currency.CurrencyType.Seconds), 2)
                                    );
                        }
                        u.LastDateSeen = LastSeen;
                    }
                });
                if (Refcontext == null)
                {
                    await context.SaveChangesAsync();
                    RefreshUsersObservableCollection();
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            });
        }

        internal Task UpdateFollowers(IEnumerable<Follow> follows, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                if (follows.Any())
                {
                    lock (followsQueue)
                    {
                        followsQueue.Enqueue(follows);
                        PostFollowsQueue();
                    }
                }
            });
        }


    }
}
