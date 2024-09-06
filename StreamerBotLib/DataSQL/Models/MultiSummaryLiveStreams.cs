using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class MultiSummaryLiveStreams(int streamCount,
                                        DateTime throughDate,
                                        string userId,
                                        string userName,
                                        Platform platform)
#else
    public class MultiSummaryLiveStreams(int streamCount = 0,
                                        DateTime throughDate = default,
                                        string userId = null,
                                        string userName = null,
                                        Platform platform = Platform.Default)
#endif
 : UserBase(userId, userName, platform)
    {

        public int StreamCount { get; set; } = streamCount;
        public DateTime ThroughDate { get; set; } = throughDate;

        public MultiChannels MultiChannels { get; set; }
    }
}
