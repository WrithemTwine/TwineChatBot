using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform), nameof(RaidDate))]
    [Index(nameof(RaidDate), IsDescending = [true])]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class InRaidData(int viewerCount,
                            DateTime raidDate,
                            string category,
                            string userId,
                            string userName,
                            Platform platform)
#else
    public class InRaidData(int viewerCount = 0,
                            DateTime raidDate = default,
                            string category = null,
                            string userId = null,
                            string userName = null,
                            Platform platform = Platform.Default)
#endif

        : UserBase(userId, userName, platform)
    {
        public int ViewerCount { get; set; } = viewerCount;
        public DateTime RaidDate { get; set; } = raidDate;
        public string Category { get; set; } = category;
    }
}
