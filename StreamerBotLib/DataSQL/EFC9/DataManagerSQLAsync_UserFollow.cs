using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        private DateTime currtime;
        private static bool ProcessFollowQueuestarted = false;

        #region Users
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
            using var context = BuildDataContext();
            return await context.Users
                .Where(U => U.UserName == User.UserName || U.UserName.ToLower() == User.UserName.ToLower())
                .Select(U => U.UserId)
                .FirstOrDefaultAsync();
        }

        internal async Task<LiveUser> GetUserById(string UserId, Platform Platform)
        {
            using var context = BuildDataContext();
            var user = await context.Users
                .Where(U => U.UserId == UserId && U.Platform == Platform)
                .Select(U => new LiveUser(U.UserName, U.Platform, U.UserId))
                .FirstOrDefaultAsync();
            return user ?? new LiveUser("Unknown", Platform, UserId);
        }

        private async Task<Users> PostNewUser(SQLDBContext context, LiveUser User, DateTime FirstSeen)
        {
            Users newuser = (from U in context.Users where (U.UserId == User.UserId && U.Platform == User.Platform) select U).Include(S => S.UserStats).FirstOrDefault();
            if (newuser == default)
            {
                newuser = context.Users.Add(new(userId: User.UserId, userName: User.UserName,
                                                platform: User.Platform, firstDateSeen: FirstSeen,
                                                currLoginDate: FirstSeen, lastDateSeen: FirstSeen)).Entity;
                await context.UserStats.AddAsync(new(userId: User.UserId, platform: User.Platform, watchTime: new(0, 0, 0)));
            }
            else
            {
                if (newuser.Platform == default) { newuser.Platform = User.Platform; }
                if (newuser.UserName != User.UserName && newuser.UserId == User.UserId) { newuser.UserName = User.UserName; }
            }

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

            return newuser;
        }

        /// <summary>
        /// Adds a new user to the Users table; updates active dates to <paramref name="NowSeen"/>.
        /// </summary>
        /// <param name="User">The user to add in database and update.</param>
        /// <param name="NowSeen">The reported date & time of the user.</param>
        internal async Task UserJoined(LiveUser User, DateTime NowSeen)
        {
            await UserJoined([User], NowSeen);
        }

        internal async Task UserJoined(IEnumerable<LiveUser> Users, DateTime NowSeen)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();

            foreach (var L in Users)
            {
                LogWriter.DebugLog("UserJoined", DebugLogTypes.DataManager,
                                $"Updating {L.UserName} now joined to the channel.");
                Users user = await PostNewUser(context, L, NowSeen);
                user.CurrLoginDate = NowSeen;
                user.LastDateSeen = NowSeen;
            }

            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshUsersList(true);
            //await RefreshUserStatsList(true);
        }

        internal async Task UserLeft(LiveUser User, DateTime LastSeen)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();

            var user = await context.Users
                .Where(U => U.UserId == User.UserId && U.Platform == User.Platform)
                .Include(U => U.UserStats)
                .Include(U => U.Currency)
                .ThenInclude(U => U.CurrencyType)
                .FirstOrDefaultAsync();

            if (LastSeen > user.LastDateSeen && LastSeen > CurrStreamStart)
            {
                user.UserStats.WatchTime = user.UserStats.WatchTime.Add(LastSeen - user.LastDateSeen);
            }

            if (OptionFlags.CurrencyStart && (OptionFlags.CurrencyOnline && OptionFlags.IsStreamOnline))
            {
                TimeSpan clock = LastSeen - user.LastDateSeen;
                foreach (Currency currency in user.Currency)
                {
                    currency.Value =
                            Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round((currency.Value + currency.CurrencyType.AccrueAmt) * (clock.TotalSeconds / currency.CurrencyType.Seconds), 2)
                            );
                }
                user.LastDateSeen = LastSeen;
            }

            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshUsersList(true);
            //await RefreshUserStatsList(true);
        }

        #endregion

        #region Followers

        internal async Task<string> GetNewestFollower()
        {
            using var context = BuildDataContext();
            var newestFollower = await context.Followers
                .OrderByDescending(F => F.FollowedDate)
                .Take(1)
                .Include(F => F.User)
                .Select(F => F.User.UserName)
                .FirstOrDefaultAsync();

            return newestFollower ?? "Not Found";
        }

        internal async Task<bool> PostFollower(Follow follow)
        {
            return (await PostFollowers([follow])).Count != 0;
        }

        internal Task<List<Follow>> PostFollowers(IEnumerable<Follow> follows)
        {
            currtime = DateTime.Now.ToLocalTime();

            return Task.Run(() =>
            {
                followsQueue.Enqueue(follows);
                PostFollowsQueue();
                using var context = BuildDataContext();
                return (List<Follow>)[.. follows.Where(F => !context.Followers.Any(DF => DF.UserId == F.FromUserId))];
            });
        }

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
                        //List<Followers> tempfollow = [];
                        foreach (Follow f in currUser)
                        {
                            await PostNewUser(context, f.FromUser, f.FollowedAt);
                            await context.SaveChangesAsync();

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
                                var newFollow = new Followers(userId: f.FromUser.UserId,
                                                                     platform: f.FromUser.Platform,
                                                                     isFollower: true, followedDate: f.FollowedAt,
                                                                     statusChangeDate: f.FollowedAt, addDate: currtime,
                                                                     category: f.Category);
                                //tempfollow.Add(newFollow);
                                await context.Followers.AddAsync(newFollow);
                            }
                        }
                    }
                    await context.SaveChangesAsync(true);
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

        internal Task UpdateFollowers(IEnumerable<Follow> follows)
        {
            return Task.Run(() =>
            {
                if (follows.Any())
                {
                    followsQueue.Enqueue(follows);
                    PostFollowsQueue();
                }
            });
        }

        internal async Task StartBulkFollowers()
        {
            BulkFollowerUpdate = true;
            currtime = DateTime.Now.ToLocalTime();
            using var context = BuildDataContext();

            await context.Followers.ExecuteUpdateAsync((f) => f.SetProperty((u) => u.IsFollower, (c) => false));
            await context.SaveChangesAsync(true);
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

                await context.SaveChangesAsync(true);
            }

            OnBulkFollowersAddFinished?.Invoke(this, new(GetNewestFollower().Result));
            await RefreshUsersList(true);
            await RefreshFollowersList(true);
            await RefreshOldFollowUsersList(true);
        }
        #endregion

    }
}
