using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class CustomWelcome(string message,
                               string userId,
                               string userName,
                               Platform platform
                               )
#else
    public class CustomWelcome(string message = null,
                               string userId = null,
                               Platform platform = Platform.Default
                               )
#endif
        : UserBase(userId, platform)
    {
        public string Message { get; set; } = message;

        public Users? Users { get; set; }
    }
}
