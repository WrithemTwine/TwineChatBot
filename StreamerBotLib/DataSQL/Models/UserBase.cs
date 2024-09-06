using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(UserId), nameof(UserName), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public abstract class UserBase(
        string userId,
    string userName,
                                   Platform platform)
#else
public abstract class UserBase(string userId = null,
    string userName = null,
                                   Platform platform = default)
#endif
 : EntityBase
    {
        public string UserId { get; set; } = userId;
        public string UserName { get; set; } = userName;
        public Platform Platform { get; set; } = platform;
    }
}
