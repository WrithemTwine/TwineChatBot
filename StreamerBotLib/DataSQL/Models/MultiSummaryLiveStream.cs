using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiSummaryLiveStream(uint id = 0,
                                        string userId = null,
                                        string userName = null,
                                        Platform platform = Platform.Default,
                                        uint streamCount = 0,
                                        DateTime throughDate = default) : UserBase(id, userId, userName, platform)
    {
        public uint StreamCount { get; set; } = streamCount;
        public DateTime ThroughDate { get; set; } = throughDate;

        public Users? Users { get; set; }
    }
}
