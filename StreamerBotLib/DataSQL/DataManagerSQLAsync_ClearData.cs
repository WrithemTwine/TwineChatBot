using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        #region Clear DataBase Records 
        internal Task ClearAllCurrencyValues(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.Currency.ExecuteUpdateAsync((c) => c.SetProperty((v) => v.Value, (c) => 0));
                await context.SaveChangesAsync();
                RefreshCurrencyObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        internal Task ClearUsersNotFollowers(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                await context.Users.Where((u) => u.Follower == null || !u.Follower.IsFollower).ExecuteDeleteAsync();
                await context.SaveChangesAsync();

                RefreshUsersObservableCollection();
                RefreshFollowersObservableCollection();

                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        internal Task ClearWatchTime(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                await context.UserStats.ExecuteUpdateAsync((us) => us.SetProperty((u) => u.WatchTime, (s) => TimeSpan.FromSeconds(0)));
                await context.SaveChangesAsync();

                RefreshUserStatsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'Followers' table records.
        /// </summary>
        internal Task RemoveAllFollowers(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.Followers.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshFollowersObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'GiveawayUserData' table records.
        /// </summary>
        internal Task RemoveAllGiveawayData(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.GiveawayUserData.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshGiveawayUserDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'InRaidData' table records.
        /// </summary>
        internal Task RemoveAllInRaidData(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.InRaidData.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshInRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'OutRaidData' table records.
        /// </summary>
        internal Task RemoveAllOutRaidData(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.OutRaidData.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshOutRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'OverlayTicker' table records.
        /// </summary>
        internal Task RemoveAllOverlayTickerData(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.OverlayTicker.ExecuteUpdateAsync((t) => t.SetProperty((o) => o.UserName, (u) => ""));
                await context.SaveChangesAsync();
                RefreshOverlayTickerObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'StreamStats' table records.
        /// </summary>
        internal Task RemoveAllStreamStats(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.StreamStats.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshStreamStatsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        /// <summary>
        /// Clear all 'Users' table records.
        /// </summary>
        internal Task RemoveAllUsers(SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                await context.Users.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshUsersObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }
        #endregion

        #region Remove Records
        internal Task<bool> RemoveCommand(string command, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                bool found = false;

                CommandsUser cmd = (from C in context.CommandsUser where C.CmdName == command select C).FirstOrDefault();
                if (cmd != default)
                {
                    context.CommandsUser.Remove(cmd);
                    found = true;
                }
                await context.SaveChangesAsync();
                RefreshCommandsUserObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return found;
            });
        }

        internal Task<bool> RemoveQuote(int QuoteNum, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                bool found = false;

                Quotes quotes = (from Q in context.Quotes where Q.Number == QuoteNum select Q).FirstOrDefault();
                if (quotes != default)
                {
                    context.Quotes.Remove(quotes);
                    found = true;
                }
                await context.SaveChangesAsync();
                RefreshQuotesObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
                return found;
            });
        }

        #endregion

    }
}
