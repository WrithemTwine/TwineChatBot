using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
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
        [AllowNull]
        public Users? User { get; set; }
    }
}
