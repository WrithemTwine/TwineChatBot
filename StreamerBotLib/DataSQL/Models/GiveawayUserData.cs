using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class GiveawayUserData(DateTime dateTime,
                                  string userId,
                                  string userName,
                                  Platform platform)
#else
    public class GiveawayUserData(DateTime dateTime = default,
                                  string userId = default,
                                  string userName = default,
                                  Platform platform = Platform.Default)
#endif 
        : UserBase(userId, userName, platform)
    {

        public DateTime DateTime { get; set; } = dateTime;
    }
}
