using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class InRaidData(uint id = 0,
                            string userId = null,
                            string userName = null,
                            Platform platform = Platform.Default,
                            uint viewerCount = 0,
                            DateTime raidDate = default,
                            string category = null,
                            CategoryList categoryList = default) : UserBase(id, userId, userName, platform)
    {
        public uint ViewerCount { get; set; } = viewerCount;
        public DateTime RaidDate { get; set; } = raidDate;
        public string Category { get; set; } = category;


        public CategoryList CategoryList { get; set; } = categoryList;
    }
}
