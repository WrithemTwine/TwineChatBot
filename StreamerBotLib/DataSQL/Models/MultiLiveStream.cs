using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class MultiLiveStream(uint id = 0,
                                 string userId = null,
                                 string userName = null,
                                 Platform platform = Platform.Default,
                                 DateTime liveDate = default) : UserBase(id, userId, userName, platform)
    {
        public DateTime LiveDate { get; set; } = liveDate;
    }
}
