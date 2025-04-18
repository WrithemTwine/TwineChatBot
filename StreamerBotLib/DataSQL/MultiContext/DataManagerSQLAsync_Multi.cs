using Microsoft.EntityFrameworkCore;

using StreamerBotLib.DataSQL.Models;
using StreamerBotLib.Enums;
using StreamerBotLib.Models;
using StreamerBotLib.Static;

namespace StreamerBotLib.DataSQL.MultiContext
{
    internal partial class DataManagerSQLAsync
    {
        #region MultiLive data
        internal event EventHandler UpdatedMonitoringChannels;
        private List<ArchiveMultiStream> CleanupList { get; } = [];
        private bool IsLiveStreamUpdated = false;

        internal List<ArchiveMultiStream> GetCleanupList()
        {
            return CleanupList;
        }

        internal async Task<bool> CheckMultiStreamDate(string UserId, Platform platform, DateTime dateTime)
        {
            using var context = BuildDataContext();
            var result = await (from P in context.MultiLiveStreams where (P.UserId == UserId && P.Platform == platform && P.LiveDate == dateTime) select P).CountAsync() > 1;

            return result;
        }

        internal async Task<bool> CheckMultiChannelName(string UserName, Platform platform)
        {
            using var context = BuildDataContext();
            var result = await context.MultiChannels.Where(M => M.UserName == UserName && M.Platform == platform).Select(M => M).AnyAsync();
            return result;
        }

        /// <summary>
        /// The user selects channels to monitor, get all of the channel Ids for the selected channels.
        /// </summary>
        /// <param name="platform">The platform to retrieve</param>
        /// <returns>A list of monitored UserIds for the provided platform.</returns>
        internal async Task<List<string>> GetMultiChannelIds(Platform platform)
        {
            using var context = BuildDataContext();
            return await context.MultiChannels
                                .Where(M => M.Platform == platform)
                                .Select(M => M.UserId)
                                .ToListAsync();
        }

        internal async Task<List<Tuple<WebhooksSource, Uri>>> GetMultiWebHooks()
        {
            using var context = BuildDataContext();
            return await context.MultiWebhooks
                                .Where(W => W.IsEnabled)
                                .Select(W => new Tuple<WebhooksSource, Uri>(W.WebhooksSource, W.Webhook))
                                .ToListAsync();
        }

        internal async Task PostMonitorChannel(IEnumerable<LiveUser> liveUsers)
        {
            using var context = BuildDataContext();

            var existingUsers = await context.MultiChannels
                .Where(mc => liveUsers.Any(lu => lu.UserName == mc.UserName && lu.UserId == mc.UserId && lu.Platform == mc.Platform))
                .ToListAsync();

            var newUsers = liveUsers
                .Where(lu => !existingUsers.Any(eu => eu.UserName == lu.UserName && eu.UserId == lu.UserId && eu.Platform == lu.Platform))
                .Select(lu => new MultiChannels(userId: lu.UserId, userName: lu.UserName, platform: lu.Platform));

            await context.MultiChannels.AddRangeAsync(newUsers);
            await context.SaveChangesAsync();

            UpdatedMonitoringChannels?.Invoke(this, EventArgs.Empty);
        }

        internal async Task<bool> PostMultiStreamDate(LiveUser liveUser, DateTime onDate)
        {
            using var context = BuildDataContext();

            var currUser = await context.MultiChannels
                .Include(mc => mc.MultiLiveStreams)
                .FirstOrDefaultAsync(mc => mc.UserId == liveUser.UserId);

            if (currUser == null)
            {
                return false; // User not found, no action needed
            }

            if (currUser.UserName != liveUser.UserName) // Update username if changed
            {
                currUser.UserName = liveUser.UserName;
            }

            if (!currUser.MultiLiveStreams.Any(m => m.LiveDate == onDate))
            {
                await context.MultiLiveStreams.AddAsync(new MultiLiveStreams
                {
                    UserId = liveUser.UserId,
                    Platform = liveUser.Platform,
                    LiveDate = onDate
                });
                await context.SaveChangesAsync();
                return true; // New stream date added
            }

            return false; // Stream date already exists
        }

        internal Task SummarizeStreamData()
        {
            return Task.Run(async () =>
            {
                if (!IsLiveStreamUpdated && CleanupList.Count > 0)
                {
                    return;
                }

                await ThreadManager.AddTaskToGUIDispatcher(new Task(() =>
                {
                    using var context = BuildDataContext();
                    CleanupList.Clear();

                    var allDates = context.MultiLiveStreams
                                          .Select(ml => ml.LiveDate.Date)
                                          .ToList();

                    var uniqueDates = allDates.Distinct().ToList();

                    CleanupList.AddRange(uniqueDates.Select(uniqueDate => new ArchiveMultiStream
                    {
                        ThroughDate = uniqueDate,
                        StreamCount = allDates.Count(date => date <= uniqueDate)
                    }));

                    IsLiveStreamUpdated = false; // Reset update flag
                }));
            });
        }

        internal Task SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            return Task.Run(async () =>
            {
                LogWriter.DebugLog("SummarizeStreamData", DebugLogTypes.DataManager, $"Summarize multi-stream data, referring to {archiveRecord}.");
                using var context = BuildDataContext();

                // Fetch all records to archive in a single query
                var archiveRecords = await context.MultiLiveStreams
                    .Where(ls => ls.LiveDate <= archiveRecord.ThroughDate.Date)
                    .ToListAsync();

                // Group records by UserId for efficient processing
                var groupedRecords = archiveRecords
                    .GroupBy(ls => ls.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Prepare new or updated summary records
                var newSummaries = new List<MultiSummaryLiveStreams>();
                foreach (var (userId, userRecords) in groupedRecords)
                {
                    var maxDate = userRecords.Max(r => r.LiveDate);
                    var platform = userRecords.First().Platform;

                    var existingSummary = await context.MultiSummaryLiveStreams
                        .FirstOrDefaultAsync(ms => ms.UserId == userId);

                    if (existingSummary == null)
                    {
                        newSummaries.Add(new MultiSummaryLiveStreams(userRecords.Count, maxDate, userId, platform));
                    }
                    else
                    {
                        existingSummary.ThroughDate = maxDate;
                        existingSummary.StreamCount += userRecords.Count;
                    }
                }

                // Add new summaries and remove archived records in batches
                if (newSummaries.Count != 0)
                {
                    await context.MultiSummaryLiveStreams.AddRangeAsync(newSummaries);
                }
                context.MultiLiveStreams.RemoveRange(archiveRecords);

                // Save changes and update state
                await context.SaveChangesAsync();
                IsLiveStreamUpdated = true;

                // Clear and refresh lists
                CleanupList.Clear();
                await SummarizeStreamData();
                RefreshMultiLiveStreamsList(true);
                RefreshMultiSummaryLiveStreamsList(true);
            });
        }

        #endregion

    }
}
