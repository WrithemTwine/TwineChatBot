using StreamerBotLib.Enums;
using StreamerBotLib.Models;

namespace StreamerBotLib.DataSQL.EFC9
{
    internal partial class DataManagerSQLAsync
    {
        #region Check_Methods

        internal async Task<bool> CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            return await Task.FromResult(true);
        }

        internal async Task<bool> CheckField(string table, string field)
        {
            return await Task.FromResult(true);
        }

        internal async Task<bool> CheckFollower(string User)
        {
            return await Task.FromResult(true);
        }

        internal async Task<bool> CheckFollower(string User, DateTime ToDateTime)
        {
            return await Task.FromResult(true);
        }

        internal async Task<Tuple<string, string>> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            return await Task.FromResult<Tuple<string, string>>(null);
        }

        /// <summary>
        /// Determines if there are multiple streams based on the same start date.
        /// </summary>
        /// <param name="streamStart">The stream start date and time to check.</param>
        /// <returns><code>true</code> if there are multiple streams on the same day
        /// <code>false</code> if there is no more than one stream for the current day.</returns>
        internal async Task<bool> CheckMultiStreams(DateTime streamStart)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
        internal async Task<bool> CheckPermission(string cmd, ViewerTypes permission)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        internal async Task<bool> CheckShoutName(string UserId)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        internal async Task<bool> CheckStreamTime(DateTime CurrTime)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
        internal async Task<bool> CheckUser(LiveUser User)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        internal async Task<bool> CheckUser(LiveUser User, DateTime ToDateTime)
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        internal async Task<string> CheckWelcomeUser(string UserId)
        {
            return await Task.FromResult<string>(null);
        }

        #endregion Check_Methods
    }
}
