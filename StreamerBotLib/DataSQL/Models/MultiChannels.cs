using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    public class MultiChannels(string userId = null,
                               string userName = null,
                               Platform platform = Platform.Default) : UserBase(userId, userName, platform)
    {
        public ICollection<MultiLiveStreams> MultiLiveStreams { get; } = [];

        public MultiSummaryLiveStreams? MultiSummaryLiveStreams { get; set; }
    }
}
