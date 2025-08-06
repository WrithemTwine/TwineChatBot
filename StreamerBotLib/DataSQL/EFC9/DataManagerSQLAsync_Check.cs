using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        #region Check_Methods

        /// <summary>
        /// Checks whether the specified user has a currency balance greater than or equal to the given value.
        /// </summary>
        /// <param name="User">The user whose currency balance is being checked. Cannot be null.</param>
        /// <param name="value">The minimum currency value to check against. Must be a non-negative number.</param>
        /// <param name="CurrencyName">The name of the currency to check. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
        /// user's currency balance is greater than or equal to the specified value; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            using var context = BuildDataContext();

            return await context.Currency
                                .Where(C => C.UserId == User.UserId && C.CurrencyName == CurrencyName)
                                .Select(C => C.Value >= value)
                                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Determines whether a specified field exists in a given database table.
        /// </summary>
        /// <remarks>This method performs the check by inspecting the database model metadata. Ensure that
        /// the table and field names are correctly specified and match the database schema. The method does not
        /// validate the input names against SQL injection or other security concerns.</remarks>
        /// <param name="table">The name of the database table to check. This must be a valid table name in the database schema.</param>
        /// <param name="field">The name of the field to check for existence within the specified table. This must be a valid field name.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the field
        /// exists in the table; otherwise, <see langword="false"/>.</returns>
        internal Task<bool> CheckField(string table, string field)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var entityType = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}");
                return entityType?.FindProperty(field) != null;
            });
        }

        /// <summary>
        /// Determines whether the specified user is a follower.
        /// </summary>
        /// <param name="User">The username to check for follower status. Cannot be null or empty.</param>
        /// <returns><see langword="true"/> if the specified user is a follower; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> CheckFollower(string User)
        {
            return await CheckFollower(User, default);
        }

        /// <summary>
        /// Determines whether the specified user has any followers that meet the given criteria.
        /// </summary>
        /// <remarks>This method queries the database asynchronously to determine if any followers exist 
        /// for the specified user. The query includes followers who are marked as active  (<c>IsFollower</c> is <see
        /// langword="true"/>) and optionally filters by the  <paramref name="ToDateTime"/> parameter.</remarks>
        /// <param name="User">The username of the user to check for followers.</param>
        /// <param name="ToDateTime">An optional date and time to filter followers. Only followers added before this date  will be considered. If
        /// <see langword="default"/> is provided, all followers are considered.</param>
        /// <returns><see langword="true"/> if the user has at least one follower that matches the criteria;  otherwise, <see
        /// langword="false"/>.</returns>
        internal async Task<bool> CheckFollower(string User, DateTime ToDateTime)
        {
            using var context = BuildDataContext();

            return await context.Followers
                                .Include(user => user.User)
                                .Where(f => f.User.UserName == User && (f.IsFollower && (ToDateTime == default || f.FollowedDate < ToDateTime)))
                                .Select(f => f)
                                .AnyAsync();
        }

        /// <summary>
        /// Checks for a moderator approval rule based on the specified moderation action type and action name.
        /// </summary>
        /// <remarks>This method queries the database for a matching moderator approval rule based on the
        /// provided moderation action type and name. If a match is found, it returns the corresponding moderation
        /// action type and action to perform.</remarks>
        /// <param name="modActionType">The type of moderation action to check for approval rules.</param>
        /// <param name="ModAction">The name of the moderation action to check for approval rules. Cannot be null or empty.</param>
        /// <returns>A <see cref="Tuple{T1, T2}"/> containing two strings: <list type="bullet"> <item><description>The type of
        /// moderation action to perform, or the moderation action type if no specific type is
        /// defined.</description></item> <item><description>The specific moderation action to perform, or the
        /// moderation action name if no specific action is defined.</description></item> </list> Returns 
        /// <see langword="null"/> if no matching approval rule is found.</returns>
        internal async Task<Tuple<string, string>> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            LogWriter.DebugLog("CheckModApprovalRule", DebugLogTypes.DataManager, $"Now checking for mod approval rule for {ModAction}.");

            using var context = BuildDataContext();

            return await context.ModeratorApprove
                    .Where(M => (M.ModActionType == modActionType && M.ModActionName == ModAction))
                    .Select(M => new Tuple<string, string>(
                        !string.IsNullOrEmpty(M.ModPerformType.ToString()) ? M.ModPerformType.ToString() : M.ModActionType.ToString(),
                        !string.IsNullOrEmpty(M.ModPerformAction) ? M.ModPerformAction : M.ModActionName
                        ))
                    .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Determines if there are multiple streams based on the same start date.
        /// </summary>
        /// <param name="streamStart">The stream start date and time to check.</param>
        /// <returns><code>true</code> if there are multiple streams on the same day
        /// <code>false</code> if there is no more than one stream for the current day.</returns>
        internal async Task<bool> CheckMultiStreams(DateTime streamStart)
        {
            using var context = BuildDataContext();
            return await context.StreamStats
                                .Where(s => s.StreamStart.Date == streamStart.Date)
                                .CountAsync() > 1;
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
        internal async Task<bool> CheckPermission(string cmd, ViewerTypes permission)
        {
            using var context = BuildDataContext();

            return await context.Commands
                            .Where(c => c.CmdName == cmd)
                            .Select(c => c.Permission > permission)
                            .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        internal async Task<bool> CheckShoutName(string UserId)
        {
            using var context = BuildDataContext();

            return await context.ShoutOuts
                                .Where(s => s.UserId == UserId)
                                .Select(s => s)
                                .AnyAsync();
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        internal async Task<bool> CheckStreamTime(DateTime CurrTime)
        {
            using var context = BuildDataContext();

            return await context.StreamStats
                                .Where(s => s.StreamStart == CurrTime)
                                .Select(s => s)
                                .AnyAsync();
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
        internal async Task<bool> CheckUser(LiveUser User)
        {
            return await CheckUser(User, default);
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        internal async Task<bool> CheckUser(LiveUser User, DateTime ToDateTime)
        {
            using var context = BuildDataContext();
            return await context.Users
                                .Where(s => (ToDateTime == default || s.FirstDateSeen < ToDateTime) && s.UserName == User.UserName && s.Platform == User.Platform)
                                .Select(s => s)
                                .AnyAsync();
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        internal async Task<string> CheckWelcomeUser(string UserId)
        {
            using var context = BuildDataContext();

            string result = (from s in context.CustomWelcome
                             where s.UserId == UserId
                             select s.Message).FirstOrDefault() ?? "";

            return await context.CustomWelcome
                                .Where(s => s.UserId == UserId)
                                .Select(s => s.Message)
                                .FirstOrDefaultAsync() ?? "";
        }

        #endregion Check_Methods
    }
}
