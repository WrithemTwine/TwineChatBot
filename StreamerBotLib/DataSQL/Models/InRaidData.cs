using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    [Index(nameof(RaidDate), IsDescending = [true])]
    public class InRaidData(int viewerCount = 0,
                            DateTime raidDate = default,
                            string category = null,
                            string userId = null,
                            string userName = null,
                            Platform platform = Platform.Default) : UserBase(userId, userName, platform)
    {
        public int ViewerCount { get; set; } = viewerCount;
        public DateTime RaidDate { get; set; } = raidDate;
        public string Category { get; set; } = category;

        public Users User { get; set; }
        public CategoryList CategoryList { get; set; }
    }
}
