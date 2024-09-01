using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    public class MultiSummaryLiveStreams(int streamCount = 0,
                                        DateTime throughDate = default,
                                        string userId = null,
                                        string userName = null,
                                        Platform platform = Platform.Default) : UserBase(userId, userName, platform)
    {
        public int StreamCount { get; set; } = streamCount;
        public DateTime ThroughDate { get; set; } = throughDate;

        public MultiChannels MultiChannels { get; set; }
    }
}
