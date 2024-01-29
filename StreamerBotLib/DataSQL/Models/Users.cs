using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    public class Users(string userId = null,
                       string userName = null,
                       Platform platform = Platform.Default,
                       DateTime firstDateSeen = default,
                       DateTime currLoginDate = default,
                       DateTime lastDateSeen = default) : UserBase(userId, userName, platform)
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

        public ICollection<MultiLiveStream>? MultiLiveStream { get; } = new List<MultiLiveStream>();
        public MultiChannels? MultiChannels { get; set; }
        public MultiSummaryLiveStream? MultiSummaryLiveStream { get; set; }
    }
}
