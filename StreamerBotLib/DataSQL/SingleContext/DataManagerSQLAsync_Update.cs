using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;

using System.Data;

namespace StreamerBotLib.DataSQL.SingleContext
{
    internal partial class DataManagerSQLAsync
    {
        internal Task UpdateCurrency(List<string> Users, DateTime dateTime, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                await context.Users
                .Include(curr => curr.Currency)
                .ThenInclude(type => type.CurrencyType)
                .Join(Users, (u) => u.UserId, (user) => user, (dbusers, curr) => dbusers)
                .ForEachAsync((u) =>
                {
                    TimeSpan clock = dateTime - u.LastDateSeen;
                    foreach (Currency currency in u.Currency)
                    {
                        currency.Value =
                            Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                            );
                    }
                    u.LastDateSeen = dateTime;
                });
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshCurrencyList();
            });
        }

        internal Task<List<LearnMsgRecord>> UpdateLearnedMsgs()
        {
            return Task.Run(() =>
            {
                List<LearnMsgRecord> result;
                // using var context = BuildDataContext();

                if (LearnMsgChanged)
                {
                    LearnMsgChanged = false;
                    result = [.. from L in context.LearnMsgs
                                 select new LearnMsgRecord(L.Id, L.MsgType.ToString(), L.TeachingMsg)];
                }
                else
                {
                    result = null;
                }

                return result;
            });
        }

        internal Task UpdateOverlayTicker(OverlayTickerItem item, string name, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                OverlayTicker ticker = (from T in context.OverlayTicker where T.TickerName == item select T).FirstOrDefault();
                if (ticker == default)
                {
                    await context.OverlayTicker.AddAsync(new(tickerName: item, userName: name));
                }
                else
                {
                    ticker.UserName = name;
                }
                //await transaction.CommitAsync();    
                await context.SaveChangesAsync();
                RefreshOverlayTickerList();
            });
        }

        internal Task UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                foreach (var user in from LiveUser L in Users
                                     let user = (from U in context.Users
                                                 where U.UserId == L.UserId && U.Platform == L.Platform
                                                 select U).FirstOrDefault()
                                     where user != default
                                     select user)
                {
                    if (user.LastDateSeen < CurrTime)
                    {
                        user.LastDateSeen = CurrTime;
                    }

                    if (CurrTime > user.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        user.UserStats.WatchTime = user.UserStats.WatchTime.Add(CurrTime - user.LastDateSeen);
                    }
                }
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();

                RefreshUsersList();
                RefreshUserStatsList();
            });
        }

        #region Update User Stats

        internal Task UpdateStats(DBUserStats Stat, string userId, Platform platform, IDbContextTransaction contextTransaction = null)
        {
            return Task.Run(async () =>
            {
                // using var context = BuildDataContext();
                //using var transaction = await context.Database.BeginTransactionAsync();

                if (userId != null)
                {
                    UserStats userStats = (from U in context.UserStats
                                           where U.UserId == userId && U.Platform == platform
                                           select U).FirstOrDefault();

                    if (userStats != null)
                    {
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
                    }

                }
                //await transaction.CommitAsync();
                await context.SaveChangesAsync();
                RefreshUserStatsList();
            });
        }

        #endregion
    }
}
