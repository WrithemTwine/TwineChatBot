using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL
{
    internal partial class DataManagerSQLAsync
    {
        #region MultiLive data
        internal event EventHandler UpdatedMonitoringChannels;
        private ObservableCollection<ArchiveMultiStream> CleanupList { get; } = [];
        private bool IsLiveStreamUpdated = false;

        internal ObservableCollection<ArchiveMultiStream> GetCleanupList()
        {
            return CleanupList;
        }

        internal Task<bool> CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from P in context.MultiLiveStreams where (P.UserId == UserId && P.Platform == platform && P.LiveDate == dateTime) select P).Count() > 1;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<bool> CheckMultiChannelName(string UserName, Platform platform, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from M in context.MultiChannels where (M.UserName == UserName && M.Platform == platform) select M).Any();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        /// <summary>
        /// The user selects channels to monitor, get all of the channel Ids for the selected channels.
        /// </summary>
        /// <param name="platform">The platform to retrieve</param>
        /// <returns>A list of monitored UserIds for the provided platform.</returns>
        internal Task<List<string>> GetMultiChannelIds(Platform platform, SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = (from M in context.MultiChannels where M.Platform == platform select M.UserId).ToList();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task<List<Tuple<WebhooksSource, Uri>>> GetMultiWebHooks(SQLDBContext Refcontext = null)
        {
            return Task.Run(() =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Tuple<WebhooksSource, Uri>> result = [.. (from W in context.MultiWebhooks
                                                              where W.IsEnabled
                                                              select new Tuple<WebhooksSource, Uri>(W.WebhooksSource, W.Webhook))];
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            });
        }

        internal Task PostMonitorChannel(IEnumerable<LiveUser> liveUsers, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (LiveUser U in liveUsers)
                {
                    if ((from L in context.MultiChannels
                         where (L.UserName == U.UserName && L.UserId == U.UserId && L.Platform == U.Platform)
                         select L).Any())
                    {
                        await context.MultiChannels.AddAsync(new(userId: U.UserId, userName: U.UserName, platform: U.Platform));
                    }
                }

                await context.SaveChangesAsync();
                UpdatedMonitoringChannels?.Invoke(this, new());
                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        internal Task<bool> PostMultiStreamDate(LiveUser liveUser, DateTime onDate, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                MultiChannels currUser = (from MC in context.MultiChannels where MC.UserId == liveUser.UserId select MC).FirstOrDefault();

                if (currUser.UserName != liveUser.UserName) // update username if it's changed
                {
                    currUser.UserName = liveUser.UserName;
                }

                bool result = (from P in context.MultiLiveStreams where (P.UserId == liveUser.UserId && P.LiveDate == onDate) select P).Any();
                if (!result)
                {
                    await context.MultiLiveStreams.AddAsync(new(userId: liveUser.UserId, platform: liveUser.Platform, liveDate: onDate));
                    await context.SaveChangesAsync();
                }
                if (Refcontext == null) { ClearDataContext(context); }

                return !result;
            });
        }

        internal void SummarizeStreamData(SQLDBContext Refcontext = null)
        {
            if (IsLiveStreamUpdated || CleanupList.Count == 0) // only perform if flag for update occurs
            {
                Task.Run(() =>
                 {
                     SQLDBContext context = Refcontext ?? BuildDataContext();
                     CleanupList.Clear();

                     List<DateTime> AllDates = new(from ML in context.MultiLiveStreams select ML.LiveDate.Date);
                     List<DateTime> UniqueDates = new(AllDates.Intersect(AllDates));

                     foreach (var A in (from M in UniqueDates.Select(uniqueDate => new ArchiveMultiStream()
                     {
                         ThroughDate = uniqueDate,
                         StreamCount = (from DateTime dates in AllDates
                                        where dates.Date <= uniqueDate
                                        select dates).Count()
                     })
                                        select M))
                     {
                         CleanupList.Add(A);
                     }

                     IsLiveStreamUpdated = false; // reset update flag indicator
                     if (Refcontext == null) { ClearDataContext(context); }
                 });
            }
        }

        internal Task SummarizeStreamData(ArchiveMultiStream archiveRecord, SQLDBContext Refcontext = null)
        {
            return Task.Run(async () =>
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                List<MultiLiveStreams> ArchiveRecords = new(from LS in context.MultiLiveStreams
                                                            where LS.LiveDate <= archiveRecord.ThroughDate.Date
                                                            select LS);
                List<string> UniqueUserIds = [];
                UniqueUserIds.UniqueAddRange(from LS in ArchiveRecords
                                             select LS.UserId);
                foreach (var (userId, CurrUser, MaxDate, CurrSummaryLiveStream) in from string userId in UniqueUserIds
                                                                                   let CurrUser = new List<MultiLiveStreams>(from AR in ArchiveRecords
                                                                                                                             where AR.UserId == userId
                                                                                                                             select AR)
                                                                                   let MaxDate = (from D in CurrUser
                                                                                                  select D.LiveDate).Max()
                                                                                   let CurrSummaryLiveStream = (from M in context.MultiSummaryLiveStreams
                                                                                                                where M.UserId == userId
                                                                                                                select M).FirstOrDefault()
                                                                                   select (userId, CurrUser, MaxDate, CurrSummaryLiveStream))
                {
                    if (CurrSummaryLiveStream == default)
                    {
                        await context.MultiSummaryLiveStreams.AddAsync(new(CurrUser.Count, MaxDate, userId, CurrUser.First().Platform));
                    }
                    else
                    {
                        CurrSummaryLiveStream.ThroughDate = MaxDate;
                        CurrSummaryLiveStream.StreamCount += CurrUser.Count;
                    }
                }

                context.MultiLiveStreams.RemoveRange(ArchiveRecords);

                IsLiveStreamUpdated = true;

                CleanupList.Clear();
                await context.SaveChangesAsync();
                SummarizeStreamData();

                if (Refcontext == null) { ClearDataContext(context); }
            });
        }

        #endregion

    }
}
