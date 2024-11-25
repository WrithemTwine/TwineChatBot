using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        #region Clear DataBase Records 
        public void ClearAllCurrencyValues(SQLDBContext Refcontext = null)
        {

            SQLDBContext context = Refcontext ?? BuildDataContext();
            lock (GUIDataManagerLock.Lock)
            {
                foreach (Currency c in from u in context.Currency
                                       select u)
                {
                    c.Value = 0;
                }
            }
            context.SaveChanges(true);
            RefreshCurrencyObservableCollection();
            if (Refcontext == null) { ClearDataContext(context); }
        }

        /// <summary>
        /// Clear all User rows for users not included in the Followers table.
        /// </summary>
        public void ClearUsersNotFollowers(SQLDBContext Refcontext = null)
        {
            SQLDBContext context = Refcontext ?? BuildDataContext();
            lock (GUIDataManagerLock.Lock)
            {


                context.Users.RemoveRange((IEnumerable<Users>)(from user in context.Users
                                                               join follower in context.Followers on user.UserId equals follower.UserId into UserFollow
                                                               from subuser in UserFollow.DefaultIfEmpty()
                                                               where !subuser.IsFollower
                                                               select subuser));
            }
            context.SaveChanges(true);
            RefreshUsersObservableCollection();
            RefreshFollowersObservableCollection();
            if (Refcontext == null) { ClearDataContext(context); }
        }

        /// <summary>
        /// Clear all user watchtimes
        /// </summary>
        public void ClearWatchTime(SQLDBContext Refcontext = null)
        {
            SQLDBContext context = Refcontext ?? BuildDataContext();
            lock (GUIDataManagerLock.Lock)
            {
                foreach (var userstat in from US in context.UserStats
                                         select US)
                {
                    userstat.WatchTime = new(0);
                }
            }

            context.SaveChanges(true);
            RefreshUserStatsObservableCollection();
            if (Refcontext == null) { ClearDataContext(context); }
        }

        /// <summary>
        /// Clear all 'Followers' table records.
        /// </summary>
        public void RemoveAllFollowers(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.Followers.RemoveRange(context.Followers);
                context.SaveChanges(true);
                RefreshFollowersObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Clear all 'GiveawayUserData' table records.
        /// </summary>
        public void RemoveAllGiveawayData(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.GiveawayUserData.RemoveRange(context.GiveawayUserData);
                context.SaveChanges(true);
                RefreshGiveawayUserDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Clear all 'InRaidData' table records.
        /// </summary>
        public void RemoveAllInRaidData(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.InRaidData.RemoveRange(context.InRaidData);
                context.SaveChanges(true);
                RefreshInRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Clear all 'OutRaidData' table records.
        /// </summary>
        public void RemoveAllOutRaidData(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.OutRaidData.RemoveRange(context.OutRaidData);
                context.SaveChanges(true);
                RefreshOutRaidDataObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Clear all 'OverlayTicker' table records.
        /// </summary>
        public void RemoveAllOverlayTickerData(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (OverlayTicker O in context.OverlayTicker)
                {
                    O.UserName = "";
                }
                context.SaveChanges(true);
                RefreshOverlayTickerObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Clear all 'StreamStats' table records.
        /// </summary>
        public void RemoveAllStreamStats(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.StreamStats.RemoveRange(context.StreamStats);
                context.SaveChanges(true);
                RefreshStreamStatsObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        /// <summary>
        /// Clear all 'Users' table records.
        /// </summary>
        public void RemoveAllUsers(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                context.Users.RemoveRange(context.Users);
                context.SaveChanges(true);
                RefreshUsersObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }
        #endregion

    }
}
