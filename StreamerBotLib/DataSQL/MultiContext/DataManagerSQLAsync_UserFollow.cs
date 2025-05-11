using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.MultiContext
{
    internal partial class DataManagerSQLAsync
    {
#if DEBUG
        private List<LiveUser> DebugUsersList = new();
#endif

        internal async Task<LiveUser> GetUser(string UserName)
        {
            using var context = BuildDataContext();
            return await context.Users
                .Where(U => U.UserName == UserName)
                .Select(U => new LiveUser(U.UserName, U.Platform, U.UserId))
                .FirstOrDefaultAsync();
        }

        internal async Task<string> GetUserId(LiveUser User)
        {
            //return Task.Run(() =>
            //{
            //    using var context = BuildDataContext();
            //    string result = null;
            //    List<LiveUser> UsersList = [.. from U in context.Users
            //                                   select new LiveUser(U.UserName, U.Platform, U.UserId)];

            //    foreach (var s in from LiveUser s in UsersList
            //                      where s.UserName.Equals(User.UserName, StringComparison.OrdinalIgnoreCase)
            //                      select s)
            //    {
            //        result = s.UserId;
            //    }


            //    return result;
            //});

            using var context = BuildDataContext();
            return await context.Users
                .Where(U => U.UserName.Equals(User.UserName, StringComparison.OrdinalIgnoreCase))
                .Select(U => U.UserId)
                .FirstOrDefaultAsync();
        }

        internal async Task<string> GetNewestFollower()
        {
            //return Task.Run(() =>
            //{
            //    using var context = BuildDataContext();
            //    string result = (from F in context.Followers.Include(user => user.User)
            //                     orderby F.FollowedDate descending
            //                     select F).FirstOrDefault().User.UserName;

            //    return result ?? "Not Found";
            //});

            using var context = BuildDataContext();
            var newestFollower = await context.Followers
                .Include(F => F.User)
                .OrderByDescending(F => F.FollowedDate)
                .Select(F => F.User.UserName)
                .FirstOrDefaultAsync();

            return newestFollower ?? "Not Found";
        }

        /// <summary>
        /// Add a new follower to the database.
        /// </summary>
        /// <param name="follow">The follower information to add to the database.</param>
        /// <returns>
        ///     <code>true</code>: first time follower; 
        ///     <code>false</code>: user previously followed.
        /// </returns>
        internal async Task<bool> PostFollower(Follow follow)
        {
            currtime = DateTime.Now.ToLocalTime();
            using var context = BuildDataContext();
            var result = !await context.Followers
                .AnyAsync(F => F.UserId == follow.FromUser.UserId && F.Platform == follow.FromUser.Platform);

            followsQueue.Enqueue(new[] { follow });
            PostFollowsQueue();

            return result;
        }

        internal Task<List<Follow>> PostFollowers(IEnumerable<Follow> follows)
        {
            currtime = DateTime.Now.ToLocalTime();
            using var context = BuildDataContext();
            followsQueue.Enqueue(follows);

            List<Follow> ReturnList = [.. follows.Where(F => !context.Followers.Any(DF => DF.UserId == F.FromUserId))];

            PostFollowsQueue();

            return Task.FromResult(ReturnList);
        }

        private static bool ProcessFollowQueuestarted = false;

        /// <summary>
        /// Threaded database update to add followers.
        /// </summary>
        private void PostFollowsQueue()
        {
            if (!ProcessFollowQueuestarted)
            {
                ProcessFollowQueuestarted = true;

                ThreadManager.AddTaskToGUIDispatcher(async () =>
                {
                    using var context = BuildDataContext();

                    while (followsQueue.TryDequeue(out IEnumerable<Follow> currUser))
                    {
                        List<Followers> tempfollow = [];
                        foreach (Follow f in currUser)
                        {
                            await PostNewUser(f.FromUser, f.FollowedAt);

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
                    await RefreshFollowersList(true);
                    await RefreshUsersList(true);

                    ProcessFollowQueuestarted = false;

                    if (!BulkFollowerUpdate)
                    {
                        await StopBulkFollows();
                    }

                });
            }
        }

        private async Task<Users> PostNewUser(LiveUser User, DateTime FirstSeen)
        {
            using var context = BuildDataContext();

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

            return newuser;
        }

        private DateTime currtime;
        internal async Task StartBulkFollowers()
        {
            BulkFollowerUpdate = true;
            currtime = DateTime.Now.ToLocalTime();
            using var context = BuildDataContext();

            await context.Followers.ExecuteUpdateAsync((f) => f.SetProperty((u) => u.IsFollower, (c) => false));
            await context.SaveChangesAsync();
        }

        internal void NotifyStopBulkFollowers()
        {
            BulkFollowerUpdate = false;
        }

        private async Task StopBulkFollows()
        {
            using var context = BuildDataContext();

            if (OptionFlags.TwitchPruneNonFollowers)
            {
                await context.Followers.Where(f => !f.IsFollower).ExecuteDeleteAsync();
            }
            else
            {
                var nonFollowers = await context.Followers
                    .Where(f => !f.IsFollower)
                    .Include(f => f.User)
                    .ToListAsync();

                var oldFollowUsersToAdd = nonFollowers
                    .Where(f => f.User != null)
                    .Where(f => !context.OldFollowUsers.Any(of => of.UserId == f.UserId && of.UserName == f.User.UserName && of.Platform == f.Platform))
                    .Select(f => new OldFollowUsers(f, f.User.UserName, currtime))
                    .ToList();

                var usersWithoutFollowers = nonFollowers
                    .Where(f => f.User == null)
                    .Select(f => new
                    {
                        Follower = f,
                        User = context.Users.FirstOrDefault(u => u.UserId == f.UserId)
                    })
                    .Where(x => x.User != null)
                    .Where(x => !context.OldFollowUsers.Any(of => of.UserId == x.Follower.UserId && of.UserName == x.User.UserName && of.Platform == x.Follower.Platform))
                    .Select(x => new OldFollowUsers(x.Follower, x.User.UserName, currtime))
                    .ToList();

                oldFollowUsersToAdd.AddRange(usersWithoutFollowers);

                if (oldFollowUsersToAdd.Any())
                {
                    await context.OldFollowUsers.AddRangeAsync(oldFollowUsersToAdd);
                }

                context.Followers.RemoveRange(nonFollowers);
                context.OldFollowUsers.RemoveRange(context.OldFollowUsers.Where(of => of.IsFollower));

                await context.SaveChangesAsync();
            }

            OnBulkFollowersAddFinished?.Invoke(this, new(GetNewestFollower().Result));
           await  RefreshUsersList(true);
          await  RefreshFollowersList(true);
          await  RefreshOldFollowUsersList(true);
        }

        /// <summary>
        /// Adds a new user to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="User">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal async Task UserJoined(LiveUser User, DateTime NowSeen)
        {
            using var context = BuildDataContext();

            Users user = await PostNewUser(User, NowSeen);
#if DEBUG
            DebugUsersList.Add(User);

            LogWriter.DebugLog("UserJoined", DebugLogTypes.SpecialPurpose, $"Update Time: {NowSeen}");

            LogWriter.DebugLog("UserJoined", DebugLogTypes.SpecialPurpose, $"Old data: user Id: {user.UserId}, Curr Login Date: {user.CurrLoginDate}, LastDateSeen: {user.LastDateSeen}");
#endif

            user.CurrLoginDate = NowSeen;
            user.LastDateSeen = NowSeen;

            LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                                $"Updating {User.UserName} now joined to the channel, Current Login: {user.CurrLoginDate}, Last Date Seen: {user.LastDateSeen}.");

            await context.SaveChangesAsync();

#if DEBUG
            // validate save occurred; finding "CurrLoginDate" is not reliably saving
            using var debugcontext = BuildDataContext();

            var debuguser = await debugcontext.Users
                             .Where(U => U.UserId == User.UserId && U.Platform == User.Platform)
                             .Select(U => U).FirstOrDefaultAsync();

            LogWriter.DebugLog("UserJoined", DebugLogTypes.SpecialPurpose, $"New data: user Id: {debuguser.UserId}, Curr Login Date: {debuguser.CurrLoginDate}, LastDateSeen: {debuguser.LastDateSeen}");
#endif

            await RefreshUsersList(true);
            await RefreshUserStatsList(true);
        }

        /// <summary>
        /// Adds a collection of new users to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="Users">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal async Task UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen)
        {
            using var context = BuildDataContext();
#if DEBUG
            DebugUsersList.AddRange(Users);
            LogWriter.DebugLog("UserJoined", DebugLogTypes.SpecialPurpose, $"Update Time: {NowSeen}");
#endif

            foreach (var L in Users)
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                                $"Updating {L.UserName} now joined to the channel.");
                Users user = await PostNewUser(L, NowSeen);
#if DEBUG


                //LogWriter.DebugLog("UserJoined", DebugLogTypes.SpecialPurpose, $"Old data: {user.GetDebugOutput()}");
#endif
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;
#if DEBUG
                //LogWriter.DebugLog("UserJoined", DebugLogTypes.SpecialPurpose, $"New data: {user.GetDebugOutput()}");
#endif
            }

            await context.SaveChangesAsync();
       await     RefreshUsersList(true);
      await      RefreshUserStatsList(true);
        }

        internal async Task UserLeft(LiveUser User, DateTime LastSeen)
        {
            using var context = BuildDataContext();
            await context.Users
                        .Include(curr => curr.Currency)
                        .ThenInclude(type => type.CurrencyType)
                        .Include(stat => stat.UserStats)
                        .Where((u) => u.UserId == User.UserId && u.Platform == User.Platform)
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
            await context.SaveChangesAsync();
         await   RefreshUsersList(true);
         await   RefreshUserStatsList(true);

//#if DEBUG
//            var contextUser = await context.Users
//                .Where(U => U.UserId == User.UserId && U.Platform == User.Platform)
//                .Select(U => U).FirstOrDefaultAsync();

//            var GUIContextUser = await GUIContext.Users
//                .Where(U => U.UserId == User.UserId && U.Platform == User.Platform)
//                .Select(U => U).FirstOrDefaultAsync();

//            // compare the data entry context to the GUIContext
//            LogWriter.DebugLog("UserLeft", DebugLogTypes.SpecialPurpose, $"Context data: {contextUser.GetDebugOutput()}");
//            LogWriter.DebugLog("UserLeft", DebugLogTypes.SpecialPurpose, $"GUIContext data: {GUIContextUser.GetDebugOutput()}");
//#endif
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
