using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using StreamerBotLib.DataSQL.Models;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        #region Clear DataBase Records 
        internal Task ClearAllCurrencyValues(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();

                //using var transaction = await context.Database.BeginTransactionAsync();
                await context.Currency.ExecuteUpdateAsync((c) => c.SetProperty((v) => v.Value, (c) => 0));
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshCurrencyList();

            });
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        internal Task ClearUsersNotFollowers(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();

                //using var transaction = await context.Database.BeginTransactionAsync();
                await context.Users.Where((u) => u.Follower == null || !u.Follower.IsFollower).ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();

                RefreshUsersList();
                RefreshFollowersList();


            });
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        internal Task ClearWatchTime(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.UserStats.ExecuteUpdateAsync((us) => us.SetProperty((u) => u.WatchTime, (s) => TimeSpan.FromSeconds(0)));
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();

                RefreshUserStatsList();

            });
        }

        /// <summary>
        /// Clear all 'Followers' table records.
        /// </summary>
        internal Task RemoveAllFollowers(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.Followers.ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshFollowersList();

            });
        }

        /// <summary>
        /// Clear all 'GiveawayUserData' table records.
        /// </summary>
        internal Task RemoveAllGiveawayData(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.GiveawayUserData.ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshGiveawayUserDataList();

            });
        }

        /// <summary>
        /// Clear all 'InRaidData' table records.
        /// </summary>
        internal Task RemoveAllInRaidData(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.InRaidData.ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshInRaidDataList();

            });
        }

        /// <summary>
        /// Clear all 'OutRaidData' table records.
        /// </summary>
        internal Task RemoveAllOutRaidData(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.OutRaidData.ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshOutRaidDataList();

            });
        }

        /// <summary>
        /// Clear all 'OverlayTicker' table records.
        /// </summary>
        internal Task RemoveAllOverlayTickerData(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.OverlayTicker.ExecuteUpdateAsync((t) => t.SetProperty((o) => o.UserName, (u) => ""));
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshOverlayTickerList();

            });
        }

        /// <summary>
        /// Clear all 'StreamStats' table records.
        /// </summary>
        internal Task RemoveAllStreamStats(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.StreamStats.ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshStreamStatsList();

            });
        }

        /// <summary>
        /// Clear all 'Users' table records.
        /// </summary>
        internal Task RemoveAllUsers(IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.Users.ExecuteDeleteAsync();
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshUsersList();

            });
        }
        #endregion

        #region Remove Records
        internal Task<bool> RemoveCommand(string command, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                bool found = false;
                //using var transaction = await context.Database.BeginTransactionAsync();

                CommandsUser cmd = (from C in context.CommandsUser where C.CmdName == command select C).FirstOrDefault();
                if (cmd != default)
                {
                    context.CommandsUser.Remove(cmd);
                    found = true;
                }
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshCommandsUserList();

                return found;
            });
        }

        internal Task<bool> RemoveQuote(int QuoteNum, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                bool found = false;
                //using var transaction = await context.Database.BeginTransactionAsync();

                Quotes quotes = (from Q in context.Quotes where Q.Number == QuoteNum select Q).FirstOrDefault();
                if (quotes != default)
                {
                    context.Quotes.Remove(quotes);
                    found = true;
                }
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshQuotesList();

                return found;
            });
        }

        #endregion

    }
}
