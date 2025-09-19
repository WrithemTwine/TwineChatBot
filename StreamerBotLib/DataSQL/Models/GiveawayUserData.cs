using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(DateTime), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, UserName={User.UserName}, DateTime={DateTime}, Platform={Platform}")]
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
        public Users User { get; set; } = null!;
    }
}
