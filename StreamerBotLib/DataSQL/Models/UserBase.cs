using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(UserId), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public abstract class UserBase(
        string userId,
    string userName,
                                   Platform platform)
#else
public abstract class UserBase(string userId = null,
                               Platform platform = default)
#endif
 : EntityBase
    {
        public string UserId { get; set; } = userId;
        public Platform Platform { get; set; } = platform;
    }
}
