
namespace StreamerBotLib.DataSQL.EFC9
{
    using Microsoft.EntityFrameworkCore;

    using StreamerBotLib.DataSQL.Models;

    internal partial class DataManagerSQLAsync
    {
        #region Clear DataBase Records 

        /// <summary>
        /// Resets all currency values in the database to zero.
        /// </summary>
        /// <remarks>This method updates all records in the database to set their currency values to zero 
        /// and saves the changes. It also refreshes the currency list to reflect the updated values.</remarks>
        /// <returns></returns>
        internal async Task ClearAllCurrencyValues()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.Currency.ExecuteUpdateAsync((c) => c.SetProperty((v) => v.Value, (c) => 0));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshCurrencyList(true);
        }

        /// <summary>
        /// Removes users from the database who are not followers.
        /// </summary>
        /// <remarks>This method deletes all users from the database whose associated follower information
        /// is either null or marked as not being a follower. After the deletion, it refreshes  the users and followers
        /// lists to reflect the updated state.</remarks>
        /// <returns></returns>
        internal async Task ClearUsersNotFollowers()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.Users.Where((u) => u.Follower == null || !u.Follower.IsFollower).ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();

            RefreshUsersList(true);
            await RefreshFollowersList(true);
        }

        /// <summary>
        /// Resets the watch time for all users to zero.
        /// </summary>
        /// <remarks>This method updates the watch time for all users in the database to a value of zero
        /// seconds  and saves the changes. After the update, it refreshes the user statistics list.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task ClearWatchTime()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.UserStats.ExecuteUpdateAsync((us) => us.SetProperty((u) => u.WatchTime, (s) => TimeSpan.FromSeconds(0)));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();

            RefreshUserStatsList(true);
        }

        /// <summary>
        /// Removes all followers from the current context.
        /// </summary>
        /// <remarks>This method deletes all follower records from the data source and refreshes the followers list. It is
        /// an asynchronous operation and should be awaited to ensure completion.</remarks>
        /// <returns></returns>
        internal async Task RemoveAllFollowers()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.Followers.ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshFollowersList(true);
        }

        /// <summary>
        /// Removes all giveaway-related user data from the database.
        /// </summary>
        /// <remarks>This method deletes all records associated with giveaway user data and refreshes the
        /// in-memory list of giveaway user data. It should be used with caution as it performs a complete removal of
        /// all giveaway-related data.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task RemoveAllGiveawayData()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.GiveawayUserData.ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshGiveawayUserDataList(true);
        }

        /// <summary>
        /// Removes all data related to in-raid activities from the database.
        /// </summary>
        /// <remarks>This method deletes all records in the in-raid data table, saves the changes to the
        /// database,  and refreshes the in-raid data list. It is intended for internal use only.</remarks>
        /// <returns></returns>
        internal async Task RemoveAllInRaidData()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.InRaidData.ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshInRaidDataList(true);
        }

        /// <summary>
        /// Removes all data related to out-of-raid activities from the database.
        /// </summary>
        /// <remarks>This method deletes all records in the out-of-raid data table and refreshes the
        /// associated data list. It ensures that the changes are persisted to the database and optionally triggers a
        /// full refresh of the data.</remarks>
        /// <returns></returns>
        internal async Task RemoveAllOutRaidData()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.OutRaidData.ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshOutRaidDataList(true);
        }

        /// <summary>
        /// Removes all overlay ticker data by clearing the associated user names.
        /// </summary>
        /// <remarks>This method updates all entries in the overlay ticker to reset the user name field to
        /// an empty string. After the update, the changes are saved to the database, and the overlay ticker list is
        /// refreshed.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task RemoveAllOverlayTickerData()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.OverlayTicker.ExecuteUpdateAsync((t) => t.SetProperty((o) => o.UserName, (u) => ""));
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshOverlayTickerList(true);
        }

        /// <summary>
        /// Removes all stream statistics from the data store.
        /// </summary>
        /// <remarks>This method deletes all entries in the stream statistics table and refreshes the
        /// in-memory list of stream statistics.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task RemoveAllStreamStats()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.StreamStats.ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshStreamStatsList(true);
        }

        /// <summary>
        /// Removes all users from the data store.
        /// </summary>
        /// <remarks>This method deletes all user records from the underlying data store and refreshes the
        /// user list. It is intended for internal use only and should be used with caution, as it will permanently 
        /// remove all user data.</remarks>
        /// <returns></returns>
        internal async Task RemoveAllUsers()
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            await context.Users.ExecuteDeleteAsync();
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            RefreshUsersList(true);
        }
        #endregion

        #region Remove Records
        /// <summary>
        /// Removes a command from the data store if it exists.
        /// </summary>
        /// <remarks>This method removes the specified command from the data store and updates the
        /// in-memory command list. If the command does not exist, no changes are made, and the method returns <see
        /// langword="false"/>.</remarks>
        /// <param name="command">The name of the command to remove. This value cannot be null or empty.</param>
        /// <returns><see langword="true"/> if the command was found and successfully removed; otherwise, <see
        /// langword="false"/>.</returns>
        internal async Task<bool> RemoveCommand(string command)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            bool found = false;

            CommandsUser cmd = await context.CommandsUser.Where(C => C.CmdName == command).Select(C => C).FirstOrDefaultAsync();
            if (cmd != default)
            {
                context.CommandsUser.Remove(cmd);
                found = true;
            }
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshCommandsUserList(true);

            return found;
        }

        /// <summary>
        /// Removes a quote with the specified quote number from the data store.
        /// </summary>
        /// <remarks>This method deletes the quote from the data store and refreshes the quotes list 
        /// after the operation. If no quote with the specified number exists, no changes are made.</remarks>
        /// <param name="QuoteNum">The unique number identifying the quote to be removed.</param>
        /// <returns><see langword="true"/> if a quote with the specified number was found and removed;  otherwise, <see
        /// langword="false"/>.</returns>
        internal async Task<bool> RemoveQuote(int QuoteNum)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            bool found = false;

            Quotes quotes = await context.Quotes.Where(Q => Q.Number == QuoteNum).Select(Q => Q).FirstOrDefaultAsync();

            if (quotes != default)
            {
                context.Quotes.Remove(quotes);
                found = true;
            }
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync();
            await RefreshQuotesList(true);

            return found;
        }

        #endregion

    }
}
