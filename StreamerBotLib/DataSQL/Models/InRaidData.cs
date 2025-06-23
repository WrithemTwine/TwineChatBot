
namespace StreamerBotLib.DataSQL.Models
{
    using Microsoft.EntityFrameworkCore;

    using StreamerBotLib.Models.Enums;

    [PrimaryKey(nameof(UserId), nameof(Platform), nameof(RaidDate))]
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
                            Platform platform = Platform.Default)
#endif

        : UserBase(userId, platform)
    {
        public int ViewerCount { get; set; } = viewerCount;
        public DateTime RaidDate { get; set; } = raidDate;
        public string Category { get; set; } = category;
        public Users User { get; set; } = null!;
    }
}
