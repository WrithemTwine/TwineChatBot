using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(DateTime), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class GiveawayUserData(DateTime dateTime,
                                  string userId,
                                  string userName,
                                  Platform platform)
#else
    public class GiveawayUserData(DateTime dateTime = default,
                                  string userId = default,
                                  Platform platform = Platform.Default)
#endif 
        : UserBase(userId, platform)
    {
        public DateTime DateTime { get; set; } = dateTime;
        public Users Users { get; set; } = null!;
    }
}
