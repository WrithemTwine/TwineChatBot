using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        #region Clear DataBase Records 
        internal async Task ClearAllCurrencyValues()
        {
            // using var context = BuildDataContext();
            await context.Currency.ExecuteUpdateAsync((c) => c.SetProperty((v) => v.Value, (c) => 0));
            await context.SaveChangesAsync();
            RefreshCurrencyList();
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        internal async Task ClearUsersNotFollowers()
        {
            // using var context = BuildDataContext();

            await context.Users.Where((u) => u.Follower == null || !u.Follower.IsFollower).ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            RefreshUsersList();
            RefreshFollowersList();
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        internal async Task ClearWatchTime()
        {
            // using var context = BuildDataContext();

            await context.UserStats.ExecuteUpdateAsync((us) => us.SetProperty((u) => u.WatchTime, (s) => TimeSpan.FromSeconds(0)));
            await context.SaveChangesAsync();

            RefreshUserStatsList();
        }

        /// <summary>
        /// Clear all 'Followers' table records.
        /// </summary>
        internal async Task RemoveAllFollowers()
        {
            // using var context = BuildDataContext();
            await context.Followers.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            RefreshFollowersList();
        }

        /// <summary>
        /// Clear all 'GiveawayUserData' table records.
        /// </summary>
        internal async Task RemoveAllGiveawayData()
        {
            // using var context = BuildDataContext();
            await context.GiveawayUserData.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            RefreshGiveawayUserDataList();
        }

        /// <summary>
        /// Clear all 'InRaidData' table records.
        /// </summary>
        internal async Task RemoveAllInRaidData()
        {
            // using var context = BuildDataContext();
            await context.InRaidData.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            RefreshInRaidDataList();
        }

        /// <summary>
        /// Clear all 'OutRaidData' table records.
        /// </summary>
        internal async Task RemoveAllOutRaidData()
        {
            // using var context = BuildDataContext();
            await context.OutRaidData.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            RefreshOutRaidDataList();
        }

        /// <summary>
        /// Clear all 'OverlayTicker' table records.
        /// </summary>
        internal async Task RemoveAllOverlayTickerData()
        {
            // using var context = BuildDataContext();
            await context.OverlayTicker.ExecuteUpdateAsync((t) => t.SetProperty((o) => o.UserName, (u) => ""));
            await context.SaveChangesAsync();
            RefreshOverlayTickerList();
        }

        /// <summary>
        /// Clear all 'StreamStats' table records.
        /// </summary>
        internal async Task RemoveAllStreamStats()
        {
            // using var context = BuildDataContext();
            await context.StreamStats.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            RefreshStreamStatsList();
        }

        /// <summary>
        /// Clear all 'Users' table records.
        /// </summary>
        internal async Task RemoveAllUsers()
        {
            // using var context = BuildDataContext();
            await context.Users.ExecuteDeleteAsync();
            await context.SaveChangesAsync();
            RefreshUsersList();
        }
        #endregion

        #region Remove Records
        internal async Task<bool> RemoveCommand(string command)
        {
            // using var context = BuildDataContext();
            bool found = false;

            CommandsUser cmd = await context.CommandsUser.Where(C => C.CmdName == command).Select(C => C).FirstOrDefaultAsync();
            if (cmd != default)
            {
                context.CommandsUser.Remove(cmd);
                found = true;
            }
            await context.SaveChangesAsync();
            RefreshCommandsUserList();

            return found;
        }

        internal async Task<bool> RemoveQuote(int QuoteNum)
        {
            // using var context = BuildDataContext();
            bool found = false;

            Quotes quotes = await context.Quotes.Where(Q => Q.Number == QuoteNum).Select(Q => Q).FirstOrDefaultAsync();

            if (quotes != default)
            {
                context.Quotes.Remove(quotes);
                found = true;
            }
            await context.SaveChangesAsync();
            RefreshQuotesList();

            return found;
        }

        #endregion

    }
}
