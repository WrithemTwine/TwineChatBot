using Microsoft.EntityFrameworkCore;

namespace EFContextTest.EF
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(UserId), nameof(Platform))]
    public abstract class UserBase(string userId = "",
                               Platform platform = default)
    {
        public string UserId { get; set; } = userId;
        public Platform Platform { get; set; } = platform;
    }
}
