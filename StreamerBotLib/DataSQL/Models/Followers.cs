
namespace StreamerBotLib.DataSQL.Models
{
    using Microsoft.EntityFrameworkCore;

    using StreamerBotLib.Models.Enums;

    using System.Diagnostics;

    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(StatusChangeDate), nameof(UserId), nameof(FollowedDate), IsDescending = [true, false, true])]
    [DebuggerDisplay("UserId={UserId}, UserName={User.UserName}, IsFollower={IsFollower}")]
#if DEBUG_EFMODELS_NODEFAULTPARAM
    public class Followers(
                           bool isFollower,
                           DateTime followedDate,
                           DateTime statusChangeDate,
                           string category,
                           DateTime addDate,
                           string userId,
                           string userName,
                           Platform platform)
#else
    public class Followers(
                           bool isFollower = false,
                           DateTime followedDate = default,
                           DateTime statusChangeDate = default,
                           string category = null,
                           DateTime addDate = default,
                           string userId = null,
                           Platform platform = Platform.Default)
#endif
        : UserBase(userId, platform)
    {
        public bool IsFollower { get; set; } = isFollower;
        public DateTime FollowedDate { get; set; } = followedDate;
        public DateTime StatusChangeDate { get; set; } = statusChangeDate;
        public string Category { get; set; } = category;
        public DateTime AddDate { get; set; } = addDate;

        public Users User { get; set; } = null!;
        public CategoryList CategoryList { get; set; } = null!;
    }
}
