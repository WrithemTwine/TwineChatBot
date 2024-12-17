using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;

using System.Data;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        internal Task UpdateCurrency(List<string> Users, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                await context.Users.Join(Users, (u) => u.UserId, (user) => user, (dbusers, curr) => dbusers).ForEachAsync((u) =>
                {
                    TimeSpan clock = dateTime - u.LastDateSeen;
                    foreach (Currency currency in u.Currency)
                    {
                        currency.Value =
                            Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round((currency.Value + currency.CurrencyType.AccrueAmt) * (clock.TotalSeconds / currency.CurrencyType.Seconds), 2)
                            );
                    }
                    u.LastDateSeen = dateTime;
                });

                if (Refcontext == null)
                {
                    await context.SaveChangesAsync();
                    RefreshCurrencyObservableCollection();
                    ClearDataContext(context);
                }
            });
        }

        internal Task<List<LearnMsgRecord>> UpdateLearnedMsgs(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                List<LearnMsgRecord> result;
                SQLDBContext context = Refcontext ?? BuildDataContext();
                if (LearnMsgChanged)
                {
                    LearnMsgChanged = false;
                    result = new(from L in context.LearnMsgs
                                 select new LearnMsgRecord(L.Id, L.MsgType.ToString(), L.TeachingMsg));
                }
                else
                {
                    result = null;
                }
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task UpdateOverlayTicker(OverlayTickerItem item, string name, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                OverlayTicker ticker = (from T in context.OverlayTicker where T.TickerName == item select T).FirstOrDefault();
                if (ticker == default)
                {
                    await context.OverlayTicker.AddAsync(new(tickerName: item, userName: name));
                }
                else
                {
                    ticker.UserName = name;
                }
                await context.SaveChangesAsync();
                RefreshOverlayTickerObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                await context.Users.Join(Users, (u) => u.UserId, (user) => user.UserId, (dbusers, curr) => dbusers).ForEachAsync(async (u) =>
                {
                    if (u.UserStats == default)
                    {
                        await context.UserStats.AddAsync(new(userId: u.UserId, platform: u.Platform));
                    }

                    if (u.LastDateSeen < CurrStreamStart)
                    {
                        u.LastDateSeen = CurrStreamStart;
                    }

                    if (CurrTime > u.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        u.UserStats.WatchTime = u.UserStats.WatchTime.Add(CurrTime - u.LastDateSeen);
                    }

                });

                if (Refcontext == null)
                {
                    await context.SaveChangesAsync();
                    //RefreshUsersObservableCollection();
                    //RefreshUserStatsObservableCollection();

                    GUIContext.Users.Load();
                    NotifyDataCollectionUpdated(nameof(GUIContext.Users));
                    GUIContext.UserStats.Load();
                    NotifyDataCollectionUpdated(nameof(GUIContext.UserStats));


                    ClearDataContext(context);
                }
            });
        }

        #region Update User Stats

        internal Task UpdateStats(DBUserStats Stat, string userId, Platform platform, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

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
                if (Refcontext == null)
                {
                    await context.SaveChangesAsync();
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            });
        }

        #endregion
    }
}
