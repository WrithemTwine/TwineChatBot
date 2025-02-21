using EFEntityEntryTesting.Enums;

using Microsoft.EntityFrameworkCore;

namespace EFEntityEntryTesting.EF
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(UserId), nameof(Platform))]
    public abstract class UserBase(string userId = null,
                               Platform platform = default)
 : EntityBase
    {
        public string UserId { get; set; } = userId;
        public Platform Platform { get; set; } = platform;
    }
}
