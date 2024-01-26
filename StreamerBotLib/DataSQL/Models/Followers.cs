using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    public class Followers(int id = 0,
                           string userId = null,
                           string userName = null,
                           Platform platform = Platform.Default,
                           bool isFollower = false,
                           DateTime followedDate = default,
                           DateTime statusChangeDate = default,
                           DateTime addDate = default,
                           string category = null) : UserBase(id, userId, userName, platform)
    {
        public bool IsFollower { get; set; } = isFollower;
        public DateTime FollowedDate { get; set; } = followedDate;
        public DateTime StatusChangeDate { get; set; } = statusChangeDate;
        public string Category { get; set; } = category;
        public DateTime AddDate { get; set; } = addDate;

        public Users? Users { get; set; }
        public CategoryList? CategoryList { get; set; }
    }
}
