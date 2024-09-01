using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform), nameof(LiveDate))]
    public class MultiLiveStreams(DateTime liveDate = default,
                                 string userId = null,
                                 string userName = null,
                                 Platform platform = Platform.Default
                                 ) : UserBase(userId, userName, platform)
    {
        public DateTime LiveDate { get; set; } = liveDate;

        public MultiChannels MultiChannels { get; set; }
    }
}
