#define FixCurrency

using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Systems.Overlay.Enums;

namespace StreamerBotLib.DataSQL.EFC10
{
    internal partial class DataManagerSQLAsync
    {
        //internal async Task UpdateIsEnabled(IEnumerable<EntityBase> entities, string TableName, bool isEnabled)
        //{
        //    if (entities != null)
        //    {
        //        switch (TableName)
        //        {
        //            case "BanReasons":
        //                using (var context = BuildDataContext())
        //                {
        //                    await context.Database.BeginTransactionAsync();
        //                    (entities.Cast<>)
        //                    await context.Database.CommitTransactionAsync();
        //                    await context.SaveChangesAsync(true);
        //                    await RefreshBanReasonsList(true);
        //                }
        //                break;
        //        }
        //    }

        //}

        internal async Task UpdateCurrency(List<LiveUser> Users, DateTime dateTime)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();

#if FixCurrency
            var CurrUsers = await context.Users
                                         .Where(d => d.LastDateSeen > CurrStreamStart)
                                         .Include(c => c.Currency)
                                         .ThenInclude(ct => ct.CurrencyType)
                                         .ToListAsync();

            var CurrencyUsers = CurrUsers.Where(u => Users.Contains(new(u.UserName, u.Platform, u.UserId)))
                                         .ToList();

            foreach (var u in CurrencyUsers)
            {
                TimeSpan clock = dateTime - u.LastDateSeen;

                if (u.Currency != null && u.LastDateSeen >= CurrStreamStart)
                {
                    foreach (var currency in u.Currency)
                    {
                        if (currency.CurrencyType != null)
                        {
                            currency.Value = Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                            );
                        }
                    }
                }
            }

#else

            var userIds = Users.Select(user => user.UserId).ToList();

            var dbUsers = await context.Users
                .Include(u => u.Currency)
                .ThenInclude(c => c.CurrencyType)
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();

            foreach (var u in dbUsers)
            {
                TimeSpan clock = dateTime - u.LastDateSeen;

                if (u.Currency != null && u.LastDateSeen >= CurrStreamStart)
                {
                    foreach (var currency in u.Currency)
                    {
                        if (currency.CurrencyType != null)
                        {
                            currency.Value = Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                            );
                        }
                    }
                }
            }

#endif



            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshCurrencyList();
            await UpdateWatchTime(Users, dateTime);
        }

        private List<LearnMsgRecord> _cachedLearnMsgRecords = null;

        internal async Task<List<LearnMsgRecord>> UpdateLearnedMsgs()
        {
            if (LearnMsgChanged)
            {
                LearnMsgChanged = false;

                using var context = BuildDataContext();
                _cachedLearnMsgRecords = await context.LearnMsgs
                    .Select(L => new LearnMsgRecord(L.Id, L.MsgType.ToString(), L.TeachingMsg))
                    .ToListAsync();
            }

            return _cachedLearnMsgRecords;
        }

        internal async Task UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            bool recordchange = false;

            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();
            OverlayTicker ticker = await context.OverlayTicker
                                                .Where(T => T.TickerName == item)
                                                .Select(T => T)
                                                .FirstOrDefaultAsync();
            if (ticker == default)
            {
                await context.OverlayTicker.AddAsync(new(tickerName: item, userName: name));
                recordchange = true;
            }
            else
            {
                ticker.UserName = name;
            }
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);
            await RefreshOverlayTickerList(recordchange);
        }

        internal async Task UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime)
        {
            using var context = BuildDataContext();
            await context.Database.BeginTransactionAsync();

            foreach (var user in Users.Select(liveUser => context.Users
                .Include(u => u.UserStats)
                .FirstOrDefault(u => u.UserId == liveUser.UserId && u.Platform == liveUser.Platform))
                .Where(user => user != null))
            {
                if (user.LastDateSeen >= CurrStreamStart && CurrTime > user.LastDateSeen && CurrTime > CurrStreamStart)
                {
                    user.UserStats.WatchTime = user.UserStats.WatchTime.Add(CurrTime - user.LastDateSeen);
                }

                user.LastDateSeen = CurrTime;
            }
            await context.Database.CommitTransactionAsync();
            await context.SaveChangesAsync(true);

            //await RefreshUserStatsList();
            await RefreshUsersList();
        }

        #region Update User Stats

        internal async Task UpdateStats(DBUserStats Stat, string userId, Platform platform)
        {
            using var context = BuildDataContext();

            if (userId != null)
            {
                UserStats userStats = await context.UserStats
                                      .Where(U => U.UserId == userId && U.Platform == platform)
                                       .Select(U => U).FirstOrDefaultAsync();


                if (userStats != null)
                {
                    await context.Database.BeginTransactionAsync();
                    switch (Stat)
                    {
                        case DBUserStats.Commands:
                            userStats.CallCommands++;
                            break;
                        case DBUserStats.Clips:
                            userStats.ClipsCreated++;
                            break;
                        case DBUserStats.Chats:
                            userStats.ChannelChat++;
                            break;
                        case DBUserStats.ChannelRewards:
                            userStats.RewardRedeems++;
                            break;
                    }
                    await context.Database.CommitTransactionAsync();
                    await context.SaveChangesAsync(true);
                    await RefreshUserStatsList();
                }
            }
        }

        #endregion
    }
}
