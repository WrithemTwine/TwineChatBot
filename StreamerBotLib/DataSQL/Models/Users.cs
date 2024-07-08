using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    [Index(nameof(LastDateSeen), IsDescending = [true])]
    public class Users(DateTime firstDateSeen = default,
                       DateTime currLoginDate = default,
                       DateTime lastDateSeen = default,
                       string userId = null,
                       string userName = null,
                       Platform platform = Platform.Default) : UserBase(userId, userName, platform)
    {
        public DateTime FirstDateSeen { get; set; } = firstDateSeen;
        public DateTime CurrLoginDate { get; set; } = currLoginDate;
        public DateTime LastDateSeen { get; set; } = lastDateSeen;

        public ICollection<Currency> Currency { get; } = new List<Currency>();
        public Followers Followers { get; set; }
        public ICollection<InRaidData>? InRaidData { get; } = new List<InRaidData>();
        public ShoutOuts? ShoutOuts { get; set; }
        public CustomWelcome? CustomWelcome { get; set; }
        public UserStats UserStats { get; set; }

        public ICollection<MultiLiveStreams>? MultiLiveStreams { get; } = new List<MultiLiveStreams>();
        public MultiChannels? MultiChannels { get; set; }
        public MultiSummaryLiveStreams? MultiSummaryLiveStreams { get; set; }
    }
}
