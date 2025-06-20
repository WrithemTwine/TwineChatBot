using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;
using StreamerBotLib.Static;

using System.Data;

namespace StreamerBotLib.DataSQL.MultiContext
{
    internal partial class DataManagerSQLAsync
    {
        internal async Task UpdateCurrency(List<LiveUser> Users, DateTime dateTime)
        {
            using var context = BuildDataContext();

#if DEBUG
            var NewCurrencyList = new List<Tuple<string, Currency>>();
            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.SpecialPurpose, $"Testing Currency update itself:");
            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.SpecialPurpose, $"CurrTime: {dateTime}");
#endif

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
#if DEBUG
                            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.SpecialPurpose, $"context UserId: {u.UserId}, LastDateSeen: {u.LastDateSeen}, Currency Value: {currency.Value}");
#endif
                            currency.Value = Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round(currency.Value + (currency.CurrencyType.AccrueAmt * (clock.TotalSeconds / currency.CurrencyType.Seconds)), 2)
                            );

#if DEBUG
                            NewCurrencyList.Add(new Tuple<string, Currency>(u.UserId, currency));
                            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.SpecialPurpose, $"context UserId: {u.UserId}, Currency Value: {currency.Value}");
#endif
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
            await RefreshCurrencyList();

#if DEBUG

            var GUICurrencyList = await context.Currency
                .Include(c => c.User)
                .Where(c => userIds.Contains(c.UserId))
                .Select(c => new Tuple<string, Currency>(c.UserId, c))
                .ToListAsync();

            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.SpecialPurpose, $"Testing Currency Context to GUI Context currency records updated:");
            LogWriter.DebugLog("UpdateCurrency", DebugLogTypes.SpecialPurpose, $"GUICurrencyList count: {GUICurrencyList.Count}");

            for (int i = 0; i < NewCurrencyList.Count; i++)
            {
                var newCurrItem = GUICurrencyList.Find(x => x.Item1 == NewCurrencyList[i].Item1);

                LogWriter.DebugLog("UpdateCurrency_GUICurrencyList", DebugLogTypes.SpecialPurpose, $"GUIContext newCurrItem is {(newCurrItem == null ? "Null" : "Not Null")}");
                LogWriter.DebugLog("UpdateCurrency_GUICurrencyList", DebugLogTypes.SpecialPurpose, $"context NewCurrencyList[{i}].Item1 = {NewCurrencyList[i].Item1}, GUIContext newCurrItem.Item1 = {newCurrItem?.Item1}");
                LogWriter.DebugLog("UpdateCurrency_GUICurrencyList", DebugLogTypes.SpecialPurpose, $"context NewCurrencyList[{i}].Item2.Value = {NewCurrencyList[i].Item2.Value}, GUIContext newCurrItem.Item2.Value = {newCurrItem?.Item2.Value}");
            }
#endif

            await UpdateWatchTime(Users, dateTime);
        }

        internal async Task<List<LearnMsgRecord>> UpdateLearnedMsgs()
        {
            if (!LearnMsgChanged)
            {
                return (List<LearnMsgRecord>)null;
            }

            LearnMsgChanged = false;

            using var context = BuildDataContext();
            return await context.LearnMsgs
                .Select(L => new LearnMsgRecord(L.Id, L.MsgType.ToString(), L.TeachingMsg))
                .ToListAsync();
        }

        internal async Task UpdateOverlayTicker(OverlayTickerItem item, string name)
        {
            bool recordchange = false;

            using var context = BuildDataContext();
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
            await context.SaveChangesAsync();
            await RefreshOverlayTickerList(recordchange);
        }

        internal Task UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime)
        {
            return Task.Run(async () =>
            {
                using var context = BuildDataContext();

#if DEBUG
                List<Tuple<string, TimeSpan>> NewWatchTimeList = [];
                LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose, $"CurrTime: {CurrTime}, Users Count: {Users.Count}");
                LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose, $"CurrStreamStart: {CurrStreamStart}");
#endif

                foreach (var user in Users.Select(liveUser => context.Users
                    .Include(u => u.UserStats)
                    .FirstOrDefault(u => u.UserId == liveUser.UserId && u.Platform == liveUser.Platform))
                    .Where(user => user != null))
                {
#if DEBUG
                    LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose,
                        $"Old stats: user Id: {user.UserId}, LastDateSeen: {user.LastDateSeen}, WatchTime: {user.UserStats.WatchTime}");
#endif

                    if (user.LastDateSeen >= CurrStreamStart && CurrTime > user.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        user.UserStats.WatchTime = user.UserStats.WatchTime.Add(CurrTime - user.LastDateSeen);
                    }

                    user.LastDateSeen = CurrTime;

#if DEBUG
                    LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose,
                        $"New stats: user Id: {user.UserId}, LastDateSeen: {user.LastDateSeen}, WatchTime: {user.UserStats.WatchTime}");
                    NewWatchTimeList.Add(new Tuple<string, TimeSpan>(user.UserId, user.UserStats.WatchTime));
#endif
                }

                await context.SaveChangesAsync();

                await RefreshUserStatsList();
                await RefreshUsersList();

                //#if DEBUG
                //                List<Tuple<string, TimeSpan>> GUICurrWatchTimeList = await GUIContext.Users
                //                    .Include(user => user.UserStats)
                //                    .Where(U => U.Platform == Platform.Twitch && Users.Any(L => L.UserId == U.UserId))
                //                    .Select(U => new Tuple<string, TimeSpan>(U.UserId, U.UserStats.WatchTime))
                //                    .ToListAsync();

                //                LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose, $"GUICurrWatchTimeList.Count = {GUICurrWatchTimeList.Count}, NewWatchTimeList.Count = {NewWatchTimeList.Count}");
                //                for (int i = 0; i < NewWatchTimeList.Count; i++)
                //                {
                //                    var newCurrItem = GUICurrWatchTimeList.Find(x => x.Item1 == NewWatchTimeList[i].Item1);

                //                    LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose, $"GUIContext newCurrItem is {(newCurrItem == null ? "Null" : "Not Null")}");
                //                    LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose, $"context NewWatchTimeList[{i}].Item1 = {NewWatchTimeList[i].Item1}, GUIContext newCurrItem.Item1 = {newCurrItem.Item1}");
                //                    LogWriter.DebugLog("UpdateWatchTime", DebugLogTypes.SpecialPurpose, $"context NewWatchTimeList[{i}].Item2 = {NewWatchTimeList[i].Item2}, GUIContext newCurrItem.Item2 = {newCurrItem.Item2}");
                //                }
                //#endif
            });
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
                    await context.SaveChangesAsync();
                    await RefreshUserStatsList();
                }
            }
        }

        #endregion
    }
}
