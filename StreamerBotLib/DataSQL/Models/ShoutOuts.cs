using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, User={User}, Platform={Platform}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class ShoutOuts(string userId,
    string userName,
                           Platform platform)
#else
    public class ShoutOuts(string userId = null,
                           Platform platform = Platform.Default)
#endif
 : UserBase(userId, platform)
    {
        public Users User { get; set; } = null!;
    }
}
