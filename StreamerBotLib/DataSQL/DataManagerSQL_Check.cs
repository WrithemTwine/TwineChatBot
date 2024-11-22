using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        #region Check_Methods

        public bool CheckCurrency(LiveUser User, double value, string CurrencyName, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from C in context.Currency
                              where (C.UserId == User.UserId && C.CurrencyName == CurrencyName)
                              select C.Value).FirstOrDefault() >= value;

                if (Refcontext == null) { ClearDataContext(context); }

                return result;
            }
        }

        public bool CheckField(string table, string field, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (context.Model.FindEntityType($"StreamerBotLib.DataSQL.Models.{table}").FindProperty(field) != null);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public bool CheckFollower(string User, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = CheckFollower(User, default, context);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public bool CheckFollower(string User, DateTime ToDateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from f in context.Followers
                              where (f.IsFollower && (ToDateTime == default || f.FollowedDate < ToDateTime))
                              select f).Any();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public Tuple<string, string> CheckModApprovalRule(ModActionType modActionType, string ModAction, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from M in context.ModeratorApprove
                              where (M.ModActionType == modActionType && M.ModActionName == ModAction)
                              select new Tuple<string, string>(
                                  !string.IsNullOrEmpty(M.ModPerformType.ToString()) ? M.ModPerformType.ToString() : M.ModActionType.ToString(),
                                  !string.IsNullOrEmpty(M.ModPerformAction) ? M.ModPerformAction : M.ModActionName
                                  )).FirstOrDefault();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Determines if there are multiple streams based on the same start date.
        /// </summary>
        /// <param name="streamStart">The stream start date and time to check.</param>
        /// <returns><code>true</code> if there are multiple streams
        /// <code>false</code> if there is no more than one stream.</returns>
        public bool CheckMultiStreams(DateTime streamStart, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                bool result = (from s in context.StreamStats
                               where (s.StreamStart == streamStart)
                               select s).Count() > 1;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Determine if the user invoking the command has permission to access the command.
        /// </summary>
        /// <param name="cmd">The command to verify the permission.</param>
        /// <param name="permission">The supplied permission to check.</param>
        /// <returns><code>true</code> - the permission is allowed to the command. <code>false</code> - the command permission is not allowed.</returns>
        public bool CheckPermission(string cmd, ViewerTypes permission, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                bool result = (from c in context.Commands
                               where c.CmdName == cmd
                               select c).FirstOrDefault().Permission > permission;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Verify if the provided UserName is within the ShoutOut table.
        /// </summary>
        /// <param name="UserName">The UserName to shoutout.</param>
        /// <returns>true if in the ShoutOut table.</returns>
        public bool CheckShoutName(string UserId, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                bool result = (from s in context.ShoutOuts
                               where s.UserId == UserId
                               select s).Any();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Find if stream data already exists for the current stream
        /// </summary>
        /// <param name="CurrTime">The time to check</param>
        /// <returns><code>true</code>: the stream already has a data entry; <code>false</code>: the stream has no data entry</returns>
        public bool CheckStreamTime(DateTime CurrTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                bool result = (from s in context.StreamStats
                               where s.StreamStart == CurrTime
                               select s).Any();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Check to see if the <paramref name="User"/> has been in the channel prior to DateTime.MaxValue.
        /// </summary>
        /// <param name="User">The user to check in the database.</param>
        /// <returns><code>true</code> if the <paramref name="User"/> has arrived anytime, <code>false</code> otherwise.</returns>
        public bool CheckUser(LiveUser User, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                return CheckUser(User, default, Refcontext);
            }
        }

        /// <summary>
        /// Check if the <paramref name="User"/> has visited the channel prior to <paramref name="ToDateTime"/>, identified as either DateTime.Now.ToLocalTime() or the current start of the stream.
        /// </summary>
        /// <param name="User">The user to verify.</param>
        /// <param name="ToDateTime">Specify the date to check if the user arrived to the channel prior to this date and time.</param>
        /// <returns><c>True</c> if the <paramref name="User"/> has been in channel before <paramref name="ToDateTime"/>, <c>false</c> otherwise.</returns>
        public bool CheckUser(LiveUser User, DateTime ToDateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                bool result = (from s in context.Users
                               where (ToDateTime == default || s.FirstDateSeen < ToDateTime) && s.UserName == User.UserName && s.Platform == User.Platform
                               select s).Any();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// Check the CustomWelcome table for the user and provide the message.
        /// </summary>
        /// <param name="User">The user to check for a welcome message.</param>
        /// <returns>The welcome message if user is available, or empty string if not found.</returns>
        public string CheckWelcomeUser(string UserId, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                string result = (from s in context.CustomWelcome
                                 where s.UserId == UserId
                                 select s.Message).FirstOrDefault() ?? "";
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        #endregion Check_Methods

    }
}
