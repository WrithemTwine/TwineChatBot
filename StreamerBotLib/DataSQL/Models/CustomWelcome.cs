using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Models.Enums;

using System.Diagnostics;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [DebuggerDisplay("UserId={UserId}, Platform={Platform}, Message={Message}")]
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
        public Users User { get; set; } = null!;
    }
}
