using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class ShoutOuts(string userId,
    string userName,
                           Platform platform)
#else
    public class ShoutOuts(string userId = null,
    string userName = null,
                           Platform platform = Platform.Default)
#endif
 : UserBase(userId, userName, platform)
    {

        public Users? Users { get; set; }
    }
}
