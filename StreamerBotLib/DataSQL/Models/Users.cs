using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class Users(uint id = 0,
                       string userId = null,
                       string userName = null,
                       Platform platform = Platform.Default,
                       DateTime firstDateSeen = default,
                       DateTime currLoginDate = default,
                       DateTime lastDateSeen = default) : UserBase(id, userId, userName, platform)
    {
        public DateTime FirstDateSeen { get; set; } = firstDateSeen;
        public DateTime CurrLoginDate { get; set; } = currLoginDate;
        public DateTime LastDateSeen { get; set; } = lastDateSeen;

        public UserStats UserStats { get; set; }
        public ICollection<Currency> Currency { get; } = new List<Currency>();
        public Followers? Followers { get; set; }
        public ShoutOuts? ShoutOuts { get; set; }
        public CustomWelcome? CustomWelcome { get; set; }

        public MultiLiveStream? MultiLiveStream { get; set; }
        public MultiChannels? MultiChannels { get; set; }
        public MultiSummaryLiveStream? MultiSummaryLiveStream { get; set; }
    }
}
