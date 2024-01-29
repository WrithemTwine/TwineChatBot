using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    public class ShoutOuts(string userId = null,
                           string userName = null,
                           Platform platform = Platform.Default) : UserBase(userId, userName, platform)
    {
        public Users Users { get; set; }
    }
}
