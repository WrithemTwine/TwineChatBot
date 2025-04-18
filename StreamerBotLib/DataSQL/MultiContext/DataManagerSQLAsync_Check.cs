using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.MultiContext
{
    internal partial class DataManagerSQLAsync
    {
        #region Check_Methods

        internal async Task<bool> CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            using var context = BuildDataContext();

            return await context.Currency
                                .Where(C => C.UserId == User.UserId && C.CurrencyName == CurrencyName)
                                .Select(C => C.Value >= value)
                                .FirstOrDefaultAsync();
        }

        internal Task<bool> CheckField(string table, string field)
        {
            return Task.Run(() =>
            {
                using var context = BuildDataContext();
                var entityType = context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}");
                return entityType?.FindProperty(field) != null;
            });
        }

        internal async Task<bool> CheckFollower(string User)
        {
            return await CheckFollower(User, default);
        }

        internal async Task<bool> CheckFollower(string User, DateTime ToDateTime)
        {
            using var context = BuildDataContext();

            return await context.Followers
                                .Include(user => user.User)
                                .Where(f => f.User.UserName == User && (f.IsFollower && (ToDateTime == default || f.FollowedDate < ToDateTime)))
                                .Select(f => f)
                                .AnyAsync();
        }

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
