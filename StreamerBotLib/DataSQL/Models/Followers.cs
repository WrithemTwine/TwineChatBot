using Microsoft.EntityFrameworkCore;

using StreamerBotLib.Enums;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StreamerBotLib.DataSQL.Models
{
    [PrimaryKey(nameof(UserId), nameof(Platform))]
    [Index(nameof(StatusChangeDate), nameof(UserId), nameof(FollowedDate), IsDescending = [true, false, true])]
    [DebuggerDisplay("UserId={UserId}, UserName={F.User.UserName}, IsFollower={IsFollower}")]
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

        [AllowNull]
        public Users? User { get; set; }
        [AllowNull]
        public CategoryList? CategoryList { get; set; }

        //public ICollection<OldFollowUsers> OldFollowUsers { get; } = [];
    }
}
