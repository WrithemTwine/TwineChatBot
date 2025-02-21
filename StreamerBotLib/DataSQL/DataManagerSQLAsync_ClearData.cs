using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        #region Clear DataBase Records 
        internal Task ClearAllCurrencyValues()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.Currency.ExecuteUpdateAsync((c) => c.SetProperty((v) => v.Value, (c) => 0));
                await context.SaveChangesAsync();
                RefreshCurrencyList();

            });
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        internal Task ClearUsersNotFollowers()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();

                await context.Users.Where((u) => u.Follower == null || !u.Follower.IsFollower).ExecuteDeleteAsync();
                await context.SaveChangesAsync();

                RefreshUsersList();
                RefreshFollowersList();


            });
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        internal Task ClearWatchTime()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();

                await context.UserStats.ExecuteUpdateAsync((us) => us.SetProperty((u) => u.WatchTime, (s) => TimeSpan.FromSeconds(0)));
                await context.SaveChangesAsync();

                RefreshUserStatsList();

            });
        }

        /// <summary>
        /// Clear all 'Followers' table records.
        /// </summary>
        internal Task RemoveAllFollowers()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.Followers.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshFollowersList();

            });
        }

        /// <summary>
        /// Clear all 'GiveawayUserData' table records.
        /// </summary>
        internal Task RemoveAllGiveawayData()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.GiveawayUserData.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshGiveawayUserDataList();

            });
        }

        /// <summary>
        /// Clear all 'InRaidData' table records.
        /// </summary>
        internal Task RemoveAllInRaidData()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.InRaidData.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshInRaidDataList();

            });
        }

        /// <summary>
        /// Clear all 'OutRaidData' table records.
        /// </summary>
        internal Task RemoveAllOutRaidData()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.OutRaidData.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshOutRaidDataList();

            });
        }

        /// <summary>
        /// Clear all 'OverlayTicker' table records.
        /// </summary>
        internal Task RemoveAllOverlayTickerData()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.OverlayTicker.ExecuteUpdateAsync((t) => t.SetProperty((o) => o.UserName, (u) => ""));
                await context.SaveChangesAsync();
                RefreshOverlayTickerList();

            });
        }

        /// <summary>
        /// Clear all 'StreamStats' table records.
        /// </summary>
        internal Task RemoveAllStreamStats()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.StreamStats.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshStreamStatsList();

            });
        }

        /// <summary>
        /// Clear all 'Users' table records.
        /// </summary>
        internal Task RemoveAllUsers()
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                await context.Users.ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                RefreshUsersList();

            });
        }
        #endregion

        #region Remove Records
        internal Task<bool> RemoveCommand(string command)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                bool found = false;

                CommandsUser cmd = (from C in context.CommandsUser where C.CmdName == command select C).FirstOrDefault();
                if (cmd != default)
                {
                    context.CommandsUser.Remove(cmd);
                    found = true;
                }
                await context.SaveChangesAsync();
                RefreshCommandsUserList();

                return found;
            });
        }

        internal Task<bool> RemoveQuote(int QuoteNum)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();
                bool found = false;

                Quotes quotes = (from Q in context.Quotes where Q.Number == QuoteNum select Q).FirstOrDefault();
                if (quotes != default)
                {
                    context.Quotes.Remove(quotes);
                    found = true;
                }
                await context.SaveChangesAsync();
                RefreshQuotesList();

                return found;
            });
        }

        #endregion

    }
}
