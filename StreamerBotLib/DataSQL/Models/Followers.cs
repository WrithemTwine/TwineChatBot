using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(UserName), nameof(Platform))]
    [Index(nameof(StatusChangeDate), nameof(UserId), nameof(FollowedDate), IsDescending = [true, false, true])]
    public class Followers(
                           bool isFollower = false,
                           DateTime followedDate = default,
                           DateTime statusChangeDate = default,
                           string category = null,
                           DateTime addDate = default,
                           string userId = null,
                           string userName = null,
                           Platform platform = Platform.Default) : UserBase(userId, userName, platform)
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
