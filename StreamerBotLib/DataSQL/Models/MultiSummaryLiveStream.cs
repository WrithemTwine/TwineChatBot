using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiSummaryLiveStream(string userId = null,
                                        string userName = null,
                                        Platform platform = Platform.Default,
                                        int streamCount = 0,
                                        DateTime throughDate = default) : UserBase(userId, userName, platform)
    {
        public int StreamCount { get; set; } = streamCount;
        public DateTime ThroughDate { get; set; } = throughDate;

        public Users? Users { get; set; }
    }
}
