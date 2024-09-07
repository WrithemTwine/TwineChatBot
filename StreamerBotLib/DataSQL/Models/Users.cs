using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Users(
                       DateTime firstDateSeen,
                       DateTime currLoginDate,
                       DateTime lastDateSeen,
                       string userId,
                       string userName,
                       Platform platform)
#else
    public class Users(DateTime firstDateSeen = default,
    DateTime currLoginDate = default,
                       DateTime lastDateSeen = default,
                       string userId = null,
                       string userName = null,
                       Platform platform = Platform.Default)
#endif
 : UserBase(userId, platform)
    {
        public string UserName { get; set; } = userName;
        public DateTime FirstDateSeen { get; set; } = firstDateSeen;
        public DateTime CurrLoginDate { get; set; } = currLoginDate;
        public DateTime LastDateSeen { get; set; } = lastDateSeen;

        public ICollection<Currency> Currency { get; } = [];
        public ICollection<Followers> Followers { get; } = [];
        public ICollection<GiveawayUserData> GiveawayUserData { get; } = [];
        public ShoutOuts? ShoutOuts { get; set; }
        public CustomWelcome? CustomWelcome { get; set; }
        public UserStats UserStats { get; set; }

    }
}
