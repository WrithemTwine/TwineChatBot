using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiLiveStream(string userId = null,
                                 string userName = null,
                                 Platform platform = Platform.Default,
                                 DateTime liveDate = default) : UserBase(userId, userName, platform)
    {
        public DateTime LiveDate { get; set; } = liveDate;

        public Users? Users { get; set; }
    }
}
