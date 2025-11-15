using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, StreamCount={StreamCount}, ThroughDate={ThroughDate}")]
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
                                        Platform platform = Platform.Default)
#endif
 : UserBase(userId, platform)
    {

        public int StreamCount { get; set; } = streamCount;
        public DateTime ThroughDate { get; set; } = throughDate;

        public MultiChannels MultiChannels { get; set; } = null!;
    }
}
