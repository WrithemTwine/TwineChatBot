using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Overlay.Enums;

using System.Data;


namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        public void UpdateCurrency(List<string> Users, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (string U in Users)
                {
                    UpdateCurrency((from user in context.Users where user.UserName == U select user).FirstOrDefault(), dateTime, context);
                }

                if (Refcontext == null)
                {
                    context.SaveChanges(true);
                    RefreshCurrencyObservableCollection();
                    ClearDataContext(context);
                }
            }
        }

        private void UpdateCurrency(Users User, DateTime CurrTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                if (User != null)
                {
                    TimeSpan clock = CurrTime - User.LastDateSeen;
                    foreach (Currency currency in User.Currency)
                    {
                        currency.Value =
                            Math.Min(
                                currency.CurrencyType.MaxValue,
                                Math.Round((currency.Value + currency.CurrencyType.AccrueAmt) * (clock.TotalSeconds / currency.CurrencyType.Seconds), 2)
                            );
                    }
                    User.LastDateSeen = CurrTime;
                }
            }
        }


        public List<LearnMsgRecord> UpdateLearnedMsgs(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
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
            }
        }

        public void UpdateOverlayTicker(OverlayTickerItem item, string name, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                OverlayTicker ticker = (from T in context.OverlayTicker where T.TickerName == item select T).FirstOrDefault();
                if (ticker == default)
                {
                    context.OverlayTicker.Add(new(tickerName: item, userName: name));
                }
                else
                {
                    ticker.UserName = name;
                }
                context.SaveChanges(true);
                RefreshOverlayTickerObservableCollection();
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void UpdateWatchTime(List<LiveUser> Users, DateTime CurrTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                foreach (LiveUser L in Users)
                {
                    Users curruser = (from S in context.Users
                                      where (S.UserId == L.UserId && S.Platform == L.Platform)
                                      select S).FirstOrDefault();

                    //if (curruser == default)
                    //{
                    //    curruser = context.UserStats.Add(new(userId: L.UserId, platform: L.Platform)).Entity;
                    //}

                    if (curruser.LastDateSeen < CurrStreamStart)
                    {
                        curruser.LastDateSeen = CurrStreamStart;
                    }

                    if (CurrTime > curruser.LastDateSeen && CurrTime > CurrStreamStart)
                    {
                        curruser.UserStats.WatchTime = curruser.UserStats.WatchTime.Add(CurrTime - curruser.LastDateSeen);
                    }
                }
                if (Refcontext == null)
                {
                    context.SaveChanges(true);
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            }
        }

        public void UpdateWatchTime(LiveUser User, DateTime CurrTime, SQLDBContext Refcontext = null)
        {
            UpdateWatchTime([User], CurrTime, Refcontext);
        }

        #region Update User Stats

        public void UpdateStats(DBUserStats Stat, string userId, Platform platform, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
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
                    context.SaveChanges(true);
                    RefreshUserStatsObservableCollection();
                    ClearDataContext(context);
                }
            }
        }

        #endregion
    }
}
