using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models;
using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.DataSQL.EFC9
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
            var result = await (from P in context.MultiLiveStreams
                                where (P.UserId == UserId
                                        && P.Platform == platform
                                        && P.LiveDate == dateTime)
                                select P).CountAsync() > 1;

            return result;
        }

        internal async Task<bool> CheckMultiChannelName(string UserName, Platform platform)
        {
            using var context = BuildDataContext();
            var result = await context.MultiChannels.Where(M => M.UserName == UserName && M.Platform == platform).Select(M => M).AnyAsync();
            return result;
        }

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
            return;
        }

        internal async Task<bool> PostMultiStreamDate(LiveUser liveUser, DateTime onDate)
        {
            return false;
        }

        internal Task SummarizeStreamData()
        {
            return null;
        }

        internal Task SummarizeStreamData(ArchiveMultiStream archiveRecord)
        {
            return null;
        }

            #endregion
        }
}
