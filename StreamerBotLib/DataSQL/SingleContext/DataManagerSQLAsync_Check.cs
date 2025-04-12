using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        #region Check_Methods

        internal Task<bool> CheckCurrency(LiveUser User, double value, string CurrencyName)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();
                var result = (from C in context.Currency
                              where (C.UserId == User.UserId && C.CurrencyName == CurrencyName)
                              select C.Value).FirstOrDefault() >= value;



                return result;
            });
        }

        internal Task<bool> CheckField(string table, string field)
        {
            return Task.Run(() =>
            {
               // using var context = BuildDataContext();
                var result = (context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}").FindProperty(field) != null);

                return result;
            });
        }

        internal Task<bool> CheckFollower(string User)
        {
            return Task.Run(async () =>
            {
                var result = await CheckFollower(User, default);

                return result;
            });
        }

        internal Task<bool> CheckFollower(string User, DateTime ToDateTime)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();
                var result = (from f in context.Followers.Include(user => user.User)
                              where f.User.UserName == User && (f.IsFollower && (ToDateTime == default || f.FollowedDate < ToDateTime))
                              select f).Any();

                return result;
            });
        }

        internal Task<Tuple<string, string>> CheckModApprovalRule(ModActionType modActionType, string ModAction)
        {
            return Task.Run(() =>
            {
                LogWriter.DebugLog("CheckModApprovalRule", DebugLogTypes.DataManager, $"Now checking for mod approval rule for {ModAction}.");

                //using var context = BuildDataContext();
                var result = (from M in context.ModeratorApprove
                              where (M.ModActionType == modActionType && M.ModActionName == ModAction)
                              select new Tuple<string, string>(
                                  !string.IsNullOrEmpty(M.ModPerformType.ToString()) ? M.ModPerformType.ToString() : M.ModActionType.ToString(),
                                  !string.IsNullOrEmpty(M.ModPerformAction) ? M.ModPerformAction : M.ModActionName
                                  )).FirstOrDefault();

                return result;
            });
        }

        /// <summary>
        /// Determines if there are multiple streams based on the same start date.
        /// </summary>
        /// <param name="streamStart">The stream start date and time to check.</param>
        /// <returns><code>true</code> if there are multiple streams on the same day
        /// <code>false</code> if there is no more than one stream for the current day.</returns>
        internal Task<bool> CheckMultiStreams(DateTime streamStart)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();

                bool result = (from s in context.StreamStats
                                   // check for the Year/Month/Day are the same, ignoring the time
                               where s.StreamStart.Year == streamStart.Year
                               && s.StreamStart.Month == streamStart.Month
                               && s.StreamStart.Day == streamStart.Day
                               select s).Count() > 1;

                return result;
            });
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
        internal Task<bool> CheckPermission(string cmd, ViewerTypes permission)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();

                bool result = (from c in context.Commands
                               where c.CmdName == cmd
                               select c).FirstOrDefault().Permission > permission;

                return result;
            });
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        internal Task<bool> CheckShoutName(string UserId)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();

                bool result = (from s in context.ShoutOuts
                               where s.UserId == UserId
                               select s).Any();

                return result;
            });
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        internal Task<bool> CheckStreamTime(DateTime CurrTime)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();

                bool result = (from s in context.StreamStats
                               where s.StreamStart == CurrTime
                               select s).Any();

                return result;
            });
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
        internal Task<bool> CheckUser(LiveUser User)
        {
            return Task.Run(async () =>
            {
                return await CheckUser(User, default);
            });
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        internal Task<bool> CheckUser(LiveUser User, DateTime ToDateTime)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();

                bool result = (from s in context.Users
                               where (ToDateTime == default || s.FirstDateSeen < ToDateTime) && s.UserName == User.UserName && s.Platform == User.Platform
                               select s).Any();

                return result;
            });
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        internal Task<string> CheckWelcomeUser(string UserId)
        {
            return Task.Run(() =>
            {
                //using var context = BuildDataContext();

                string result = (from s in context.CustomWelcome
                                 where s.UserId == UserId
                                 select s.Message).FirstOrDefault() ?? "";

                return result;
            });
        }

        #endregion Check_Methods

    }
}
