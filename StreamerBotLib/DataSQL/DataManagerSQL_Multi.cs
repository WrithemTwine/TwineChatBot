using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.GUI;
using StreamerBotLib.Interfaces;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

using System.Collections.ObjectModel;

namespace StreamerBotLib.DataSQL
{
    public partial class DataManagerSQL : IDataManager, IDataManagerReadOnly, IDataManagerTestMethods
    {
        #region MultiLive data
        public event EventHandler UpdatedMonitoringChannels;
        public ObservableCollection<ArchiveMultiStream> CleanupList { get; } = [];
        private bool IsLiveStreamUpdated = false;
        public string MultiLiveStatusLog { get; private set; } = "";
        private List<string> MultiLiveStatusList = [];
        private const int MaxList = 50;

        public ObservableCollection<ArchiveMultiStream> GetCleanupList()
        {
            return CleanupList;
        }

        public bool CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from P in context.MultiLiveStreams where (P.UserId == UserId && P.Platform == platform && P.LiveDate == dateTime) select P).Count() > 1;
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public bool CheckMultiChannelName(string UserName, Platform platform, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                var result = (from M in context.MultiChannels where (M.UserName == UserName && M.Platform == platform) select M).Any();
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        /// <summary>
        /// The user selects channels to monitor, get all of the channel Ids for the selected channels.
        /// </summary>
        /// <param name="platform">The platform to retrieve</param>
        /// <returns>A list of monitored UserIds for the provided platform.</returns>
        public List<string> GetMultiChannelIds(Platform platform, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<string> result = new(from M in context.MultiChannels where M.Platform == platform select M.UserId);
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public List<Tuple<WebhooksSource, Uri>> GetMultiWebHooks(SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                List<Tuple<WebhooksSource, Uri>> result = new(from W in context.MultiWebhooks where W.IsEnabled select new Tuple<WebhooksSource, Uri>(W.WebhooksSource, W.Webhook));
                if (Refcontext == null) { ClearDataContext(context); }
                return result;
            }
        }

        public void PostMonitorChannel(IEnumerable<LiveUser> liveUsers, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();
                foreach (LiveUser U in liveUsers)
                {
                    if ((from L in context.MultiChannels
                         where (L.UserName == U.UserName && L.UserId == U.UserId && L.Platform == U.Platform)
                         select L).Any())
                    {
                        context.MultiChannels.Add(new(userId: U.UserId, userName: U.UserName, platform: U.Platform));
                    }
                }

                context.SaveChanges(true);
                UpdatedMonitoringChannels?.Invoke(this, new());
                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        public void PostMultiLiveLog(string LogItem, SQLDBContext Refcontext = null)
        {
            lock (MultiLiveStatusLog)
            {
                MultiLiveStatusList.Insert(0, LogItem);

                if (MultiLiveStatusList.Count > MaxList)
                {
                    MultiLiveStatusList.RemoveRange(MaxList - 1, MultiLiveStatusList.Count - MaxList);
                }
                MultiLiveStatusLog = string.Join("\r\n", MultiLiveStatusList);
            }

            NotifyDataCollectionUpdated(nameof(MultiLiveStatusLog));
        }

        public bool PostMultiStreamDate(string userid, string username, Platform platform, DateTime onDate, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
            {
                SQLDBContext context = Refcontext ?? BuildDataContext();

                MultiChannels currUser = (from MC in context.MultiChannels where MC.UserId == userid select MC).FirstOrDefault();

                if (currUser.UserName != username) // update username if it's changed
                {
                    currUser.UserName = username;
                }

                bool result = (from P in context.MultiLiveStreams where (P.UserId == userid && P.LiveDate == onDate) select P).Any();
                if (!result)
                {
                    //var channeldata = (from C in context.MultiChannels where (userid == C.UserId) select C).FirstOrDefault();
                    //if ( channeldata == null || (!string.Equals(channeldata.UserName, username, StringComparison.OrdinalIgnoreCase)) )
                    //{ // add missing updated user names to channels to ensure relation integrity

                    //    context.MultiChannels.Add(new(userid, username, platform: Platform.Twitch));
                    //}

                    context.MultiLiveStreams.Add(new(userId: userid, platform: platform, liveDate: onDate));
                    context.SaveChanges(true);
                }
                if (Refcontext == null) { ClearDataContext(context); }

                return !result;
            }
        }

        public void SummarizeStreamData(SQLDBContext Refcontext = null)
        {
            if (IsLiveStreamUpdated || CleanupList.Count == 0) // only perform if flag for update occurs
            {
                lock (GUIDataManagerLock.Lock)
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
                }
            }
        }

        public void SummarizeStreamData(ArchiveMultiStream archiveRecord, SQLDBContext Refcontext = null)
        {
            lock (GUIDataManagerLock.Lock)
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
                        context.MultiSummaryLiveStreams.Add(new(CurrUser.Count, MaxDate, userId, CurrUser.First().Platform));
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
                context.SaveChanges(true);
                SummarizeStreamData();

                if (Refcontext == null) { ClearDataContext(context); }
            }
        }

        #endregion

    }
}
