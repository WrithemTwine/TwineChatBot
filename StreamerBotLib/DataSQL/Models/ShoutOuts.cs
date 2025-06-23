
namespace StreamerBotLib.DataSQL.Models
{
    using Microsoft.EntityFrameworkCore;

    using StreamerBotLib.Models.Enums;

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
        public Users User { get; set; } = null!;
    }
}
