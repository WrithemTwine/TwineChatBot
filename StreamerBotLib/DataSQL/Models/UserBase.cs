using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(UserId), nameof(UserName), nameof(Platform))]
    public abstract class UserBase(string userId = null,
                                   string userName = null,
                                   Platform platform = default) : EntityBase
    {
        public string UserId { get; set; } = userId;
        public string UserName { get; set; } = userName;
        public Platform Platform { get; set; } = platform;
    }
}
