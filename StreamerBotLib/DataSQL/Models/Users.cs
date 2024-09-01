using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
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

        public ICollection<Currency> Currency { get; } = [];
        public Followers? Followers { get; set; }
        public ShoutOuts? ShoutOuts { get; set; }
        public CustomWelcome? CustomWelcome { get; set; }
        public UserStats UserStats { get; set; }

    }
}
