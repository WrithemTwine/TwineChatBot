using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        internal Task<LiveUser> GetUser(string UserName)
        {
            return Task.Run(() =>
            {
                // using var context = BuildDataContext();

                LiveUser result = (from U in context.Users
                                   where U.UserName == UserName
                                   select new LiveUser(U.UserName, U.Platform, U.UserId)).FirstOrDefault();


                return result;
            });
        }

        internal Task<string> GetUserId(LiveUser User)
        {
            return Task.Run(() =>
            {
                // using var context = BuildDataContext();
                string result = null;
                List<LiveUser> UsersList = [.. from U in context.Users
                                               select new LiveUser(U.UserName, U.Platform, U.UserId)];

                foreach (var s in from LiveUser s in UsersList
                                  where s.UserName.Equals(User.UserName, StringComparison.OrdinalIgnoreCase)
                                  select s)
                {
                    result = s.UserId;
                }


                return result;
            });
        }

        internal Task<string> GetNewestFollower()
        {
            return Task.Run(() =>
            {
                // using var context = BuildDataContext();
                string result = (from F in context.Followers.Include(user => user.User)
                                 orderby F.FollowedDate descending
                                 select F).FirstOrDefault().User.UserName;

                return result ?? "Not Found";
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
        internal Task<bool> PostFollower(Follow follow)
        {
            return Task.Run(() =>
            {
                currtime = DateTime.Now.ToLocalTime();
                // using var context = BuildDataContext();
                var result = !(from F in context.Followers
                               where F.UserId == follow.FromUser.UserId && F.Platform == follow.FromUser.Platform
                               select F).Any();
                followsQueue.Enqueue([follow]);
                PostFollowsQueue();

                return result;
            });
        }

        internal Task<List<Follow>> PostFollowers(IEnumerable<Follow> follows)
        {
            return Task.Run(() =>
            {
                currtime = DateTime.Now.ToLocalTime();
                // using var context = BuildDataContext();

                List<Follow> ReturnList = [.. from F in follows
                                              join DF in context.Followers on F.FromUserId equals DF.UserId
                                              where DF.UserId is null
                                              select F];

                followsQueue.Enqueue(follows);
                PostFollowsQueue();


                return ReturnList;
            });
        }

        private static bool ProcessFollowQueuestarted = false;

        /// <summary>
        /// Threaded database update to add followers.
        /// </summary>
        private void PostFollowsQueue(IDbContextTransaction contextTransaction = null)
        {
            if (!ProcessFollowQueuestarted)
            {
                ProcessFollowQueuestarted = true;

                ThreadManager.CreateThreadStart("PostFollowsQueue", async () =>
                {
                    // using var context = BuildDataContext();
                    //using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

                    while (followsQueue.TryDequeue(out IEnumerable<Follow> currUser))
                    {
                        List<Followers> tempfollow = [];
                        foreach (Follow f in currUser)
                        {
                            ThreadManager.AddAsyncTaskToGUIDispatcher("PostFollowsQueue", async () =>
                            {
                                await PostNewUser(f.FromUser, f.FollowedAt);
                            });

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
                        ThreadManager.AddTaskToGUIDispatcher(async () =>
                        {
                            await context.Followers.AddRangeAsync(tempfollow);
                        });
                    }
                    //await transaction.CommitAsync();
                    await context.SaveChangesAsync();
                    RefreshFollowersList();
                    RefreshUsersList();

                    ProcessFollowQueuestarted = false;

                    if (!BulkFollowerUpdate)
                    {
                        await StopBulkFollows();
                    }

                });
            }
        }

        private Task<Users> PostNewUser(LiveUser User, DateTime FirstSeen, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

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
                //await transaction.CommitAsync();
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
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();

                return newuser;
            });
        }

        private DateTime currtime;
        internal Task StartBulkFollowers(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                BulkFollowerUpdate = true;
                currtime = DateTime.Now.ToLocalTime();
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.Followers.ExecuteUpdateAsync((f) => f.SetProperty((u) => u.IsFollower, (c) => false));
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();

            });
        }

        internal void NotifyStopBulkFollowers()
        {
            BulkFollowerUpdate = false;
        }

        private Task StopBulkFollows(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                if (OptionFlags.TwitchPruneNonFollowers)
                {
                    ThreadManager.AddTaskToGUIDispatcher(async () =>
                    {
                        await context.Followers.Where((f) => !f.IsFollower).ExecuteDeleteAsync();
                    });
                }
                else // if pruning followers, there won't be multiple 'UserId' records
                {
                    foreach (Followers F in (from f in context.Followers
                                             where !f.IsFollower
                                             select f))
                    {
                        if (F.User != null)
                        {
                            if (!(from f in context.OldFollowUsers
                                  where f.UserId == F.UserId && f.UserName == F.User.UserName && f.Platform == F.Platform
                                  select f).Any())
                            {
                                LogWriter.DebugLog("StopBulkFollows", DebugLogTypes.DataManager, $"Moving follower {F.User.UserName} to OldFollowUsers table.");

                                ThreadManager.AddTaskToGUIDispatcher(() =>
                                {
                                    context.OldFollowUsers.Add(new OldFollowUsers(F, F.User.UserName, currtime));
                                });
                            }
                        }
                        else
                        {
                            Users currUser = (from U in context.Users
                                              where U.UserId == F.UserId
                                              select U).FirstOrDefault();
                            if (!(from f in context.OldFollowUsers
                                  where f.UserId == F.UserId && f.UserName == currUser.UserName && f.Platform == F.Platform
                                  select f).Any())
                            {
                                LogWriter.DebugLog("StopBulkFollows", DebugLogTypes.DataManager, $"Moving user {F.User.UserName} to OldFollowUsers table.");

                                ThreadManager.AddTaskToGUIDispatcher(() =>
                                {
                                    context.OldFollowUsers.Add(new OldFollowUsers(F, currUser.UserName, currtime));
                                });
                            }
                        }
                    }

                    ThreadManager.AddTaskToGUIDispatcher(() =>
                    {
                        context.Followers.RemoveRange(from F in context.Followers where !F.IsFollower select F);
                        context.OldFollowUsers.RemoveRange(from F in context.OldFollowUsers where F.IsFollower select F); // clean accidental current followers included
                    });
                    //await transaction.CommitAsync();
                    await context.SaveChangesAsync();
                }
                OnBulkFollowersAddFinished?.Invoke(this, new(GetNewestFollower().Result));
                RefreshUsersList();
                RefreshFollowersList();
                RefreshOldFollowUsersList();

            });
        }

        /// <summary>
        /// Adds a new user to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="User">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal Task UserJoined(LiveUser User, DateTime NowSeen, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                Users user = await PostNewUser(User, NowSeen);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;

                LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                                    $"Updating {User.UserName} now joined to the channel, Current Login: {user.CurrLoginDate}, Last Date Seen: {user.LastDateSeen}.");

                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshUsersList();
                RefreshUserStatsList();
            });
        }

        /// <summary>
        /// Adds a collection of new users to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="Users">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal Task UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();
                foreach (var L in Users)
                {
                    LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                                    $"Updating {L.UserName} now joined to the channel.");
                    await UserJoined(L, NowSeen);
                }
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshUsersList();
                RefreshUserStatsList();

            });
        }

        internal Task UserLeft(LiveUser User, DateTime LastSeen, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();
                await context.Users
                            .Include(curr => curr.Currency)
                            .ThenInclude(type => type.CurrencyType)
                            .Where((u) => u.UserId == User.UserId)
                .ForEachAsync(async (u) =>
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
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshUsersList();
                RefreshUserStatsList();
            });
        }

        internal Task UpdateFollowers(IEnumerable<Follow> follows)
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
